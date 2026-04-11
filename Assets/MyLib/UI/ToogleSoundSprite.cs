using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToogleSoundSprite : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO _boolEventChannelSO;
    [SerializeField] private TypeToogle _type = default;
    [SerializeField] private Image _img = default;
    [SerializeField] private Sprite[] _spriteOnOff = default;

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
        if (!_isOn)
        {
            _img.sprite = _spriteOnOff[1];
        }
        else
        {
            _img.sprite = _spriteOnOff[0];
        }
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
                break;
        }
        _boolEventChannelSO?.RaiseEvent(_isOn);

        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
#if MOREMOUNTAINS_NICEVIBRATIONS
        HapticManager.I.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
#endif
    }
}