using System;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
#if SHOW_DEBUG
    [SerializeField] bool m_showDebug = false;
    [SerializeField] bool m_showFPS = false;
    [SerializeField] bool m_showQuality = false;
    [SerializeField] bool m_showCameraSize = false;
    [SerializeField] bool m_showScreenSize = false;
    [SerializeField] bool m_showResolutionSize = false;

    float m_deltaTime = 0.0f;
    string m_qualityName;

    Vector2 m_screen;
    Vector2 m_camera;
    string m_resolution = String.Empty;

    Rect m_rect;
    Rect m_rect1;
    GUIStyle m_style;
    GUIStyle m_style1;
#endif

    void Start()
    {
#if SHOW_DEBUG
        m_qualityName = QualitySettings.names[QualitySettings.GetQualityLevel()];
        m_screen.x = Screen.width;
        m_screen.y = Screen.height;
        if (Camera.main)
        {
            m_camera.x = Camera.main.pixelWidth;
            m_camera.y = Camera.main.pixelHeight;
        }
        m_resolution = Screen.currentResolution.ToString();
        m_rect = new Rect(0, 0, m_screen.x, m_screen.y * 3 / 100);
        m_rect1 = new Rect(0, 0, m_screen.x * 15 / 100, m_screen.y * 3 / 100);

        m_style = new GUIStyle();
        m_style.alignment = TextAnchor.UpperRight;
        m_style.fontSize = (int)m_screen.y * 3 / 100;
        m_style.fontStyle = FontStyle.Bold;
        m_style.normal.textColor = Color.red;

        m_style1 = new GUIStyle();
        m_style1.alignment = TextAnchor.UpperLeft;
#endif
    }

    void Update()
    {
#if SHOW_DEBUG
        if (m_showDebug && Time.timeScale > 0f)
        {
            m_deltaTime += (Time.deltaTime - m_deltaTime) * 0.1f;
        }
#endif
    }

    void OnGUI()
    {
#if SHOW_DEBUG
        if (m_showDebug)
        {
            float msec = m_deltaTime * 1000.0f;
            float fps = 1.0f / m_deltaTime;
            string text = string.Empty;

            if (m_showFPS)
            {
                //text += string.Format("{0:0.0}ms - {1:0.0}fps\n", msec, fps);
                text += string.Format("{0:0.0}fps\n", fps);
            }
            if (m_showQuality)
            {
                text += string.Format("quality {0}\n", m_qualityName);
            }

            if (m_showCameraSize)
            {
                text += string.Format("camera\t= ({0} x {1})\n", m_camera.x, m_camera.y);
            }

            if (m_showScreenSize)
            {
                text += string.Format("screen\t= ({0} x {1})\n", m_screen.x, m_screen.y);
            }

            if (m_showResolutionSize)
            {
                text += string.Format("resolution \t= ({0})\n", m_resolution);
            }

            GUI.Label(m_rect, text, m_style);
            //if (GUI.Button(m_rect1, "Reload"))
            //{
            //    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            //}
        }
#endif
    }
}