using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class FloorIcon
    {
        private const string SELECTION_EVENT = "IconSelected";
        private const string ICON_INFO = "iconInfo";
        private const string ICON_REMOVED_EVENT = "IconRemoved";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";

        private VisualElement _icon;
        private VisualElement _floor;
        private VisualElement _dragArea;
        private LayoutIconInfo _iconInfo;
        private IconMover _mover;
        private IconFactory<VisualElement> _iconFactory;

        public LayoutIconInfo IconInfo => _iconInfo;
        
        public FloorIcon(VisualElement icon, VisualElement dragArea, VisualElement floor, IconFactory<VisualElement> factory)
        {
            _icon = icon;
            _dragArea = dragArea;
            _floor = floor;
            _iconInfo = _icon.userData as LayoutIconInfo;
            _mover = new IconMover(_icon, dragArea, OnIconDropped);
            _iconFactory = factory;
            SetIconInfo();
        }

        private void SetIconInfo()
        {
            _iconInfo.Position = _icon.transform.position;
            Debug.Log("FloorIcon SetIconInfo position: " + _iconInfo.Position);
            _iconInfo.Rotation = 0f;
            _iconInfo.Width = _icon.resolvedStyle.width;
            _iconInfo.Height = _icon.resolvedStyle.height;
            _iconInfo.IconElement = _icon;
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
                EventManager.TriggerEvent(SELECTION_EVENT, new Dictionary<string, object> { { ICON_INFO, _iconInfo } });
                _iconInfo.Position = _icon.transform.position;
                Debug.Log("FloorIcon OnIconDropped position: " + _iconInfo.Position);
                //update info
            }
            else
            {
                _iconFactory.PreReturn(_icon);
                EventManager.TriggerEvent(ICON_REMOVED_EVENT, new Dictionary<string, object> { { FLOOR_ICON_EVENT_KEY, this } });
            }
        }


    }

}
