using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UISettingBoard : MonoBehaviour
{
    private RectMask2D _rectMask2D;
    private float _height;

    private bool _isShow = false;


    private void Awake()
    {
        _height = GetComponent<RectTransform>().sizeDelta.y;
        _rectMask2D = GetComponent<RectMask2D>();
    }

    public void Setting_OnClick()
    {
        float startVal = _height;
        float endVal = 0f;
        if (_isShow)
        {
            startVal = 0f;
            endVal = _height;
        }

        Vector4 curPadding = _rectMask2D.padding;
        Tween t = DOTween.To(() => startVal, x => startVal = x, endVal, 0.1f);
        t.OnUpdate(() =>
        {
            curPadding.y = startVal;
            _rectMask2D.padding = curPadding;
        });

        _isShow = !_isShow;

        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
    }
}
