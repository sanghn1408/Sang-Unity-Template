using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveRandomInStart : MonoBehaviour
{
    [SerializeField] private GameObject[] _gameObjects;

    private void Start()
    {
        _gameObjects[Random.Range(0, _gameObjects.Length)].SetActive(true);
    }
}
