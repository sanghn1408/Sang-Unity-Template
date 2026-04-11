using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TypeEffect
{
    Eff_Foot_Player,
    Eff_Collect_Banana,
    Eff_Collect_Chest,
    Eff_Collect_Final_Chest,
    Eff_Collect_Coin,
    Eff_Boss_Colision_Ground,
    Eff_Win,
}

public class EffectManager : Singleton<EffectManager>
{
    public List<EffectObject> _effects = new List<EffectObject>();
    private Dictionary<TypeEffect,List<GameObject>> _mapper = new Dictionary<TypeEffect, List<GameObject>>();

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void OnValidate()
    {
        for (int i = 0; i < _effects.Count; i++)
        {
            _effects[i].Name = _effects[i].Type.ToString();
        }
    }

    private void Init()
    {
        foreach (EffectObject item in _effects)
        {
            if (_mapper.ContainsKey(item.Type) == false)
            {
                _mapper.Add(item.Type, new List<GameObject>());
            }
            for (int i = 0; i < item.CountInit; i++)
            {
                GameObject effectObject = Instantiate(item.Obj, transform) as GameObject;
                _mapper[item.Type].Add(effectObject);
                effectObject.SetActive(false);
            }
        }
    }

    public GameObject PlayEffect(TypeEffect type, Vector3 pos, float time = 1.5f)
    {
        bool isPool = false;
        for (int j = 0; j < _mapper[type].Count; j++)
        {
            GameObject efobj2 = _mapper[type][j];
            if (efobj2.activeSelf == false)
            {
                efobj2.transform.position = pos;
                efobj2.gameObject.SetActive(true);
                isPool = true;
                if (time != 0)
                {
                    StartCoroutine(WatingHideEffect(efobj2, time));
                }
                return efobj2.gameObject;
            }
        }
        if (isPool == false)
        {
            Debug.LogWarning("PLEASE ADD MORE POOL: " + type.ToString());
            GameObject effectObject = Instantiate(_mapper[type][0], transform) as GameObject;
            effectObject.transform.position = pos;
            if (time != 0)
            {
                StartCoroutine(WatingHideEffect(effectObject, time));
            }
            _mapper[type].Add(effectObject);
            return effectObject.gameObject;
        }
        Debug.LogError("Not find this effect");
        return null;
    }

    public void HideEffectOne(TypeEffect type)
    {
        for (int i = 0; i < _mapper[type].Count; i++)
        {
            if (_mapper[type][i].activeSelf)
                _mapper[type][i].SetActive(false);
        }
    }

    public void HideEffectAll()
    {
        for (int i = 0; i < _effects.Count; i++)
        {
            for (int j = 0; j < _mapper[_effects[i].Type].Count; j++)
            {
                _mapper[_effects[i].Type][j].SetActive(false);
            }
        }
    }

    IEnumerator WatingHideEffect(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}


[System.Serializable]
public class EffectObject
{
    [HideInInspector]
    public string Name;
    public TypeEffect Type;
    public GameObject Obj;
    public int CountInit;
}
