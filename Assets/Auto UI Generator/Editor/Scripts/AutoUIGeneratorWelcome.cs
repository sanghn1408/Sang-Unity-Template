using Ionic.Zip;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace VFF
{
    [InitializeOnLoad]
    public class AutoUIGeneratorWelcome : EditorWindow
    {
        internal static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                .Replace("Local", "LocalLow"),
            "FireFire/AutoUIGenerator/Engine"
        );

        private static Texture2D bannerImage;
        private static AutoUIGeneratorWelcome _instance;

        private const string base64Key = "puLj3bmKQipNyg6KwMUDxHWcEsOBwYgaUBcFpFW+yWU=";
        private const string base64IV = "WOBggnZwnqjrmqB9FDDwDw==";
        public static string zipPassWord = "hrvlPKYHo6lY5Y/dDXqZ/z60HwfQTeBnI18f2nDkXWY";

        // Colors for the "Modern AI" look
        private static readonly Color AccentColor = new Color(0.2f, 0.6f, 1f); // Blue-ish AI accent
        private static readonly Color SuccessColor = new Color(0.4f, 0.85f, 0.4f);

        static AutoUIGeneratorWelcome()
        {
            EditorApplication.delayCall += RunOnce;
        }

        private static void RunOnce()
        {
            if (!IsEngineInstalled()) ShowWindow();
        }

        internal static void TryRepaint() => _instance?.Repaint();

        [MenuItem("Tools/Auto UI Generator/Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoUIGeneratorWelcome>("Auto UI Generator | Setup");
            window.titleContent = new GUIContent("Auto UI Start Up Window");
            window.minSize = new Vector2(520, 600);
            window.maxSize = new Vector2(520, 600);
            window.ShowUtility();
        }

        private void OnEnable()
        {
            _instance = this;
            bannerImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Auto UI Generator/Editor/Icons/WelcomeBanner.png");
        }

        private void OnGUI()
        {
            DrawHeader();

            using (new GUILayout.VerticalScope(new GUIStyle { padding = new RectOffset(25, 25, 20, 20) }))
            {
                DrawHeroSection();
                DrawResourceButtons();
                GUILayout.Space(20);
                DrawInstallerSection();
            }

            DrawFooter();
        }

        private void DrawHeader()
        {
            if (bannerImage != null)
            {
                Rect headerRect = GUILayoutUtility.GetRect(520, 108);
                GUI.DrawTexture(headerRect, bannerImage, ScaleMode.ScaleAndCrop);
            }
            else
            {
                // Placeholder if image is missing
                Rect headerRect = GUILayoutUtility.GetRect(500, 100);
                EditorGUI.DrawRect(headerRect, new Color(0.15f, 0.15f, 0.15f));
                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 20 };
                EditorGUI.LabelField(headerRect, "AUTO UI GENERATOR", style);
            }
        }
        internal static void ClearEngineData()
        {
            try
            {
                if (Directory.Exists(AppDataPath))
                {
                    Directory.Delete(AppDataPath, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[AutoUI] Failed to clear engine folder: " + e.Message);
            }
        }

        private void DrawHeroSection()
        {
            // --- STYLES ---
            var titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                margin = new RectOffset(0, 0, 0, 10)
            };

            var descStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 12,
                richText = true,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) },
                alignment = TextAnchor.UpperLeft
            };

            var tipBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };

            // --- RENDER ---

            // 1. Tiêu đề
            EditorGUILayout.LabelField("Activate AI Power", titleStyle, GUILayout.Height(30));

            // 2. Lời chào & Cảm ơn
            string description = "Transform your workflow with AI-driven UI detection. Set up the engine to begin.\n\n" +
                                 "<color=#FFD700>Thank you for being part of our community!</color> We're excited to help you build faster.";
            GUILayout.Label(description, descStyle);

            GUILayout.Space(12);

            // 3. Khối thông tin quan trọng
            // Khai báo một Style chung để tái sử dụng và đảm bảo wordWrap được bật
            GUIStyle wrapLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                richText = true,
                fontSize = 11,
                wordWrap = true // Quan trọng nhất: cho phép tự động xuống dòng
            };

            using (new GUILayout.VerticalScope(tipBoxStyle))
            {
                // Ý 1: Tải một lần duy nhất
                using (new GUILayout.HorizontalScope())
                {
                    var globalIcon = EditorGUIUtility.IconContent("d_ToolHandleGlobal");
                    GUILayout.Label(globalIcon, GUILayout.Width(18), GUILayout.Height(18));

                    GUILayout.Label("<b>Global Setup:</b> Install once, use across all Unity projects on this PC.", wrapLabelStyle);
                }

                GUILayout.Space(6);

                using (new GUILayout.HorizontalScope())
                {
                    // Icon dành cho Unity 2020, 2021, 2022+
                    var taskIcon = EditorGUIUtility.IconContent("d_Progress");

                    // Kiểm tra dự phòng: nếu d_Progress vẫn null ở bản mới, dùng d_Loading
                    if (taskIcon == null || taskIcon.image == null)
                        taskIcon = EditorGUIUtility.IconContent("d_Loading");

                    GUILayout.Label(taskIcon, GUILayout.Width(18), GUILayout.Height(18));

                    // Tạo một style biến thể cho dòng cuối (có màu riêng)
                    GUIStyle greenWrapStyle = new GUIStyle(wrapLabelStyle)
                    {
                        normal = { textColor = new Color(0.6f, 0.9f, 0.6f) }
                    };

                    GUILayout.Label("<b>Background:</b> You can close this window. Download continues in background.", greenWrapStyle);
                }
            }

            GUILayout.Space(10);
        }

        private void DrawResourceButtons()
        {
            using (new GUILayout.HorizontalScope())
            {
                // 📘 Documentation: Sử dụng "_Help" - Tồn tại từ rất lâu và rất rõ nghĩa
                GUIContent docContent = new GUIContent(" Documentation", EditorGUIUtility.IconContent("_Help").image);
                if (CustomButton(docContent, AccentColor))
                    Application.OpenURL("https://auto-ui-generator.onrender.com/");

                GUILayout.Space(8); // Thêm một chút khoảng cách giữa 2 nút

                // 💬 Community: Sử dụng "d_UserDefinedStatic" hoặc "BuildSettings.Web.Small"
                // "BuildSettings.Web.Small" là icon quả địa cầu, cực kỳ bền vững qua các phiên bản
                GUIContent communityContent = new GUIContent(" Community", EditorGUIUtility.IconContent("BuildSettings.Web.Small").image);
                if (CustomButton(communityContent, new Color(0.45f, 0.48f, 0.9f)))
                    Application.OpenURL("https://discord.com/invite/vs8PUqxbpY");
            }
        }

        // Đảm bảo hàm CustomButton của bạn đã hỗ trợ GUIContent
        private bool CustomButton(GUIContent content, Color color, float height = 30)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            // Sử dụng style "Button" để text và icon nằm giữa đẹp mắt
            bool clicked = GUILayout.Button(content, GUILayout.Height(height));

            GUI.backgroundColor = oldColor;
            return clicked;
        }

        private void DrawInstallerSection()
        {
            float progress = AutoUIBackgroundInstaller.Progress;
            bool isDone = AutoUIBackgroundInstaller.IsCompleted;
            string status = AutoUIBackgroundInstaller.Status;

            var boxStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(15, 15, 15, 15) };

            using (new GUILayout.VerticalScope(boxStyle))
            {
                if (progress < 0)
                {
                    bool installed = IsEngineInstalled();

                    // --- PHẦN TRANG TRÍ STATUS CÓ ĐÈN TÍN HIỆU ---
                    using (new GUILayout.HorizontalScope())
                    {
                        // Chọn Icon dựa trên trạng thái (Sử dụng Built-in Icons của Unity)
                        string iconName = installed ? "greenlight" : "redlight";
                        GUIContent statusContent = EditorGUIUtility.IconContent(iconName);

                        // Vẽ Icon (Đèn tín hiệu)
                        GUILayout.Label(statusContent, GUILayout.Width(20), GUILayout.Height(20));

                        // Vẽ Text Trạng thái
                        var statusTextStyle = new GUIStyle(EditorStyles.boldLabel)
                        {
                            normal = { textColor = installed ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f) },
                            fontSize = 13
                        };
                        GUILayout.Label(installed ? "ENGINE STATUS: READY" : "ENGINE STATUS: NOT READY", statusTextStyle);
                        GUILayout.FlexibleSpace();
                    }

                    // HIỂN THỊ ĐƯỜNG DẪN DƯỚI DÒNG STATUS
                    if (installed)
                    {
                        using (new GUILayout.VerticalScope(new GUIStyle { margin = new RectOffset(26, 0, 5, 5) })) // Thụt lề 26 để thẳng hàng với text
                        {
                            EditorGUILayout.LabelField("Executable Location:", EditorStyles.miniLabel);
                            EditorGUILayout.SelectableLabel(AppDataPath, EditorStyles.textField, GUILayout.Height(18));
                        }
                    }
                    else
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.HelpBox("The AI Core is required for UI recognition.", MessageType.Info);
                    }

                    GUILayout.Space(10);
                    string btnText = installed ? "REPAIR / UPDATE ENGINE" : "INSTALL AI ENGINE CORE";
                    Color btnColor = installed ? new Color(0.3f, 0.3f, 0.3f) : SuccessColor;

                    if (CustomButton(btnText, btnColor, 50))
                        AutoUIBackgroundInstaller.StartInstall();
                }
                else if (!isDone)
                {
                    GUILayout.Label("INSTALLATION IN PROGRESS", EditorStyles.miniBoldLabel);
                    GUILayout.Space(5);

                    Rect r = EditorGUILayout.GetControlRect(false, 25);
                    EditorGUI.ProgressBar(r, progress, $"{status} ({(progress * 100):F0}%)");

                    GUILayout.Space(10);
                    if (GUILayout.Button("Abort", GUILayout.Width(80)))
                        AutoUIBackgroundInstaller.Cancel();
                }
                else
                {
                    GUI.color = SuccessColor;
                    GUILayout.Label("COMPLETED SUCCESSFULLY ✅", EditorStyles.boldLabel);
                    GUI.color = Color.white;

                    EditorGUILayout.LabelField("Path:", EditorStyles.miniLabel);
                    EditorGUILayout.SelectableLabel(AutoUIBackgroundInstaller.ResultFolder, EditorStyles.textField, GUILayout.Height(20));

                    if (CustomButton("📂 OPEN ENGINE FOLDER", AccentColor, 35))
                        EditorUtility.RevealInFinder(AutoUIBackgroundInstaller.ResultFolder);
                }
            }
        }

        private void DrawFooter()
        {
            GUILayout.FlexibleSpace();
            var footerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("VFF Studio © 2026", footerStyle);
            GUILayout.Space(10);
        }

        // Helper for pretty buttons
        private bool CustomButton(string text, Color color, float height = 30)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool clicked = GUILayout.Button(text, GUILayout.Height(height));
            GUI.backgroundColor = oldColor;
            return clicked;
        }

        // --- Keep existing logic methods (IsEngineInstalled, ExtractZip, etc.) unchanged ---
        // (Vẫn giữ nguyên các hàm IsEngineInstalled, ExtractZip, DecryptExeStatic, GetDownloadUrlStatic)

        internal static string GetDownloadUrlStatic()
        {
#if UNITY_EDITOR_WIN
            return "https://github.com/hungwnguyen/Auto-UI-Generator-Backend/releases/download/Free/ui-detect.zip";
#elif UNITY_EDITOR_OSX
            return "https://github.com/user/repo/releases/download/v1.0/AIEngine_Mac.zip";
#else
            return "https://github.com/user/repo/releases/download/v1.0/AIEngine_Linux.zip";
#endif
        }

        public static void ExtractZip(string zipPath, string outputDir, string password)
        {
            // Sử dụng cấu trúc ngoặc nhọn để chạy được trên Unity 2019
            using (ZipFile zip = ZipFile.Read(zipPath))
            {
                if (!string.IsNullOrEmpty(password))
                    zip.Password = password;

                foreach (ZipEntry entry in zip)
                {
                    if (entry.IsDirectory) continue;

                    // Path.Combine giúp đường dẫn chuẩn xác trên cả Windows/Mac
                    string fullPath = Path.GetFullPath(Path.Combine(outputDir, entry.FileName));

                    // Bảo mật: Chống lỗi Zip Slip
                    if (!fullPath.StartsWith(Path.GetFullPath(outputDir), StringComparison.OrdinalIgnoreCase))
                        continue;

                    string directoryPath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    // Giải nén và ghi đè nếu đã tồn tại
                    entry.Extract(outputDir, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        internal static void ShowUpdatePackageDialog(string errorMessage = "")
        {
            string message =
                "AI Engine extraction failed.\n\n" +
                "This usually means the package version is outdated or corrupted.\n\n" +
                "Please update the package:\n" +
                "1. Open Window → Package Manager\n" +
                "2. Select 'My Assets'\n" +
                "3. Find 'Auto UI Generator'\n" +
                "4. Click Update or Re-download\n\n";

            if (!string.IsNullOrEmpty(errorMessage))
                message += "Error Details:\n" + errorMessage;

            EditorUtility.DisplayDialog(
                "Auto UI Generator - Update Required",
                message,
                "Open Package Manager",
                "Close"
            );

            // Mở thẳng Package Manager
            UnityEditor.PackageManager.UI.Window.Open("My Assets");
        }

        internal static void DecryptExeStatic(string enc, string exe)
        {
            if (!File.Exists(enc)) return;

            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(enc);
                byte[] key = Convert.FromBase64String(base64Key);
                byte[] iv = Convert.FromBase64String(base64IV);

                // Cấu trúc using chuẩn cho mọi phiên bản C#
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                        // Ghi file EXE đã giải mã ra ổ cứng
                        File.WriteAllBytes(exe, decrypted);
                    }
                }

                // Xóa file .enc sau khi giải mã thành công
                File.Delete(enc);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Lỗi giải mã Engine: " + e.Message);
            }
        }

        private static bool IsEngineInstalled()
        {
            string exeName = "ui-detect" + (Application.platform == RuntimePlatform.WindowsEditor ? ".exe" : "");
            return File.Exists(Path.Combine(AppDataPath, "ui-detect", exeName));
        }
    }

    // --- AutoUIBackgroundInstaller class stays essentially the same logic-wise ---
    [InitializeOnLoad]
    public static class AutoUIBackgroundInstaller
    {
        public static float Progress = -1f;
        public static string Status = "";
        public static bool IsCompleted = false;
        public static string ResultFolder = "";
        private static CancellationTokenSource _cts;

        static AutoUIBackgroundInstaller()
        {
            EditorApplication.update += () => { if (Progress >= 0) AutoUIGeneratorWelcome.TryRepaint(); };
        }

        public static void StartInstall()
        {
            if (_cts != null) return;

            IsCompleted = false;
            ResultFolder = "";

            AutoUIGeneratorWelcome.ClearEngineData();

            _cts = new CancellationTokenSource();
            _ = Run(_cts.Token);
        }

        public static void Cancel()
        {
            _cts?.Cancel();
            _cts = null;
            Progress = -1;
        }

        static async Task Run(CancellationToken token)
        {
            try
            {
                Progress = 0.01f;
                Status = "Preparing...";

                string rootPath = AutoUIGeneratorWelcome.AppDataPath;
                Directory.CreateDirectory(rootPath);

                string zipPath = Path.Combine(rootPath, "AIEngine.zip");
                string url = AutoUIGeneratorWelcome.GetDownloadUrlStatic();

                // ===== CLEAN INVALID FILE =====
                if (File.Exists(zipPath))
                {
                    long size = new FileInfo(zipPath).Length;
                    if (size < 1024 * 1024) // <1MB = suspicious
                        File.Delete(zipPath);
                }

                long existingBytes = File.Exists(zipPath) ? new FileInfo(zipPath).Length : 0;

                // ===== DOWNLOAD =====
                Progress = 0.05f;
                Status = existingBytes > 0 ? "Resuming download..." : "Starting download...";

                // FIX: Sử dụng khối using truyền thống thay vì using declaration (C# 8.0)
                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    www.downloadHandler = new DownloadHandlerFile(zipPath, existingBytes > 0);

                    // FIX: Thuộc tính này chỉ có từ Unity 2019.3+
#if UNITY_2019_3_OR_NEWER
                    www.disposeDownloadHandlerOnDispose = true;
#endif

                    if (existingBytes > 0)
                        www.SetRequestHeader("Range", "bytes=" + existingBytes + "-");

                    var op = www.SendWebRequest();

                    const float baseProgress = 0.1f;
                    const float downloadWeight = 0.75f;

                    while (!op.isDone)
                    {
                        if (token.IsCancellationRequested)
                        {
                            www.Abort();
                            return;
                        }

                        Progress = baseProgress + (www.downloadProgress * downloadWeight);

                        long totalDownloaded = existingBytes + (long)www.downloadedBytes;
                        Status = string.Format("Downloading {0:F1} MB", totalDownloaded / 1048576f);

                        await Task.Yield();
                    }

                    // ===== NETWORK CHECK =====
                    bool isError = false;
#if UNITY_2020_1_OR_NEWER
                    isError = (www.result != UnityWebRequest.Result.Success);
#else
                    isError = (www.isNetworkError || www.isHttpError);
#endif

                    if (isError && www.responseCode != 416)
                    {
                        Debug.LogError("[AutoUI] Download failed: " + www.error);
                        if (!string.IsNullOrEmpty(www.error) && (www.error.Contains("Data") || www.error.Contains("verify")))
                        {
                            if (File.Exists(zipPath)) File.Delete(zipPath);
                        }
                        return;
                    }
                } // Giải phóng www tại đây

                // ===== EXTRACT =====
                Progress = 0.99f;
                Status = "Verifying & Extracting...";

                try
                {
                    await Task.Run(() =>
                    {
                        if (!File.Exists(zipPath)) return;

                        // FIX: Sử dụng ngoặc nhọn cho ZipFile để tương thích C# 7.3
                        using (ZipFile zip = ZipFile.Read(zipPath))
                        {
                            zip.Password = AutoUIGeneratorWelcome.zipPassWord;
                            foreach (ZipEntry entry in zip)
                            {
                                // Bảo mật: Chống lỗi Zip Slip
                                string fullPath = Path.GetFullPath(Path.Combine(rootPath, entry.FileName));
                                if (!fullPath.StartsWith(Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase))
                                    continue;

                                entry.Extract(rootPath, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }

                        File.Delete(zipPath);

                        string engineDir = Path.Combine(rootPath, "ui-detect");
                        string enc = Path.Combine(engineDir, "ui-detect.exe.enc");
                        string exe = Path.Combine(engineDir, "ui-detect.exe");

                        if (File.Exists(enc))
                            AutoUIGeneratorWelcome.DecryptExeStatic(enc, exe);

                        ResultFolder = engineDir;
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError("[AutoUI] Extraction Error: " + e.Message);

                    if (File.Exists(zipPath))
                        File.Delete(zipPath);

                    Progress = -1;

                    // 👉 Hiển thị hướng dẫn update
                    AutoUIGeneratorWelcome.ShowUpdatePackageDialog(e.Message);

                    return;
                }

                IsCompleted = true;
                Progress = 1f;
                Status = "Success!";
                await Task.Delay(5000);
            }
            catch (OperationCanceledException)
            {
                Status = "Cancelled";
            }
            catch (Exception e)
            {
                Debug.LogError("[AutoUI] Install failed: " + e);
                Status = "Failed";
                string zipPath = Path.Combine(AutoUIGeneratorWelcome.AppDataPath, "AIEngine.zip");
                if (File.Exists(zipPath)) File.Delete(zipPath);
            }
            finally
            {
                Progress = -1f;
                _cts = null;
            }
        }
    }
}