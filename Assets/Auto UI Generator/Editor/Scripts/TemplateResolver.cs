using UnityEditor;
using UnityEngine;

namespace VFF
{
    public static class TemplateResolver
    {
        public static GameObject LoadTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
                return null;

            string[] guids = AssetDatabase.FindAssets(templateName + " t:Prefab");

            if (guids.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}
