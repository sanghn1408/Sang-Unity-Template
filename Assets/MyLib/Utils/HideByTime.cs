using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideByTime : MonoBehaviour
{
    [SerializeField] private float _timeHide = default;

    private void OnEnable()
    {
        Invoke(nameof(HideObject), _timeHide);
    }

    private void HideObject()
    {
        gameObject.SetActive(false);
    }
}
