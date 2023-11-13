using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private OvenCounter ovenCounter;
    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        if (stoveCounter)
        {
            stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
            stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        }
        if (ovenCounter)
        {
            ovenCounter.OnStateChanged += OvenCounter_OnStateChanged;
            ovenCounter.OnProgressChanged += OvenCounter_OnProgressChanged;
        }
    }

    private void OvenCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        playWarningSound = ovenCounter.IsBaked() && e.progressNormalized >= burnShowProgressAmount;
    }

    private void OvenCounter_OnStateChanged(object sender, OvenCounter.OnStateChangedEventArgs e)
    {
        bool playSound = e.state == OvenCounter.State.Baking || e.state == OvenCounter.State.Baked;
        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        playWarningSound = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        bool playSound = e.state == StoveCounter.State.Frying || e.state == StoveCounter.State.Fried;
        if (playSound)
        {
            audioSource.Play();
        }
        else 
        {
            audioSource.Pause();
        }
    }
    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if(warningSoundTimer <= 0f)
            {
                float warningSoundTimerMax = .2f;
                warningSoundTimer = warningSoundTimerMax;
                if (stoveCounter)
                {
                    SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
                }
                if (ovenCounter)
                {
                    SoundManager.Instance.PlayWarningSound(ovenCounter.transform.position);
                }
                
            }
        }
    }
}
