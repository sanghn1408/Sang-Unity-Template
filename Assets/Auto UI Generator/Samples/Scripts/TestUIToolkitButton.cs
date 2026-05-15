using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VFF
{
    [RequireComponent(typeof(UIDocument))]
    public class TestUIToolkitButton : MonoBehaviour
    {
        [SerializeField] string buttonName = "helloButton";

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            var button = root.Q<Button>(buttonName);

            button.clicked += () =>
            {
                Debug.Log("Hello World!");
            };
        }
    }
}
