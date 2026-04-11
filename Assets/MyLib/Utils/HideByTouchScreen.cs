using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideByTouchScreen : MonoBehaviour
{
    //[SerializeField] private float _timeCheckAfterEnable = 1.5f;

    private float _timeActive;

    private void OnEnable()
    {
        _timeActive = Time.time;
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButton(0) && Time.time > _timeActive + _timeCheckAfterEnable)
    //        gameObject.SetActive(false);
    //}
}
