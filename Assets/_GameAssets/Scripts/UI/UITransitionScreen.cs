using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
//using Spine.Unity;
using UnityEngine;

public class UITransitionScreen : Singleton<UITransitionScreen>
{
    [Header("Content")]
    [SerializeField] private RectTransform mask;

    [Header("Target size")]
    [SerializeField] private Vector2 targetOutSizeDelta = new(4000, 4000);
    [SerializeField] private Vector2 targetInSizeDelta = Vector2.zero;

    [Header("Old Transition")]
    [SerializeField] private GameObject oldTransObj;

    //[Header("Anim")]
    //[SerializeField] private SkeletonGraphic sg;

    //private int count = 0;
    private readonly float transitionTime = 0.45f;

    // public override void Show()
    // {
    //     base.Show();
    // }

    // public override void Hide()
    // {
    //     // if (count == 0)
    //     // {
    //     //     count++;
    //     //     await UniTask.WaitForSeconds(1f);
    //     //     oldTransObj.SetActive(false);
    //     //     base.Hide();
    //     //     return;
    //     // }
    //     // else
    //     // {
    //     //     TransitionOut();
    //     // }
    //     TransitionOut();
    // }

    public void ShowTransition(Action stepFinish = null, Action finish = null)
    {
        _ = PlayTransitionFlow(stepFinish, finish);
    }

    private async UniTask PlayTransitionFlow(Action stepFinish, Action finish)
    {
        if (mask == null)
        {
            stepFinish?.Invoke();
            finish?.Invoke();
            return;
        }

        await TransitionIn();
        stepFinish?.Invoke();
        TransitionOut(() => finish?.Invoke());
    }

    public async UniTask TransitionIn()
    {

        // if (count == 0)
        // {
        //     //sg.gameObject.SetActive(false);
        //     oldTransObj.SetActive(true);
        //     return;
        // }

        mask.DOKill();
        mask.sizeDelta = targetOutSizeDelta;

        mask.DOSizeDelta(targetInSizeDelta, transitionTime)
                    .SetUpdate(true)
                    .SetEase(Ease.Linear);

        await UniTask.WaitForSeconds(transitionTime);

        // Anim
        //sg.transform.localScale = Vector3.zero;
        //sg.gameObject.SetActive(true);
        //sg.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        //wait UniTask.WaitForSeconds(0.5f);

        await UniTask.CompletedTask;
    }

    public async void TransitionOut(Action onComplete = null)
    {
        mask.DOKill();
        mask.sizeDelta = targetInSizeDelta;

        //sg.transform.DOScale(0f, 0.25f).SetEase(Ease.InSine)
        //    .OnComplete(() =>
        //    {
        //        sg.gameObject.SetActive(false);
        //    });

        await UniTask.WaitForSeconds(transitionTime);
        mask.DOSizeDelta(targetOutSizeDelta, transitionTime)
            .SetUpdate(true)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }
}
