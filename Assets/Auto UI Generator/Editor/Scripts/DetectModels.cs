using System;
using System.Collections.Generic;

namespace VFF
{
    [System.Serializable]
    public class DetectElement
    {
        public string name;
        public float x;
        public float y;
        public float z;
        public string parent;
        public float width;
        public float height;
        public string text;
        public bool is_button;
        public float scale;
    }

    [System.Serializable]
    public class DetectResult
    {
        public DetectElement[] elements;
    }

}
