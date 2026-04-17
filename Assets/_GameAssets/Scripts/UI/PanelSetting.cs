using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PanelSetting : PanelBase
{
    void OnEnable()
    {
        //AdsManager.Ins.ShowMRecBanner(MaxSdkBase.AdViewPosition.BottomCenter);
        //AdsManager.Ins.HideBanner();
    }

    public void Close_OnClick()
    {
        DeActiveMe(null);
        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
    }

    private void OnDisable()
    {
        //AdsManager.Ins.HideMRecBanner();
        //AdsManager.Ins.ShowBanner();
    }
}
