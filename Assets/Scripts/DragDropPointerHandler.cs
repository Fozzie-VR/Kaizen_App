using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.Rendering;

namespace KaizenApp
{
    public class DragDropPointerHandler : PointerManipulator
    {

        private Dictionary<VisualElement, Action<FloorDropEvent>> _floorDropHandlers = new();
        private Dictionary<VisualElement, Action<FloorMoveEvent>> _floorMoveHandlers = new();
        private Dictionary<VisualElement, Action<DragOverFloorEvent>> _dragOverFloorHandlers = new();
        private Dictionary<VisualElement, Action<DragOffFloorEvent>> _dragOffFloorHandlers = new();
        private Dictionary<VisualElement, bool > _dragOverState = new();


        public event Action<DragStartEvent> DragStart;
        public event Action<DragMoveEvent> DragMove;    
        public event Action<DragEndEvent> DragEnd;

        public bool Enabled { get; set; }

        public DragDropPointerHandler(VisualElement target)
        {
            this.target = target;
        }

        public void AddFloorDropHandler(VisualElement floor, Action<FloorDropEvent> handler)
        {
            if (_floorDropHandlers == null)
            {
                _floorDropHandlers = new Dictionary<VisualElement, Action<FloorDropEvent>>();
            }

            if (!_floorDropHandlers.ContainsKey(floor))
            {
                _floorDropHandlers.Add(floor, handler);
            }
            else
            {
                _floorDropHandlers[floor] += handler;
            }
        } 
        
        public void AddFloorMoveHandler(
            VisualElement floor, Action<FloorMoveEvent> moveHandler, 
            Action<DragOverFloorEvent> dragOverHandler, 
            Action<DragOffFloorEvent> dragOffHandler)
        {
            _floorMoveHandlers[floor] = moveHandler;
            _dragOverFloorHandlers[floor] = dragOverHandler;
            _dragOffFloorHandlers[floor] = dragOffHandler;
            _dragOverState[floor] = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            //register pointer handlers here
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
            target.RegisterCallback<PointerCaptureEvent>(PointerCaptureHandler);

        }

        protected override void UnregisterCallbacksFromTarget()
        {
           //unregister pointer handlers
           target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
           target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
           target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
           target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
           target.UnregisterCallback<PointerCaptureEvent>(PointerCaptureHandler);

        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            target.CapturePointer(evt.pointerId);
            Enabled = true;
            DragStartEvent dragStartEvent = new DragStartEvent()
            {
                _target = evt.target as VisualElement,
                _position = evt.position,
                _localPosition = evt.localPosition
            };
            DragStart?.Invoke(dragStartEvent);
            
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if(Enabled && target.HasPointerCapture(evt.pointerId))
            {
                DragMoveEvent dragMoveEvent = new DragMoveEvent()
                {
                    _target = evt.target as VisualElement,
                    _position = evt.position
                };
                DragMove?.Invoke(dragMoveEvent);

                foreach(var kvp in _floorMoveHandlers)
                {
                    HandleDragOverFloor(kvp.Key, evt, kvp.Value);
                }
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if(Enabled && target.HasPointerCapture(evt.pointerId))
            {
                DragEndEvent dragEndEvent = new DragEndEvent()
                {
                    _target = evt.target as VisualElement,
                    _position = evt.position
                };
                DragEnd?.Invoke(dragEndEvent);

                target.ReleasePointer(evt.pointerId);
               
                foreach(var kvp in _floorDropHandlers)
                {
                    _dragOverState[kvp.Key] = false;
                    Rect mousePointer = new Rect(evt.position, Vector2.one);
                    if (kvp.Key.worldBound.Overlaps(mousePointer))
                    {
                        FloorDropEvent floorDropEvent = new FloorDropEvent()
                        {
                            _target = evt.target as VisualElement,
                            _floor = kvp.Key,
                            _position = evt.position
                        };
                        kvp.Value?.Invoke(floorDropEvent);
                    }
                }   
            }
        }

        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            Enabled = false;
        } 
        
        private void PointerCaptureHandler(PointerCaptureEvent evt)
        {
            Enabled = true;
        }

       private void HandleDragOverFloor(VisualElement floor, PointerMoveEvent moveEvent, Action<FloorMoveEvent> floorMoveHandler)
       {
            bool lastHoverState = _dragOverState[floor];
            Rect mousePointer = new Rect(moveEvent.position, Vector2.one);
            if(floor.worldBound.Overlaps(mousePointer))
            {
                  if(!lastHoverState)
                {
                    _dragOverState[floor] = true;
                    DragOverFloorEvent dragOverFloorEvent = new DragOverFloorEvent()
                    {
                        _target = moveEvent.target as VisualElement,
                        _floor = floor,
                        _position = moveEvent.position
                    };
                    _dragOverFloorHandlers[floor]?.Invoke(dragOverFloorEvent);
                }
                FloorMoveEvent floorMoveEvent = new FloorMoveEvent()
                {
                    _target = moveEvent.target as VisualElement,
                    _floor = floor,
                    _position = moveEvent.position
                };
                floorMoveHandler?.Invoke(floorMoveEvent);
            }
            else
            {
                if(lastHoverState)
                {
                    _dragOverState[floor] = false;
                    DragOffFloorEvent dragOffFloorEvent = new DragOffFloorEvent()
                    {
                        _target = moveEvent.target as VisualElement,
                        _floor = floor
                    };
                    _dragOffFloorHandlers[floor]?.Invoke(dragOffFloorEvent);
                }
            }
       }
    }

    public class DragStartEvent
    {
        public VisualElement _target;
        public Vector2 _position;
        public Vector2 _localPosition;
    }

    public class DragMoveEvent
    {
        public VisualElement _target;
       
        public Vector2 _position;
      
    }

    public class DragEndEvent
    {
        public VisualElement _target;
        public Vector2 _position;
    }

    public class DragOverFloorEvent
    {
        public VisualElement _target;
        public VisualElement _floor;
        public Vector2 _position;
    }

    public class FloorMoveEvent
    {
        public VisualElement _target;
        public VisualElement _floor;
        public Vector2 _position;
    }

    public class DragOffFloorEvent
    {
        public VisualElement _target;
        public VisualElement _floor;
       
    }



    public class FloorDropEvent
    {
        public VisualElement _target;
        public VisualElement _floor;
        public Vector2 _position;
    }

}
