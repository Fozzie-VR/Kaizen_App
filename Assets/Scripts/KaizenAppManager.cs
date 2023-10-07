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
        [SerializeField] private VisualTreeAsset FloorPlanner;
        [SerializeField] private VisualTreeAsset LandingPage;
        private VisualElement _root;
        
        private FloorPlanner _floorPlan;
        private IconSpawner _iconSpawner;
        
        public KaizenEvents KaizenEvents;
        private EventManager _eventManager;
        private SelectionInspector _selectionInspector;

        private int _maxPixelsPerMeter;
        public int DefaultPixelsPerMeter {
            get { return _maxPixelsPerMeter; }
            set { _maxPixelsPerMeter = value; } }
     
        public int PixelsPerMeter => _floorPlan.PixelsPerMeter;
        public float FloorWidthMeters => _floorPlan.FloorWidthMeters;
        public float FloorHeightMeters => _floorPlan.FloorHeightMeters;

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

            RootDocument.visualTreeAsset = LandingPage;
            _root = RootDocument.rootVisualElement;
            Button preKaizenLayout = _root.Q<Button>("btn_pre_kaizen_layout");
            preKaizenLayout.clicked += () => { 
                
                RootDocument.visualTreeAsset = FloorPlanner;
                _root = RootDocument.rootVisualElement;
                InitializeLayoutTool(); 
            
            };

           
            //InitializeFloorPlan();
           
            
        }

        private void InitializeLayoutTool()
        {
            InitializeIconSpawner();
            InitializeEventManager();
            InitializeFloorPlan();
            InitializeSelectionInspector();
        }

        private void InitializeSelectionInspector()
        {
            _selectionInspector = new SelectionInspector(_root);
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
