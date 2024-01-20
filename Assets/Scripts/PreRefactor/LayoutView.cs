
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private const string SCROLL_VIEW = "scroll_layout_area";
       
        public const string ICON_OFF_FLOOR_EVENT = "IconOffFloor";
        public const string ICON_OFF_FLOOR_EVENT_KEY = "IconOffFloorKey";

        private const string RESET_FLOOR_PLAN_EVENT = "reset_floor_plan";

        public const string PIXELS_PER_METER_EVENT = "PixelsPerMeterChanged";
        public const string PIXELS_PER_METER_EVENT_KEY = "pixelsPerMeter";


        public const string FLOOR_HEIGHT_CHANGED_EVENT = "floor_height_changed";
        public const string FLOOR_HEIGHT_CHANGED_EVENT_KEY = "floor_height";

        public const string FLOOR_WIDTH_CHANGED_EVENT = "floor_width_changed";
        public const string FLOOR_WIDTH_CHANGED_EVENT_KEY = "floor_width";

        public const string BACK_TO_KAIZEN_FORM_EVENT = "back_to_kaizen_form";

        public const string LAYOUT_CAPTURED_EVENT = "layout_captured";
        public const string LAYOUT_CAPTURED_EVENT_KEY = "layout_captured_key";

        //all of these floats and ints should be in a model class
        private const float DEFAULT_FLOOR_WIDTH = 5; //meters
        private const float DEFAULT_FLOOR_HEIGHT = 5; //meters

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
        private VisualElement _dragArea;

        private VisualElement _floorInspector;
        private FloatField _floorHeight;
        private FloatField _floorWidth;

        private Button _backButton;

        private IconFactory<VisualElement> _iconFactory = new IconFactory<VisualElement>();

        private Dictionary<int, VisualElement> _iconsOnFloor = new ();
        public Dictionary<int, VisualElement> IconsOnFloor { get => _iconsOnFloor; }

        private bool _isPreKaizenLayout = true;
        private bool _preKaizenLayoutActive = true;
        
        public LayoutView(bool isPreKaizenLayout) 
        { 
        
            _isPreKaizenLayout = isPreKaizenLayout;
            _iconFactory.Factory += GetIcon;
            _iconFactory.PreGet += PreGetIcon;
            _iconFactory.PreReturn += ReturnIcon;

            EventManager.StartListening(GridDrawer.GRID_DRAWN_EVENT, OnGridDrawn);
            EventManager.StartListening(AddIconCommand.ADD_ICON_COMMAND, OnAddIcon);
            EventManager.StartListening(SetFloorSizeCommand.FLOOR_SIZE_SET_EVENT, OnFloorDimensionsSet);
            EventManager.StartListening(MoveIconCommand.ICON_MOVE_COMMAND, OnIconMoved);
            EventManager.StartListening(RemoveIconCommand.REMOVE_ICON_COMMAND, RemoveIcon);
            EventManager.StartListening(RotateIconCommand.ICON_ROTATE_COMMAND, OnIconRotated);
            EventManager.StartListening(ResizeIconCommand.RESIZE_ICON_COMMAND, OnIconResized);
            EventManager.StartListening(KaizenFormView.POST_KAIZEN_LAYOUT_CLICKED, OnPostKaizenLayoutClicked);
            EventManager.StartListening(KaizenFormView.PRE_KAIZEN_LAYOUT_CLICKED, OnPreKaizenLayoutClicked);
        }

        

        public void BindElements(VisualElement root)
        {
            _dragArea = root.Q(DRAG_AREA);
            _container = root.Q(FLOOR_PARENT);
            _backButton = root.Q<Button>("btn_back");
            _backButton.clicked += BackButtonClicked;
            _floor = root.Q(FLOOR);
            _floorInspector = root.Q("ve_floor_specs");
            _floorHeight = _floorInspector.Q<FloatField>("float_floor_height");
            _floorWidth = _floorInspector.Q<FloatField>("float_floor_width");

            _floorHeight.RegisterCallback<FocusOutEvent>(ChangeFloorHeight);
            _floorWidth.RegisterCallback<FocusOutEvent>(ChangeFloorWidth);

            if (_floorWidthMeters != 0 && _floorHeightMeters != 0)
            {
                _floorHeight.SetValueWithoutNotify(_floorHeightMeters);
                _floorWidth.SetValueWithoutNotify(_floorWidthMeters);
                SetContainerSize();
            }
        }  
        
        private async void BackButtonClicked()
        {
           await CaptureLayout();
           EventManager.TriggerEvent(BACK_TO_KAIZEN_FORM_EVENT, null);
        }

        async Awaitable CaptureLayout()
        {
            //hide photo icons
            foreach (var icon in _iconsOnFloor)
            {
                IconViewInfo iconViewInfo = icon.Value.userData as IconViewInfo;
                if (iconViewInfo.IconType == IconType.Photo)
                {
                    icon.Value.AddToClassList("hidden");
                }
            }

            //hide grid
            _floor.style.backgroundImage = null;
            Debug.Log("hiding grid and photo icons");
            await Awaitable.EndOfFrameAsync();

            Texture2D layout = ScreenCapturer.GetScreenCapturer(_floor);
            await Task.Delay(10);
            Debug.Log("showing grid and photo icons");
            //show photo icons
            foreach (var icon in _iconsOnFloor)
            {
                IconViewInfo iconViewInfo = icon.Value.userData as IconViewInfo;
                if (iconViewInfo.IconType == IconType.Photo)
                {
                    icon.Value.RemoveFromClassList("hidden");
                }
            }
            EventManager.TriggerEvent(LAYOUT_CAPTURED_EVENT, new Dictionary<string, object> { 
                { LAYOUT_CAPTURED_EVENT_KEY, layout } 
            });


            
        }

        private void OnFloorDimensionsSet(Dictionary<string, object> eventArgs)
        {
            //get floor dimensions from event args
            FloorDimensions floorDimensions = 
                (FloorDimensions)eventArgs[SetFloorSizeCommand.FLOOR_SIZE_SET_EVENT_KEY];
            SetFloorDimensions(floorDimensions);
        }

        private void SetFloorDimensions(FloorDimensions floorDimensions)
        {
            _floorWidthMeters = floorDimensions.FloorWidthMeters;
            _floorHeightMeters = floorDimensions.FloorHeightMeters;
            _pixelsPerMeter = _maxPixelsPerMeter;

            if(_floor != null)
            {
                _floorHeight.SetValueWithoutNotify(_floorHeightMeters);
                _floorWidth.SetValueWithoutNotify(_floorWidthMeters);
                SetContainerSize();
            }
        }

        private void OnGridDrawn(Dictionary<string, object> eventArgs)
        {
            if (_preKaizenLayoutActive && !_isPreKaizenLayout)
            {
                Debug.Log("floor is null, so not drawing grid");
                return;
            }

            if(!_preKaizenLayoutActive && _isPreKaizenLayout)
            {
                Debug.Log("floor is null, so not drawing grid");
                return;
            }

            Debug.Log("OnGridDrawn");
            object args = eventArgs[GridDrawer.GRID_DRAWN_EVENT_KEY];
            Texture2D gridTexture = (Texture2D)args;
            
            
            _floor.style.backgroundImage = gridTexture;
            _floor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

        }

        private void SetContainerSize()
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
            //KaizenAppManager.Instance.DefaultPixelsPerMeter = _maxPixelsPerMeter;
            SetPixelsPerMeter();
            int heightPixels = Mathf.RoundToInt(_pixelsPerMeter * _floorHeightMeters);
            _floor.style.height = heightPixels;
            _floor.style.width = heightPixels;

            GridDrawer gridDrawer = new GridDrawer();
            gridDrawer.DrawGrid(_floorWidthMeters, _floorHeightMeters, _pixelsPerMeter);
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

            float heightPixels = _pixelsPerMeter * _floorHeightMeters;
            _floor.style.height = heightPixels;

            float widthPixels = _pixelsPerMeter * _floorWidthMeters;
            _floor.style.width = widthPixels;

            //DrawGrid();
            GridDrawer gridDrawer = new GridDrawer();
            gridDrawer.DrawGrid(_floorWidthMeters, _floorHeightMeters, _pixelsPerMeter);
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
            
            GridDrawer gridDrawer = new GridDrawer();
            gridDrawer.DrawGrid(_floorWidthMeters, _floorHeightMeters, _pixelsPerMeter);
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
            IconViewInfo iconViewInfo = evntArgs[AddIconCommand.ADD_ICON_COMMAND_KEY] as IconViewInfo;
            
            IconType iconType = iconViewInfo.IconType;
            int id = iconViewInfo.iconID;
            Vector2 localPosition = iconViewInfo.LocalPosition;
            int iconHeight = iconViewInfo.Height;
            int iconWidth = iconViewInfo.Width;

            //check if icon is inside the floor container
            if(!IconIsOnFloor(localPosition, iconWidth, iconHeight))
            {
                return;
            }

            VisualElement icon = _iconFactory.GetIcon();
            icon.userData = iconViewInfo;
            string iconStyleClass = GetFloorIconContainerStyle(iconType);
            icon.AddToClassList(iconStyleClass);
            _floor.Add(icon);
            icon.style.translate = new Translate(localPosition.x, localPosition.y);
            icon.style.height = iconHeight;
            icon.style.width = iconWidth;
            icon.RegisterCallback<GeometryChangedEvent>(OnIconGeometryChanged);
            IconMover iconMover = new IconMover(icon, _dragArea);
            _iconsOnFloor.Add(id, icon);
            EventManager.TriggerEvent(IconMover.ICON_CLICKED_EVENT, new Dictionary<string, object> { { IconMover.ICON_CLICKED_EVENT_KEY, iconViewInfo } });
        }

        private void OnIconGeometryChanged(GeometryChangedEvent evt)
        {
            VisualElement icon = evt.target as VisualElement;
            float xOffset = icon.resolvedStyle.width / 2;
            float yOffset = icon.resolvedStyle.height / 2;
            Vector2 position = icon.transform.position;
            icon.style.translate = new Translate(position.x - xOffset, position.y - yOffset);

        }

        private void OnIconMoved(Dictionary<string, object> evntArgs)
        {
            IconViewInfo iconViewInfo = evntArgs[MoveIconCommand.ICON_MOVE_COMMAND_KEY] as IconViewInfo;
            int id = iconViewInfo.iconID;
            Vector3 position = iconViewInfo.Position;
            
            Vector2 localPosition = iconViewInfo.LocalPosition;
            VisualElement icon = _iconsOnFloor[id];
            if(position != icon.transform.position)
            {
                float xOffset = iconViewInfo.Width / 2;
                float yOffset = iconViewInfo.Height / 2;
                icon.style.translate = new Translate(localPosition.x - xOffset, localPosition.y - yOffset);
            }

            int width = Mathf.RoundToInt(icon.resolvedStyle.width);
            int height = Mathf.RoundToInt(icon.resolvedStyle.height);
            if(!IconIsOnFloor(localPosition, width, height))
            {
                EventManager.TriggerEvent(ICON_OFF_FLOOR_EVENT, 
                    new Dictionary<string, object> { { ICON_OFF_FLOOR_EVENT_KEY, id } });
                return;
            }
            icon.userData = iconViewInfo;
            //Debug.Log($"icon {id} position: {icon.transform.position}");
        }

        private void OnIconRotated(Dictionary<string, object> evntArgs)
        {
           IconViewInfo iconViewInfo = evntArgs[RotateIconCommand.ICON_ROTATE_COMMAND_KEY] as IconViewInfo;
            int id = iconViewInfo.iconID;
            VisualElement icon = _iconsOnFloor[id];
            icon.style.rotate = new Rotate(iconViewInfo.RotationAngle);
            icon.userData = iconViewInfo;
        }

        private void OnIconResized(Dictionary<string, object> dictionary)
        {
            IconViewInfo iconViewInfo = dictionary[ResizeIconCommand.RESIZE_ICON_COMMAND_KEY] as IconViewInfo;
            int id = iconViewInfo.iconID;
            VisualElement icon = _iconsOnFloor[id];
            Debug.Log($"changing icon {id} width to {iconViewInfo.Width}");
            Debug.Log($"changing icon {id} height to {iconViewInfo.Height}");
            icon.userData = iconViewInfo;
            if(iconViewInfo.Width != (int)icon.resolvedStyle.width || iconViewInfo.Height != (int)icon.resolvedStyle.height)
            {
                icon.style.width = iconViewInfo.Width;
                icon.style.height = iconViewInfo.Height;
                float xOffset = iconViewInfo.Width / 2;
                float yOffset = iconViewInfo.Height / 2;
                var localPosition = iconViewInfo.LocalPosition;
                icon.style.translate = new Translate(localPosition.x, localPosition.y);

            }

           
        }


        public void RemoveIcon(Dictionary<string, object> evntArgs)
        {
            //object[] args = (object[])message[RemoveIconCommand.REMOVE_ICON_COMMAND_KEY];
            IconViewInfo iconViewInfo = evntArgs[RemoveIconCommand.REMOVE_ICON_COMMAND_KEY] as IconViewInfo;
            
            int id = iconViewInfo.iconID;
            IconType iconType = iconViewInfo.IconType;
            VisualElement icon = _iconsOnFloor[id];
            string iconStyleClass = GetFloorIconContainerStyle(iconType);
            icon.RemoveFromClassList(iconStyleClass);
            _iconFactory.ReturnIcon(icon);
        }

        private void ReturnIcon(VisualElement icon)
        {
            if (icon.parent == _floor)
            {
                _floor.Remove(icon);
            }
            icon.userData = null;
            icon.AddToClassList("hidden");
        }

        private void PreGetIcon(VisualElement icon)
        {
            icon.RemoveFromClassList("hidden");
        }

        private VisualElement GetIcon()
        {
            VisualElement iconContainer = new VisualElement();
            iconContainer.style.position = new StyleEnum<Position>(Position.Absolute);
            iconContainer.name = "Floor_Icon";
            iconContainer.usageHints = UsageHints.DynamicTransform;
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

        private void OnPostKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            _preKaizenLayoutActive = false;
        }

        private void OnPreKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            _preKaizenLayoutActive = true;
        }
        private void OnSwitchKaizenLayoutClicked(Dictionary<string, object> switchEvent)
        {
            
            //_isPostKaizenLayout = (bool)switchEvent[POST_KAIZEN_LAYOUT_EVENT_KEY];
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

