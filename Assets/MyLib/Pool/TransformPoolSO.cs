using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UOP1.Factory;
using UOP1.Pool;

[CreateAssetMenu(fileName = "TransformPoolSO", menuName = "Pool/Transform Pool")]
public class TransformPoolSO : ComponentPoolSO<Transform>
{
    [SerializeField]
    private TransformFactorySO _factory;

    public override IFactory<Transform> Factory
    {
        get
        {
            return _factory;
        }
        set
        {
            _factory = value as TransformFactorySO;
        }
    }
}
