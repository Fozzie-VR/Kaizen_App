using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class LayoutModel 
    {
        public const string ICON_ADDED_EVENT = "iconAdded";
        public const string ICON_ADDED_EVENT_KEY = "iconAddedKey";
        public const string ICON_REMOVED_EVENT = "iconRemoved";
        public const string ICON_REMOVED_EVENT_KEY = "iconRemovedKey";

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

        private Dictionary<int, FloorIconInfo> _icons = new Dictionary<int, FloorIconInfo>();
        private int _nextIconId = 0;

        CommandHandler _commandHandler;
        
        //this class will hold all the info about the layout; icons, floor size, pixels per meter, etc.
        //will need to listen to events from view classes to update info
        //will issue commands to view classes to update view
        public LayoutModel()
        {
            _commandHandler = new CommandHandler();
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            EventManager.StartListening(FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
            EventManager.StartListening(IconSpawner.ICON_SPAWN_REQUESTED, OnIconSpawned);
            EventManager.StartListening(IconMover.ICON_MOVED_EVENT, OnIconMoved);
            EventManager.StartListening(LayoutView.ICON_OFF_FLOOR_EVENT, OnIconOffFloor);
        }
       
        private void OnFloorDimensionsSet(Dictionary<string, object> evntMessage)
        {
            FloorDimensions floorDimensions = 
                (FloorDimensions)evntMessage[FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT_KEY];

            FloorDimensions oldDimensions = new FloorDimensions 
            { 
                FloorWidthMeters = _floorWidthMeters, 
                FloorHeightMeters = _floorHeightMeters 
            };
            _floorWidthMeters = floorDimensions.FloorWidthMeters;
            _floorHeightMeters = floorDimensions.FloorHeightMeters;

            SetFloorSizeCommand setFloorSizeCommand = new SetFloorSizeCommand(floorDimensions, oldDimensions);
            _commandHandler.AddCommand(setFloorSizeCommand);
        }

        private void OnIconSpawned(Dictionary<string, object> evntArgs)
        {
            object[] objects = (object[])evntArgs[IconSpawner.ICON_SPAWN_REQUESTED_KEY];
            IconType iconType = (IconType)objects[0];
            Vector3 position = (Vector3)objects[1];
            Vector2 localPosition = (Vector2)objects[2];
            int iconHeight = (int)objects[3];
            int iconWidth = (int)objects[4];
            int id = _nextIconId;
            _nextIconId++;
            //create icon info
            FloorIconInfo iconInfo = new FloorIconInfo
            {
                IconType = iconType,
                iconID = id,
                Height = iconHeight,
                Width = iconWidth,
                Position = position,
                LocalPosition = localPosition,
                RotationAngle = 0,
                IsActive = true
            };
            //set vars
            _icons.Add(id, iconInfo);

            Debug.Log("LayoutModel received icon spawned event for icon " + iconType);
            object[] iconSpawnedArgs = new object[] { id, iconType, position, localPosition, iconHeight, iconWidth};
            AddIconCommand addIconCommand = new AddIconCommand(iconSpawnedArgs);
            _commandHandler.AddCommand(addIconCommand);
        }

        private void OnIconMoved(Dictionary<string, object> eventArgs)
        {
            object[] objects = (object[])eventArgs[IconMover.ICON_MOVED_EVENT_KEY];
            int iconID = (int)objects[0];
            Vector3 position = (Vector3)objects[1];
            Vector2 localPosition = (Vector2)objects[2];
            Vector2 oldLocalPosition = _icons[iconID].LocalPosition;
            Vector3 oldPosition = _icons[iconID].Position;
            //issue move command for views
            MoveIconCommand moveIconCommand = new MoveIconCommand(iconID, position, localPosition, oldPosition, oldLocalPosition);
            _commandHandler.AddCommand(moveIconCommand);
            //use id to update icon info
            FloorIconInfo iconInfo = _icons[iconID];
            iconInfo.Position = position;
            iconInfo.LocalPosition = localPosition;
            _icons[iconID] = iconInfo;
        }

        private void OnIconOffFloor(Dictionary<string, object> eventArgs)
        {
            object eventData = eventArgs[LayoutView.ICON_OFF_FLOOR_EVENT_KEY];
            int iconID = (int)eventData;
            //change active state of icon
            var floorIconInfo = _icons[iconID];
            floorIconInfo.IsActive = false;
            //issue remove command for views with same args as for add command

            object[] iconRemovedArgs = new object[] {
                iconID, floorIconInfo.IconType, floorIconInfo.Position, floorIconInfo.LocalPosition, floorIconInfo.Height, floorIconInfo.Width };
            RemoveIconCommand removeIconCommand = new RemoveIconCommand(iconRemovedArgs);
            _commandHandler.AddCommand(removeIconCommand);
        }

        //incorporate Icon info here...
        private struct FloorIconInfo
        {
            public int iconID;
            public IconType IconType;
            public int Height;
            public int Width;
            public Vector3 Position;
            public Vector2 LocalPosition;
            public float RotationAngle;
            public bool IsActive;
        }
    }

}
