using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class FloorIcon: IView
    {
        private const string SELECTION_EVENT = "IconSelected";
        private const string ICON_INFO = "iconInfo";
        private const string ICON_REMOVED_EVENT = "IconRemoved";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";
        private const string PIXELS_PER_METER_EVENT = "PixelsPerMeterChanged";
        private const string PIXELS_PER_METER_EVENT_KEY = "pixelsPerMeter";

        private VisualElement _icon;
        private VisualElement _floor;
        private VisualElement _dragArea;
        private LayoutIconInfo _iconInfo;
        private IconMover _mover;
       
        private int _defaultPixelsPerMeter = KaizenAppManager.Instance.DefaultPixelsPerMeter;
        private int _pixelsPerMeter;

        private float _defaultIconWidth;
        private float _defaultIconHeight;
        private float _iconScale;

        public LayoutIconInfo IconInfo => _iconInfo;
        
        public FloorIcon(VisualElement icon, VisualElement dragArea, VisualElement floor)
        {
            _icon = icon;
            _dragArea = dragArea;
            _floor = floor;
            _iconInfo = _icon.userData as LayoutIconInfo;
            _mover = new IconMover(_icon, dragArea, OnIconDropped);
            _pixelsPerMeter = KaizenAppManager.Instance.PixelsPerMeter;
            _iconScale = 1f;
            EventManager.StartListening(PIXELS_PER_METER_EVENT, OnPixelsPerMeterChanged);
            SetIconInfo();
        }

       
        private void SetIconInfo()
        {
            //Debug.Log("Set icon info");
            _iconInfo.Position = _icon.transform.position;
            _iconInfo.LocalPosition = _floor.WorldToLocal(_iconInfo.Position);
            _iconInfo.Rotation = 0f;
            if(_iconInfo.Width == 0f)
            {
                _iconInfo.Width = _icon.resolvedStyle.width;
            }
            if(_iconInfo.Height == 0f)
            {
                _iconInfo.Height = _icon.resolvedStyle.height;
            }
            
            _defaultIconHeight = _iconInfo.Height;
            _defaultIconWidth = _iconInfo.Width;
            _iconInfo.IconElement = _icon;
            _iconInfo.FloorIcon = this;
            if (_pixelsPerMeter != _defaultPixelsPerMeter)
            {
                RescaleIcon();
            }
        }

        private void OnPixelsPerMeterChanged(Dictionary<string, object> eventDictionary)
        {
            int newPixelsPerMeter = (int)eventDictionary[PIXELS_PER_METER_EVENT_KEY];
            
            if (newPixelsPerMeter == _pixelsPerMeter)
            {
                return;
            }
            _pixelsPerMeter = newPixelsPerMeter;
            RescaleIcon();
        }

        public void RescaleIcon()
        {
            //Debug.Log("RescaleIcon");
            float scaleFactor = (float)_pixelsPerMeter / _defaultPixelsPerMeter;
            float newScale = scaleFactor / _iconScale;
           
            _iconInfo.Width *= newScale;
            _iconInfo.Height *= newScale;
            _iconScale = scaleFactor;
            _icon.style.width = _iconInfo.Width;
            _icon.style.height = _iconInfo.Height;
        }


        private void OnIconDropped(Vector2 dropPosition, VisualElement droppedIcon)
        {
            //Debug.Log("FloorIcon OnIconDropped");
            if(_icon != droppedIcon)
            {
                return;
            }
            //TODO need a helper class to check if the icon is on the floor

            var position = _floor.WorldToLocal(dropPosition);
            Debug.Log("icon local position = " + position);
            bool floorContainsIcon = _floor.ContainsPoint(position);

            float xOffset = droppedIcon.resolvedStyle.width / 2;
            float yOffset = droppedIcon.resolvedStyle.height / 2;

            if (position.x - xOffset < 0)
            {
                floorContainsIcon = false;
            }

            if (position.x + xOffset > _floor.resolvedStyle.width)
            {
                floorContainsIcon = false;
            }

            if (position.y - yOffset < 0)
            {
                floorContainsIcon = false;
            }

            if (position.y + yOffset > _floor.resolvedStyle.height)
            {
                floorContainsIcon = false;
            }


            if (floorContainsIcon)
            {
                _iconInfo.Position = _icon.transform.position;
                //Debug.Log("icon position: " + _iconInfo.Position);
                _iconInfo.LocalPosition = position;
                EventManager.TriggerEvent(SELECTION_EVENT, new Dictionary<string, object> { { ICON_INFO, _iconInfo } });
            }
            else
            {
                EventManager.TriggerEvent(ICON_REMOVED_EVENT, new Dictionary<string, object> { { FLOOR_ICON_EVENT_KEY, this } });
            }
        }

    }

}
