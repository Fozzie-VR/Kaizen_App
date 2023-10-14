using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class KaizenAppManager : MonoBehaviour
    {
        private const string SWITCH_KAIZEN_LAYOUT_CLICKED = "post_kaizen_layout_clicked";
        private const string POST_KAIZEN_LAYOUT_EVENT_KEY = "post_kaizen";

        private const string COMPARE_LAYOUTS_EVENT = "compare_layouts";

        private const string ICONS_CONTAINER = "ve_icons";
        private const string BACK_BUTTON = "btn_back";
        private const string NEXT_BUTTON = "btn_next";

        public UIDocument RootDocument;
        [SerializeField] private VisualTreeAsset FloorPlanner;
        [SerializeField] private VisualTreeAsset LandingPage;
        [SerializeField] private VisualTreeAsset FloorDimensionsInputPage;
        private VisualElement _root;
        private VisualElement _iconsContainer;
        private Button _nextButton;
        private Button _backButton;
        private Label _layoutHeaderText;
        
        private FloorPlanner _floorPlanner;
        private IconSpawner _iconSpawner;
        
        public KaizenEvents KaizenEvents;
        private EventManager _eventManager;
        private SelectionInspector _selectionInspector;

        private int _maxPixelsPerMeter;
        public int DefaultPixelsPerMeter {
            get { return _maxPixelsPerMeter; }
            set { _maxPixelsPerMeter = value; } }
     
        public int PixelsPerMeter => _floorPlanner.PixelsPerMeter;
        public float FloorWidthMeters => _floorPlanner.FloorWidthMeters;
        public float FloorHeightMeters => _floorPlanner.FloorHeightMeters;

        private bool _isPostKaizenLayout = false;
        //public bool IsPostKaizenLayout => _isPostKaizenLayout;

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
            _isPostKaizenLayout = false;

            RootDocument.visualTreeAsset = LandingPage;
            _root = RootDocument.rootVisualElement;

            Button preKaizenLayout = _root.Q<Button>("btn_pre_kaizen_layout");
            preKaizenLayout.clicked += OnPreKaizenLayoutClicked;

           
            //InitializeFloorPlan();
           
            
        }

        private void OnPreKaizenLayoutClicked()
        {
            RootDocument.visualTreeAsset = FloorDimensionsInputPage;
            _root = RootDocument.rootVisualElement;
            Button finished = _root.Q<Button>("btn_finished");
            finished.clicked += OnFloorInputFinished;
            InitializeFloorDimensionsInputPage();
        }

        private void OnFloorInputFinished()
        {
            RootDocument.visualTreeAsset = FloorPlanner;
            _root = RootDocument.rootVisualElement;
            _layoutHeaderText = _root.Q<Label>("lbl_layout_header");
            _nextButton = _root.Q<Button>(NEXT_BUTTON);
            _backButton = _root.Q<Button>(BACK_BUTTON);
            _backButton.AddToClassList("hidden");
            _nextButton.clicked += OnNextClicked;
            _backButton.clicked += OnBackClicked;
            _iconsContainer = _root.Q<VisualElement>(ICONS_CONTAINER);
            InitializeLayoutTool();
        }


        private void OnNextClicked()
        {
            if(_isPostKaizenLayout == false)
            {
                _isPostKaizenLayout = true;
                _layoutHeaderText.text = "Post-Kaizen Layout";
                EventManager.TriggerEvent(SWITCH_KAIZEN_LAYOUT_CLICKED, new Dictionary<string, object> { { POST_KAIZEN_LAYOUT_EVENT_KEY, _isPostKaizenLayout } });
            }
            else
            {
                _isPostKaizenLayout = false;
                _iconsContainer.AddToClassList("hidden");
                _layoutHeaderText.text = "Layout Comparison";
                Debug.Log("compare event triggered");
                EventManager.TriggerEvent(COMPARE_LAYOUTS_EVENT, new Dictionary<string, object>());
            }
        }

        private void OnBackClicked()
        {             
            if (_isPostKaizenLayout == true)
            {
                _isPostKaizenLayout = false;
                _layoutHeaderText.text = "Pre-Kaizen Layout";
            }
            else
            {
                _isPostKaizenLayout = true;
                
            }
            EventManager.TriggerEvent(SWITCH_KAIZEN_LAYOUT_CLICKED, new Dictionary<string, object> { { POST_KAIZEN_LAYOUT_EVENT_KEY, _isPostKaizenLayout } });
        }

        private void InitializeFloorDimensionsInputPage()
        {
            
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
           _floorPlanner = new FloorPlanner(_root);
        }

        private void InitializeIconSpawner()
        {
            _iconSpawner = new IconSpawner(_root);
        }
    }

}
