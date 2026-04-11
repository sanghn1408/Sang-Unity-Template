using UnityEngine;
using UOP1.Factory;

[CreateAssetMenu(fileName = "TransformFactorySO", menuName = "Factory/Transform Factory")]
public class TransformFactorySO : FactorySO<Transform>
{
	public GameObject _prefab = default;

	public override Transform Create()
	{
		return Instantiate(_prefab).transform;
	}
}
