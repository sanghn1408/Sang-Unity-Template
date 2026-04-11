using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIManager : Singleton<UIManager>, IUIManager
{
    private PanelBase[] _panelBases;
    private Dictionary<string,PanelBase> _mapper;

    protected override void Awake()
    {
        base.Awake();
        Init();
        
    }

    private void Init()
    {
        _panelBases = GetComponentsInChildren<PanelBase>(true);
        _mapper = new Dictionary<string, PanelBase>();
        
        for (int i = 0; i < _panelBases.Length; i++)
        {
            if (_mapper.ContainsKey(_panelBases[i].GetType().ToString()) == false)
            {
                _mapper.Add(_panelBases[i].GetType().ToString(), _panelBases[i]);
            }
            else Debug.LogWarning(_panelBases[i].GetType().ToString() + "has been register !");
        }
    }
    public void Hide<T>(System.Action callBack = null) where T : PanelBase
    {
        if (_mapper.ContainsKey(typeof(T).ToString()))
            _mapper[typeof(T).ToString()].DeActiveMe(callBack);
        else Debug.LogWarning(typeof(T).ToString() + "not register");
    }

    public void Show<T>(System.Action callBack = null) where T : PanelBase
    {
        if (_mapper.ContainsKey(typeof(T).ToString()))
            _mapper[typeof(T).ToString()].ActiveMe(callBack);
        else Debug.LogWarning(typeof(T).ToString() + " not register");
    }


    public T Get<T>() where T : PanelBase
    {
        return (T)_mapper[typeof(T).ToString()];
    }
}
