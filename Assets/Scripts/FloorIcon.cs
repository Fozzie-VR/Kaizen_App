using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class FloorIcon
    {
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
        }

        private void OnIconDropped(Vector2 iconPosition, VisualElement icon)
        {
            if(_icon != icon)
            {
                return;
            }

            Vector2 position = _dragArea.WorldToLocal(iconPosition);
            float xOffset = _icon.resolvedStyle.width / 2;
            float yOffset = _icon.resolvedStyle.height / 2;
            bool floorContainsIcon = _floor.ContainsPoint(position);

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
                _icon.style.translate = new Translate(position.x - xOffset, position.y - yOffset);
                //update info
            }
            else
            {
                _iconFactory.PreReturn(_icon);
                KaizenAppManager._instance.KaizenEvents.OnFloorIconRemoved(this);
            }
        }


    }

}
