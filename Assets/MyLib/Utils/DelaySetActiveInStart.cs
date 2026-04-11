using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaySetActiveInStart : MonoBehaviour
{
    [SerializeField] private GameObject _go;
    [SerializeField] private float _delay;

    private void Start()
    {
        Invoke(nameof(DelaySetActive), _delay);
    }

    private void DelaySetActive()
    {
        _go.SetActive(true);
    }
}
