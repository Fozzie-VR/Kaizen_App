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


        private VisualElement _selectionInspector;
        private VisualElement _photoIconInspector;
        private VisualElement _photoElement;
        private VisualElement _layoutIconInspector;
        private FloatField _widthField;
        private FloatField _heightField;
        private Vector2Field _positionField;
        private Slider _rotationSlider;
        private Button _deleteButton;
        private Button _takePhotoButton;
        private Button _choosePhotoButton;

        private VisualElement _icon;
        private LayoutIconInfo _iconInfo;
        private float _selectionInspectorHeight;

        private int _pixelsPerMeter = KaizenAppManager.Instance.PixelsPerMeter;

        public SelectionInspector(VisualElement root)
        {
            _selectionInspector = root.Q(SELECTION_INSPECTOR);
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
            EventManager.StartListening(SELECTION_EVENT, SetData);
            EventManager.StartListening(PIXELS_PER_METER_EVENT, OnPixelsPerMeterChanged);
            EventManager.StartListening(COMPARE_LAYOUTS_EVENT, OnCompareLayouts);
            EventManager.StartListening(PHOTO_TAKEN_EVENT, OnPhotoTaken);

            _widthField.RegisterValueChangedCallback(OnWidthChanged);
            _heightField.RegisterValueChangedCallback(OnHeightChanged);
            _positionField.RegisterValueChangedCallback(OnPositionChanged);
            _rotationSlider.RegisterValueChangedCallback(OnRotationChanged);
            //_deleteButton.clicked += OnDeleteClicked;
            _takePhotoButton.clicked += OnTakePhotoClicked;

        }

       
        private void SetData(Dictionary<string, object> selectionEvent)
        {
           // _pixelsPerMeter = KaizenAppManager._instance.PixelsPerMeter;
            _iconInfo = selectionEvent[ICON_INFO] as LayoutIconInfo;
           if(_iconInfo.Type == IconType.Photo)
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
            _photoIconInspector.style.display = DisplayStyle.None;
            _layoutIconInspector.style.display = DisplayStyle.Flex;
            _widthField.SetValueWithoutNotify(_iconInfo.Width / _pixelsPerMeter);
            _heightField.SetValueWithoutNotify(_iconInfo.Height / _pixelsPerMeter);

            float positionX = _iconInfo.LocalPosition.x / _pixelsPerMeter;
            float positionY = KaizenAppManager.Instance.FloorHeightMeters - _iconInfo.LocalPosition.y / _pixelsPerMeter;
            Vector2 positionMeters = new Vector2(positionX, positionY);
            _positionField.SetValueWithoutNotify(positionMeters);
            _rotationSlider.SetValueWithoutNotify(_iconInfo.Rotation);
            _icon = _iconInfo.IconElement;
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
            if(_iconInfo.PhotoTexture != null)
            {
                _photoElement.style.backgroundImage = _iconInfo.PhotoTexture;
                Debug.Log("should be rotating photo by " + _iconInfo.Rotation);
                _photoElement.style.rotate = new Rotate(_iconInfo.Rotation);
                //_photoElement.style.scale = new Scale(new Vector2(-1f, 1f));
            }
            else
            {
                _photoElement.style.backgroundImage = null;
            }

        }

        private void OnTakePhotoClicked()
        {
            EventManager.TriggerEvent("TakePhoto", new Dictionary<string, object> { { "iconInfo", _iconInfo } });
        }

        private void OnPhotoTaken(Dictionary<string, object> eventDictionary)
        {
            Texture2D photoTexture = eventDictionary[PHOTO_TAKEN_EVENT_KEY] as Texture2D;
            _photoElement.style.backgroundImage = photoTexture;
            _photoElement.style.rotate = new Rotate(GetRotation());
            //_photoElement.style.scale = new Scale(new Vector2(-1f, 1f));
            Debug.Log("should be rotating photo by " + _iconInfo.Rotation);
        }

        private float GetRotation()
        {
            switch (Screen.orientation)
            {
                case ScreenOrientation.Portrait:
                    return 90;
                case ScreenOrientation.LandscapeLeft:
                    return 0;
                case ScreenOrientation.LandscapeRight:
                    return 180;
                case ScreenOrientation.PortraitUpsideDown:
                    return 90;
                default:
                    return 270;
            }
            
        }


        private void OnRotationChanged(ChangeEvent<float> evt)
        {
            if(_icon == null)
            {
                return;
            }   

           float rotation = evt.newValue;
           _icon.style.rotate = new Rotate(rotation);
           _iconInfo.Rotation = rotation;
        }

        private void OnPositionChanged(ChangeEvent<Vector2> evt)
        {
            Debug.Log("pixels per meter: " + _pixelsPerMeter);  
            Vector2 delta = (evt.newValue - evt.previousValue) * _pixelsPerMeter;
            float newX = _icon.transform.position.x + delta.x;
            float newY = _icon.transform.position.y - delta.y;

            //_icon.style.translate = new Translate(delta.x, delta.y);
            _icon.transform.position = new Vector2(newX, newY);
            _iconInfo.Position = _icon.transform.position;
            
        }

        private void OnHeightChanged(ChangeEvent<float> evt)
        {
            float height = evt.newValue * _pixelsPerMeter;
            _icon.style.height = height;
            _iconInfo.Height = height;
        }

        private void OnWidthChanged(ChangeEvent<float> evt)
        {
            float width = evt.newValue * _pixelsPerMeter;
            _icon.style.width = width;
            _iconInfo.Width = width;   
        }

        private void OnPixelsPerMeterChanged(Dictionary<string, object> pixelsPerMeterEvent)
        {
            _pixelsPerMeter = (int)pixelsPerMeterEvent[PIXELS_PER_METER_EVENT_KEY];
        }

        private void OnDeleteClicked()
        {
            Debug.Log("triggering delete event");
            EventManager.TriggerEvent(ICON_REMOVED_EVENT, new Dictionary<string, object> { { FLOOR_ICON_EVENT_KEY, _iconInfo.FloorIcon } });
        }

        private void OnCompareLayouts(Dictionary<string, object> eventMessage)
        {
            _selectionInspector.AddToClassList("hidden");
        }

    }

}
