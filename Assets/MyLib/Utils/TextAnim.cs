using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextAnim : MonoBehaviour
{
    [SerializeField] private string[] _contents;
    [SerializeField] private float _timeInterval;

    private TextMeshProUGUI _tmp;
    private int _id=0;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        StartCoroutine(IE_Loop());
    }

    private IEnumerator IE_Loop()
    {
        while (true)
        {
            _tmp.text = _contents[_id];
            _id++;
            if (_id == _contents.Length) _id = 0;
            yield return new WaitForSeconds(_timeInterval);
        }
    }
}
