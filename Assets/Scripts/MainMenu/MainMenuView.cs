using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{

    
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] UIDocument _mainMenuDocument;
        [SerializeField] StyleSheet _mainMenuStyles;
        [SerializeField] float _validateFloat;


        private void Start()
        {
            StartCoroutine(Generate());
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            StartCoroutine(Generate());
        }

        private IEnumerator Generate()
        {
            yield return null;
            var root = _mainMenuDocument.rootVisualElement;
            root.Clear();
            root.styleSheets.Add(_mainMenuStyles);

            var container = Create("container");    

            var viewBox = Create("view-box", "bordered-box");
            
            container.Add(viewBox);
            var controlBox = Create("control-box", "bordered-box");

            var spinButton = Create<Button>();
            spinButton.text = "Spin";
            controlBox.Add(spinButton);

            var scaleSlider = Create<Slider>();
            controlBox.Add(scaleSlider);

            container.Add(controlBox);

            root.Add(container);

        }

        private VisualElement Create(params string[] className)
        {
            var element = new VisualElement();
            foreach (var name in className)
            {
                element.AddToClassList(name);
            }
            return element;
        }

        private T Create<T>(params string[] className) where T : VisualElement, new()
        {
            var element = new T();
            foreach (var name in className)
            {
                element.AddToClassList(name);
            }
            
            return new T();
        }


    }

}
