using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.DemiLib;


public class PanelRating : MonoBehaviour
{
    [SerializeField] private bool _isForceRateWhenSelect5Star = true;
    [Header("Common")]
    [SerializeField] private GameObject _step1;
    [SerializeField] private GameObject _step2;

    [Header("Step 1")]
    [SerializeField] private Button[] _btnArray;
    [SerializeField] private Sprite _goldStar;
    [SerializeField] private Sprite _silverStar;
    [SerializeField] private Button _btnRate;
    [SerializeField] private Button _btnLater;
    [SerializeField] private Button _btnNever;

    [Header("Step 2")]
    [SerializeField] public Button _btnNoThanks;
    [SerializeField] public Button _btnGiveFeedback;

    private int _rateCount;

    private void Awake()
    {

        for (int i = 0; i < _btnArray.Length; i++)
        {
            int starIndex = i;
            var btn = _btnArray[starIndex];

            btn.onClick.AddListener(() =>
            {
                OnChooseStar(starIndex);
            });
        }
        _btnRate.onClick.AddListener(() =>
        {
            RateForUs(_rateCount);
            ES3.Save("IS_SHOW_RATE", true);
        });

        _btnLater.onClick.AddListener(() =>
        {
            DataManager.I.IntRate = -2;
            Hide();
        });

        _btnNever.onClick.AddListener(() =>
        {
            ES3.Save("IS_SHOW_RATE", true);
            Hide();
        });

        _btnNoThanks.onClick.AddListener(() =>
        {
            ES3.Save("IS_SHOW_RATE", true);
            Hide();
        });

        _btnGiveFeedback.onClick.AddListener(() =>
        {
            ES3.Save("IS_SHOW_RATE", true);
            Hide();
        });
    }

    public bool ShowIfSatisfiedCondition(bool isShowImmediate = true)
    {
        ES3.Save("IS_CALL_RATING", true);
        if (ES3.Load("IS_SHOW_RATE", false) == false)
        {
            if (isShowImmediate)
                Show();
            Debug.Log("showrate");

            return true;
        }
        return false;
    }

    public void Show()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
        OnShow();
    }

    public void Hide()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
       
    }

    private void OnShow()
    {
        for (int i = 0; i < _btnArray.Length; i++)
        {
            _btnArray[i].image.sprite = _silverStar;
        }
        _btnRate.interactable = false;

        _step1.SetActive(true);
        _step2.SetActive(false);
    }

    private void OnChooseStar(int star)
    {
        _rateCount = star;
        StartCoroutine(I_Choose());
        Debug.Log("OnChooseStar");
    }

    private IEnumerator I_Choose()
    {

        for (int i = 0; i < _btnArray.Length; i++)
        {
            _btnArray[i].image.sprite = _silverStar;
        }
        for (int i = 0; i < _rateCount + 1; i++)
        {
            _btnArray[i].image.sprite = _goldStar;
            yield return new WaitForSecondsRealtime(0.1f);
            //yield return new WaitForSeconds(0.1f);
        }
        _btnRate.interactable = true;

        //force rate when 5 star
        if (_isForceRateWhenSelect5Star && _rateCount == 4)
        {
            RateForUs(_rateCount);
            ES3.Save("IS_SHOW_RATE", true);
        }
    }

    public void RateForUs(int rateCount)
    {
        _rateCount = rateCount;
        StartCoroutine(I_Rate(rateCount));
    }

    private IEnumerator I_Rate(int rateCount)
    {
        float delay = rateCount * 0.1f + 0.5f;
        if (rateCount >= 4)
        {
#if UNITY_ANDROID
            //InAppReview.Instance.StartRequestReview(); chưa có thư viện gg
#elif UNITY_IPHONE
            UnityEngine.iOS.Device.RequestStoreReview();
#endif
            yield return new WaitForSecondsRealtime(delay);
            //yield return new WaitForSeconds(delay);
            Hide();
        }
        else
        {
            _step1.SetActive(false);
            _step2.SetActive(true);
        }
    }
}
