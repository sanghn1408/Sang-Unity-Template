using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRandomForceAtStart : MonoBehaviour
{
    [SerializeField] private Vector3 _dirRandom1;
    [SerializeField] private Vector3 _dirRandom2;

    [SerializeField] private float _forceValue;

    private Rigidbody _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Vector3 dir = Vector3.zero;
        dir.x = Random.Range(_dirRandom1.x, _dirRandom2.x);
        dir.y = Random.Range(_dirRandom1.y, _dirRandom2.y);
        dir.z = Random.Range(_dirRandom1.z, _dirRandom2.z);

        dir.Normalize();

        _rigidbody.AddForce(dir * _forceValue);
    }
}
