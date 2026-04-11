using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScreenShootHelper : MonoBehaviour
{
    [SerializeField] private string _path="";
    [SerializeField] private GameObject _obj=default;

    public void GenAvatarFromRT(Camera cam, int width, int height)
    {
#if UNITY_EDITOR
        RenderTexture rt = new RenderTexture(width, height, 16, RenderTextureFormat.Default);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        Texture2D virtualPhoto = new Texture2D(width, height, TextureFormat.RGBA32, false);
        virtualPhoto.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;
        cam.targetTexture = null;
        DestroyImmediate(rt);
        byte[] bytes;
        bytes = virtualPhoto.EncodeToPNG();
        string path = Application.dataPath+_path + _obj.name+".png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset("Assets/"+ _path + _obj.name + ".png");
        AssetDatabase.Refresh();

        Debug.Log("Done " + _obj.name);
#endif
    }

    [ContextMenu("Test")]
    public void Test()
    {
        GenAvatarFromRT(Camera.main, 256, 256);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ScreenShootHelper))]
public class ScreenShootEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ScreenShootHelper myScript = (ScreenShootHelper)target;

        GUILayout.Space(25);

        if (GUILayout.Button("Take Screen Shoot"))
        {
            myScript.Test();
        }
    }
}
#endif
