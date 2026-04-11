using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelNoInternet : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private Button btnOk;

    private bool isShow;
    private bool isShowMREC;

    void Awake()
    {
        DontDestroyOnLoad(this);
        btnOk.onClick.AddListener(OnBtnOkClicked);
    }

    public void OnShow()
    {
        _content.SetActive(true);
        isShow = true;
        Time.timeScale = 0f;
        //isShowMREC = AdsManager.Instance.IsMrecShow;
        //if (isShowMREC)
        //    AdsManager.Instance.HideMRec();
    }

    public void OnHide()
    {
        isShow = false;
        _content.SetActive(false);
        //AdsManager.Instance.TryToReInit();
        
        DOVirtual.DelayedCall(0.2f, () =>
        {
            Time.timeScale = 1f;
        });
        //if (isShowMREC)
        //    AdsManager.Instance.ShowMRec();
    }

    void Update()
    {
        if (isShow)
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
                Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                OnHide();
            }
            Time.timeScale = 0f;
        }
        else
        {
            //Time.timeScale = 1f;
            //if (Application.internetReachability == NetworkReachability.NotReachable && RemoteConfigControl.Instance.offline_play_on_off)
            //    OnShow();
        }
    }

    private void OnBtnOkClicked()
    {
        try
        {
#if UNITY_ANDROID
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.WIFI_SETTINGS"))
                {
                    currentActivityObject.Call("startActivity", intentObject);
                }
            }
#endif
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}