using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class UIMoneyBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmp;
    [SerializeField] private Transform _coinObject;

    [Header("Listening on")]
    [SerializeField] private VoidEventChannelSO _onChangeCoin;

    private int _oldCoin;

    private void OnEnable()
    {
        _onChangeCoin.OnEventRaised += OnUpdateCoin;
    }

    private void OnDisable()
    {
        _onChangeCoin.OnEventRaised -= OnUpdateCoin;
    }

    private void Start()
    {
        UpdateText(false);
    }

    private void OnUpdateCoin()
    {
        UpdateText(true);
    }

    private void UpdateText(bool isAnim)
    {
        if (isAnim)
        {
            Tweener t1 = _coinObject.DOScale(Vector3.one * 1.15f, 0.15f);
            t1.SetLoops(2, LoopType.Yoyo);

            int startVal = _oldCoin;
            Tweener t = DOTween.To(() => startVal, x => startVal = x, (int)GamePlayManager.I.COIN, 1);
            t.OnUpdate(() =>
            {
                _tmp.text = startVal.ToString();
            });
        }
        else _tmp.text = GamePlayManager.I.COIN.ToString();
        _oldCoin = (int)GamePlayManager.I.COIN;
    }

    [Button(ButtonSizes.Large)]
    public void CheatCoin()
    {
        GamePlayManager.I.COIN += 10000;
    }
}
