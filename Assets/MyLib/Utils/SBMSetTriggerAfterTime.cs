using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SBMSetTriggerAfterTime : StateMachineBehaviour
{
    [SerializeField] private string _paramaterName;
    [SerializeField] private Vector2 _time;

    private Tween _tween;

    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        float time = Random.Range(_time.x, _time.y);
        int val = 0;
        _tween = DOTween.To(() => val, (x) => val = x, 0, time);
        _tween.OnComplete(() =>
        {
            if (animator)
                animator.SetTrigger(_paramaterName);
        });
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        if (_tween != null)
            _tween.Kill();
    }
}
