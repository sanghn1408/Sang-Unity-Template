using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CheckTriggerWithPlayer : MonoBehaviour
{
    public Action OnTriggerWithPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Tag.Player))
        {
            OnTriggerWithPlayer?.Invoke();
        }
    }
}
