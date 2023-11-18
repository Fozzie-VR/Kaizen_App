using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{

    
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] UIDocument _mainMenuDocument;


        private void Start()
        {
            Generate();
        }

        private void Generate()
        {
            var root = _mainMenuDocument.rootVisualElement;
            var titleLabel = new Label("Welcome to Kaizen");
            root.Add(titleLabel);
        }
    }

}
