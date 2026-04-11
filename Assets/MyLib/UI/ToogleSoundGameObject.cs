using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToogleSoundGameObject : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO _boolEventChannelSO;
    [SerializeField] private TypeToogle _type = default;
    [SerializeField] private GameObject[] _objectOnOff = default;

    private bool _isOn;

    private void Start()
    {
        switch (_type)
        {
            case TypeToogle.ToogleHapic:
                _isOn = DataManager.I.SaveData.IsHapic;
                break;
            case TypeToogle.ToogleSound:
                _isOn = DataManager.I.SaveData.IsSound;
                break;
            case TypeToogle.TooggleMusic:
                _isOn = DataManager.I.SaveData.IsMusic;
                break;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        _objectOnOff[0].SetActive(_isOn);
        _objectOnOff[1].SetActive(!_isOn);
    }

    public void ClickMe()
    {
        _isOn = !_isOn;
        UpdateUI();

        switch (_type)
        {
            case TypeToogle.ToogleHapic:
                DataManager.I.SaveData.IsHapic = _isOn;
                break;
            case TypeToogle.ToogleSound:
                DataManager.I.SaveData.IsSound = _isOn;
                break;
            case TypeToogle.TooggleMusic:
                DataManager.I.SaveData.IsMusic = _isOn;
                if(DataManager.I.SaveData.IsMusic)
                {
                    //MusicPlayer.I.ResumeMusic ();
                Debug.LogError("Múicplat");
                }    
                break;
        }
        _boolEventChannelSO?.RaiseEvent(_isOn);

        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
#if MOREMOUNTAINS_NICEVIBRATIONS
        HapticManager.I.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
       
#endif
    }
}