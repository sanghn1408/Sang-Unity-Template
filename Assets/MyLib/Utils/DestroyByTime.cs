using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    [SerializeField] private float _timeDelay = default;

    private void Start()
    {
        Destroy(gameObject, _timeDelay);
    }
}
