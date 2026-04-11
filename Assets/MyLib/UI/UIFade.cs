using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class UIFade : UIElementAnim
{
    [SerializeField] private Color _hideValue = default;
    [SerializeField] private Color _showValue = default;
    [SerializeField] private Ease _ease = Ease.Linear;
    private Image m_curImg;

    private void Awake()
    {
        m_curImg = GetComponent<Image>();
    }

    public override void Hide()
    {

    }

    public override void Show()
    {
        if (gameObject.activeSelf == false || gameObject.gameObject.activeInHierarchy == false) return;

        if (Tweener == null)
        {
            m_curImg = GetComponent<Image>();
            Tweener = m_curImg.DOColor(_showValue, Duration);
            Tweener.SetEase(_ease);
        }
        Tweener.ChangeValues(_hideValue, _showValue);
        Tweener.Play();
    }

    private void OnDisable()
    {
        Tweener.Kill();
    }
}
