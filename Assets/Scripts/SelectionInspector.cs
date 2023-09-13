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

        private const string SELECTION_EVENT = "IconSelected";
        private const string ICON_INFO = "iconInfo";

        private VisualElement _selectionInspector;
        private FloatField _widthField;
        private FloatField _heightField;
        private Vector2Field _positionField;
        private Slider _rotationSlider;

        private VisualElement _icon;
        private LayoutIconInfo _iconInfo;

        public SelectionInspector(VisualElement root)
        {
            _selectionInspector = root.Q(SELECTION_INSPECTOR);
            BindElements();
            EventManager.StartListening(SELECTION_EVENT, SetData);
            RegisterCallbacks();
        }

        private void BindElements()
        {
            _widthField = _selectionInspector.Q<FloatField>(WIDTH_FIELD);
            _heightField = _selectionInspector.Q<FloatField>(HEIGHT_FIELD);
            _positionField = _selectionInspector.Q<Vector2Field>(POSITION_FIELD);
            _rotationSlider = _selectionInspector.Q<Slider>(ROTATION_SLIDER);
        }

        private void RegisterCallbacks()
        {
            _widthField.RegisterValueChangedCallback(OnWidthChanged);
            _heightField.RegisterValueChangedCallback(OnHeightChanged);
            _positionField.RegisterValueChangedCallback(OnPositionChanged);
            _rotationSlider.RegisterValueChangedCallback(OnRotationChanged);
        }

        private void SetData(Dictionary<string, object> selectionEvent)
        {
            _iconInfo = selectionEvent[ICON_INFO] as LayoutIconInfo;
            _widthField.value = _iconInfo.Width;
            _heightField.value = _iconInfo.Height;
            _positionField.value = _iconInfo.Position;
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
            Vector2 position = evt.newValue;
            _icon.style.translate = new Translate(position.x, position.y);
            _iconInfo.Position = position;
        }

        private void OnHeightChanged(ChangeEvent<float> evt)
        {
            float height = evt.newValue;
            _icon.style.height = height;
            _iconInfo.Height = height;
        }

        private void OnWidthChanged(ChangeEvent<float> evt)
        {
            float width = evt.newValue;
            _icon.style.width = width;
            _iconInfo.Width = width;   
        }
    }

}
