using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateTriggerParent : MonoBehaviour
{
    public bool HasTriggered { get; set; }

    private void Start()
    {
        HasTriggered = false;
    }
}