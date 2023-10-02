using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


namespace KaizenApp
{
    //Manages layout of icons on the visual element that represents the floor
    //Handles pointer events related to the floor plan
    //Keeps track of icons on the floor
    //Keeps floor measurements and handles scaling
    //could refactor to remove Pointer events related to the icons to a separate class

    public class FloorPlanner

    {
        private const string FLOOR_PARENT = "ve_layout_container";
        private const string FLOOR = "ve_layout_area";
        private const string SCROLL_VIEW = "scroll_layout_area";

        private const string ICON_SPAWNED_EVENT = "IconSpawned";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";
        private const string ICON_REMOVED_EVENT = "IconRemoved";

        private const string PIXELS_PER_METER_EVENT = "PixelsPerMeterChanged";
        private const string PIXELS_PER_METER_EVENT_KEY = "pixelsPerMeter";

        private const float DEFAULT_FLOOR_WIDTH = 4; //meters
        private const float DEFAULT_FLOOR_HEIGHT = 4; //meters
        private int _minPixelsPerMeter = 32;
        private int _maxPixelsPerMeter = 384;//height/width
        private float _floorWidthMeters;
        public float FloorWidthMeters => _floorWidthMeters;
       
        private float _floorHeightMeters;
        public float FloorHeightMeters => _floorHeightMeters;
      
        private int _pixelsPerMeter;
        public int PixelsPerMeter => _pixelsPerMeter;

        private VisualElement _floor;
        private VisualElement _container;
        private ScrollView _scrollView;
        private VisualElement _floorInspector;
        private FloatField _floorHeight;
        private FloatField _floorWidth;

        private List<FloorIcon> _floorIcons = new();

        private Texture2D _gridTexture;

        public FloorPlanner(VisualElement root)
        {
            _floor = root.Q(FLOOR);
            _container = root.Q(FLOOR_PARENT); 
            _scrollView = root.Q<ScrollView>(SCROLL_VIEW);
           
            //_floor.style.height = _container.style.height;
            //_floor.style.width = 1024;
            //_floor.RegisterCallback<OnEnable>(SetDimensions);
            _floorInspector = root.Q("ve_floor_specs");
            _floorHeight = _floorInspector.Q<FloatField>("float_floor_height");
            _floorWidth = _floorInspector.Q<FloatField>("float_floor_width");

            _floorHeight.RegisterCallback<FocusOutEvent>(ChangeFloorHeight);
            _floorWidth.RegisterCallback<FocusOutEvent>(ChangeFloorWidth);

            _floorWidthMeters = DEFAULT_FLOOR_WIDTH;
            _floorHeightMeters = DEFAULT_FLOOR_HEIGHT;
            
            _pixelsPerMeter = _maxPixelsPerMeter;

          
            //KaizenAppManager._instance.KaizenEvents.FloorIconSpawned += AddIcon;
            EventManager.StartListening(ICON_SPAWNED_EVENT, AddIcon);
            EventManager.StartListening(ICON_REMOVED_EVENT, RemoveIcon);
            _container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
            
            
        }
        private void OnContainerGeometryChanged(GeometryChangedEvent evt)
        {
            Debug.Log(_container.resolvedStyle.width);
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
            Debug.Log(_maxPixelsPerMeter);
            Debug.Log(_minPixelsPerMeter);
            KaizenAppManager._instance.DefaultPixelsPerMeter = _maxPixelsPerMeter;
            SetPixelsPerMeter();

            int heightPixels = Mathf.RoundToInt(_pixelsPerMeter * _floorHeightMeters);
            _floor.style.height = heightPixels;
            _floor.style.width = heightPixels;
            int widthPixels = heightPixels;
            int pixelsPerMeter = Mathf.RoundToInt(widthPixels / _floorWidthMeters);

            float heightMeters = heightPixels / pixelsPerMeter;
            float widthMeters = widthPixels / pixelsPerMeter;
            _floorHeight.SetValueWithoutNotify(heightMeters);
            _floorWidth.SetValueWithoutNotify(widthMeters);
            DrawGrid();
        }

        private void ChangeFloorHeight(FocusOutEvent evt)
        {
            if(_floorHeightMeters == _floorHeight.value)
            {
                return;
            }
            _floorHeightMeters = _floorHeight.value;
            SetPixelsPerMeter();
           
            _floor.style.height = _pixelsPerMeter * _floorHeightMeters;
            _floor.style.width = _pixelsPerMeter * _floorWidthMeters;

            DrawGrid();
        }

        private void ChangeFloorWidth(FocusOutEvent evt)
        {
            if(_floorWidthMeters == _floorWidth.value)
            {
                return;
            }
            _floorWidthMeters = _floorWidth.value;
           SetPixelsPerMeter();


            _floor.style.width = _pixelsPerMeter * _floorWidthMeters;
            _floor.style.height = _pixelsPerMeter * _floorHeightMeters;
            DrawGrid();
        }

        private void SetPixelsPerMeter()
        {
            _floorHeightMeters = _floorHeight.value;
            _floorWidthMeters = _floorWidth.value;
            Debug.Log(_container.resolvedStyle.width);
            if(_container.resolvedStyle.width > _container.resolvedStyle.height)
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
       
        

        private void DrawGrid()
        {
            //int gridWidth = Mathf.RoundToInt(_floor.resolvedStyle.width);
            int gridWidth = Mathf.RoundToInt(_floorWidthMeters * _pixelsPerMeter);
            
            int gridHeight = Mathf.RoundToInt(_floorHeightMeters * _pixelsPerMeter);
            
            _gridTexture = new Texture2D(gridWidth, gridHeight);
            
            //create array of colors to fill texture
            Color[] pixels = _gridTexture.GetPixels();
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }   

            for (int y = 0; y < gridHeight; y++)
            {
                
                for (int x = 0; x < gridWidth; x++)
                {
                    if (x % _pixelsPerMeter == 0 && y % 2 == 0  || y % _pixelsPerMeter == 0 && x % 2 == 0 || y == gridHeight - 1 && x % 2 == 0 || x == gridWidth - 1 && y % 2 == 0)
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
            //_floor.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);

        }


        public void AddIcon(Dictionary<string, object> message)
        {
            var icon = (FloorIcon)message[FLOOR_ICON_EVENT_KEY];
            _floorIcons.Add(icon);
        }

        public void RemoveIcon(Dictionary<string, object> message)
        {
            var icon = (FloorIcon)message[FLOOR_ICON_EVENT_KEY];
            _floorIcons.Remove(icon);
        }

    }
}

