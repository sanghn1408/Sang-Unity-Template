using UnityEngine;

namespace GameUp.Core.Helpers
{
    public static class LocalStorageUtils
    {
        public static bool GetBoolean(string key)
        {
            return PlayerPrefs.GetInt(key, 0) != 0;
        }

        public static void SetBoolean(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
