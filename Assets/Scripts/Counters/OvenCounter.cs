using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class OvenCounter : BaseCounter,IHasProgress
{
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnPlayerInteracted;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle,
        Baking,
        Baked,
        Burned
    }

    [SerializeField] private BakingRecipeSO[] bakingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> bakingTimer = new NetworkVariable<float>(0f);
    private BakingRecipeSO bakingRecipeSO;
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private BurningRecipeSO burningRecipeSO;

    public override void OnNetworkSpawn()
    {
        bakingTimer.OnValueChanged += BakingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }
    private void BakingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float bakingTimerMax = bakingRecipeSO != null ? bakingRecipeSO.bakingTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = bakingTimer.Value / bakingTimerMax
        });
    }
    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }
    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = state.Value
        });

        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            });
        }

    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Baking:
                    bakingTimer.Value += Time.deltaTime;

                    if (bakingTimer.Value > bakingRecipeSO.bakingTimerMax)
                    {
                        //Baked
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(bakingRecipeSO.output, this);

                        state.Value = State.Baked;
                        burningTimer.Value = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                        SetBurningRecipeSOClientRpc(
                          KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
                      );
                    }
                    break;
                case State.Baked:
                    burningTimer.Value += Time.deltaTime;

                    if (burningTimer.Value > burningRecipeSO.burningTimerMax)
                    {
                        //Baked
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                    break;
            }
        }

    }
    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //There is no KitchenObject here
            if (player.HasKitchenObject())
            {
                player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject);
                if (plateKitchenObject)
                {
                    if (HasRecipeWithInput(plateKitchenObject.GetKitchenObjectSOList()))
                    {
                        KitchenObject kitchenObject = player.GetKitchenObject();
                        //Player is carrying something that can be baked
                        kitchenObject.SetKitchenObjectParent(this);

                        InteractLogicPlaceObjectOnCounterServerRpc(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
                        );
                        //Player loses the plate give them a new one.
                        KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);

                    }
                }

            }
            else
            {
                //player not carrying anything
            }
        }
        else
        {
            //There is KitchenObject here
            if (player.HasKitchenObject())
            {
                //Player is carrying something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //player is holding a Plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        SetStateIdleServerRpc();
                    }
                }
            }
            else
            {
                //player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        bakingTimer.Value = 0f;
        state.Value = State.Baking;
        
        SetBakingRecipeSOClientRpc(kitchenObjectSOIndex);
        OnPlayerInteracted?.Invoke(this, EventArgs.Empty);
    }
    [ClientRpc]
    private void SetBakingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        //KITCHEN OBJECT IS PLATE
        //FIND A WAY TO GET THE PLATE CONTENT AND COMPARE AGAINST 
        //bakingRecipeSOArray.Any(recipe => recipe.inputList)
        //IF MATCHES 
        //bakingRecipeSO = GetBakingRecipeSOWithInput(kitchenObjectSOList);        
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }
    private bool HasRecipeWithInput(List<KitchenObjectSO> inputKitchenObjectSOList)
    {
        BakingRecipeSO bakingRecipeSO = GetBakingRecipeSOWithInput(inputKitchenObjectSOList);
        return bakingRecipeSO != null;
    }
    private KitchenObjectSO GetOutputForInput(List<KitchenObjectSO> inputKitchenObjectSOList)
    {
        BakingRecipeSO bakingRecipeSO = GetBakingRecipeSOWithInput(inputKitchenObjectSOList);
        if (bakingRecipeSO != null)
        {
            return bakingRecipeSO.output;
        }
        else
        {
            return null;
        }

    }

    private BakingRecipeSO GetBakingRecipeSOWithInput(List<KitchenObjectSO> inputKitchenObjectSOList)
    {
        foreach (BakingRecipeSO bakingRecipeSO in bakingRecipeSOArray)
        {
            if (bakingRecipeSO.inputList.Count != inputKitchenObjectSOList.Count)
            {
                return null;
            }
            List<KitchenObjectSO> recipe = bakingRecipeSO.inputList;
            List<KitchenObjectSO> plateContent = inputKitchenObjectSOList;

            bool contentsMatch = false;
            for (int i = 0; i < recipe.Count; i++)
            {
                if (plateContent.Contains(recipe[i]))
                {
                    contentsMatch = true;
                }
                else
                {
                    contentsMatch = false;
                    break;
                }
            }
           
            if (contentsMatch)
            {
                return bakingRecipeSO;
            }
            
        }
        return null;
    }
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }

    public bool IsBaked()
    {
        return state.Value == State.Baked;
    }
}
