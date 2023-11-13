using System;
using System.Collections;
using System.Collections.Generic;
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
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    rollingProgress = 0;

                    RollingRecipeSO rollingRecipeSO = GetRollingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = (float)rollingProgress / rollingRecipeSO.rollingProgressMax
                    });
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
    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            //There is a KitchenObject here AND it can be rolled
            rollingProgress++;

            OnRoll?.Invoke(this, EventArgs.Empty);
            OnAnyRoll?.Invoke(this, EventArgs.Empty);


            RollingRecipeSO rollingRecipeSO = GetRollingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = (float)rollingProgress / rollingRecipeSO.rollingProgressMax
            });

            if (rollingProgress >= rollingRecipeSO.rollingProgressMax)
            {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
                GetKitchenObject().DestroySelf();

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
