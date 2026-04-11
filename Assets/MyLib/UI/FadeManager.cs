using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private FadeChannelSO _fadeChannelSO;
    [SerializeField] private Image _imageComponent;

    private void OnEnable()
    {
        _fadeChannelSO.OnEventRaised += InitiateFade;
    }

    private void OnDisable()
    {
        _fadeChannelSO.OnEventRaised -= InitiateFade;
    }

    /// <summary>
    /// Controls the fade-in and fade-out.
    /// </summary>
    /// <param name="fadeIn">If false, the screen becomes black. If true, rectangle fades out and gameplay is visible.</param>
    /// <param name="duration">How long it takes to the image to fade in/out.</param>
    /// <param name="color">Target color for the image to reach. Disregarded when fading out.</param>
    private void InitiateFade(bool fadeIn, float duration, Color desiredColor)
    {
        DOTween.Kill(_imageComponent);
        if (fadeIn)
        {
            _imageComponent.color = Color.clear;
            desiredColor = Color.black;
            Debug.Log("FADE IN");
        }
        else
        {
            _imageComponent.color = Color.black;
            desiredColor = Color.clear;
            Debug.Log("FADE OUT");
        }
        _imageComponent.DOBlendableColor(desiredColor, duration);
    }
}
