using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private State state;
    private float bakingTimer;
    private BakingRecipeSO bakingRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }
    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Baking:
                    bakingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = bakingTimer / bakingRecipeSO.bakingTimerMax
                    });

                    if (bakingTimer > bakingRecipeSO.bakingTimerMax)
                    {
                        //Baked
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(bakingRecipeSO.output, this);


                        state = State.Baked;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = bakingTimer / bakingRecipeSO.bakingTimerMax
                        });
                    }
                    break;
                case State.Baked:
                    burningTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                    });

                    if (burningTimer > burningRecipeSO.burningTimerMax)
                    {
                        //Baked
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state = State.Burned;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
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
                        bakingTimer = 0f;
                        OnPlayerInteracted?.Invoke(this, EventArgs.Empty);
                        //Player is carrying something that can be baked
                        player.GetKitchenObject().SetKitchenObjectParent(this);
                        //Player loses the plate give them a new one.
                        KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);


                        bakingRecipeSO = GetBakingRecipeSOWithInput(plateKitchenObject.GetKitchenObjectSOList());

                        state = State.Baking;
                        bakingTimer = 0f;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });
                    }
                }

            }
            else
            {
                //player has carrying anything
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
                        GetKitchenObject().DestroySelf();
                        OnPlayerInteracted?.Invoke(this, EventArgs.Empty);
                        state = State.Idle;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                }
            }
            else
            {
                //player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);

                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    state = state
                });

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });
            }
        }
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
        return state == State.Baked;
    }
}
