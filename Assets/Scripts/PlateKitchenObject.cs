using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    private const string BREAD = "Bread";
    private const string DOUGH_PIZZA = "Dough Pizza";
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs: EventArgs
    {
        public KitchenObjectSO KitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    private List<KitchenObjectSO> kitchenObjectSOList;
    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            //not a valid ingredient
            return false;
        }

        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            //Already has this type
            return false;
        }
        //If trying to add bread or dough
        if (kitchenObjectSO.objectName == BREAD || kitchenObjectSO.objectName == DOUGH_PIZZA)
        {
            //and the plate contains one or the other
            foreach (KitchenObjectSO kitchenObject in kitchenObjectSOList)
            {
                if (kitchenObject.objectName == BREAD || kitchenObject.objectName == DOUGH_PIZZA)
                {
                    return false;
                }
            }
          
        } 

        kitchenObjectSOList.Add(kitchenObjectSO);
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            KitchenObjectSO = kitchenObjectSO
        });
        return true;
       
    } 
    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}
