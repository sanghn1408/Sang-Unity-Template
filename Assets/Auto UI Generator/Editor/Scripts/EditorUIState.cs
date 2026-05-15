using System.Collections.Generic;
using UnityEngine;

namespace VFF
{
    [System.Serializable]
    public class EditorUIState
    {
        public bool HasScreenshot;
        public bool HasTemplates;
        public bool HasCanvas;

        public bool IsReady => HasScreenshot && HasTemplates && HasCanvas;

        public void Update(Texture2D screenshot, List<Texture2D> templates, RectTransform root)
        {
            HasScreenshot = screenshot != null;
            HasTemplates = templates != null && templates.Count > 0 && templates.Exists(t => t != null);
            HasCanvas = root != null;
        }
    }
}
