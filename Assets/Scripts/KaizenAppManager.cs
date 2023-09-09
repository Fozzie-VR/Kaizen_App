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
        
        public KaizenEvents KaizenEvents;
        private EventManager _eventManager;

        public static KaizenAppManager _instance;
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
            InitializeEventManager();
            InitializeFloorPlan();
        }

        private void InitializeEventManager()
        {
            _eventManager = new EventManager();
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
