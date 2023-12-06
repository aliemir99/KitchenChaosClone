using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RollingCounter : BaseCounter,IHasProgress
{
    public static event EventHandler OnAnyRoll;

    new public static void ResetStaticData()
    {
        OnAnyRoll = null;
    }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnRoll;

    [SerializeField] private RollingRecipeSO[] rollingRecipeSOArray;

    private int rollingProgress;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //There is no KitchenObject here
            if (player.HasKitchenObject())
            {
                //Player is carrying something
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    //Player is carrying something that can be rolled
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);
                    InteractLogicPlaceObjectOnCounterServerRpc();
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
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            }
            else
            {
                //player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        rollingProgress = 0;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }
    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            //There is a KitchenObject here AND it can be rolled
            RollObjectServerRpc();
            TestRollingProgressDoneServerRpc();

        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void RollObjectServerRpc()
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            //There is a KitchenObject here AND it can be rolled
            RollObjectClientRpc();
        }
    }
    [ClientRpc]
    private void RollObjectClientRpc()
    {
        rollingProgress++;

        OnRoll?.Invoke(this, EventArgs.Empty);
        OnAnyRoll?.Invoke(this, EventArgs.Empty);


        RollingRecipeSO rollingRecipeSO = GetRollingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)rollingProgress / rollingRecipeSO.rollingProgressMax
        });
    }
    [ServerRpc(RequireOwnership = false)]
    private void TestRollingProgressDoneServerRpc()
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            //There is a KitchenObject here AND it can be rolled
            RollingRecipeSO rollingRecipeSO = GetRollingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            if (rollingProgress >= rollingRecipeSO.rollingProgressMax)
            {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

                KitchenObject.DestroyKitchenObject(GetKitchenObject());

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        RollingRecipeSO rollingRecipeSO = GetRollingRecipeSOWithInput(inputKitchenObjectSO);
        return rollingRecipeSO != null;
    }
    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        RollingRecipeSO rollingRecipeSO = GetRollingRecipeSOWithInput(inputKitchenObjectSO);
        if (rollingRecipeSO != null)
        {
            return rollingRecipeSO.output;
        }
        else
        {
            return null;
        }

    }

    private RollingRecipeSO GetRollingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (RollingRecipeSO rollingRecipeSO in rollingRecipeSOArray)
        {
            if (rollingRecipeSO.input == inputKitchenObjectSO)
            {
                return rollingRecipeSO;
            }
        }
        return null;
    }
}
