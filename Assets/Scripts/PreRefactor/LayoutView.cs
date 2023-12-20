
using System;
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
    public class LayoutView: IView
    {
        private const string DRAG_AREA = "ve_floor_plan_screen";
        private const string FLOOR_PARENT = "ve_layout_container";
        private const string FLOOR = "ve_floor";
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
        private const float DEFAULT_FLOOR_WIDTH = 5; //meters
        private const float DEFAULT_FLOOR_HEIGHT = 5; //meters

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

       
        private VisualElement _floor;
        public VisualElement Floor => _floor;
        private VisualElement _container;

        private VisualElement _floorInspector;
        private FloatField _floorHeight;
        private FloatField _floorWidth;

        private IconFactory<VisualElement> _iconFactory = new IconFactory<VisualElement>();

        private List<VisualElement> _iconsOnFloor = new List<VisualElement>();
        
        public LayoutView()
        {
            _iconFactory.Factory += GetIcon;
            _iconFactory.PreReturn += ReturnIcon;

            EventManager.StartListening(GridDrawer.GRID_DRAWN_EVENT, OnGridDrawn);
            EventManager.StartListening(AddIconCommand.ADD_ICON_COMMAND, OnAddIcon);
            EventManager.StartListening(FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
        }

        public LayoutView(VisualElement root, FloorDimensions floorDimensions)
        {
            
            _container = root.Q(FLOOR_PARENT);
            _floor = root.Q(FLOOR);
            
            if(floorDimensions.FloorWidthMeters != 0 && floorDimensions.FloorHeightMeters != 0)
            {
                SetFloorDimensions(floorDimensions);
            }
            else
            {
                _floorHeightMeters = DEFAULT_FLOOR_HEIGHT;
                _floorWidthMeters = DEFAULT_FLOOR_WIDTH;
                SetFloorDimensions(floorDimensions);
            }
           

            EventManager.StartListening(GridDrawer.GRID_DRAWN_EVENT, OnGridDrawn);
            EventManager.StartListening(AddIconCommand.ADD_ICON_COMMAND, OnAddIcon);
            EventManager.StartListening(FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
            _container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        }

        private void OnFloorDimensionsSet(Dictionary<string, object> eventArgs)
        {
            //get floor dimensions from event args
            FloorDimensions floorDimensions = (FloorDimensions)eventArgs[FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT_KEY];
           
        }

        private void SetFloorDimensions(FloorDimensions floorDimensions)
        {
            _floorHeight = _floorInspector.Q<FloatField>("float_floor_height");
            _floorWidth = _floorInspector.Q<FloatField>("float_floor_width");

            _floorHeight.RegisterCallback<FocusOutEvent>(ChangeFloorHeight);
            _floorWidth.RegisterCallback<FocusOutEvent>(ChangeFloorWidth);

            _floorWidthMeters = floorDimensions.FloorWidthMeters;
            _floorHeightMeters = floorDimensions.FloorHeightMeters;
            Debug.Log("LayoutView floor width: " + _floorWidthMeters); ;
            Debug.Log("LayoutView floor height: " + _floorHeightMeters);

            _floorHeight.SetValueWithoutNotify(_floorHeightMeters);
            _floorWidth.SetValueWithoutNotify(_floorWidthMeters);

            _pixelsPerMeter = _maxPixelsPerMeter;
        }

       

        private void OnGridDrawn(Dictionary<string, object> eventArgs)
        {
            object args = eventArgs[GridDrawer.GRID_DRAWN_EVENT_KEY];
            Texture2D gridTexture = (Texture2D)args;
            
            _floor.style.backgroundImage = gridTexture;
            _floor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

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
            Debug.Log(_maxPixelsPerMeter);
            Debug.Log(_minPixelsPerMeter);
            //KaizenAppManager.Instance.DefaultPixelsPerMeter = _maxPixelsPerMeter;
            SetPixelsPerMeter();
            Debug.Log("pixels per meter: " + _pixelsPerMeter);
            int heightPixels = Mathf.RoundToInt(_pixelsPerMeter * _floorHeightMeters);
            _floor.style.height = heightPixels;
            _floor.style.width = heightPixels;
           
            int widthPixels = heightPixels;
            int pixelsPerMeter = Mathf.RoundToInt(widthPixels / _floorWidthMeters);

            float heightMeters = heightPixels / pixelsPerMeter;
            float widthMeters = widthPixels / pixelsPerMeter;

            Debug.Log("height meters: " + heightMeters);
            Debug.Log("width meters: " + widthMeters);
            GridDrawer gridDrawer = new GridDrawer();
            gridDrawer.DrawGrid(widthMeters, heightMeters, pixelsPerMeter);
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

            float heightMeters = _pixelsPerMeter * _floorHeightMeters;
            _floor.style.height = heightMeters;

            float widthMeters = _pixelsPerMeter * _floorWidthMeters;
            _floor.style.width = widthMeters;

            //DrawGrid();
            GridDrawer gridDrawer = new GridDrawer();
            gridDrawer.DrawGrid(widthMeters, heightMeters, _pixelsPerMeter);
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
            //DrawGrid();
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
       

        //floor icons are data that should be kept in a model class
        //This should be callback from command issued by layout model
        //Callback will generate an icon and place it on the floor
        public void OnAddIcon(Dictionary<string, object> evntArgs)
        {
            //unpack event args
            object[] args = (object[])evntArgs[AddIconCommand.ADD_ICON_COMMAND_KEY];
            int id = (int)args[0];
            IconType iconType = (IconType)args[1];
            Vector3 position = (Vector3)args[2];
            Vector3 localPosition = (Vector3)args[3];
            int iconHeight = (int)args[4];
            int iconWidth = (int)args[5];

            //check if icon is inside the floor container
            if(!IconIsOnFloor(localPosition, iconWidth, iconHeight))
            {
                return;
            }

            VisualElement icon = GetIcon();
            icon.style.width = iconWidth;
            icon.style.height = iconHeight;
            icon.style.position = new StyleEnum<Position>(Position.Absolute);
            icon.style.left = localPosition.x;
            icon.style.top = localPosition.y;
            string iconStyleClass = GetFloorIconContainerStyle(iconType);
            icon.AddToClassList(iconStyleClass);

            //_floor.Add(icon);
        }

        public void RemoveIcon(Dictionary<string, object> message)
        {
            object[] args = (object[])message[RemoveIconCommand.REMOVE_ICON_COMMAND_KEY];
            int id = (int)args[0];
        }

        private void ReturnIcon(VisualElement element)
        {
            
        }

        private VisualElement GetIcon()
        {
            VisualElement iconContainer = new VisualElement();
            iconContainer.style.position = new StyleEnum<Position>(Position.Absolute);
            iconContainer.name = "Floor_Icon";

            iconContainer.usageHints = UsageHints.DynamicTransform;

            _container.Add(iconContainer);
            return iconContainer;
        }

        private string GetFloorIconContainerStyle(IconType iconType)
        {
            switch (iconType)
            {
                case IconType.CustomItem:
                    return "floor_custom_item";
                case IconType.CustomLabel:
                    return "floor_custom_label";
                case IconType.ProductFlow:
                    return "floor_product_flow";
                case IconType.WorkerMovement:
                    return "floor_worker_movement";
                case IconType.TransportFlow:
                    return "floor_transport_flow";
                case IconType.Product:
                    return "floor_product";
                case IconType.Kanban:
                    return "floor_kanban";
                case IconType.PartsShelf:
                    return "floor_parts_shelf";
                case IconType.Table:
                    return "floor_table";
                case IconType.Worker:
                    return "floor_worker";
                case IconType.Machine:
                    return "floor_machine";
                case IconType.Trolley:
                    return "floor_trolley";
                case IconType.Conveyor:
                    return "floor_conveyor";
                case IconType.Photo:
                    return "floor_photo";
                default:
                    return "floor_table";
            }
        }

        private bool IconIsOnFloor(Vector2 iconPosition, int width, int height)
        {
            bool floorContainsIcon = _floor.ContainsPoint(iconPosition);

            float xOffset = width / 2;
            float yOffset = height / 2;

            if (iconPosition.x - xOffset < 0)
            {
                floorContainsIcon = false;
            }

            if (iconPosition.x + xOffset > _floor.resolvedStyle.width)
            {
                floorContainsIcon = false;
            }

            if (iconPosition.y - yOffset < 0)
            {
                floorContainsIcon = false;
            }

            if (iconPosition.y + yOffset > _floor.resolvedStyle.height)
            {
                floorContainsIcon = false;
            }

            return floorContainsIcon;
        }

        //
        private void OnSwitchKaizenLayoutClicked(Dictionary<string, object> switchEvent)
        {
            
            _isPostKaizenLayout = (bool)switchEvent[POST_KAIZEN_LAYOUT_EVENT_KEY];
            //Debug.Log("switching layouts");
            //DisplayIconsForCurrentLayout();
            
        }

        //private void DisplayIconsForCurrentLayout()
        //{
        //    if (_isPostKaizenLayout)
        //    {
        //        _postKaizenFloor.style.height = _preKaizenFloor.style.height.value;
        //        _postKaizenFloor.style.width = _preKaizenFloor.style.width.value;
        //        _postKaizenFloor.style.backgroundImage = _gridTexture;
        //        _postKaizenFloor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        //        _floor = _postKaizenFloor;
        //        Debug.Log("floor width = " + _floor.style.width.value);

        //        if (_postKaizenFloorIcons.Count <= 0 && _preKaizenFloorIcons.Count > 0)
        //        {
        //            foreach (var floorIcon in _preKaizenFloorIcons)
        //            {
        //                EventManager.TriggerEvent(ICON_CLONE_EVENT, new Dictionary<string, object> { { ICON_CLONE_EVENT_KEY, floorIcon.IconInfo.IconElement } });
        //            }
        //        }

        //        foreach (var floorIcon in _postKaizenFloorIcons)
        //        {
        //            floorIcon.IconInfo.IconElement.RemoveFromClassList("hidden");
        //        }

        //        foreach (var floorIcon in _preKaizenFloorIcons)
        //        {
        //            floorIcon.IconInfo.IconElement.AddToClassList("hidden");
        //        }

        //        _preKaizenFloor.AddToClassList("hidden");
        //        _postKaizenFloor.RemoveFromClassList("hidden");

        //    }
        //    else
        //    {
        //        _postKaizenFloor.AddToClassList("hidden");
        //        _preKaizenFloor.RemoveFromClassList("hidden");

        //        _floor = _preKaizenFloor;
        //        _floor.style.backgroundImage = _gridTexture;
        //        _floor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        //        foreach (var floorIcon in _postKaizenFloorIcons)
        //        {
        //            floorIcon.IconInfo.IconElement.AddToClassList("hidden");
        //        }

        //        foreach (var floorIcon in _preKaizenFloorIcons)
        //        {
        //            floorIcon.IconInfo.IconElement.RemoveFromClassList("hidden");
        //        }
        //    }
        //}

        //need to move the icon data to a model class and use it to make the comparizon view
        //private void ComparePrePostLayouts(Dictionary<string, object> message)
        //{
        //    _comparingLayouts = true;
        //    _preKaizenFloor.RemoveFromClassList("hidden");

        //    SetFloorSizesForComparison();
        //    //_preKaizenFloor.AddToClassList("comparison_container");
        //    //_postKaizenFloor.AddToClassList("comparison_container");
        //    //need to resize the floor containers to be the same size

        //    //all icons will need to reposition themselves inside the respective floor containers
        //    foreach (var floorIcon in _preKaizenFloorIcons)
        //    {
        //        VisualElement icon = floorIcon.IconInfo.IconElement;
        //        icon.RemoveFromClassList("hidden");
        //        _preKaizenFloor.Add(icon);
        //        icon.transform.position = Vector3.zero;
        //        float xPos = floorIcon.IconInfo.LocalPosition.x / 2;
        //        float yPos = floorIcon.IconInfo.LocalPosition.y / 2;
        //        xPos -= floorIcon.IconInfo.Width / 2;
        //        yPos -= floorIcon.IconInfo.Height / 2;
        //        icon.style.translate = new Translate(xPos, yPos);

        //        //floorIcon.RescaleIcon();
        //        Debug.Log("pre kaizen icon position: " + icon.transform.position);
        //    }

        //    foreach (var floorIcon in _postKaizenFloorIcons)
        //    {
        //        VisualElement icon = floorIcon.IconInfo.IconElement;
        //        icon.RemoveFromClassList("hidden");
        //        _postKaizenFloor.Add(icon);
        //        icon.transform.position = Vector3.zero;
        //        float xPos = floorIcon.IconInfo.LocalPosition.x / 2;
        //        float yPos = floorIcon.IconInfo.LocalPosition.y / 2;
        //        xPos -= floorIcon.IconInfo.Width / 2;
        //        yPos -= floorIcon.IconInfo.Height / 2;
        //        icon.style.translate = new Translate(xPos, yPos);
        //        Debug.Log("post kaizen icon position: " + icon.transform.position);
        //    }
        //}

        private void SetFloorSizesForComparison()
        {
            _pixelsPerMeter = Mathf.RoundToInt(_pixelsPerMeter / 2);
            //Debug.Log(_maxPixelsPerMeter);
            //Debug.Log(_minPixelsPerMeter);
            //KaizenAppManager._instance.DefaultPixelsPerMeter = _maxPixelsPerMeter;
            //SetPixelsPerMeter();

            int heightPixels = Mathf.RoundToInt(_pixelsPerMeter * _floorHeightMeters);
            _floor.style.height = heightPixels;
            _floor.style.width = heightPixels;
           
            GridDrawer gridDrawer = new GridDrawer();
            gridDrawer.DrawGrid(heightPixels, heightPixels, _pixelsPerMeter);

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
            _floor.style.height = heightPixels;
            _floor.style.width = heightPixels;
            
            GridDrawer gridDrawer = new GridDrawer();
            gridDrawer.DrawGrid(heightPixels, heightPixels, _pixelsPerMeter);

            EventManager.TriggerEvent(PIXELS_PER_METER_EVENT, new Dictionary<string, object> { { PIXELS_PER_METER_EVENT_KEY, _pixelsPerMeter } });
        }
    }

    
}

