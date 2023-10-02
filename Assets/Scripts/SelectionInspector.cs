using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class SelectionInspector
    {
        private const string SELECTION_INSPECTOR = "ve_selection_inspector";
        private const string WIDTH_FIELD = "float_width";
        private const string HEIGHT_FIELD = "float_height";
        private const string POSITION_FIELD = "v2_position";
        private const string ROTATION_SLIDER = "slider_rotation";
        private const string DELETE_BUTTON = "btn_delete";  

        private const string SELECTION_EVENT = "IconSelected";
        private const string ICON_INFO = "iconInfo";

        private const string ICON_REMOVED_EVENT = "IconRemoved";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";

        private const string PIXELS_PER_METER_EVENT = "PixelsPerMeterChanged";
        private const string PIXELS_PER_METER_EVENT_KEY = "pixelsPerMeter";


        private VisualElement _selectionInspector;
        private FloatField _widthField;
        private FloatField _heightField;
        private Vector2Field _positionField;
        private Slider _rotationSlider;
        private Button _deleteButton;

        private VisualElement _icon;
        private LayoutIconInfo _iconInfo;

        private int _pixelsPerMeter = KaizenAppManager._instance.PixelsPerMeter;

        public SelectionInspector(VisualElement root)
        {
            _selectionInspector = root.Q(SELECTION_INSPECTOR);
            BindElements();
           
            RegisterCallbacks();
        }

        private void BindElements()
        {
            _widthField = _selectionInspector.Q<FloatField>(WIDTH_FIELD);
            _heightField = _selectionInspector.Q<FloatField>(HEIGHT_FIELD);
            _positionField = _selectionInspector.Q<Vector2Field>(POSITION_FIELD);
            _rotationSlider = _selectionInspector.Q<Slider>(ROTATION_SLIDER);
            _deleteButton = _selectionInspector.Q<Button>(DELETE_BUTTON);

        }

        private void RegisterCallbacks()
        {
            EventManager.StartListening(SELECTION_EVENT, SetData);
            EventManager.StartListening(PIXELS_PER_METER_EVENT, OnPixelsPerMeterChanged);

            _widthField.RegisterValueChangedCallback(OnWidthChanged);
            _heightField.RegisterValueChangedCallback(OnHeightChanged);
            _positionField.RegisterValueChangedCallback(OnPositionChanged);
            _rotationSlider.RegisterValueChangedCallback(OnRotationChanged);
            _deleteButton.clicked += OnDeleteClicked;
        }

       
        private void SetData(Dictionary<string, object> selectionEvent)
        {
            _pixelsPerMeter = KaizenAppManager._instance.PixelsPerMeter;
            _iconInfo = selectionEvent[ICON_INFO] as LayoutIconInfo;
            _widthField.SetValueWithoutNotify(_iconInfo.Width/_pixelsPerMeter);
            _heightField.SetValueWithoutNotify(_iconInfo.Height/_pixelsPerMeter);

            float positionX = _iconInfo.LocalPosition.x / _pixelsPerMeter;
            float positionY = KaizenAppManager._instance.FloorHeightMeters - _iconInfo.LocalPosition.y / _pixelsPerMeter;
            Vector2 positionMeters = new Vector2(positionX, positionY);
            _positionField.SetValueWithoutNotify(positionMeters);
            _rotationSlider.SetValueWithoutNotify(_iconInfo.Rotation);
            _icon = _iconInfo.IconElement;
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



    }

}
