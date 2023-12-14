
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace KaizenApp
{
    //Manages layout of icons on the visual element that represents the floor
    //Handles pointer events related to the floor plan
    //Keeps track of icons on the floor
    //Keeps floor measurements and handles scaling
    //could refactor to remove Pointer events related to the icons to a separate class


    //need to turn this into a view class that just handles the floor plan UI
    public class FloorPlanner: IView
    {
        private const string DRAG_AREA = "ve_floor_plan_screen";
        private const string FLOOR_PARENT = "ve_layout_container";
        private const string PRE_KAIZEN_FLOOR = "ve_pre_kaizen_layout_area";
        private const string POST_KAIZEN_FLOOR = "ve_post_kaizen_layout_area";
        private const string SCROLL_VIEW = "scroll_layout_area";


        private const string ICON_SPAWNED_EVENT = "IconSpawned";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";
        private const string ICON_REMOVED_EVENT = "IconRemoved";

        private const string SWITCH_KAIZEN_LAYOUT_CLICKED = "post_kaizen_layout_clicked";
        private const string POST_KAIZEN_LAYOUT_EVENT_KEY = "post_kaizen";

        private const string RESET_FLOOR_PLAN_EVENT = "reset_floor_plan";

        private const string PIXELS_PER_METER_EVENT = "PixelsPerMeterChanged";
        private const string PIXELS_PER_METER_EVENT_KEY = "pixelsPerMeter";

        private const string ICON_CLONE_EVENT = "SpawnIcon";
        private const string ICON_CLONE_EVENT_KEY = "icon";

        private const string COMPARE_LAYOUTS_EVENT = "compare_layouts";

        public const string FLOOR_HEIGHT_CHANGED_EVENT = "floor_height_changed";
        public const string FLOOR_HEIGHT_CHANGED_EVENT_KEY = "floor_height";

        public const string FLOOR_WIDTH_CHANGED_EVENT = "floor_width_changed";
        public const string FLOOR_WIDTH_CHANGED_EVENT_KEY = "floor_width";

        //all of these floats and ints should be in a model class
        private const float DEFAULT_FLOOR_WIDTH = 4; //meters
        private const float DEFAULT_FLOOR_HEIGHT = 4; //meters

        private bool _isPostKaizenLayout = false;
        private bool _comparingLayouts = false;

        private int _minPixelsPerMeter = 32;
        private int _maxPixelsPerMeter = 384;//height/width
        private float _floorWidthMeters;
        public float FloorWidthMeters => _floorWidthMeters;

        private float _floorHeightMeters;
        public float FloorHeightMeters => _floorHeightMeters;

        private int _pixelsPerMeter;
        public int PixelsPerMeter => _pixelsPerMeter;

        private VisualElement _preKaizenFloor;
        private VisualElement _postKaizenFloor;
        private VisualElement _floor;
        public VisualElement Floor => _floor;
        private VisualElement _container;

        private VisualElement _floorInspector;
        private FloatField _floorHeight;
        private FloatField _floorWidth;

        //all floor icon data should be moved to a model class
        private List<FloorIcon> _preKaizenFloorIcons = new();
        public List<FloorIcon> PreKaizenFloorIcons => _preKaizenFloorIcons;

        private List<FloorIcon> _postKaizenFloorIcons = new();
        public List<FloorIcon> PostKaizenFloorIcons => _postKaizenFloorIcons;

        //grid texture should be in another class
        private Texture2D _gridTexture;


        
        public FloorPlanner(VisualElement root, FloorDimensions floorDimensions)
        {
            _preKaizenFloor = root.Q(PRE_KAIZEN_FLOOR);
            _postKaizenFloor = root.Q(POST_KAIZEN_FLOOR);
            //_dragArea = root.Q(DRAG_AREA);
            _postKaizenFloor.AddToClassList("hidden");
            _floor = _preKaizenFloor;
            _container = root.Q(FLOOR_PARENT);
            //_scrollView = root.Q<ScrollView>(SCROLL_VIEW);


            _floorInspector = root.Q("ve_floor_specs");
            _floorHeight = _floorInspector.Q<FloatField>("float_floor_height");
            _floorWidth = _floorInspector.Q<FloatField>("float_floor_width");

            _floorHeight.RegisterCallback<FocusOutEvent>(ChangeFloorHeight);
            _floorWidth.RegisterCallback<FocusOutEvent>(ChangeFloorWidth);

            _floorWidthMeters = floorDimensions.FloorWidthMeters;
            _floorHeightMeters = floorDimensions.FloorHeightMeters;

            _floorHeight.SetValueWithoutNotify(_floorHeightMeters);
            _floorWidth.SetValueWithoutNotify(_floorWidthMeters);

            _pixelsPerMeter = _maxPixelsPerMeter;

            //KaizenAppManager._instance.KaizenEvents.FloorIconSpawned += AddIcon;
            EventManager.StartListening(ICON_SPAWNED_EVENT, AddIcon);
            EventManager.StartListening(ICON_REMOVED_EVENT, RemoveIcon);
            EventManager.StartListening(SWITCH_KAIZEN_LAYOUT_CLICKED, OnSwitchKaizenLayoutClicked);
            //EventManager.StartListening(COMPARE_LAYOUTS_EVENT, ComparePrePostLayouts);
            //EventManager.StartListening(RESET_FLOOR_PLAN_EVENT, ResetFloorSizes);
            _container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        }

        private void OnContainerGeometryChanged(GeometryChangedEvent evt)
        {
            if (_container.resolvedStyle.width > _container.resolvedStyle.height)
            {
                _maxPixelsPerMeter = Mathf.RoundToInt(_container.resolvedStyle.height / 5);
                _minPixelsPerMeter = Mathf.RoundToInt(_container.resolvedStyle.height / 10);
            }
            else
            {
                _maxPixelsPerMeter = Mathf.RoundToInt(_container.resolvedStyle.width / 5);
                _minPixelsPerMeter = Mathf.RoundToInt(_container.resolvedStyle.width / 10);
            }
            //Debug.Log(_maxPixelsPerMeter);
            //Debug.Log(_minPixelsPerMeter);
            KaizenAppManager.Instance.DefaultPixelsPerMeter = _maxPixelsPerMeter;
            SetPixelsPerMeter();

            int heightPixels = Mathf.RoundToInt(_pixelsPerMeter * _floorHeightMeters);
            _preKaizenFloor.style.height = heightPixels;
            _preKaizenFloor.style.width = heightPixels;
            _postKaizenFloor.style.height = heightPixels;
            _postKaizenFloor.style.width = heightPixels;
            int widthPixels = heightPixels;
            int pixelsPerMeter = Mathf.RoundToInt(widthPixels / _floorWidthMeters);

            float heightMeters = heightPixels / pixelsPerMeter;
            float widthMeters = widthPixels / pixelsPerMeter;

            DrawGrid();
        }

        private void ChangeFloorHeight(FocusOutEvent evt)
        {
            if (_floorHeightMeters == _floorHeight.value)
            {
                return;
            }
            EventManager.TriggerEvent(FLOOR_HEIGHT_CHANGED_EVENT, 
                new Dictionary<string, object> { { FLOOR_HEIGHT_CHANGED_EVENT_KEY, _floorHeight.value } });

            _floorHeightMeters = _floorHeight.value;
            SetPixelsPerMeter();

            _floor.style.height = _pixelsPerMeter * _floorHeightMeters;
            _floor.style.width = _pixelsPerMeter * _floorWidthMeters;

            DrawGrid();
        }

        private void ChangeFloorWidth(FocusOutEvent evt)
        {
            if (_floorWidthMeters == _floorWidth.value)
            {
                return;
            }
            EventManager.TriggerEvent(FLOOR_WIDTH_CHANGED_EVENT, 
                new Dictionary<string, object> { { FLOOR_WIDTH_CHANGED_EVENT_KEY, _floorWidth.value } });
            
            _floorWidthMeters = _floorWidth.value;
            SetPixelsPerMeter();

            _floor.style.width = _pixelsPerMeter * _floorWidthMeters;
            _floor.style.height = _pixelsPerMeter * _floorHeightMeters;
            DrawGrid();
        }

        //need this to scale the floor plan in different contexts (comparison view, etc.)
        private void SetPixelsPerMeter()
        {
            _floorHeightMeters = _floorHeight.value;
            _floorWidthMeters = _floorWidth.value;
            //Debug.Log(_container.resolvedStyle.width);
            if (_container.resolvedStyle.width > _container.resolvedStyle.height)
            {
                float pixels = _floorHeightMeters * _pixelsPerMeter;
                if (pixels > _container.resolvedStyle.height - 8)
                {
                    if (Mathf.RoundToInt((_container.resolvedStyle.height - 8) / _floorHeightMeters) >= _minPixelsPerMeter)
                    {
                        _pixelsPerMeter = Mathf.RoundToInt((_container.resolvedStyle.height - 8) / _floorHeightMeters);
                    }
                    else
                    {
                        _pixelsPerMeter = _minPixelsPerMeter;
                    }

                }
                else
                {
                    if (Mathf.RoundToInt((_container.resolvedStyle.height - 8) / _floorHeightMeters) <= _maxPixelsPerMeter)
                    {
                        _pixelsPerMeter = Mathf.RoundToInt((_container.resolvedStyle.height - 8) / _floorHeightMeters);
                    }
                    else
                    {
                        _pixelsPerMeter = _maxPixelsPerMeter;
                    }

                }

            }
            else
            {
                float pixels = _floorWidthMeters * _pixelsPerMeter;
                if (pixels > _container.resolvedStyle.width - 8)
                {
                    if (Mathf.RoundToInt((_container.resolvedStyle.width - 8) / _floorHeightMeters) >= _minPixelsPerMeter)
                    {
                        _pixelsPerMeter = Mathf.RoundToInt((_container.resolvedStyle.width - 8) / _floorWidthMeters);
                    }
                    else
                    {
                        _pixelsPerMeter = _minPixelsPerMeter;
                    }
                }
                else
                {
                    if (Mathf.RoundToInt((_container.resolvedStyle.width - 8) / _floorHeightMeters) <= _maxPixelsPerMeter)
                    {
                        _pixelsPerMeter = Mathf.RoundToInt((_container.resolvedStyle.width - 8) / _floorWidthMeters);
                    }
                    else
                    {
                        _pixelsPerMeter = _maxPixelsPerMeter;
                    }
                }
            }
            EventManager.TriggerEvent(PIXELS_PER_METER_EVENT, new Dictionary<string, object> { { PIXELS_PER_METER_EVENT_KEY, _pixelsPerMeter } });
        }


        //could refactor grid drawing to a separate class
        private void DrawGrid()
        {
            //int gridWidth = Mathf.RoundToInt(_floor.resolvedStyle.width);
            int gridWidth = Mathf.RoundToInt(_floorWidthMeters * _pixelsPerMeter);

            int gridHeight = Mathf.RoundToInt(_floorHeightMeters * _pixelsPerMeter);

            _gridTexture = new Texture2D(gridWidth, gridHeight);

            //create array of colors to fill texture
            Color[] pixels = _gridTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            for (int y = 0; y < gridHeight; y++)
            {

                for (int x = 0; x < gridWidth; x++)
                {
                    if (x % _pixelsPerMeter == 0 && y % 2 == 0 || y % _pixelsPerMeter == 0 && x % 2 == 0 || y == gridHeight - 1 && x % 2 == 0 || x == gridWidth - 1 && y % 2 == 0)
                    {
                        pixels[x + y * gridWidth] = Color.black;
                        //Debug.Log(x + y * gridWidth);
                    }

                }
            }

            _gridTexture.SetPixels(pixels);
            _gridTexture.Apply();
            _floor.style.backgroundImage = _gridTexture;
            _floor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

        }


        //floor icons are data that should be kept in a model class
        public void AddIcon(Dictionary<string, object> message)
        {
            var icon = (FloorIcon)message[FLOOR_ICON_EVENT_KEY];
            if (_isPostKaizenLayout)
            {
                _postKaizenFloorIcons.Add(icon);

            }
            else
            {
                _preKaizenFloorIcons.Add(icon);

            }

        }

        public void RemoveIcon(Dictionary<string, object> message)
        {
            var icon = (FloorIcon)message[FLOOR_ICON_EVENT_KEY];
            if (_isPostKaizenLayout)
            {
                _postKaizenFloorIcons.Remove(icon);

            }
            else
            {
                _preKaizenFloorIcons.Remove(icon);

            }
        }

        //
        private void OnSwitchKaizenLayoutClicked(Dictionary<string, object> switchEvent)
        {
            
            _isPostKaizenLayout = (bool)switchEvent[POST_KAIZEN_LAYOUT_EVENT_KEY];
            //Debug.Log("switching layouts");
            DisplayIconsForCurrentLayout();
            
        }

        private void DisplayIconsForCurrentLayout()
        {
            if (_isPostKaizenLayout)
            {
                _postKaizenFloor.style.height = _preKaizenFloor.style.height.value;
                _postKaizenFloor.style.width = _preKaizenFloor.style.width.value;
                _postKaizenFloor.style.backgroundImage = _gridTexture;
                _postKaizenFloor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                _floor = _postKaizenFloor;
                Debug.Log("floor width = " + _floor.style.width.value);

                if (_postKaizenFloorIcons.Count <= 0 && _preKaizenFloorIcons.Count > 0)
                {
                    foreach (var floorIcon in _preKaizenFloorIcons)
                    {
                        EventManager.TriggerEvent(ICON_CLONE_EVENT, new Dictionary<string, object> { { ICON_CLONE_EVENT_KEY, floorIcon.IconInfo.IconElement } });
                    }
                }

                foreach (var floorIcon in _postKaizenFloorIcons)
                {
                    floorIcon.IconInfo.IconElement.RemoveFromClassList("hidden");
                }

                foreach (var floorIcon in _preKaizenFloorIcons)
                {
                    floorIcon.IconInfo.IconElement.AddToClassList("hidden");
                }

                _preKaizenFloor.AddToClassList("hidden");
                _postKaizenFloor.RemoveFromClassList("hidden");

            }
            else
            {
                _postKaizenFloor.AddToClassList("hidden");
                _preKaizenFloor.RemoveFromClassList("hidden");

                _floor = _preKaizenFloor;
                _floor.style.backgroundImage = _gridTexture;
                _floor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                foreach (var floorIcon in _postKaizenFloorIcons)
                {
                    floorIcon.IconInfo.IconElement.AddToClassList("hidden");
                }

                foreach (var floorIcon in _preKaizenFloorIcons)
                {
                    floorIcon.IconInfo.IconElement.RemoveFromClassList("hidden");
                }
            }
        }

        //need to move the icon data to a model class and use it to make the comparizon view
        private void ComparePrePostLayouts(Dictionary<string, object> message)
        {
            _comparingLayouts = true;
            _preKaizenFloor.RemoveFromClassList("hidden");

            SetFloorSizesForComparison();
            //_preKaizenFloor.AddToClassList("comparison_container");
            //_postKaizenFloor.AddToClassList("comparison_container");
            //need to resize the floor containers to be the same size

            //all icons will need to reposition themselves inside the respective floor containers
            foreach (var floorIcon in _preKaizenFloorIcons)
            {
                VisualElement icon = floorIcon.IconInfo.IconElement;
                icon.RemoveFromClassList("hidden");
                _preKaizenFloor.Add(icon);
                icon.transform.position = Vector3.zero;
                float xPos = floorIcon.IconInfo.LocalPosition.x / 2;
                float yPos = floorIcon.IconInfo.LocalPosition.y / 2;
                xPos -= floorIcon.IconInfo.Width / 2;
                yPos -= floorIcon.IconInfo.Height / 2;
                icon.style.translate = new Translate(xPos, yPos);

                //floorIcon.RescaleIcon();
                Debug.Log("pre kaizen icon position: " + icon.transform.position);
            }

            foreach (var floorIcon in _postKaizenFloorIcons)
            {
                VisualElement icon = floorIcon.IconInfo.IconElement;
                icon.RemoveFromClassList("hidden");
                _postKaizenFloor.Add(icon);
                icon.transform.position = Vector3.zero;
                float xPos = floorIcon.IconInfo.LocalPosition.x / 2;
                float yPos = floorIcon.IconInfo.LocalPosition.y / 2;
                xPos -= floorIcon.IconInfo.Width / 2;
                yPos -= floorIcon.IconInfo.Height / 2;
                icon.style.translate = new Translate(xPos, yPos);
                Debug.Log("post kaizen icon position: " + icon.transform.position);
            }
        }

        private void SetFloorSizesForComparison()
        {
            _pixelsPerMeter = Mathf.RoundToInt(_pixelsPerMeter / 2);
            //Debug.Log(_maxPixelsPerMeter);
            //Debug.Log(_minPixelsPerMeter);
            //KaizenAppManager._instance.DefaultPixelsPerMeter = _maxPixelsPerMeter;
            //SetPixelsPerMeter();

            int heightPixels = Mathf.RoundToInt(_pixelsPerMeter * _floorHeightMeters);
            _preKaizenFloor.style.height = heightPixels;
            _preKaizenFloor.style.width = heightPixels;
            _postKaizenFloor.style.height = heightPixels;
            _postKaizenFloor.style.width = heightPixels;

            DrawGrid();
            _preKaizenFloor.style.backgroundImage = _gridTexture;
            _postKaizenFloor.style.backgroundImage = _gridTexture;

            EventManager.TriggerEvent(PIXELS_PER_METER_EVENT, new Dictionary<string, object> { { PIXELS_PER_METER_EVENT_KEY, _pixelsPerMeter } });
        }

        private void ResetFloorSizes(Dictionary<string, object> message)
        {
            _pixelsPerMeter = Mathf.RoundToInt(_pixelsPerMeter * 2);
            //Debug.Log(_maxPixelsPerMeter);
            //Debug.Log(_minPixelsPerMeter);
            //KaizenAppManager._instance.DefaultPixelsPerMeter = _maxPixelsPerMeter;
            //SetPixelsPerMeter();

            int heightPixels = Mathf.RoundToInt(_pixelsPerMeter * _floorHeightMeters);
            _preKaizenFloor.style.height = heightPixels;
            _preKaizenFloor.style.width = heightPixels;
            _postKaizenFloor.style.height = heightPixels;
            _postKaizenFloor.style.width = heightPixels;

            DrawGrid();
            _preKaizenFloor.style.backgroundImage = _gridTexture;
            _postKaizenFloor.style.backgroundImage = _gridTexture;

            EventManager.TriggerEvent(PIXELS_PER_METER_EVENT, new Dictionary<string, object> { { PIXELS_PER_METER_EVENT_KEY, _pixelsPerMeter } });
        }
    }

    
}

