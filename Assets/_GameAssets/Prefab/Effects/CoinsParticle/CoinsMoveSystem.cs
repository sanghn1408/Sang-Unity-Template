using System;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class CoinsMoveSystem : MonoBehaviour
{
    [SerializeField] Transform coinPref;
    [SerializeField] [Range(0f, 100f)] float radius;
    public void PlayCoinsEfx(float3 startPos, float3 endPos, int amount = 6, Action OnComplete = null)
    {
        var pos = startPos;
        var atPosition = 0.05f;
        var scaleDuration = 0.3f;
        var moveDuration = 0.4f;
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < amount; i++)
        {
            var coin = Instantiate(coinPref, transform);
            coin.gameObject.SetActive(true);
            coin.localScale = Vector3.zero;
            pos.x += UnityEngine.Random.Range(-radius, radius);
            pos.y += UnityEngine.Random.Range(-radius, radius);
            coin.position = pos;

            seq.Insert(
                atPosition * i,
                coin.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack)
            );
            seq.Insert(
                atPosition * i + scaleDuration,
                coin.DOMove(endPos, moveDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    coin.gameObject.SetActive(false);
                    //SoundManager.Instance.PlayCollectCoinSfx();
                })
            );
        }
        seq.OnComplete(() =>
        {
            OnComplete?.Invoke();
        });
    }
}
