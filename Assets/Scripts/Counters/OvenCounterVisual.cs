using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenCounterVisual : MonoBehaviour
{
    private const string OPEN_CLOSE = "OpenClose";
    [SerializeField] private OvenCounter ovenCounter;
    [SerializeField] private GameObject stoveOnGameObject;
    [SerializeField] private GameObject particlesGameObject;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        ovenCounter.OnStateChanged += OvenCounter_OnStateChanged;
        ovenCounter.OnPlayerInteracted += OvenCounter_OnPlayerInteracted;
    }

    private void OvenCounter_OnPlayerInteracted(object sender, System.EventArgs e)
    {
        animator.SetTrigger(OPEN_CLOSE);
    }

    private void OvenCounter_OnStateChanged(object sender, OvenCounter.OnStateChangedEventArgs e)
    {
        bool showVisual = e.state == OvenCounter.State.Baking || e.state == OvenCounter.State.Baked;
        stoveOnGameObject.SetActive(showVisual);
        particlesGameObject.SetActive(showVisual);
    }
}
