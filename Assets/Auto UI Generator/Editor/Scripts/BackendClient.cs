using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace VFF
{
    enum UIBuildTarget
    {
        UGUI,
        UIToolkit
    }

    [System.Serializable]
    public class MetaWrapper
    {
        public bool has_text;
        public bool has_button;

        public MetaWrapper(Dictionary<string, object> dict)
        {
            has_text = dict.ContainsKey("has_text") && (bool)dict["has_text"];
            has_button = dict.ContainsKey("has_button") && (bool)dict["has_button"];
        }
    }

    [System.Serializable]
    public class ProgressInfo
    {
        public int progress;
        public bool is_running;
        public string stage;
    }

    [System.Serializable]
    public class UpdateAvailabilityInfo
    {
        public string version;
    }

    [System.Serializable]
    public class BackendPingInfo
    {
        public string status;
        public string version;
    }

    [System.Serializable]
    public class BackendVersionInfo
    {
        public string version;
    }

    public class AutoUIToolsWindow : EditorWindow
    {
        // ================= CONFIG & PATHS =================
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData",
            "LocalLow",
            "FireFire",
            "AutoUIGenerator",
            "Engine",
            "ui-detect"
        );

        private const string EngineFileName = "ui-detect";
        private const string BASE_URL = "http://127.0.0.1:38421";
        private const string UPDATE_CHECK_URL = "https://auto-ui-generator.onrender.com/updateAvailable.json";

        // ================= CACHE KEYS =================
        const string PREF_SCREENSHOT = "AutoUI_Cache_Screenshot";
        const string PREF_ROOT = "AutoUI_Cache_RootRect";
        const string PREF_ROOTNAME = "AutoUI_Cache_RootName";
        const string PREF_DETECT_TEXT = "AutoUI_Detect_Text";
        const string PREF_DETECT_BUTTON = "AutoUI_Detect_Button";
        const string PREF_USE_SCREENSHOT_BG = "AutoUI_Use_Screenshot_Background";
        const string PREF_TEMPLATE_PATHS = "AutoUI_Cache_TemplatePaths";
        const string PREF_FAKE_SCREEN_ALPHA = "AutoUI_Fake_Screen_Alpha";

        // ================= STATE =================
        private Process backendProcess;
        private bool backendReady;
        private bool isDetecting;
        private Vector2 templateScrollPos;
        private float detectProgress;
        private float detectProgressTarget;
        private string detectStage = "idle";
        private double nextProgressPollTime;
        private double lastProgressTickTime;
        private double lastServerProgressTime;

        // Data
        private Texture2D mainScreenshot;
        private List<Texture2D> manualTemplates = new List<Texture2D>();
        private RectTransform targetRoot;
        private string rootName = "Root";
        private bool enableTextDetect = true;
        private bool enableButtonDetect = true;
        private bool enableScreenshotBackground = true;
        private float fakeScreenAlpha = 0.5f;
        private DetectResult detectResult;
        private EditorUIState uiState = new EditorUIState();
        private bool isInstalled;
        // Search + Sort
        private string templateSearch = "";
        private bool sortBySizeDesc = true;
        private int objectPickerControlID;
        private const string STYLE_PRESET_PATH =
            "Assets/Auto UI Generator/Config/AutoUIProjectSettings.asset";
        private const int DEFAULT_REFERENCE_WIDTH = 1080;
        private const int DEFAULT_REFERENCE_HEIGHT = 1920;

        private const string DOC_URL =
            "https://auto-ui-generator.onrender.com/";
        private const string PREMIUM_URL =
            "https://assetstore.unity.com/packages/tools/utilities/auto-ui-generator-359282";
        private const string ICON_DOC_PATH =
            "Assets/Auto UI Generator/Editor/Icons/doc.png";

        private const string ICON_SETTING_PATH =
            "Assets/Auto UI Generator/Editor/Icons/setting.png";
        Texture2D iconDoc;
        Texture2D iconSetting;
        private const string ICON_READY_PATH =
            "Assets/Auto UI Generator/Editor/Icons/Ready.png";

        private const string ICON_MISSING_PATH =
            "Assets/Auto UI Generator/Editor/Icons/Missing.png";

        private Texture2D iconReady;
        private Texture2D iconMissing;

        [MenuItem("Tools/Auto UI Generator/Auto UI Tools #a")]
        static void Open()
        {
            var window = GetWindow<AutoUIToolsWindow>("Auto UI Tools");
            window.titleContent = new GUIContent("Auto UI Tools");
        }

        void OnEnable()
        {
            if (IsEngineInstalled())
            {
                isInstalled = true;
                StartServer();
            }
            else
            {
                isInstalled = false;
                EditorApplication.delayCall += () =>
                {
                    Close();
                    AutoUIGeneratorWelcome.ShowWindow();
                };
                Debug.LogError("[Auto UI] AI Engine not found. Please run Tools > Auto UI Generator > Setup Wizard first.");
            }
            EditorCoroutine(CheckBackendReady());
            minSize = new Vector2(420, 620);
            maxSize = new Vector2(420, 620);
            LoadCache();
            iconDoc = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_DOC_PATH);
            iconSetting = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_SETTING_PATH);
            iconReady = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_READY_PATH);
            iconMissing = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_MISSING_PATH);
            ResetProgressUI();
        }


        void OnDisable() => StopServer();

        // ================= SERVER LOGIC =================
        private static bool IsEngineInstalled()
        {
#if UNITY_EDITOR_WIN
            return File.Exists(Path.Combine(AppDataPath, EngineFileName + ".exe"));
#else
            return File.Exists(Path.Combine(AppDataPath, EngineFileName));
#endif
        }

        void DrawTopRightButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle iconStyle = new GUIStyle(GUI.skin.button);
            iconStyle.fixedWidth = 28;
            iconStyle.fixedHeight = 28;
            iconStyle.padding = new RectOffset(4, 4, 4, 4);

            // ===== SETTINGS BUTTON =====
            GUIContent settingContent = new GUIContent(
                iconSetting,
                "Open Default Style Preset"
            );

            if (GUILayout.Button(settingContent, iconStyle))
            {
                Object preset = AssetDatabase.LoadAssetAtPath<Object>(STYLE_PRESET_PATH);

                if (preset != null)
                {
                    EditorGUIUtility.PingObject(preset);
                    Selection.activeObject = preset;
                }
                else
                {
                    Debug.LogWarning("DefaultPreset.asset not found.");
                }
            }

            // ===== DOC BUTTON =====
            GUIContent docContent = new GUIContent(
                iconDoc,
                "Open Documentation"
            );

            if (GUILayout.Button(docContent, iconStyle))
            {
                Application.OpenURL(DOC_URL);
            }
            if (isDetecting)
                DrawProgressStatus();
            EditorGUILayout.EndHorizontal();
        }

        // ================= ENGINE RUNNER =================
        public void RunExe(string args = "")
        {
            string exePath = Path.Combine(AppDataPath,
#if UNITY_EDITOR_WIN
                EngineFileName + ".exe"
#else
                EngineFileName
#endif
            );

            if (!File.Exists(exePath)) return;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                WorkingDirectory = AppDataPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                backendProcess = Process.Start(startInfo);
            }
            catch (Exception ex) { Debug.LogError("Failed to run AI Engine: " + ex.Message); }
        }

        void StartServer() => RunExe();
        void StopServer()
        {
            try { if (backendProcess != null && !backendProcess.HasExited) backendProcess.Kill(); } catch { }
            backendReady = false;
        }

        IEnumerator CheckBackendReady()
        {
            yield return new WaitForSeconds(1.5f); // Initial delay for server startup
            UnityWebRequest req = UnityWebRequest.Get(BASE_URL + "/ping");
            UnityWebRequestAsyncOperation op = req.SendWebRequest();
            while (true)
            {
                if (op.isDone)
                    break;

                yield return null;
            }

            backendReady = true;
            Repaint();

            string serverVersion = "";

            if (string.IsNullOrEmpty(serverVersion))
            {
                UnityWebRequest versionReq = UnityWebRequest.Get(BASE_URL + "/version");
                UnityWebRequestAsyncOperation versionOp = versionReq.SendWebRequest();

                while (!versionOp.isDone)
                    yield return null;

                if (versionReq.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        BackendVersionInfo versionInfo = JsonUtility.FromJson<BackendVersionInfo>(versionReq.downloadHandler.text);
                        if (versionInfo != null)
                            serverVersion = versionInfo.version;
                    }
                    catch
                    {
                        // ignore parse errors
                    }
                }
            }

            backendReady = true;
            Repaint();
        }
       
        private static string ResolveRemoteVersion(UpdateAvailabilityInfo updateInfo)
        {
            if (updateInfo == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(updateInfo.version))
                return NormalizeVersion(updateInfo.version);

            return string.Empty;
        }

        private static bool AreVersionsEqual(string versionA, string versionB)
        {
            return string.Equals(
                NormalizeVersion(versionA),
                NormalizeVersion(versionB),
                StringComparison.OrdinalIgnoreCase
            );
        }

        private static string NormalizeVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return string.Empty;

            return version.Trim().TrimStart('v', 'V');
        }

        // ================= UI DRAWING =================
        void OnGUI()
        {
            DrawAutoUITab();
            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == objectPickerControlID)
                {
                    Object picked = EditorGUIUtility.GetObjectPickerObject();

                    Texture2D textureToAdd = null;

                    if (picked is Texture2D tex)
                    {
                        textureToAdd = tex;
                    }
                    else if (picked is Sprite sprite)
                    {
                        textureToAdd = sprite.texture;
                    }

                    if (TryAddTemplate(textureToAdd))
                    {
                        SortTemplates();
                        SaveTemplateCache();
                        Repaint();
                    }
                }
            }
        }

        void DrawAutoUITab()
        {
            DrawTopRightButtons();
            GUILayout.Space(5);
            DrawTemplateArea();
            DrawSeparator();

            GUILayout.Label(new GUIContent(" Configuration", EditorGUIUtility.IconContent("SettingsIcon").image), EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope("box"))
            {
                DrawScreenshotField();
                targetRoot = (RectTransform)EditorGUILayout.ObjectField(new GUIContent("Target Root", EditorGUIUtility.IconContent("d_GameObject Icon").image), targetRoot, typeof(RectTransform), true);
                rootName = EditorGUILayout.TextField("Root Name", rootName);
            }

            uiState.Update(mainScreenshot, manualTemplates, targetRoot);
            GUILayout.Space(10);
            DrawBackendStatus();


            EditorGUILayout.BeginHorizontal();
            using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                DrawDetectionOptions();
            }

            GUILayout.Space(8);

            using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                DrawChecklist();
            }
            EditorGUILayout.EndHorizontal();
            bool hasFakeScreenImage = TryGetFakeScreenImage(out Image fakeScreenImage);
            if (hasFakeScreenImage)
            {
                using (new EditorGUI.DisabledScope(!enableScreenshotBackground))
                {
                    fakeScreenAlpha = EditorGUILayout.Slider("Fake Screen Alpha", fakeScreenAlpha, 0f, 1f);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(PREF_DETECT_TEXT, enableTextDetect);
                EditorPrefs.SetBool(PREF_DETECT_BUTTON, enableButtonDetect);
                EditorPrefs.SetBool(PREF_USE_SCREENSHOT_BG, enableScreenshotBackground);
                EditorPrefs.SetFloat(PREF_FAKE_SCREEN_ALPHA, fakeScreenAlpha);

                if (hasFakeScreenImage && fakeScreenImage != null)
                    ApplyFakeScreenAlpha(fakeScreenImage, fakeScreenAlpha);
            }

            GUILayout.FlexibleSpace();
            DrawSeparator();
            DrawUpgradeToPremiumButton();
            DrawApplyButton();
        }

        void DrawTemplateArea()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Templates Assets", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (manualTemplates.Count > 0)
                    GUILayout.Label($"{manualTemplates.Count} Images", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                GUIStyle searchStyle = new GUIStyle(EditorStyles.toolbarSearchField)
                {
                    fixedHeight = 20
                };

                templateSearch = GUILayout.TextField(
                    templateSearch,
                    searchStyle,
                    GUILayout.ExpandWidth(true),
                    GUILayout.MinWidth(210)
                );

                if (DrawColoredMiniButton("+ Add", new Color(0.24f, 0.72f, 1f), 58f))
                {
                    AddTemplateViaPicker();
                }

                EditorGUI.BeginDisabledGroup(manualTemplates.Count == 0);
                if (DrawColoredMiniButton("Clear All", new Color(0.95f, 0.38f, 0.38f), 76f))
                {
                    manualTemplates.Clear();
                    SaveTemplateCache();
                }
                EditorGUI.EndDisabledGroup();

                sortBySizeDesc = EditorGUILayout.ToggleLeft("Size ↓", sortBySizeDesc, GUILayout.Width(66));
                EditorGUILayout.EndHorizontal();
            }

            float dropZoneHeight = manualTemplates.Count == 0 ? 150f : 30f;
            Rect dropArea = GUILayoutUtility.GetRect(0, dropZoneHeight, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "", EditorStyles.helpBox);

            var dropContent = manualTemplates.Count == 0
                ? new GUIContent(" DROP FOLDERS / IMAGES HERE", EditorGUIUtility.IconContent("Folder Icon").image)
                : new GUIContent(" Drag to add more...", EditorGUIUtility.IconContent("Toolbar Plus").image);

            GUI.Label(dropArea, dropContent, new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic
            });

            HandleTemplateDrag(dropArea);

            if (manualTemplates.Count > 0)
            {
                // 🔎 Filter by search
                var filtered = manualTemplates.FindAll(t =>
                    t != null &&
                    (string.IsNullOrEmpty(templateSearch) ||
                     t.name.IndexOf(templateSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                );

                float scrollHeight = Mathf.Min(filtered.Count * 22 + 5, 120f);
                templateScrollPos = EditorGUILayout.BeginScrollView(templateScrollPos, GUILayout.Height(scrollHeight));

                Texture2D toRemove = null;

                foreach (var tex in filtered)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.Height(18));
                    GUILayout.Label($"{tex.width}x{tex.height}", GUILayout.Width(70));

                    if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(22)))
                    {
                        toRemove = tex;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (toRemove != null)
                {
                    manualTemplates.Remove(toRemove);
                    SaveTemplateCache();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        bool DrawColoredMiniButton(string label, Color color, float width)
        {
            Color previousColor = GUI.color;
            GUI.color = color;

            bool clicked = GUILayout.Button(
                label,
                EditorStyles.miniButton,
                GUILayout.Width(width),
                GUILayout.Height(20)
            );

            GUI.color = previousColor;
            return clicked;
        }

        void DrawBackendStatus()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            style.normal.textColor = backendReady && isInstalled
                ? new Color(0.2f, 0.8f, 0.2f)
                : Color.red;
            GUILayout.Label(backendReady && isInstalled ? "● AI Engine Service: Connected" : "○ AI Engine Service: Disconnected", style);
        }

        void DrawProgressStatus()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Rect progressRect = GUILayoutUtility.GetRect(10, 20, GUILayout.ExpandWidth(true));
                float normalized = Mathf.Clamp01(detectProgress / 100f);
                EditorGUI.ProgressBar(progressRect, normalized, $"{Mathf.RoundToInt(detectProgress)}%");

                string stageText = string.IsNullOrEmpty(detectStage)
                    ? "idle"
                    : detectStage.Replace("_", " ");
            }
        }

        void DrawDetectionOptions()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();

                enableTextDetect = EditorGUILayout.Toggle(
                    new GUIContent(" Detect Text (OCR)", EditorGUIUtility.IconContent("TextAsset Icon").image),
                    enableTextDetect);

                using (new EditorGUI.DisabledScope(true))
                {
                    enableButtonDetect = false;

                    EditorGUILayout.Toggle(new GUIContent(" Detect Buttons (Premium)"), false);
                    EditorGUILayout.Toggle(new GUIContent(" Auto 9-Slicer (Premium)"), false);
                    EditorGUILayout.Toggle(new GUIContent(" Auto ScrollView (Premium)"), false);
                }

                enableScreenshotBackground = EditorGUILayout.Toggle(
                    new GUIContent(" Use Fake Screen", EditorGUIUtility.IconContent("Image Icon").image),
                    enableScreenshotBackground);
            }
        }

        void DrawUpgradeToPremiumButton()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Color originalColor = GUI.color;
            GUI.color = new Color(1f, 0.67f, 0.2f);

            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 340f,
                fixedHeight = 32f
            };

            if (GUILayout.Button("Upgrade to Premium - Unlock Smarter AI Now", style))
            {
                Application.OpenURL(PREMIUM_URL);
            }

            GUI.color = originalColor;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(6);
        }

        bool TryGetFakeScreenImage(out Image fakeScreenImage)
        {
            fakeScreenImage = null;

            if (targetRoot == null)
                return false;

            Transform[] transforms = targetRoot.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in transforms)
            {
                if (!string.Equals(child.name, "MainScreenShot", StringComparison.Ordinal))
                    continue;

                Image image = child.GetComponent<Image>();

                if (image == null)
                    continue;

                fakeScreenImage = image;
                return true;
            }

            return false;
        }

        void ApplyFakeScreenAlpha(Image fakeScreenImage, float alpha)
        {
            if (fakeScreenImage == null)
                return;

            Color color = fakeScreenImage.color;
            color.a = Mathf.Clamp01(alpha);
            fakeScreenImage.color = color;
            EditorUtility.SetDirty(fakeScreenImage);
        }

        void DrawChecklist()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Requirements Status", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                DrawStatusRow("Templates Loaded", uiState.HasTemplates);
                DrawStatusRow("Screenshot Ready", uiState.HasScreenshot);
                DrawStatusRow("Canvas Target Set", uiState.HasCanvas);
            }
        }

        void DrawStatusRow(string label, bool ok)
        {
            EditorGUILayout.BeginHorizontal();

            Texture2D icon = ok ? iconReady : iconMissing;

            if (icon != null)
            {
                GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));
            }
            else
            {
                GUILayout.Space(20); // fallback nếu icon null
            }

            GUILayout.Space(4);

            GUILayout.Label(label, EditorStyles.label);

            GUILayout.FlexibleSpace();

            GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
            statusStyle.fontSize = 11;
            statusStyle.normal.textColor = ok
                ? new Color(0.2f, 0.85f, 0.2f)
                : new Color(1f, 0.35f, 0.35f);

            GUILayout.Label(ok ? "Ready" : "Missing", statusStyle);

            EditorGUILayout.EndHorizontal();
        }

        void DrawApplyButton()
        {
            bool canGenerateUGUI =
                backendReady &&
                uiState.HasTemplates &&
                uiState.HasScreenshot &&
                uiState.HasCanvas &&
                !isDetecting;

            bool canGenerateUIToolkit =
                backendReady &&
                uiState.HasTemplates &&
                uiState.HasScreenshot &&
                !isDetecting;

            const float buttonWidth = 160f;
            const float buttonHeight = 45f;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // căn giữa

#if UNITY_2021_3_OR_NEWER
            DrawStyledButton(
                isDetecting ? "DETECTING..." : "Generate UI Toolkit",
                buttonWidth,
                buttonHeight,
                canGenerateUIToolkit,
                new Color(0.25f, 0.65f, 1f),
                () =>
                {
                    isDetecting = true;
                    EditorCoroutine(DetectAndProcessRoutine(UIBuildTarget.UIToolkit));
                });
#endif

            GUILayout.Space(20);

            DrawStyledButton(
                isDetecting ? "DETECTING..." : "Generate UGUI",
                buttonWidth,
                buttonHeight,
                canGenerateUGUI,
                new Color(0.3f, 0.9f, 0.3f),
                () =>
                {
                    isDetecting = true;
                    EditorCoroutine(DetectAndProcessRoutine(UIBuildTarget.UGUI));
                });

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        void DrawStyledButton(
            string label,
            float width,
            float height,
            bool enabled,
            Color activeColor,
            System.Action onClick)
        {
            GUI.enabled = enabled;

            Color originalColor = GUI.color;
            GUI.color = enabled ? activeColor : new Color(1f, 1f, 1f, 0.5f);

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 13;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.fixedWidth = width;
            style.fixedHeight = height;

            if (GUILayout.Button(label, style))
            {
                onClick?.Invoke();
            }

            GUI.color = originalColor;
            GUI.enabled = true;
        }

        // ================= LOGIC & UTILS =================
        void HandleTemplateDrag(Rect area)
        {
            Event evt = Event.current;
            if (!area.Contains(evt.mousePosition)) return;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    int addedCount = 0;
                    DragAndDrop.AcceptDrag();
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is Texture2D tex)
                        {
                            if (TryAddTemplate(tex))
                                addedCount++;
                        }
                        else if (obj is Sprite sprite)
                        {
                            if (TryAddTemplate(sprite.texture))
                                addedCount++;
                        }
                        else if (obj is DefaultAsset) // Folder
                        {
                            string path = AssetDatabase.GetAssetPath(obj);
                            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
                            foreach (string guid in guids)
                            {
                                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
                                if (TryAddTemplate(t))
                                    addedCount++;
                            }
                        }
                    }

                    SortTemplates();
                    if (addedCount > 0)
                        SaveTemplateCache();
                    Repaint();
                    evt.Use();
                }
            }
        }

        bool TryAddTemplate(Texture2D tex)
        {
            if (tex == null)
                return false;

            if (manualTemplates.Contains(tex))
                return false;

            // Check if the asset path contains ".handle9slicer"
            string assetPath = AssetDatabase.GetAssetPath(tex);
            if (!string.IsNullOrEmpty(assetPath) && assetPath.Contains(".handle9slicer", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"Cannot add template: '{tex.name}' contains '.handle9slicer' in its path. This type of file is not allowed as a template.");
                return false;
            }

            manualTemplates.Add(tex);
            return true;
        }

        void SortTemplates()
        {
            manualTemplates.Sort((a, b) =>
            {
                if (a == null || b == null) return 0;

                int areaA = a.width * a.height;
                int areaB = b.width * b.height;

                return sortBySizeDesc
                    ? areaB.CompareTo(areaA)
                    : areaA.CompareTo(areaB);
            });
        }

        void DrawScreenshotField()
        {
            EditorGUI.BeginChangeCheck();
            mainScreenshot = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Main Screenshot", EditorGUIUtility.IconContent("Image Icon").image), mainScreenshot, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck() && mainScreenshot != null) EditorPrefs.SetString(PREF_SCREENSHOT, AssetDatabase.GetAssetPath(mainScreenshot));
        }

        void LoadCache()
        {
            enableTextDetect = EditorPrefs.GetBool(PREF_DETECT_TEXT, true);
            enableButtonDetect = false;
            enableScreenshotBackground = EditorPrefs.GetBool(PREF_USE_SCREENSHOT_BG, true);
            fakeScreenAlpha = Mathf.Clamp01(EditorPrefs.GetFloat(PREF_FAKE_SCREEN_ALPHA, 0.5f));
            LoadTemplateCache();
            // Screenshot
            if (EditorPrefs.HasKey(PREF_SCREENSHOT))
            {
                string path = EditorPrefs.GetString(PREF_SCREENSHOT);
                mainScreenshot = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            // Canvas (scene object)
            if (EditorPrefs.HasKey(PREF_ROOT))
            {
                string gidStr = EditorPrefs.GetString(PREF_ROOT);
                if (GlobalObjectId.TryParse(gidStr, out var gid))
                {
                    Object obj =
                        GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
                    targetRoot = obj as RectTransform;

                }
            }

            // Root name
            if (EditorPrefs.HasKey(PREF_ROOTNAME))
            {
                rootName = EditorPrefs.GetString(PREF_ROOTNAME);
            }
        }

        void SaveTemplateCache()
        {
            if (manualTemplates == null || manualTemplates.Count == 0)
            {
                EditorPrefs.DeleteKey(PREF_TEMPLATE_PATHS);
                return;
            }

            List<string> paths = new List<string>();

            foreach (Texture2D tex in manualTemplates)
            {
                if (tex == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(path))
                    continue;

                paths.Add(path);
            }

            string joinedPaths = string.Join("|", paths.Distinct().ToArray());
            if (string.IsNullOrEmpty(joinedPaths))
            {
                EditorPrefs.DeleteKey(PREF_TEMPLATE_PATHS);
                return;
            }

            EditorPrefs.SetString(PREF_TEMPLATE_PATHS, joinedPaths);
        }

        void LoadTemplateCache()
        {
            manualTemplates.Clear();

            string joinedPaths = EditorPrefs.GetString(PREF_TEMPLATE_PATHS, string.Empty);
            if (string.IsNullOrEmpty(joinedPaths))
                return;

            string[] paths = joinedPaths.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string path in paths)
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null && !manualTemplates.Contains(tex))
                    manualTemplates.Add(tex);
            }

            SortTemplates();
        }

        void AddTemplateViaPicker()
        {
            objectPickerControlID = EditorGUIUtility.GetControlID(FocusType.Passive);

            EditorGUIUtility.ShowObjectPicker<Texture2D>(
                null,
                false,
                "t:Sprite",
                objectPickerControlID
            );
        }

        void DrawSeparator() { GUILayout.Space(5); Rect r = EditorGUILayout.GetControlRect(false, 1); EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 0.2f)); GUILayout.Space(5); }

        IEnumerator DetectAndProcessRoutine(UIBuildTarget target)
        {
            detectProgress = 1f;
            detectProgressTarget = 1f;
            detectStage = "starting";
            lastProgressTickTime = EditorApplication.timeSinceStartup;
            lastServerProgressTime = EditorApplication.timeSinceStartup;
            Repaint();

            // 1️⃣ Detect
            IEnumerator detect = DetectRoutine();

            while (detect.MoveNext())
                yield return detect.Current;

            if (detectResult == null)
            {
                Debug.LogWarning("UI Generation aborted: Detect failed");
                isDetecting = false;
                ResetProgressUI();
                yield break;
            }

            // 3️⃣ Generate theo target
            switch (target)
            {
                case UIBuildTarget.UGUI:
                    GenerateCanvasUI();
                    break;

#if UNITY_2021_3_OR_NEWER
                case UIBuildTarget.UIToolkit:
                    GenerateUIToolkitUI();
                    break;
#endif
            }
            isDetecting = false;
            ResetProgressUI();
            Repaint();
        }

        void GenerateCanvasUI()
        {
            AutoUIGenerator.Generate(
                targetRoot,
                detectResult,
                manualTemplates,
                rootName,
                enableScreenshotBackground ? mainScreenshot : null,
                fakeScreenAlpha
            );

            Debug.Log("UGUI generated successfully");
        }

#if UNITY_2021_3_OR_NEWER
        void GenerateUIToolkitUI()
        {
            AutoUIToolkitGenerator.Generate(
                detectResult,
                manualTemplates,
                rootName,
                GetReferenceResolution()
            );
        }
#endif
        Vector2Int GetReferenceResolution()
        {
            AutoUIProjectSettings settings =
                AssetDatabase.LoadAssetAtPath<AutoUIProjectSettings>(STYLE_PRESET_PATH);

            if (settings == null)
                return new Vector2Int(DEFAULT_REFERENCE_WIDTH, DEFAULT_REFERENCE_HEIGHT);

            Vector2Int configuredResolution = settings.uiToolkitReferenceResolution;

            if (configuredResolution.x <= 0 || configuredResolution.y <= 0)
                return new Vector2Int(DEFAULT_REFERENCE_WIDTH, DEFAULT_REFERENCE_HEIGHT);

            return configuredResolution;
        }

        IEnumerator DetectRoutine()
        {
            if (mainScreenshot == null || manualTemplates.Count == 0)
            {
                Debug.LogWarning("Missing input data: Screenshot or Templates");
                isDetecting = false;
                yield break;
            }

            detectProgress = 5f;
            detectProgressTarget = 5f;
            detectStage = "uploading";
            nextProgressPollTime = EditorApplication.timeSinceStartup;
            lastProgressTickTime = EditorApplication.timeSinceStartup;
            lastServerProgressTime = EditorApplication.timeSinceStartup;
            Repaint();

            string mainPath = AssetDatabase.GetAssetPath(mainScreenshot);

            WWWForm form = new WWWForm();
            Dictionary<string, object> meta = new Dictionary<string, object>
            {
                { "has_text", enableTextDetect },
                { "has_button", false }
            };

            string metaJson = JsonUtility.ToJson(new MetaWrapper(meta));
            form.AddField("meta", metaJson);

            form.AddBinaryData(
                "main",
                File.ReadAllBytes(mainPath),
                "main"
            );

            for (int i = 0; i < manualTemplates.Count; i++)
            {
                Texture2D tex = manualTemplates[i];
                if (tex == null) continue;

                string path = AssetDatabase.GetAssetPath(tex);

                form.AddBinaryData(
                    "templates",
                    File.ReadAllBytes(path),
                    i.ToString()
                );
            }


            UnityWebRequest req = UnityWebRequest.Post(BASE_URL + "/detect", form);
            UnityWebRequestAsyncOperation op = req.SendWebRequest();

            while (true)
            {
                if (op.isDone)
                    break;

                if (EditorApplication.timeSinceStartup >= nextProgressPollTime)
                {
                    yield return PollProgressOnce();
                    nextProgressPollTime = EditorApplication.timeSinceStartup + 0.1d;
                }

                TickProgressSmoothly();

                Repaint();

                yield return null;
            }

            yield return PollProgressOnce();

            if (req.result != UnityWebRequest.Result.Success)
            {
                detectResult = null;
                detectStage = "error";
                Debug.LogError("Detect request failed: " + req.error);
                yield break;
            }

            try
            {
                string wrappedJson =
                    "{ \"elements\": " + req.downloadHandler.text + " }";

                detectResult =
                    JsonUtility.FromJson<DetectResult>(wrappedJson);

                detectProgress = 100f;
                detectProgressTarget = 100f;
                detectStage = "done";
            }
            catch
            {
                detectResult = null;
                detectStage = "error";
                Debug.LogError("JSON Parse Failed:\n" + req.downloadHandler.text);
            }

            Repaint();
        }

        IEnumerator PollProgressOnce()
        {
            UnityWebRequest progressReq = UnityWebRequest.Get(BASE_URL + "/progress");
            UnityWebRequestAsyncOperation progressOp = progressReq.SendWebRequest();

            while (!progressOp.isDone)
                yield return null;

            if (progressReq.result != UnityWebRequest.Result.Success)
                yield break;

            try
            {
                ProgressInfo info = JsonUtility.FromJson<ProgressInfo>(progressReq.downloadHandler.text);

                if (info == null)
                    yield break;

                float serverProgress = Mathf.Clamp(info.progress, 0, 100);
                detectProgressTarget = Mathf.Max(detectProgressTarget, serverProgress);
                detectStage = string.IsNullOrEmpty(info.stage) ? "processing" : info.stage;
                lastServerProgressTime = EditorApplication.timeSinceStartup;
            }
            catch
            {
                // ignore parse errors from transient responses
            }
        }

        void TickProgressSmoothly()
        {
            double now = EditorApplication.timeSinceStartup;
            float dt = Mathf.Max(0f, (float)(now - lastProgressTickTime));
            lastProgressTickTime = now;

            if (dt <= 0f)
                return;

            if (now - lastServerProgressTime > 0.8d && detectProgressTarget < 95f)
            {
                detectProgressTarget = Mathf.Min(95f, detectProgressTarget + 18f * dt);
            }

            float moveSpeed = 120f;
            detectProgress = Mathf.MoveTowards(detectProgress, detectProgressTarget, moveSpeed * dt);
        }

        void ResetProgressUI()
        {
            detectProgress = 0f;
            detectProgressTarget = 0f;
            detectStage = "idle";
            lastProgressTickTime = 0d;
            lastServerProgressTime = 0d;
        }

        void EditorCoroutine(IEnumerator routine)
        {
            void Tick()
            {
                if (!routine.MoveNext())
                    EditorApplication.update -= Tick;
            }
            EditorApplication.update += Tick;
        }
    }
}