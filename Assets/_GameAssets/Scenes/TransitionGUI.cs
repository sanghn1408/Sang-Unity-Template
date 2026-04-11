using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TransitionGUI : Singleton<TransitionGUI>
{
    [SerializeField] float timeShow;

    [SerializeField] float timeWait;

    [SerializeField] Image curtain;

    Sequence seq;

    private void Start()
    {
        curtain.gameObject.SetActive(false);
    }
    public void ShowTransition(UnityAction stepFinish = null, UnityAction finish = null)
    {
        curtain.gameObject.SetActive(true);
        var curtainColor = curtain.color;
        curtainColor.a = 0;
        curtain.color = curtainColor;
        seq?.Kill();
        seq = DOTween.Sequence();
        seq.Insert(0, curtain.DOFade(1, timeShow).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            stepFinish?.Invoke();
        }));
        seq.Insert(timeShow + timeWait, curtain.DOFade(0, timeShow).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            curtain.gameObject.SetActive(false);
        }));
        seq.OnComplete(() =>
        {
            finish?.Invoke();
        });
        seq.SetUpdate(true);
    }
}
