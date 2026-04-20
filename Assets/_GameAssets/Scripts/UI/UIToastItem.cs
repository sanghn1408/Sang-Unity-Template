using UnityEngine;
using Lean.Pool;
using TMPro;
using DG.Tweening;

public class UIToastItem : MonoBehaviour, IPoolable
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageTxt;

    [Header("Animation Settings")]
    [SerializeField] private float secondDespawn = 3f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float showPosY = 50f;
    [SerializeField] private float moveYDuration = 0.75f;
    [SerializeField] RectTransform rectToMove;

    private Sequence seq;

    public void OnSpawn()
    {

    }

    public void OnDespawn()
    {

    }

    public void ShowMessage(string message)
    {
        messageTxt.text = message;

        PlayAnimation();
    }

    private void PlayAnimation()
    {
        canvasGroup.alpha = 0f;
        rectToMove.anchoredPosition = Vector2.zero;

        seq.Kill();
        seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1, fadeDuration))
           .Join(rectToMove.DOAnchorPosY(showPosY, moveYDuration))
           .AppendInterval(secondDespawn)
           .Join(canvasGroup.DOFade(0, fadeDuration))
           .OnComplete(() =>
           {
               LeanPool.Despawn(this);
           });
    }
}
