using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class DraggableElement
    {
        private const float DRAG_THRESHOLD = 3f;
        private DragDropPointerHandler _pointerHandler;
        private VisualElement _icon;
        private VisualElement _floor;
        private VisualElement _iconGhost;
        private IconFactory<VisualElement> _iconFactory;

        private Vector2 _moveDelta;
        private Vector2 _startPosition;
        private bool _overDragThreshold;

        private Action<FloorDropEvent> _floorDropHandler;

        public Vector2 Delta => _moveDelta;
        private Vector2 _ghostPosition;



        public DraggableElement(VisualElement icon, IconFactory<VisualElement> iconFactory)
        {
            _icon = icon;
            _iconFactory = iconFactory;
            _pointerHandler = new DragDropPointerHandler(_icon);
            _pointerHandler.DragStart += OnDragStart;
        }

        public void AddDropHandler(VisualElement floor, Action<FloorDropEvent> handler)
        {
            _floor = floor;
            _floorDropHandler = handler;
            _pointerHandler.AddFloorDropHandler(floor, Drop);
            _pointerHandler.AddFloorMoveHandler(floor, OnFloorMoveEvent, OnDragOverFloor, OnDragOffFloor);
        }   


        private void OnDragStart(DragStartEvent evt)
        {
            Debug.Log("drag start called");
            _overDragThreshold = false;
            _startPosition = evt._position;
            _moveDelta = evt._localPosition;
            Debug.Log("delta = " + _moveDelta);
            if (_iconGhost is null)
            {
                _iconGhost = _iconFactory.GetIcon();
                _iconGhost.style.translate = new Translate(_startPosition.x, _startPosition.y);
            }
        }

        //called each time icon moves over the floor
        private void OnFloorMoveEvent(FloorMoveEvent evt)
        {
           if((evt._position - _startPosition).sqrMagnitude > DRAG_THRESHOLD *DRAG_THRESHOLD )
            {
                _overDragThreshold = true;
            }

            if (_overDragThreshold)
            {
                //Debug.Log("On floor movement event delta = " + _moveDelta);
                _ghostPosition = _floor.WorldToLocal(evt._position) - _moveDelta;
                _iconGhost.style.translate = new Translate(_ghostPosition.x, _ghostPosition.y);
            }

            
        }

        //icon enters the floor
        private void OnDragOverFloor(DragOverFloorEvent evt)
        {
            
            if(_iconGhost is null)
            {
                _iconGhost = _iconFactory.GetIcon();
            }
            
            _ghostPosition = _floor.WorldToLocal(evt._position) - _moveDelta;
            Debug.Log("drag over floor position = " + _ghostPosition);
            
            _iconGhost.style.translate = new Translate(_ghostPosition.x, _ghostPosition.y);
            
        }

        private void OnDragOffFloor(DragOffFloorEvent evt)
        {
            if(_iconGhost != null)
            {
                //_iconFactory.ReturnObject(_iconGhost);
                _iconGhost = null;
            }
          
            
        }

        private void Drop(FloorDropEvent evt)
        {
            if(_iconGhost!= null)
            {
                //_iconFactory.ReturnObject(_iconGhost);
                _iconGhost = null;
            }
          
            evt._position -= _moveDelta;
            Debug.Log("delta on drop = " + _moveDelta);
            _floorDropHandler?.Invoke(evt);
        }

    }

   
}
