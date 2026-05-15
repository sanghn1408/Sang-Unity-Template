using UnityEngine;
using UnityEngine.TextCore.Text;

namespace VFF
{
    [CreateAssetMenu(
        fileName = "UIToolkitStylePreset",
        menuName = "Auto UI Generator/UI Toolkit Style Preset")]
    public class UIToolkitStylePreset : ScriptableObject
    {
        [Header("TEXT BASE")]
        public Color textColor = Color.white;
        public FontAsset fontAsset;
        public Font fallbackFont;
        public FontStyle fontStyle = FontStyle.Normal;
        public float letterSpacing = 0f;
        public float lineHeight = 0f;
        public float estimatedCharWidthRatio = 0.6f;
        public TextAnchor textAlignment = TextAnchor.MiddleCenter;

        [Header("TEXT OUTLINE")]
        public bool useOutline = false;
        public Color outlineColor = Color.black;
        public float outlineWidth = 1f;

        [Header("BUTTON BASE")]
        public float borderRadius = 0f;

        [Header("BUTTON HOVER")]
        public Color buttonHoverColor = new Color(0.88f, 0.88f, 0.88f);
        public float hoverScale = 1.03f;

        [Header("BUTTON ACTIVE")]
        public Color buttonActiveColor = new Color(0.5f, 0.5f, 0.5f);
        public float activeScale = 0.97f;

        [Header("ANIMATION")]
        public float transitionDuration = 0.15f;
    }
}