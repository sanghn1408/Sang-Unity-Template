using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
    [SerializeField]
    private VoidEventChannelSO _startNewGameEvent = default;

    

    public void StartGame_OnClick()
    {
        _startNewGameEvent.RaiseEvent();
    }
}
