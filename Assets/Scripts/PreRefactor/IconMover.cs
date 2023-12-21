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
        public const string ICON_MOVED_EVENT = "iconMovedEvent";
        public const string ICON_MOVED_EVENT_KEY = "iconMovedEventKey";
        //needs reference to icon visual element and floor visual element
        //needs reference to layout icon info
        VisualElement _iconElement;
        VisualElement _floorElement;
        VisualElement _draggableArea;

        private Vector3 _iconStartPosition;
        private Vector3 _pointerStartPosition;
        private bool _isDragging;

        
        public event Action<Vector2, VisualElement> DropIcon;
        public event Action<PointerDownEvent> PointerDown;
        public IconMover(VisualElement iconElement, VisualElement draggableArea, Action<Vector2, VisualElement> dropAction)
        {
            _iconElement = iconElement;
            _draggableArea = draggableArea;
            _floorElement = draggableArea.Q<VisualElement>("ve_floor");
            DropIcon = dropAction;
            RegisterCallbacks();
        }

        public IconMover(VisualElement iconElement, VisualElement draggableArea)
        {
            _iconElement = iconElement;
            _draggableArea = draggableArea;
            _floorElement = draggableArea.Q<VisualElement>("ve_floor");
            RegisterCallbacks();
        }

        public void StartDragging(PointerDownEvent evt)
        {
            OnPointerDown(evt);
        }

        public void RegisterPointerDownCallback(Action<PointerDownEvent> callback)
        {
            PointerDown = callback;
        }   

        public void UnregisterDropAction(Action<Vector2, VisualElement> dropAction)
        {
            DropIcon -= dropAction;
        }

        private void RegisterCallbacks()
        {
            _iconElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            //_draggableArea.RegisterCallback<PointerUpEvent>(OnPointerUp);
            
        }
            
        //select icon for movement
        private void OnPointerDown(PointerDownEvent evt)
        {
            //Debug.Log("IconMover: OnPointerDown");
            //_draggableArea.Add(_iconElement);
            //_iconElement.BringToFront();
            PointerDown?.Invoke(evt);
            _draggableArea.CapturePointer(evt.pointerId);

            //set icon and pointer start positions
            _iconStartPosition = _iconElement.transform.position;
            _pointerStartPosition = evt.position;
            _isDragging = true;
            _draggableArea.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _draggableArea.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        //drop icon
        //need different behavior for floor icons and layout icons; pass through a delegate
        private void OnPointerUp(PointerUpEvent evt)
        {
            VisualElement icon = evt.target as VisualElement;
            //Debug.Log("On pointer up called");
            _draggableArea.ReleasePointer(evt.pointerId);
            _isDragging = false;
            _draggableArea.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _draggableArea.UnregisterCallback<PointerUpEvent>(OnPointerUp);

            if(DropIcon != null)
            {
                DropIcon?.Invoke(evt.position, _iconElement);
            }
            else
            {
                int id = (int)_iconElement.userData;
                Debug.Log("IconMover: OnPointerUp: id = " + id);
                Vector3 position = _iconElement.transform.position;
                Debug.Log("IconMover: OnPointerUp: position = " + position);
                Vector2 localPositon = _floorElement.WorldToLocal(position);
                object[] args = new object[] { id, position, localPositon };
                EventManager.TriggerEvent(ICON_MOVED_EVENT,
                    new Dictionary<string, object> { { ICON_MOVED_EVENT_KEY, args } });
            }
          
        }


        //move icon
        private void OnPointerMove(PointerMoveEvent evt)
        {
            //Debug.Log("move icon event");
            if (_isDragging && _draggableArea.HasPointerCapture(evt.pointerId))
            {
                //Debug.Log("should be moving icon");
                float newX = _iconStartPosition.x + (evt.position.x - _pointerStartPosition.x);
                float newY = _iconStartPosition.y + (evt.position.y - _pointerStartPosition.y);

                _iconElement.transform.position = new Vector2(newX, newY);
            }
        }


    }

}

