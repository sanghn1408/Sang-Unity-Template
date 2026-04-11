using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHelper : MonoBehaviour
{
    [SerializeField] private Transform _rootIK=default;

    public Transform RootIK { get { return _rootIK; } }

    private Rigidbody[] _rigidbodys;
    private Collider[] _colliders;

    private Vector3[] _originPoses;

    private Animator _myAnim;

    private void Awake()
    {
        _myAnim = GetComponent<Animator>();

        _rigidbodys = GetComponentsInChildren<Rigidbody>(true);
        _colliders = GetComponentsInChildren<Collider>(true);
        _originPoses = new Vector3[_rigidbodys.Length];

        for (int i = 0; i < _rigidbodys.Length; i++)
        {
            _originPoses[i] = _rigidbodys[i].transform.localPosition;
        }

        ActiveRagdoll(false);
    }

    public void AddForce(Vector3 force)
    {
        for (int i = 0; i < _rigidbodys.Length; i++)
        {
            _rigidbodys[i].AddForce(force);
        }
    }

    public void ActiveRagdoll(bool b)
    {
        for (int i = 0; i < _rigidbodys.Length; i++)
        {
            _rigidbodys[i].isKinematic = !b;
            _rigidbodys[i].velocity = Vector3.zero;
        }

        for (int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i].enabled = b;
        }

        _myAnim.enabled = !b;

        if (b == false)
            ResetPos();
    }

    public void ResetPos()
    {
        for (int i = 0; i < _rigidbodys.Length; i++)
        {
            _rigidbodys[i].transform.localPosition = _originPoses[i];
        }
    }

}
