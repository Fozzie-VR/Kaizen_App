using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class LayoutModel 
    {
        //in meters
        private const float DEFAULT_FLOOR_WIDTH = 4; 
        private const float DEFAULT_FLOOR_HEIGHT = 4; 

        private int _minPixelsPerMeter = 32;
        //heigt/width
        private int _maxPixelsPerMeter = 384;
        
        private float _floorWidthMeters;
        public float FloorWidthMeters => _floorWidthMeters;

        private float _floorHeightMeters;
        public float FloorHeightMeters => _floorHeightMeters;

        private int _pixelsPerMeter;
        public int PixelsPerMeter => _pixelsPerMeter;

        private Dictionary<int, LayoutIconInfo> _icons = new Dictionary<int, LayoutIconInfo>();
        private int _nextIconId = 0;

        CommandHandler _commandHandler;
        
        //this class will hold all the info about the layout; icons, floor size, pixels per meter, etc.
        //will need to listen to events from view classes to update info
        //will issue commands to view classes to update view
        public LayoutModel()
        {
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            EventManager.StartListening(FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
            EventManager.StartListening(IconSpawner.ICON_SPAWNED_EVENT, OnIconSpawned);
        }

        private void OnFloorDimensionsSet(Dictionary<string, object> evntMessage)
        {
            FloorDimensions floorDimensions = 
                (FloorDimensions)evntMessage[FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT_KEY];

            _floorWidthMeters = floorDimensions.FloorWidthMeters;
            _floorHeightMeters = floorDimensions.FloorHeightMeters;
        }

        private void OnIconSpawned(Dictionary<string, object> evntArgs)
        {
            

            object[] objects = (object[])evntArgs[IconSpawner.ICON_SPAWNED_EVENT_KEY];
            IconInfo icon = (IconInfo)objects[0];
            Vector3 position = (Vector3)objects[1];
            int id = _nextIconId;
            _nextIconId++;
            icon.Id = id;
            LayoutIconInfo iconInfo = new LayoutIconInfo { 
                IconModel = icon,
                Position = position,
                RotationAngle = 0f
            };

            _icons.Add(id, iconInfo);

            Debug.Log("LayoutModel received icon spawned event for icon " + icon.Type);

        }

        private struct LayoutIconInfo
        {
            public IconInfo IconModel;
            public Vector3 Position;
            public float RotationAngle;
        }
    }

}
