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
        private FloorPlanner _floorPlan;
        private IconSpawner _iconSpawner;

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
            //InitializeFloorPlan();
            InitializeIconSpawner();
        }

        private void InitializeFloorPlan()
        {
           _floorPlan = new FloorPlanner(_root);
        }

        private void InitializeIconSpawner()
        {
            _iconSpawner = new IconSpawner(_root);
        }
    }

}
