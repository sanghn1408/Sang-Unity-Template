using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlignIconByText : MonoBehaviour
{
    public TMP_Text _tmp;
    public Transform _icon;

    public float _offsetStart;
    public float _spaceOneText;


    public void SetText(string str)
    {
        _tmp.text = str;

        if (_icon != null)
        {
            Vector3 newPos = _icon.transform.localPosition;
            newPos.x = _offsetStart + _spaceOneText * str.Length;
            _icon.transform.localPosition = newPos;
        }
    }


    [ContextMenu("Test")]
    public void Test()
    {
        SetText(_tmp.text);
    }
}
