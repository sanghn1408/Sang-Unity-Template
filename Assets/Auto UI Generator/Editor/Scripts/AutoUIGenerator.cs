using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VFF
{
    public enum NineSliceRegion
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public static class AutoUIGenerator
    {
        private static Canvas Canvas;
        const string PROJECT_SETTINGS_PATH = "Assets/Auto UI Generator/Config/AutoUIProjectSettings.asset";
        static List<Texture2D> templates;
        static AutoUIProjectSettings cachedProjectSettings;
        const float STRETCH_THRESHOLD = 0.99f;

        public static void Generate(
            RectTransform targetRoot,
            DetectResult data,
            List<Texture2D> manualTemplates,
            string rootName,
            Texture2D mainScreenshot,
            float fakeScreenAlpha
            )
        {
            if (targetRoot == null || data?.elements == null)
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Generate Auto UI");

            try
            {

                templates = manualTemplates;
                float textHeightOffset = 1;

                // 🔍 Auto find Canvas
                Canvas = null;
                Transform t = targetRoot;
                while (t != null)
                {
                    Canvas c = t.GetComponent<Canvas>();
                    if (c != null)
                    {
                        Canvas = c;
                        break;
                    }
                    t = t.parent;
                }

                if (Canvas == null)
                {
                    Debug.LogError("No Canvas found!");
                    return;
                }

                GameObject root = new GameObject(rootName);
                RectTransform rootRT = root.AddComponent<RectTransform>();
                root.transform.SetParent(targetRoot, false);

                rootRT.anchorMin = Vector2.zero;
                rootRT.anchorMax = Vector2.one;
                rootRT.offsetMin = Vector2.zero;
                rootRT.offsetMax = Vector2.zero;
                rootRT.position = Canvas.GetComponent<RectTransform>().position;

                Undo.RegisterCreatedObjectUndo(root, "Create Auto UI Root");


                // 🔥 KEY FIX: mỗi element có key duy nhất theo index
                var sorted = data.elements.OrderBy(e => e.z).ToList();

                Dictionary<int, RectTransform> created = new Dictionary<int, RectTransform>();
                Dictionary<int, DetectElement> elementMap = new Dictionary<int, DetectElement>();

                // -------- PASS 1: CREATE ALL ----------
                for (int i = 0; i < sorted.Count; i++)
                {
                    DetectElement e = sorted[i];
                    elementMap[i] = e;

                    RectTransform rt =
                        e.name == "__text__"
                                ? CreateTextElement(e, rootRT, textHeightOffset)
                                : CreateImageElement(e, rootRT);

                    if (rt != null)
                        created[i] = rt;
                }

                // -------- PASS 2: APPLY PARENT ----------
                for (int i = 0; i < sorted.Count; i++)
                {
                    DetectElement e = elementMap[i];

                    RectTransform parentRT = rootRT;

                    if (!string.IsNullOrEmpty(e.parent))
                    {
                        // tìm instance gần nhất có cùng name
                        float bestDist = float.MaxValue;
                        int bestIndex = -1;

                        foreach (var kv in elementMap)
                        {
                            if (kv.Value.name != e.parent)
                                continue;

                            float dist = Vector2.Distance(
                                new Vector2(e.x, e.y),
                                new Vector2(kv.Value.x, kv.Value.y)
                            );

                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestIndex = kv.Key;
                            }
                        }

                        if (bestIndex >= 0 && created.ContainsKey(bestIndex))
                            parentRT = created[bestIndex];
                    }

                    if (created.ContainsKey(i))
                        created[i].SetParent(parentRT, true);
                }
                CreateMainScreenshotBackground(rootRT, mainScreenshot, fakeScreenAlpha);
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroup);
            }
        }

        static void CreateMainScreenshotBackground(RectTransform root, Texture2D screenshot, float alpha)
        {
            if (root == null || screenshot == null)
                return;

            alpha = Mathf.Clamp01(alpha);

            GameObject bgGO = new GameObject("MainScreenShot");

            RectTransform bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.SetParent(root, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.raycastTarget = false;
            bgImage.preserveAspect = true;

            Sprite sprite = null;
            string texPath = AssetDatabase.GetAssetPath(screenshot);

            if (!string.IsNullOrEmpty(texPath))
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

            if (sprite == null)
            {
                sprite = Sprite.Create(
                    screenshot,
                    new Rect(0, 0, screenshot.width, screenshot.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
            }

            bgImage.sprite = sprite;
            bgImage.color = new Color(1, 1, 1, alpha);
            bgImage.SetNativeSize();
        }

        static RectTransform CreateTextElement(
            DetectElement e,
            RectTransform parent,
            float textSizeOffset)
        {
            GameObject prefab = GetTextPrefab();

            if (prefab == null)
            {
                Debug.LogError("Text prefab not found!");
                return null;
            }

            GameObject go = InstantiateAndUnpack(prefab, parent);
            go.name = $"TMP_{e.text}";

            RectTransform rt = go.GetComponent<RectTransform>();
            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();

            rt.sizeDelta = new Vector2(e.width, e.height) * textSizeOffset;

            if (tmp != null)
            {
                tmp.text = string.IsNullOrEmpty(e.text) ? "TEXT" : e.text;
                ConfigureTMPTextFromDetection(tmp, e);
                tmp.color = Color.white;
            }

            ApplyLayout(rt, e, Canvas);
            ApplyTextOffsetsFromSettings(rt, tmp);
            return rt;
        }

        static void ApplyTextOffsetsFromSettings(RectTransform rt, TextMeshProUGUI tmp)
        {
            if (rt == null)
                return;

            AutoUIProjectSettings settings = GetProjectSettings();

            if (settings == null)
                return;

            if (tmp != null)
            {
                tmp.fontSize = Mathf.Max(1f, tmp.fontSize + settings.textFontSizeOffset);
            }

            Vector3 offset = settings.textPositionOffset;
            rt.anchoredPosition += new Vector2(offset.x, offset.y);

            Vector3 localPosition = rt.localPosition;
            localPosition.z += offset.z;
            rt.localPosition = localPosition;
        }

        static void ConfigureTMPTextFromDetection(TextMeshProUGUI tmp, DetectElement element)
        {
            if (tmp == null || element == null)
                return;

            RectTransform textRT = tmp.rectTransform;
            if (textRT != null)
            {
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.pivot = new Vector2(0.5f, 0.5f);
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;
                textRT.anchoredPosition = Vector2.zero;
            }

            string content = string.IsNullOrEmpty(element.text) ? "TEXT" : element.text;
            int lineCount = Mathf.Max(1, content.Split('\n').Length);
            bool multiLine = lineCount > 1;

            float estimatedLineHeight = Mathf.Max(10f, element.height / lineCount);
            float targetFontSize = Mathf.Max(8f, estimatedLineHeight * 0.75f);

            tmp.enableAutoSizing = false;
            tmp.fontSize = targetFontSize;
            tmp.margin = Vector4.zero;

            tmp.alignment = multiLine
                ? TextAlignmentOptions.TopLeft
                : TextAlignmentOptions.MidlineLeft;

            tmp.enableWordWrapping = multiLine;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.characterSpacing = 0f;
            tmp.wordSpacing = 0f;
            tmp.lineSpacing = 0f;
            tmp.richText = false;
            tmp.ForceMeshUpdate();
        }

        static RectTransform CreateImageElement(
            DetectElement e,
            RectTransform parent)
        {
            string imageName = Path.GetFileNameWithoutExtension(e.name);

            GameObject go = new GameObject(imageName);

            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();

            Sprite sprite = FindSpriteInTemplates(imageName);
            go.name = sprite != null ? sprite.name : imageName;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Simple;
            }
            else
            {
                img.color = new Color(1, 0, 1, 0.5f);
                rt.sizeDelta = new Vector2(100, 100);
                Debug.LogWarning($"Sprite not found in Templates: {imageName}");
            }

            ApplyLayout(rt, e, Canvas);
            return rt;
        }

        public static NineSliceRegion GetNineSliceRegion(
            RectTransform target,
            Canvas canvas)
        {
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT,
                RectTransformUtility.WorldToScreenPoint(
                    canvas.worldCamera,
                    target.position),
                canvas.worldCamera,
                out localPos);

            Rect rect = canvasRT.rect;
            float width = rect.width;
            float height = rect.height;

            float x = localPos.x;
            float y = localPos.y;

            float leftX = -width / 6f;
            float rightX = width / 6f;

            int col;
            if (x < leftX)
                col = 0;
            else if (x > rightX)
                col = 2;
            else
                col = 1;

            float quarterH = height / 4f;
            float topBoundary = quarterH;
            float bottomBoundary = -quarterH;

            int row;
            if (y > topBoundary)
                row = 0;
            else if (y < bottomBoundary)
                row = 2;
            else
                row = 1;

            return (NineSliceRegion)(row * 3 + col);
        }

        public static Vector2 GetAnchorFromRegion(NineSliceRegion region)
        {
            switch (region)
            {
                case NineSliceRegion.TopLeft:
                    return new Vector2(0f, 1f);

                case NineSliceRegion.TopCenter:
                    return new Vector2(0.5f, 1f);

                case NineSliceRegion.TopRight:
                    return new Vector2(1f, 1f);

                case NineSliceRegion.MiddleLeft:
                    return new Vector2(0f, 0.5f);

                case NineSliceRegion.MiddleCenter:
                    return new Vector2(0.5f, 0.5f);

                case NineSliceRegion.MiddleRight:
                    return new Vector2(1f, 0.5f);

                case NineSliceRegion.BottomLeft:
                    return new Vector2(0f, 0f);

                case NineSliceRegion.BottomCenter:
                    return new Vector2(0.5f, 0f);

                case NineSliceRegion.BottomRight:
                    return new Vector2(1f, 0f);

                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        public static void ApplyAnchorFromRegion(
            RectTransform rt,
            NineSliceRegion region)
        {
            Vector3 worldPos = rt.position;

            Vector2 anchor = GetAnchorFromRegion(region);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.position = worldPos;
        }

        static GameObject LoadPrefabFromFolder(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        static AutoUIProjectSettings GetProjectSettings()
        {
            if (cachedProjectSettings == null)
                cachedProjectSettings = AssetDatabase.LoadAssetAtPath<AutoUIProjectSettings>(PROJECT_SETTINGS_PATH);

            return cachedProjectSettings;
        }

        static GameObject GetTextPrefab()
        {
            AutoUIProjectSettings settings = GetProjectSettings();
            return settings.textPrefab;
        }

        static GameObject InstantiateAndUnpack(
            GameObject prefab,
            Transform parent)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            PrefabUtility.UnpackPrefabInstance(
                go,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);

            return go;
        }

        // ---------- LAYOUT ----------
        static void ApplyLayout(RectTransform rt, DetectElement e, Canvas canvas)
        {
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            float canvasW = canvasRT.rect.width;
            float canvasH = canvasRT.rect.height;

            float widthRatio = e.width / canvasW;
            float heightRatio = e.height / canvasH;

            if (widthRatio >= STRETCH_THRESHOLD && heightRatio >= STRETCH_THRESHOLD)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                rt.anchoredPosition = Vector2.zero;

                Debug.Log($"[AutoUI] Detected full-screen element: {rt.name}. Applying Stretch-Stretch.");
            }
            else
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.anchoredPosition = new Vector2(e.x, e.y);
                rt.sizeDelta = new Vector2(e.width, e.height);

                NineSliceRegion region = GetNineSliceRegion(rt, canvas);
                ApplyAnchorFromRegion(rt, region);
            }
        }

        static Sprite FindSpriteInTemplates(string spriteName)
        {
            if (templates == null || templates.Count == 0)
                return null;

            if (!int.TryParse(spriteName, out int index))
                return null;

            if (index < 0 || index >= templates.Count)
                return null;

            Texture2D sourceTexture = templates[index];
            if (sourceTexture == null)
                return null;

            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrEmpty(sourcePath))
                return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(sourcePath);
        }
    }
}
