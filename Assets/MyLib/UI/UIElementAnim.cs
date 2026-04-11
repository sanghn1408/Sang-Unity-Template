using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
public abstract class UIElementAnim : MonoBehaviour
{
    [SerializeField] private float m_duration = 0.2f;
    private Tweener m_tweener;

    public float Duration { get { return m_duration; } set { m_duration = value; } }
    public Tweener Tweener { get { return m_tweener; } set { m_tweener = value; } }
    public abstract void Show();
    public abstract void Hide();

    protected virtual void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}
