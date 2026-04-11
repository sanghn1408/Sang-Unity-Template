using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class UIMove : UIElementAnim
{
    [SerializeField] private Vector3 _hidePos = default;
    [SerializeField] private Vector3 _showPos = default;
    [SerializeField] private Ease _ease = Ease.Linear;

    private RectTransform m_curTrf;

    public Vector3 _HidePos { get { return _hidePos; } }
    public Vector3 _ShowPos { get { return _showPos; } }

    protected void Awake()
    {
        m_curTrf = GetComponent<RectTransform>();
    }

    public override void Hide()
    {
        if (gameObject.activeSelf == false || gameObject.gameObject.activeInHierarchy == false) return;

        Tweener = m_curTrf.DOAnchorPos3D(_HidePos, Duration);
        Tweener.SetEase(_ease);

        Tweener.ChangeValues(_ShowPos, _HidePos);
        Tweener.Play();
    }

    public override void Show()
    {
        if (gameObject.activeSelf == false || gameObject.gameObject.activeInHierarchy == false) return;

        Tweener = m_curTrf.DOAnchorPos3D(_ShowPos, Duration);
        Tweener.SetEase(_ease);

        Tweener.ChangeValues(_HidePos, _ShowPos);
        Tweener.Play();
    }

    private void OnDisable()
    {
        Tweener.Kill();
    }

}
