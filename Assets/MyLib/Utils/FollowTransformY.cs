using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransformY : MonoBehaviour
{
    [SerializeField] private Transform _target = default;

    private Vector3 _offset;
    private bool _isActive;

    private void OnEnable()
    {
        Invoke(nameof(OnEnableDelay), 0.1f);
    }

    private void OnEnableDelay()
    {
        _isActive = true;

        if (_target == null) return;
        _offset = _target.position - transform.position;
    }

    private void OnDisable()
    {
        _isActive = false;
    }

    private void LateUpdate()
    {
        if (!_isActive || _target == null) return;

        Vector3 wantedPos = transform.position;
        wantedPos.y = (_target.position - _offset).y;

        transform.position = wantedPos;
    }
}
