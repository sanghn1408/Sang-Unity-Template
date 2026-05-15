#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VFF
{
    public static class AutoUIToolkitGenerator
    {
        enum LayoutType
        {
            None,
            Vertical,
            Horizontal,
            Grid
        }
       
        const string OUTPUT_FOLDER = "Assets/Auto UI Generator/Generated UI Toolkit/";
        const string PANEL_SETTINGS_PATH = OUTPUT_FOLDER + "PanelSettings.asset";
        const float STRETCH_THRESHOLD = 0.99f;
        const float CENTER_THRESHOLD_RATIO = 0.05f; // 5% màn hình
        static List<Texture2D> templates;
        static UIToolkitStylePreset stylePreset;

        // ========================= ENTRY =========================
        public static void Generate(
            DetectResult data,
            List<Texture2D> manualTemplates,
            string rootName,
            Vector2Int referenceResolution)
        {
            if (data == null || data.elements == null)
                return;
            templates = manualTemplates ?? new List<Texture2D>();

            if (!Directory.Exists(OUTPUT_FOLDER))
                Directory.CreateDirectory(OUTPUT_FOLDER);
            stylePreset = AssetDatabase.LoadAssetAtPath<UIToolkitStylePreset>(
                "Assets/Auto UI Generator/UI Toolkit/StylePresets/DefaultPreset.asset");
            var nodes = BuildNodes(data.elements);

            string uxmlPath = OUTPUT_FOLDER + rootName + ".uxml";
            string ussPath = OUTPUT_FOLDER + rootName + ".uss";

            GenerateUSS(nodes, ussPath, referenceResolution);
            GenerateUXML(nodes, rootName, uxmlPath, ussPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(uxmlPath);
            AssetDatabase.ImportAsset(ussPath);

            var panelSettings = GetOrCreatePanelSettings(referenceResolution);
            CreateUIDocumentInScene(uxmlPath, rootName, panelSettings);
        }

        // ========================= NODE =========================
        class Node
        {
            public DetectElement data;
            public string className;
            public string spritePath;

            public int index;
            public int parentIndex = -1;
            public List<Node> children = new();

            // ===== Scroll Logic =====
            public bool isScroll;
            public LayoutType layoutType;
            public int gridColumns;

            public float globalLeft;
            public float globalTop;
        }

        static List<Node> BuildNodes(DetectElement[] elements)
        {
            var list = new List<Node>();

            if (elements == null || elements.Length == 0)
                return list;

            var sorted = elements
                .OrderBy(e => e.z)
                .ToList();

            // PASS 1: CREATE NODE
            for (int i = 0; i < sorted.Count; i++)
            {
                var e = sorted[i];

                list.Add(new Node
                {
                    data = e,
                    className = "el_" + i,
                    spritePath = ResolveSprite(e.name),
                    index = i
                });
            }

            // PASS 2: RESOLVE PARENT
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i].data;

                if (string.IsNullOrEmpty(e.parent))
                    continue;

                float bestDist = float.MaxValue;
                int bestIndex = -1;

                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;

                    if (list[j].data.name != e.parent)
                        continue;

                    float dist = Vector2.Distance(
                        new Vector2(e.x, e.y),
                        new Vector2(list[j].data.x, list[j].data.y)
                    );

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestIndex = j;
                    }
                }

                if (bestIndex >= 0)
                {
                    list[i].parentIndex = bestIndex;
                    list[bestIndex].children.Add(list[i]);
                }
            }

            return list;
        }

        // ========================= UXML =========================
        static void GenerateUXML(
     List<Node> nodes,
     string rootName,
     string uxmlPath,
     string ussPath)
        {
            StringBuilder sb = new();

            sb.AppendLine("<ui:UXML xmlns:ui=\"UnityEngine.UIElements\">");
            sb.AppendLine($"    <Style src=\"project://database/{ussPath}\" />");
            sb.AppendLine("    <ui:VisualElement class=\"root\">");
            sb.AppendLine("        <ui:VisualElement class=\"canvas\">");

            // Chỉ render node không có parent
            foreach (var n in nodes.Where(n => n.parentIndex == -1))
            {
                WriteNodeRecursive(sb, n, 3);
            }

            sb.AppendLine("        </ui:VisualElement>");
            sb.AppendLine("    </ui:VisualElement>");
            sb.AppendLine("</ui:UXML>");

            File.WriteAllText(uxmlPath, sb.ToString());
        }

        static void WriteNodeRecursive(
       StringBuilder sb,
       Node node,
       int indent)
        {
            string space = new string(' ', indent * 4);

            if (node.data.name == "__text__")
            {
                sb.AppendLine(
                    $"{space}<ui:Label class=\"{node.className}\" text=\"{node.data.text}\" />");
                return;
            }

            sb.AppendLine($"{space}<ui:VisualElement class=\"{node.className}\">");

            foreach (var child in node.children)
                WriteNodeRecursive(sb, child, indent + 1);

            sb.AppendLine($"{space}</ui:VisualElement>");
        }

        // ========================= USS =========================
        static void GenerateUSS(
            List<Node> nodes,
            string ussPath,
            Vector2 resolution)
        {
           
            StringBuilder sb = new();
            // ================= GLOBAL TEXT STYLE =================
            if (stylePreset != null)
            {
                sb.AppendLine(".unity-text-element {");
                sb.AppendLine($"    color: {ToUSSColor(stylePreset.textColor)};");
                sb.AppendLine("}");
            }
            // ROOT
            sb.AppendLine(".root {");
            sb.AppendLine("    position: absolute;");
            sb.AppendLine("    left: 0;");
            sb.AppendLine("    top: 0;");
            sb.AppendLine("    right: 0;");
            sb.AppendLine("    bottom: 0;");
            sb.AppendLine("}");

            sb.AppendLine(".canvas {");
            sb.AppendLine("    position: absolute;");
            sb.AppendLine("    left: 0;");
            sb.AppendLine("    top: 0;");
            sb.AppendLine("    right: 0;");
            sb.AppendLine("    bottom: 0;");
            sb.AppendLine("}");

            foreach (var n in nodes)
            {
                WriteStyleBlock(sb, n, nodes, resolution);
            }

            File.WriteAllText(ussPath, sb.ToString());
        }

        // ===== MAIN STYLE BLOCK =====
        static void WriteStyleBlock(
    StringBuilder sb,
    Node n,
    List<Node> nodes,
    Vector2 resolution)
        {
            var e = n.data;

            sb.AppendLine($".{n.className} {{");

            // ================= SCROLL =================
            if (n.isScroll)
            {
                sb.AppendLine("    flex-grow: 1;");
                sb.AppendLine("    overflow: hidden;");

                if (n.layoutType == LayoutType.Vertical)
                    sb.AppendLine("    flex-direction: column;");

                if (n.layoutType == LayoutType.Horizontal)
                    sb.AppendLine("    flex-direction: row;");

                if (n.layoutType == LayoutType.Grid)
                {
                    sb.AppendLine("    flex-direction: row;");
                    sb.AppendLine("    flex-wrap: wrap;");
                }

                sb.AppendLine("}");
                return;
            }

            // ================= TEXT =================
            if (e.name == "__text__" && stylePreset != null && !string.IsNullOrEmpty(e.text))
            {
                float estimatedCharWidthRatio = stylePreset.estimatedCharWidthRatio;
                float autoFontSize = e.width / (e.text.Length * estimatedCharWidthRatio);

                // ===== FONT SIZE AUTO (GIỮ LOGIC CỦA BẠN) =====
                sb.AppendLine($"    font-size: {autoFontSize}px;");

                // ===== COLOR =====
                sb.AppendLine($"    color: {ToUSSColor(stylePreset.textColor)};");

                // ===== FONT =====
                if (stylePreset.fontAsset != null)
                {
                    string fontPath = AssetDatabase.GetAssetPath(stylePreset.fontAsset);
                    sb.AppendLine(
                        $"    -unity-font-definition: url(\"project://database/{fontPath}\");");
                }
                if (stylePreset.fallbackFont != null)
                {
                    string fontPath = AssetDatabase.GetAssetPath(stylePreset.fallbackFont);
                    sb.AppendLine(
                        $"    -unity-font: url(\"project://database/{fontPath}\");");
                }

                // ===== LETTER SPACING =====
                sb.AppendLine($"    letter-spacing: {stylePreset.letterSpacing}px;");

                // ===== LINE HEIGHT =====
                if (stylePreset.lineHeight > 0)
                    sb.AppendLine($"    line-height: {stylePreset.lineHeight}px;");

                // ===== ALIGNMENT =====
                sb.AppendLine(
                    $"    -unity-text-align: {ConvertAlignment(stylePreset.textAlignment)};");

                // =========================================
                // 🔥 OUTLINE (THÊM Ở ĐÂY)
                // =========================================
                if (stylePreset.useOutline && stylePreset.outlineWidth > 0f)
                {
                    sb.AppendLine(
                        $"    -unity-text-outline-color: {ToUSSColor(stylePreset.outlineColor)};");
                    sb.AppendLine(
                        $"    -unity-text-outline-width: {stylePreset.outlineWidth}px;");
                }

                // ===== FONT STYLE =====
                sb.AppendLine(
                    $"    -unity-font-style: {ConvertFontStyle(stylePreset.fontStyle)};");
            }

            sb.AppendLine("    position: absolute;");

            float widthRatio = e.width / resolution.x;
            float heightRatio = e.height / resolution.y;

            float halfW = e.width * 0.5f;
            float halfH = e.height * 0.5f;

            float centerX = e.x;
            float centerY = e.y;

            // ================= FULL STRETCH =================
            if (widthRatio >= STRETCH_THRESHOLD &&
                heightRatio >= STRETCH_THRESHOLD)
            {
                sb.AppendLine("    left: 0;");
                sb.AppendLine("    top: 0;");
                sb.AppendLine("    right: 0;");
                sb.AppendLine("    bottom: 0;");

                n.globalLeft = 0;
                n.globalTop = 0;
            }
            else
            {
                // ===== GLOBAL POSITION (center → top-left) =====
                float absLeft = centerX - halfW + resolution.x * 0.5f;
                float absTop = resolution.y * 0.5f - centerY - halfH;

                float absRight = resolution.x - absLeft - e.width;
                float absBottom = resolution.y - absTop - e.height;

                n.globalLeft = absLeft;
                n.globalTop = absTop;

                // ===== Parent offset =====
                if (n.parentIndex >= 0)
                {
                    Node parent = nodes[n.parentIndex];

                    absLeft -= parent.globalLeft;
                    absTop -= parent.globalTop;

                    absRight = parent.data.width - absLeft - e.width;
                    absBottom = parent.data.height - absTop - e.height;
                }

                // ===== LOCAL CENTER DETECT (FIX MIDDLE RIGHT BUG) =====

                // center local theo parent
                float localCenterX;
                float localCenterY;

                if (n.parentIndex >= 0)
                {
                    Node parent = nodes[n.parentIndex];
                    localCenterX = absLeft + e.width * 0.5f - parent.data.width * 0.5f;
                    localCenterY = parent.data.height * 0.5f - (absTop + e.height * 0.5f);
                }
                else
                {
                    localCenterX = centerX;
                    localCenterY = centerY;
                }

                float centerThresholdX = resolution.x * CENTER_THRESHOLD_RATIO;
                float centerThresholdY = resolution.y * CENTER_THRESHOLD_RATIO;

                bool isCenterX = Mathf.Abs(localCenterX) <= centerThresholdX;
                bool isCenterY = Mathf.Abs(localCenterY) <= centerThresholdY;

                // ===== Anchor by distance (NO GLOBAL THRESHOLD) =====

                float horizontalDiff = Mathf.Abs(absLeft - absRight);
                float verticalDiff = Mathf.Abs(absTop - absBottom);

                // tolerance theo parent size
                float parentWidth = (n.parentIndex >= 0) ? nodes[n.parentIndex].data.width : resolution.x;
                float parentHeight = (n.parentIndex >= 0) ? nodes[n.parentIndex].data.height : resolution.y;

                float centerToleranceX = parentWidth * CENTER_THRESHOLD_RATIO;
                float centerToleranceY = parentHeight * CENTER_THRESHOLD_RATIO;

                // ===== Horizontal =====
                if (horizontalDiff <= centerToleranceX)
                {
                    sb.AppendLine("    left: 50%;");
                    sb.AppendLine($"    margin-left: {-e.width * 0.5f}px;");
                }
                else if (absLeft < absRight)
                {
                    sb.AppendLine($"    left: {absLeft}px;");
                }
                else
                {
                    sb.AppendLine($"    right: {absRight}px;");
                }

                // ===== Vertical =====
                if (verticalDiff <= centerToleranceY)
                {
                    sb.AppendLine("    top: 50%;");
                    sb.AppendLine($"    margin-top: {-e.height * 0.5f}px;");
                }
                else if (absTop < absBottom)
                {
                    sb.AppendLine($"    top: {absTop}px;");
                }
                else
                {
                    sb.AppendLine($"    bottom: {absBottom}px;");
                }

                sb.AppendLine($"    width: {e.width}px;");
                sb.AppendLine($"    height: {e.height}px;");
            }
            // ================= SPRITE =================
            bool hasSprite = !string.IsNullOrEmpty(n.spritePath);

            if (hasSprite)
            {
                sb.AppendLine($"    background-image: url(\"project://database/{n.spritePath}\");");
                sb.AppendLine("    -unity-background-scale-mode: stretch-to-fill;");
                sb.AppendLine("    background-color: transparent;"); // đảm bảo không có màu nền
            }

            // ===== MAIN CLASS CLOSE =====
            sb.AppendLine("}");
        }

        static string ConvertAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.MiddleCenter => "middle-center",
                TextAnchor.MiddleLeft => "middle-left",
                TextAnchor.MiddleRight => "middle-right",
                TextAnchor.UpperCenter => "upper-center",
                TextAnchor.UpperLeft => "upper-left",
                TextAnchor.UpperRight => "upper-right",
                TextAnchor.LowerCenter => "lower-center",
                TextAnchor.LowerLeft => "lower-left",
                TextAnchor.LowerRight => "lower-right",
                _ => "middle-center"
            };
        }
        static string ConvertFontStyle(FontStyle style)
        {
            return style switch
            {
                FontStyle.Bold => "bold",
                FontStyle.Italic => "italic",
                FontStyle.BoldAndItalic => "bold-and-italic",
                _ => "normal"
            };
        }

        static string ToUSSColor(Color c)
        {
            return $"rgba({(int)(c.r * 255)}, {(int)(c.g * 255)}, {(int)(c.b * 255)}, {c.a.ToString("0.##")})";
        }

        // ========================= PANEL SETTINGS =========================
        static PanelSettings GetOrCreatePanelSettings(Vector2Int reference)
        {
            PanelSettings settings = null;

            if (File.Exists(PANEL_SETTINGS_PATH))
            {
                settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PANEL_SETTINGS_PATH);

                if (settings == null)
                {
                    AssetDatabase.DeleteAsset(PANEL_SETTINGS_PATH);
                }
            }

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, PANEL_SETTINGS_PATH);
            }

            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = reference;
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;

            // ===== LOAD DEFAULT UNITY THEME =====
            var defaultTheme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Assets/Auto UI Generator/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss");

            if (defaultTheme != null)
                settings.themeStyleSheet = defaultTheme;
            else
                Debug.LogWarning("UnityDefaultRuntimeTheme not found.");

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            return settings;
        }

        // ========================= SPRITE =========================
        static string ResolveSprite(string name)
        {
            if (templates == null) return null;

            if (!int.TryParse(name, out int index))
                return null;

            if (index < 0 || index >= templates.Count)
                return null;

            return AssetDatabase.GetAssetPath(templates[index]);
        }

        // ========================= SCENE =========================
        static void CreateUIDocumentInScene(
            string uxmlPath,
            string rootName,
            PanelSettings panelSettings)
        {
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (vta == null)
            {
                Debug.LogError("Failed to load UXML.");
                return;
            }

            GameObject go = new("UIDocument_" + rootName);
            Undo.RegisterCreatedObjectUndo(go, "Create UIDocument");

            var doc = go.AddComponent<UIDocument>();
            doc.visualTreeAsset = vta;
            doc.panelSettings = panelSettings;
            EditorGUIUtility.PingObject(vta);
            Selection.activeGameObject = go;
        }
    }
}

#endif