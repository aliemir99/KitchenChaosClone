using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHGING = "IsFlashing";
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private OvenCounter ovenCounter;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        if (stoveCounter)
        {
            stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        }
        if (ovenCounter)
        {
            ovenCounter.OnProgressChanged += OvenCounter_OnProgressChanged; ;
        }
        animator.SetBool(IS_FLASHGING, false);
    }

    private void OvenCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        bool show = ovenCounter.IsBaked() && e.progressNormalized >= burnShowProgressAmount;

        animator.SetBool(IS_FLASHGING, show);
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        bool show = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;

        animator.SetBool(IS_FLASHGING, show);
    }

}
