using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisibleRenderEvent : MonoBehaviour
{
    public Action OnVisible;
    public Action OnInvisible;

    private void OnBecameVisible()
    {
        OnVisible?.Invoke();
    }

    private void OnBecameInvisible()
    {
        OnInvisible?.Invoke();
    }
}
