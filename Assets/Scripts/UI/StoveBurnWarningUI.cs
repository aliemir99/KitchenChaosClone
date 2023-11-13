using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnWarningUI : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private OvenCounter ovenCounter;

    private void Start()
    {
        if (stoveCounter)
        {
            stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        }
        if (ovenCounter)
        {
            ovenCounter.OnProgressChanged += OvenCounter_OnProgressChanged;
        }

        Hide();
    }

    private void OvenCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        bool show = ovenCounter.IsBaked() && e.progressNormalized >= burnShowProgressAmount;

        if (show)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        bool show = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;

        if (show)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
