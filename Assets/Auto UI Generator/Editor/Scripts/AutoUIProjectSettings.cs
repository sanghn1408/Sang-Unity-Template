using UnityEngine;

namespace VFF
{
    [CreateAssetMenu(
        fileName = "AutoUIProjectSettings",
        menuName = "Auto UI Generator/Project Settings")]
    public class AutoUIProjectSettings : ScriptableObject
    {
        [Header("UI Toolkit")]
        public UIToolkitStylePreset uiToolkitStylePreset;
        public Vector2Int uiToolkitReferenceResolution = new Vector2Int(1080, 1920);

        [Header("UGUI Prefabs")]
        public GameObject textPrefab;
        public GameObject buttonPrefab;

        [Header("UGUI Text Offsets")]
        public float textFontSizeOffset = 0f;
        public Vector3 textPositionOffset = Vector3.zero;
    }
}