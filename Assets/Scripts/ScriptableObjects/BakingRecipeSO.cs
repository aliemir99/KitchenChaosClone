using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BakingRecipeSO : ScriptableObject
{
    public List<KitchenObjectSO> inputList;
    public KitchenObjectSO output;
    public float bakingTimerMax;
}
