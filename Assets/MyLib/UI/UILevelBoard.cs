using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UILevelBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmpLevel;

    private void Start()
    {
        _tmpLevel.text = "DAY " + (GamePlayManager.I.LEVEL + 1);
    }
}
