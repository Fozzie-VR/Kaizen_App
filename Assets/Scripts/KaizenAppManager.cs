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

        private const string RESET_FLOOR_PLAN_EVENT = "reset_floor_plan";

        private const string FLOOR_DIMENSIONS_SET_EVENT = "floor_dimensions_set";
        private const string FLOOR_DIMENSIONS_SET_EVENT_KEY = "floor_dimensions";

        private const string ICONS_CONTAINER = "ve_icons";
        private const string BACK_BUTTON = "btn_back";
        private const string NEXT_BUTTON = "btn_next";

        public UIDocument RootDocument;
        public UIDocument FloorPlannerDocument;
        public UIDocument LayoutComparisonDocument;
        public UIDocument FloorDimensionsInputDocument;
        public UIDocument LandingPageDocument;
        [SerializeField] private VisualTreeAsset RootContainer;
        [SerializeField] private VisualTreeAsset FloorPlannerTree;
        [SerializeField] private VisualTreeAsset LandingPageTree;
        [SerializeField] private VisualTreeAsset FloorDimensionsInputPageTree;
        [SerializeField] private VisualTreeAsset LayoutComparisonTree;
        private VisualElement _root;
        private VisualElement _landingPageContainer;
        private VisualElement _floorPlannerContainer;
        private VisualElement _layoutComparisonContainer;
        private VisualElement _floorDimensionsContainer;
        private VisualElement _iconsContainer;
        private Button _nextButton;
        private Button _backButton;
        private Label _layoutHeaderText;

        private FloorDimensionsPage _floorDimensionsPage;
        private FloorDimensions _floorDimensions;
        
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
        private bool _isComparisonLayout = false;

        public Texture2D PreKaizenLayout;
        public Texture2D PostKaizenLayout;

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
            LandingPageDocument.enabled = true;
            FloorDimensionsInputDocument.enabled = false;
            FloorPlannerDocument.enabled = false;
            LayoutComparisonDocument.enabled = false;
            RootDocument.enabled = false;

            //RootDocument.visualTreeAsset = LandingPageTree;
            _root = LandingPageDocument.rootVisualElement;
            Button preKaizenLayout = _root.Q<Button>("btn_pre_kaizen_layout");
            preKaizenLayout.clicked += OnPreKaizenLayoutClicked;

            PhotoIconController photoIconController = new PhotoIconController(_root);
            
        }

        private void OnPreKaizenLayoutClicked()
        {
            FloorDimensionsInputDocument.enabled = true;
            LandingPageDocument.enabled = false;
            LandingPageDocument.enabled = false;
            FloorPlannerDocument.enabled = false;

            //RootDocument.visualTreeAsset = FloorDimensionsInputPageTree;
            _root = FloorDimensionsInputDocument.rootVisualElement;
            _floorDimensionsPage = new FloorDimensionsPage(_root);
            //Button finishedButton = _root.Q<Button>("btn_finished");
            //finishedButton.clicked += OnFinishedClicked;
            EventManager.StartListening(FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
        }

        private void OnFloorDimensionsSet(Dictionary<string, object> dictionary)
        {
            _floorDimensions = (FloorDimensions)dictionary[FLOOR_DIMENSIONS_SET_EVENT_KEY];
            OnFloorInputFinished();
        }

        private void OnFloorInputFinished()
        {
            //RootDocument.visualTreeAsset = FloorPlannerTree;
            FloorDimensionsInputDocument.enabled = false;
            FloorPlannerDocument.enabled = true;


            _root = FloorPlannerDocument.rootVisualElement;
            _layoutHeaderText = _root.Q<Label>("lbl_layout_header");
            _nextButton = _root.Q<Button>(NEXT_BUTTON);
            _backButton = _root.Q<Button>(BACK_BUTTON);
            _backButton.AddToClassList("hidden");
            _nextButton.clicked += OnNextClicked;
            _backButton.clicked += OnBackClicked;
            _iconsContainer = _root.Q<VisualElement>(ICONS_CONTAINER);
            if(_floorPlanner is null)
            {
                Debug.Log("Floor planner is null");
                InitializeLayoutTool();
            }
            else
            {
                Debug.Log("Floor planner is not null");
                //_floorPlanner.ResetFloorPlan();
            }
        }


        private void OnNextClicked()
        {
            if(_isPostKaizenLayout == false)
            {
                _isPostKaizenLayout = true;
                _layoutHeaderText.text = "Post-Kaizen Layout";
                StartCoroutine(CapturePreKaizenLayout());
            }
            else
            {
                _isPostKaizenLayout = false;
                _isComparisonLayout = true;
                StartCoroutine(CapturePostKaizenLayout());

                //_iconsContainer.AddToClassList("hidden");
                //_layoutHeaderText.text = "Layout Comparison";
                //Debug.Log("compare event triggered");
                //EventManager.TriggerEvent(COMPARE_LAYOUTS_EVENT, new Dictionary<string, object>());
            }
        }

        private void OnBackClicked()
        {             
            if (_isPostKaizenLayout == true)
            {
                _isPostKaizenLayout = false;
                _layoutHeaderText.text = "Pre-Kaizen Layout";
                StartCoroutine(CapturePreKaizenLayout());
                
            }
            else if(_isPostKaizenLayout == false && ! _isComparisonLayout)
            {
                _isPostKaizenLayout = true;
            }
            else if(_isComparisonLayout)
            { 
                _isPostKaizenLayout = true;
                //EventManager.TriggerEvent(RESET_FLOOR_PLAN_EVENT, new Dictionary<string, object>());
                return;
            }
            EventManager.TriggerEvent(SWITCH_KAIZEN_LAYOUT_CLICKED, new Dictionary<string, object> { { POST_KAIZEN_LAYOUT_EVENT_KEY, _isPostKaizenLayout } });
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
           _floorPlanner = new FloorPlanner(_root, _floorDimensions);
        }

        private void InitializeIconSpawner()
        {
            _iconSpawner = new IconSpawner(_root);
        }

        private void InitializeLayoutComparisonPage()
        {
            FloorPlannerDocument.sortingOrder = 0;
            LayoutComparisonDocument.enabled = true;
            LayoutComparisonDocument.sortingOrder = 1;
            //RootDocument.visualTreeAsset = LayoutComparisonTree;
            _root = LayoutComparisonDocument.rootVisualElement;
            LayoutComparisonPage layoutComparisonPage = new LayoutComparisonPage(_root);
        }

        private IEnumerator CapturePreKaizenLayout()
        {
            _floorPlanner.Floor.style.backgroundImage = null;
            yield return new WaitForEndOfFrame();
            float rootWidth = _root.resolvedStyle.width;
            float rootHeight = _root.resolvedStyle.height;  
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float scaleFactor = FloorPlannerDocument.panelSettings.scale;
            Debug.Log("scal factor: " + scaleFactor);

            float widthMultiplier = screenWidth / rootWidth;
            float heightMultiplier = screenHeight / rootHeight;


            PreKaizenLayout = ScreenCapturer.GetScreenCapturer(_floorPlanner.Floor, widthMultiplier, heightMultiplier);
            yield return null;
            EventManager.TriggerEvent(SWITCH_KAIZEN_LAYOUT_CLICKED, new Dictionary<string, object> { { POST_KAIZEN_LAYOUT_EVENT_KEY, _isPostKaizenLayout } });
        }

        private IEnumerator CapturePostKaizenLayout()
        {
            _floorPlanner.Floor.style.backgroundImage = null;
            yield return new WaitForEndOfFrame();
            float rootWidth = _root.resolvedStyle.width;
            float rootHeight = _root.resolvedStyle.height;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float scaleFactor = FloorPlannerDocument.panelSettings.scale;


            float widthMultiplier = screenWidth / rootWidth;
            float heightMultiplier = screenHeight / rootHeight;
            PostKaizenLayout = ScreenCapturer.GetScreenCapturer(_floorPlanner.Floor, widthMultiplier, heightMultiplier);
            yield return null;
            InitializeLayoutComparisonPage();
        }

        public void BackFromComparisonPage()
        {
            _isComparisonLayout = false;
            _isPostKaizenLayout = true;
            //OnFloorInputFinished();
            FloorPlannerDocument.sortingOrder = 1;
            LayoutComparisonDocument.sortingOrder = 0;
            //_layoutHeaderText.text = "Post-Kaizen Layout";
            EventManager.TriggerEvent(SWITCH_KAIZEN_LAYOUT_CLICKED, new Dictionary<string, object> { { POST_KAIZEN_LAYOUT_EVENT_KEY, _isPostKaizenLayout } });
        }
    }

}
