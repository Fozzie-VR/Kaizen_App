using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class SelectionInspector
    {
        private const string SELECTION_INSPECTOR = "ve_selection_inspector";
        private const string LAYOUT_ICON_INSPECTOR = "ve_layout_icon_inspector";
        private const string WIDTH_FIELD = "float_width";
        private const string HEIGHT_FIELD = "float_height";
        private const string POSITION_FIELD = "v2_position";
        private const string ROTATION_SLIDER = "slider_rotation";
        private const string DELETE_BUTTON = "btn_delete"; 
        private const string PHOTO_ICON_INSPECTOR = "ve_photo_icon_inspector";

        private const string SELECTION_EVENT = "IconSelected";
        private const string ICON_INFO = "iconInfo";

        private const string ICON_REMOVED_EVENT = "IconRemoved";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";

        private const string COMPARE_LAYOUTS_EVENT = "compare_layouts";

        private const string PIXELS_PER_METER_EVENT = "PixelsPerMeterChanged";
        private const string PIXELS_PER_METER_EVENT_KEY = "pixelsPerMeter";

        private const string TAKE_PHOTO_EVENT = "TakePhoto";
        private const string TAKE_PHOTO_EVENT_KEY = "iconInfo";

        private const string PHOTO_TAKEN_EVENT = "PhotoTaken";
        private const string PHOTO_TAKEN_EVENT_KEY = "photoTexture";

        public const string ROTATION_CHANGED_EVENT = "RotationChanged";
        public const string ROTATION_CHANGED_EVENT_KEY = "rotation";

        public const string ICON_DIMENSIONS_CHANGED_EVENT = "IconDimensionsChanged";
        public const string ICON_DIMENSIONS_CHANGED_EVENT_KEY = "iconInfo";

        public const string POSITION_CHANGED_EVENT = "PositionChanged";
        public const string POSITION_CHANGED_EVENT_KEY = "position";


        private VisualElement _selectionInspector;
        private VisualElement _photoIconInspector;
        private VisualElement _photoElement;
        private VisualElement _layoutIconInspector;
        private VisualElement _currentIcon;
        private FloatField _widthField;
        private FloatField _heightField;
        private Vector2Field _positionField;
        private Slider _rotationSlider;
        private Button _deleteButton;
        private Button _takePhotoButton;
        private Button _choosePhotoButton;

        //private VisualElement _icon;
        private IconViewInfo _iconInfo;
        private float _selectionInspectorHeight;

        private int _pixelsPerMeter = 384;
        private LayoutView _layoutView;

        public SelectionInspector(VisualElement root, LayoutView layoutView)
        {
            _selectionInspector = root.Q(SELECTION_INSPECTOR);
            _layoutView = layoutView;
            BindElements();
            RegisterCallbacks();
            
        }

        private void BindElements()
        {
            _photoIconInspector = _selectionInspector.Q<VisualElement>(PHOTO_ICON_INSPECTOR);
            _photoElement = _photoIconInspector.Q<VisualElement>("ve_photo");
            _layoutIconInspector = _selectionInspector.Q<VisualElement>(LAYOUT_ICON_INSPECTOR);
            _widthField = _selectionInspector.Q<FloatField>(WIDTH_FIELD);
            _heightField = _selectionInspector.Q<FloatField>(HEIGHT_FIELD);
            _positionField = _selectionInspector.Q<Vector2Field>(POSITION_FIELD);
            _rotationSlider = _selectionInspector.Q<Slider>(ROTATION_SLIDER);
            //_deleteButton = _selectionInspector.Q<Button>(DELETE_BUTTON);
            _takePhotoButton = _photoIconInspector.Q<Button>("btn_take_photo");

            _photoIconInspector.style.display = DisplayStyle.None;

        }

        private void RegisterCallbacks()
        {
            //EventManager.StartListening(SELECTION_EVENT, SetData);
            EventManager.StartListening(IconMover.ICON_CLICKED_EVENT, SetData);
            EventManager.StartListening(PIXELS_PER_METER_EVENT, OnPixelsPerMeterChanged);
            EventManager.StartListening(COMPARE_LAYOUTS_EVENT, OnCompareLayouts);
            EventManager.StartListening(PHOTO_TAKEN_EVENT, OnPhotoTaken);
            EventManager.StartListening(UndoRedoView.UNDO_EVENT, OnUndoRedo);

            _widthField.RegisterValueChangedCallback(OnWidthChanged);
            _heightField.RegisterValueChangedCallback(OnHeightChanged);

            _positionField.RegisterValueChangedCallback(OnPositionChanged);
            _rotationSlider.RegisterValueChangedCallback(OnRotationChanged);
            VisualElement sliderDragHandle = _rotationSlider.Q("unity-drag-container");
            sliderDragHandle.RegisterCallback<PointerUpEvent>(OnSliderOff);
            //_deleteButton.clicked += OnDeleteClicked;
            _takePhotoButton.clicked += OnTakePhotoClicked;

        }

       
        private void SetData(Dictionary<string, object> selectionEvent)
        {
            _pixelsPerMeter = _layoutView.PixelsPerMeter;
            _iconInfo = selectionEvent[IconMover.ICON_CLICKED_EVENT_KEY] as IconViewInfo;
            
            int id = _iconInfo.iconID;
            _currentIcon = _layoutView.IconsOnFloor[id];

            if (_iconInfo.IconType == IconType.Photo)
            {
               ShowPhotoIconInspector();
           }
           else
            {
               ShowLayoutIconInspector();
           }
        }

        private void OnUndoRedo(Dictionary<string, object> undoRedoEvent)
        {
            if(_iconInfo == null)
            {
                return;
            }
            _currentIcon = _layoutView.IconsOnFloor[_iconInfo.iconID];
            _iconInfo = _currentIcon.userData as IconViewInfo;

            if (_iconInfo != null && _iconInfo.IconType == IconType.Photo)
            {
                ShowPhotoIconInspector();
            }
            else
            {
                ShowLayoutIconInspector();
            }
        }   

        private void ShowLayoutIconInspector()
        {
            //Debug.Log("showing layout icon inspector");
            if (_iconInfo is null)
            {
                return;
            }
            _photoIconInspector.style.display = DisplayStyle.None;
            _layoutIconInspector.style.display = DisplayStyle.Flex;
            _widthField.SetValueWithoutNotify(_iconInfo.Width / (float)_pixelsPerMeter);
            _heightField.SetValueWithoutNotify(_iconInfo.Height / (float)_pixelsPerMeter);
            float positionX = _iconInfo.LocalPosition.x / _pixelsPerMeter;
            float positionY = _layoutView.FloorHeightMeters - _iconInfo.LocalPosition.y / _pixelsPerMeter;
            Vector2 positionMeters = new Vector2(positionX, positionY);
            _positionField.SetValueWithoutNotify(positionMeters);
            _rotationSlider.SetValueWithoutNotify(_iconInfo.RotationAngle);
            //_icon = _iconInfo.IconElement;
        }

        private void ShowPhotoIconInspector()
        {
            if(_selectionInspectorHeight == 0)
            {
                _selectionInspectorHeight = _selectionInspector.resolvedStyle.height;
            }
            _photoIconInspector.style.height = _selectionInspectorHeight;
            _photoIconInspector.style.display = DisplayStyle.Flex;
            _layoutIconInspector.style.display = DisplayStyle.None;
            //TODO: set photo texture and figure out where to save it
            //if(_iconInfo.PhotoTexture != null)
            //{
            //    _photoElement.style.backgroundImage = _iconInfo.PhotoTexture;
            //    Debug.Log("should be rotating photo by " + _iconInfo.Rotation);
            //    _photoElement.style.rotate = new Rotate(_iconInfo.Rotation);
            //    //_photoElement.style.scale = new Scale(new Vector2(-1f, 1f));
            //}
            //else
            //{
            //    _photoElement.style.backgroundImage = null;
            //}

        }

        private void OnTakePhotoClicked()
        {
            EventManager.TriggerEvent("TakePhoto", new Dictionary<string, object> { { "iconInfo", _iconInfo } });
        }

        private void OnPhotoTaken(Dictionary<string, object> eventDictionary)
        {
            Texture2D photoTexture = eventDictionary[PHOTO_TAKEN_EVENT_KEY] as Texture2D;
            _photoElement.style.backgroundImage = photoTexture;
            _photoElement.style.rotate = new Rotate(_iconInfo.RotationAngle);
            //_photoElement.style.scale = new Scale(new Vector2(-1f, 1f));
            Debug.Log("should be rotating photo by " + _iconInfo.RotationAngle);
        }


        private void OnRotationChanged(ChangeEvent<float> evt)
        {
           float rotation = evt.newValue;
           _iconInfo.RotationAngle = rotation;
           _currentIcon.style.rotate = new Rotate(rotation);
        }

        private void OnSliderOff(PointerUpEvent evt)
        {
            if(_iconInfo == null)
            {
                return;
            }
            EventManager.TriggerEvent(ROTATION_CHANGED_EVENT, new Dictionary<string, object> { { ROTATION_CHANGED_EVENT_KEY, _iconInfo } });

        }

        private void OnPositionChanged(ChangeEvent<Vector2> evt)
        {
            Vector2 delta = (evt.newValue - evt.previousValue) * _pixelsPerMeter;
            float newX = _currentIcon.transform.position.x + delta.x;
            float newY = _currentIcon.transform.position.y - delta.y;

            _currentIcon.style.translate = new Translate(delta.x, delta.y);
            _currentIcon.transform.position = new Vector2(newX, newY);
            _iconInfo.Position = _currentIcon.transform.position;
            _iconInfo.LocalPosition = _layoutView.Floor.WorldToLocal(_currentIcon.worldBound.center);
            _positionField.RegisterCallback<PointerLeaveEvent>(OnPositionChanged);

        }

        private void OnPositionChanged(PointerLeaveEvent evt)
        {
            Debug.Log("position changed");
            EventManager.TriggerEvent(IconMover.ICON_MOVED_EVENT, new Dictionary<string, object> { 
                { IconMover.ICON_MOVED_EVENT_KEY, _iconInfo } });
            _positionField.UnregisterCallback<PointerLeaveEvent>(OnPositionChanged);
        }

        private void OnHeightChanged(ChangeEvent<float> evt)
        {
            float height = evt.newValue * _pixelsPerMeter;
            _iconInfo.Height = Mathf.RoundToInt(height);
            _currentIcon.style.height = height;
            var localPosition = _layoutView.Floor.WorldToLocal(_currentIcon.worldBound.center);
            _currentIcon.style.translate = new Translate(localPosition.x, localPosition.y);
            _iconInfo.LocalPosition = localPosition;
            _iconInfo.Position = _currentIcon.transform.position;
            _heightField.RegisterCallback<PointerLeaveEvent>(OnIconHeightChanged);

        }

        private void OnWidthChanged(ChangeEvent<float> evt)
        {
            float width = evt.newValue * _pixelsPerMeter;
            float widthDelta = width - _iconInfo.Width;
            _iconInfo.Width = Mathf.RoundToInt(width);
            _currentIcon.style.width = width;
            var localPosition = _layoutView.Floor.WorldToLocal(_currentIcon.worldBound.center);
            _currentIcon.style.translate = new Translate(localPosition.x, localPosition.y);
            _iconInfo.LocalPosition = localPosition;
            _iconInfo.Position = _currentIcon.transform.position;
            _widthField.RegisterCallback<PointerLeaveEvent>(OnIconWidthChanged);
        }

        private void OnIconWidthChanged(PointerLeaveEvent evt)
        {
            Debug.Log($"icon {_iconInfo.iconID} width changed to {_iconInfo.Width}");
            Debug.Log("icon height changed to: " + _iconInfo.Height);
            EventManager.TriggerEvent(ICON_DIMENSIONS_CHANGED_EVENT, new Dictionary<string, object> {
                { ICON_DIMENSIONS_CHANGED_EVENT_KEY, _iconInfo }
            });
            _widthField.UnregisterCallback<PointerLeaveEvent>(OnIconWidthChanged);
        }

        private void OnIconHeightChanged(PointerLeaveEvent evt)
        {
            Debug.Log($"icon {_iconInfo.iconID} height changed to {_iconInfo.Height}");
            Debug.Log("icon width changed to: " + _iconInfo.Width);
            EventManager.TriggerEvent(ICON_DIMENSIONS_CHANGED_EVENT, new Dictionary<string, object>
            {
                { ICON_DIMENSIONS_CHANGED_EVENT_KEY, _iconInfo }
            });
            _heightField.UnregisterCallback<PointerLeaveEvent>(OnIconHeightChanged);
        }   

       
        private void OnPixelsPerMeterChanged(Dictionary<string, object> pixelsPerMeterEvent)
        {
            Debug.Log("changing pixels per meter");
            _pixelsPerMeter = (int)pixelsPerMeterEvent[PIXELS_PER_METER_EVENT_KEY];
        }

        private void OnDeleteClicked()
        {
            Debug.Log("triggering delete event");
            EventManager.TriggerEvent(ICON_REMOVED_EVENT, new Dictionary<string, object> { { FLOOR_ICON_EVENT_KEY, _iconInfo} });
        }

        private void OnCompareLayouts(Dictionary<string, object> eventMessage)
        {
            _selectionInspector.AddToClassList("hidden");
        }

    }

}
