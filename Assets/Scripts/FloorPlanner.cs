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
        private const string START_DRAG = "ve_icon_container";
        private const string ICON_SPAWNED_EVENT = "IconSpawned";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";

        private const string ICON_REMOVED_EVENT = "IconRemoved";

        private const string ICON_IMAGE = "ve_icon_image";
        private const string FLOOR = "ve_layout_area";

        private const string ICON_IMAGE_STYLE = "icon_image";
        private const string ICON_CONTAINER_STYLE = "icon_container";
        private const string ICON_LABEL_STYLE = "icon_label";
    

        private VisualElement _floor;
        private VisualElement _floorInspector;
        private FloatField _floorHeight;
        private FloatField _floorWidth;

        private List<FloorIcon> _floorIcons = new();

        private Texture2D _gridTexture;


        public FloorPlanner(VisualElement root)
        {
            _floor = root.Q(FLOOR);
            _floor.style.width = 1024;
            _floor.style.height = 480;
            //_floor.RegisterCallback<OnEnable>(SetDimensions);
            _floorInspector = root.Q("ve_floor_specs");
            _floorHeight = _floorInspector.Q<FloatField>("float_floor_height");
            _floorWidth = _floorInspector.Q<FloatField>("float_floor_width");
          
            //KaizenAppManager._instance.KaizenEvents.FloorIconSpawned += AddIcon;
            EventManager.StartListening(ICON_SPAWNED_EVENT, AddIcon);
            EventManager.StartListening(ICON_REMOVED_EVENT, RemoveIcon);
            _floor.RegisterCallback<GeometryChangedEvent>(OnFloorGeometryChanged);
            

        }

        private void OnFloorGeometryChanged(GeometryChangedEvent evt)
        {
            //Debug.Log("floor width = " + _floor.resolvedStyle.width);
            _floorHeight.value = _floor.resolvedStyle.height;
            _floorWidth.value = _floor.resolvedStyle.width;
            DrawGrid();
        }

        private void SetDimensions()
        {
            
        }

        public void AddIcon(Dictionary<string, object> message)
        {
            var icon = (FloorIcon)message[FLOOR_ICON_EVENT_KEY];
            _floorIcons.Add(icon);
            Debug.Log("number of floor icons = " + _floorIcons.Count);
        }

        public void RemoveIcon(Dictionary<string, object> message)
        {
            var icon = (FloorIcon)message[FLOOR_ICON_EVENT_KEY];
            _floorIcons.Remove(icon);
            Debug.Log("number of floor icons = " + _floorIcons.Count);
        }

        //method to create grid texture with black background and white lines

        private void DrawGrid()
        {
            //grid should be size of floor
            
            int gridWidth = Mathf.RoundToInt(_floorWidth.value);
            int gridRowWidth = gridWidth/8;
            int numberOfColumns = (int)_floor.resolvedStyle.height / gridRowWidth;
            int floorHeight = numberOfColumns * gridRowWidth;
            _floor.style.height = floorHeight;


            _gridTexture = new Texture2D(gridWidth, gridRowWidth);
            Debug.Log("grid width = " + gridWidth + " grid row width = " + gridRowWidth);
            
            //create array of colors to fill texture
            Color[] pixels = _gridTexture.GetPixels();
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }   

            for (int y = 0; y < gridRowWidth; y++)
            {
                
                for (int x = 0; x < gridWidth; x++)
                {
                    
                    if (x % gridRowWidth == 0 && y % 2 == 0  || y % gridRowWidth == 0 && x % 2 == 0 || x == gridWidth - 1 && y % 2 == 0)
                    {
                        pixels[x + y * gridWidth] = Color.black;
                        Debug.Log(x + y * gridWidth);
                    }

                    //if(x == 0 && y% 2 == 0 || x == gridWidth - 1 && y % 2 == 0 || y == 0 && x % 2 == 0 || y == gridRowWidth - 1 && x % 2 == 0)
                    //{
                    //    pixels[x + y * gridWidth] = Color.black;
                    //}
                }
            }

            _gridTexture.SetPixels(pixels);
            _gridTexture.Apply();
            _floor.style.backgroundImage = _gridTexture;
            _floor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            _floor.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.Repeat);
            


        }
       
    }
}

