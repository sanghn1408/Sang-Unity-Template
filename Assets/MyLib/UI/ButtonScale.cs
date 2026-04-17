
//using Lofelt.NiceVibrations;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScale : Button
{
    ButtonScaleType scaleType = ButtonScaleType.ZoomIn;
    private bool hasStarted = false;
    bool isPressed = false;

    [SerializeField] private Vector3 scaleButton ;
    [SerializeField] private float zoomInScale ;
    public float _zoomInScale
    {
        get { return zoomInScale; }
        set { zoomInScale = value; }
    }

    float zoomSpeed = 0.5f;

    private void Start()
    {
        hasStarted = true;
    }
    private enum ButtonScaleType
    {
        ZoomIn,
        ZoomOut
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!interactable) return;
        if(DataManager.I.SaveData.IsSound)
        {

        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
        }    
        HapticManager.I.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
        //if(soundmanager.ismusicon)
        //SoundManager.Ins.PlaySFX(0);
        //if (DataManager.Ins.IsVibrateOn)
        //{
        //    HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        //    Debug.Log("rung rung");
        //}
        isPressed = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isPressed = false;
    }

    private void LateUpdate()
    {
        if (!hasStarted)
        {
            return; // Không làm gì nếu chưa chạy game
        }

        Vector3 targetScale = scaleButton * _zoomInScale;
        if (isPressed)
        {
            targetScale = scaleButton * _zoomInScale;
        }
        else
        {
            targetScale = scaleButton;
        }
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, zoomSpeed);
    }
}
