using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class KaizenAppManager : MonoBehaviour
    {

        public UIDocument RootDocument;
        private VisualElement _root;
        private FloorPlan _floorPlan;

        static KaizenAppManager _instance;
        private void Awake()
        {
            if(_instance != null)
            {
                Destroy(this);
            }
            else
            {
                _instance = this;
            }

            _root = RootDocument.rootVisualElement;
            InitializeFloorPlan();
        }

        private void InitializeFloorPlan()
        {
           _floorPlan = new FloorPlan(_root);
        }
    }

}
