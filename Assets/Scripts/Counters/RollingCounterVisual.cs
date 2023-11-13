using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingCounterVisual : MonoBehaviour
{
    private const string ROLL = "Roll";
    [SerializeField] private RollingCounter rollingCounter;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        rollingCounter.OnRoll += RollingCounter_OnRoll;
    }

    private void RollingCounter_OnRoll(object sender, EventArgs e)
    {
        animator.SetTrigger(ROLL);
    }
}
