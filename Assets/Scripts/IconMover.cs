using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    //used to move any icon, whether it is a floor icon or a spawner icon
    public class IconMover
    {
        //needs reference to icon visual element and floor visual element
        //needs reference to layout icon info
        VisualElement _iconElement;
        VisualElement _floorElement;
        VisualElement _draggableArea;

        private Vector3 _iconStartPosition;
        private Vector3 _pointerStartPosition;
        private bool _isDragging;

        
        public event Action<PointerUpEvent> DropIcon;

        public IconMover(VisualElement iconElement, VisualElement draggableArea, Action<PointerUpEvent> dropAction)
        {
            LayoutIconInfo iconInfo = iconElement.userData as LayoutIconInfo;
            Debug.Log("IconMover constructor for icon " + iconInfo.Type);
            _iconElement = iconElement;
            //_floorElement = floor;
            _draggableArea = draggableArea;
            DropIcon = dropAction;
            RegisterCallbacks();
        }

        public void RegisterDropEvent(Action<PointerUpEvent> dropAction)
        {
            DropIcon = dropAction;
        }

        private void RegisterCallbacks()
        {
            _iconElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _draggableArea.RegisterCallback<PointerUpEvent>(OnPointerUp);
            
        }
            
        //select icon for movement
        private void OnPointerDown(PointerDownEvent evt)
        {
            Debug.Log("IconMover: OnPointerDown");
            _draggableArea.CapturePointer(evt.pointerId);

            //set icon and pointer start positions
            _iconStartPosition = _iconElement.transform.position;
            _pointerStartPosition = evt.position;
            _isDragging = true;
            _draggableArea.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        //drop icon
        //need different behavior for floor icons and layout icons; pass through a delegate
        private void OnPointerUp(PointerUpEvent evt)
        {
            Debug.Log("On pointer up called");
            _draggableArea.ReleasePointer(evt.pointerId);
            _isDragging = false;
            _draggableArea.UnregisterCallback<PointerMoveEvent>(OnPointerMove);

            DropIcon?.Invoke(evt);
        }


        //move icon
        private void OnPointerMove(PointerMoveEvent evt)
        {
            Debug.Log("move icon event");
            if (_isDragging && _draggableArea.HasPointerCapture(evt.pointerId))
            {
                Debug.Log("should be moving icon");
                float newX = _iconStartPosition.x + (evt.position.x - _pointerStartPosition.x);
                float newY = _iconStartPosition.y + (evt.position.y - _pointerStartPosition.y);

                _iconElement.transform.position = new Vector2(newX, newY);
            }
        }


    }

}

