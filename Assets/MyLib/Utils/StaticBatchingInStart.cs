using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StaticBatchingInStart : MonoBehaviour
{
    [SerializeField] private float _delayStart = default;
    private void Start()
    {
        DOVirtual.DelayedCall(_delayStart, () =>
        {
            StaticBatchingUtility.Combine(gameObject);
        });
    }
}
