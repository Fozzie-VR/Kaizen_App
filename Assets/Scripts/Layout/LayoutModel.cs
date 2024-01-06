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

        private Dictionary<int, IconModelInfo> _icons = new Dictionary<int, IconModelInfo>();
        private int _nextIconId = 0;

        CommandHandler _commandHandler;
        
        //this class will hold all the info about the layout; icons, floor size, pixels per meter, etc.
        //will need to listen to events from view classes to update info
        //will issue commands to view classes to update view
        public LayoutModel()
        {
            _commandHandler = new CommandHandler(UndoRedoView.UNDO_EVENT, UndoRedoView.REDO_EVENT);
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            EventManager.StartListening(FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
            EventManager.StartListening(IconSpawner.ICON_SPAWN_REQUESTED, OnIconSpawned);
            EventManager.StartListening(IconMover.ICON_MOVED_EVENT, OnIconMoved);
            EventManager.StartListening(LayoutView.ICON_OFF_FLOOR_EVENT, OnIconOffFloor);
            EventManager.StartListening(SelectionInspector.ROTATION_CHANGED_EVENT, OnRotationChanged);
            EventManager.StartListening(SelectionInspector.ICON_DIMENSIONS_CHANGED_EVENT, OnIconDimensionsChanged);
            EventManager.StartListening(MoveIconCommand.ICON_MOVE_COMMAND_UNDO, OnMoveIconUndo);
            EventManager.StartListening(RotateIconCommand.ICON_ROTATE_COMMAND_UNDO, OnRotateIconUndo);
            EventManager.StartListening(ResizeIconCommand.UNDO_RESIZE_ICON_COMMAND, OnResizeIconUndo);

        }

       

        private void OnRotationChanged(Dictionary<string, object> evntArgs)
        {
            IconViewInfo iconViewInfo = (IconViewInfo)evntArgs[SelectionInspector.ROTATION_CHANGED_EVENT_KEY];
            int iconID = iconViewInfo.iconID;
            IconViewInfo oldIconViewInfo = ConvertModelInfoToViewInfo(_icons[iconID]);
            RotateIconCommand rotateIconCommand = new RotateIconCommand(iconViewInfo, oldIconViewInfo);
            _commandHandler.AddCommand(rotateIconCommand);
            IconModelInfo iconInfo = _icons[iconID];
            UpdateModelInfoFromView(iconViewInfo, iconInfo);

        }

        private void OnRotateIconUndo(Dictionary<string, object> dictionary)
        {
           //update model info
            IconViewInfo iconViewInfo = (IconViewInfo)dictionary[RotateIconCommand.ICON_ROTATE_COMMAND_UNDO_KEY];
            int iconID = iconViewInfo.iconID;
            IconModelInfo iconInfo = _icons[iconID];
            UpdateModelInfoFromView(iconViewInfo, iconInfo);
        }

        private void OnIconDimensionsChanged(Dictionary<string, object> evntArgs)
        {
            IconViewInfo iconViewInfo = (IconViewInfo)evntArgs[SelectionInspector.ICON_DIMENSIONS_CHANGED_EVENT_KEY];
            int iconID = iconViewInfo.iconID;
            IconViewInfo oldIconViewInfo = ConvertModelInfoToViewInfo(_icons[iconID]);
            ResizeIconCommand resizeIconCommand = new ResizeIconCommand(iconViewInfo, oldIconViewInfo);
            _commandHandler.AddCommand(resizeIconCommand);
            IconModelInfo iconInfo = _icons[iconID];
            UpdateModelInfoFromView(iconViewInfo, iconInfo);
            
        }

        private void OnResizeIconUndo(Dictionary<string, object> dictionary)
        {
            IconViewInfo iconViewInfo = (IconViewInfo)dictionary[ResizeIconCommand.UNDO_RESIZE_ICON_COMMAND_KEY];
            int iconID = iconViewInfo.iconID;
            IconModelInfo iconInfo = _icons[iconID];
            UpdateModelInfoFromView(iconViewInfo, iconInfo);
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
            int iconWidth = (int)objects[3];
            int iconHeight = (int)objects[4];
            int id = _nextIconId;
            _nextIconId++;
            //create icon info
            IconModelInfo iconInfo = new IconModelInfo
            {
                IconType = iconType,
                IconID = id,
                Height = iconHeight,
                Width = iconWidth,
                Position = position,
                LocalPosition = localPosition,
                RotationAngle = 0,
                IsActive = true
            };
            
            _icons.Add(id, iconInfo);

            IconViewInfo iconViewInfo = ConvertModelInfoToViewInfo(iconInfo);

            //object[] iconSpawnedArgs = new object[] { id, iconType, position, localPosition, iconHeight, iconWidth};
            AddIconCommand addIconCommand = new AddIconCommand(iconViewInfo);
            _commandHandler.AddCommand(addIconCommand);

        }

        private void OnIconMoved(Dictionary<string, object> eventArgs)
        {
            //object[] objects = (object[])eventArgs[IconMover.ICON_MOVED_EVENT_KEY];
            IconViewInfo iconViewInfo = (IconViewInfo)eventArgs[IconMover.ICON_MOVED_EVENT_KEY];
            int iconID = iconViewInfo.iconID;
           
            IconViewInfo oldIconViewInfo = ConvertModelInfoToViewInfo(_icons[iconID]);

            
            //issue move command for views
            MoveIconCommand moveIconCommand = new MoveIconCommand(iconViewInfo, oldIconViewInfo);
            _commandHandler.AddCommand(moveIconCommand);
            //use id to update icon info
            IconModelInfo iconInfo = _icons[iconID];
            UpdateModelInfoFromView(iconViewInfo, iconInfo);
            
            //_icons[iconID] = iconInfo;
        }

        private void OnMoveIconUndo(Dictionary<string, object> evntArgs)
        {
            //update model info
            IconViewInfo iconViewInfo = (IconViewInfo)evntArgs[MoveIconCommand.ICON_MOVE_COMMAND_UNDO_KEY];
            int iconID = iconViewInfo.iconID;
            IconModelInfo iconInfo = _icons[iconID];
            UpdateModelInfoFromView(iconViewInfo, iconInfo);
        }

        private void OnIconOffFloor(Dictionary<string, object> eventArgs)
        {
            object eventData = eventArgs[LayoutView.ICON_OFF_FLOOR_EVENT_KEY];
            int iconID = (int)eventData;
            //change active state of icon
            var floorIconInfo = _icons[iconID];
            floorIconInfo.IsActive = false;
            //issue remove command for views with same args as for add command
            IconViewInfo iconViewInfo = ConvertModelInfoToViewInfo(floorIconInfo);
            
            RemoveIconCommand removeIconCommand = new RemoveIconCommand(iconViewInfo);
            _commandHandler.AddCommand(removeIconCommand);
        }
       

        private void UpdateModelInfoFromView(IconViewInfo iconViewInfo, IconModelInfo iconModelInfo)
        {
            IconModelInfo iconInfo = new IconModelInfo 
            { 
                IconID = iconViewInfo.iconID,
                IconType = iconViewInfo.IconType,
                Height = iconViewInfo.Height,
                Width = iconViewInfo.Width,
                Position = iconViewInfo.Position,
                LocalPosition = iconViewInfo.LocalPosition,
                RotationAngle = iconViewInfo.RotationAngle,
                IsActive = true
            };
            _icons[iconInfo.IconID] = iconInfo;

        }

        private IconViewInfo ConvertModelInfoToViewInfo(IconModelInfo iconModelInfo)
        {
            IconViewInfo iconViewInfo = new IconViewInfo();
            iconViewInfo.iconID = iconModelInfo.IconID;
            iconViewInfo.IconType = iconModelInfo.IconType;
            iconViewInfo.Height = iconModelInfo.Height;
            iconViewInfo.Width = iconModelInfo.Width;
            iconViewInfo.Position = iconModelInfo.Position;
            iconViewInfo.LocalPosition = iconModelInfo.LocalPosition;
            iconViewInfo.RotationAngle = iconModelInfo.RotationAngle;
            return iconViewInfo;
        }

        //incorporate Icon info here...
        private struct IconModelInfo
        {
            public int IconID;
            public IconType IconType;
            public int Height;
            public int Width;
            public Vector3 Position;
            public Vector2 LocalPosition;
            public float RotationAngle;
            public bool IsActive;
        }
    }

    public class IconViewInfo
    {
        public int iconID;
        public IconType IconType;
        public int Height;
        public int Width;
        public Vector3 Position;
        public Vector2 LocalPosition;
        public float RotationAngle;
    }

}
