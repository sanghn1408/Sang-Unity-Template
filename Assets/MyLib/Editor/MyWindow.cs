using UnityEditor;
using UnityEngine;

public class MyWindow : EditorWindow
{
    // Add menu item named "My Window" to the Window menu
    [MenuItem("Tools/DEBUG")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow<MyWindow>("DEBUG");
    }

    void OnGUI()
    {
        EditorGUILayout.Space(25);
        GUIStyle gUIStyle = new GUIStyle();
        gUIStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.LabelField("----------DEBUG----------", gUIStyle);

        if (GUILayout.Button("DELETE ALL DATA"))
        {
            PlayerPrefs.DeleteAll();
            FileManager.DeleteAllData();
            Log.Info("Delete Data Done");
        }

        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("----------CHEAT----------", gUIStyle);

        if (GUILayout.Button("CHEAT COIN"))
        {
            GamePlayManager.I.COIN += 100000;
        }

        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("----------OTHERS----------", gUIStyle);
        if (GUILayout.Button("OPEN SCENE SELECTOR"))
        {
            SceneSelector.Open();
        }
    }
}