using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    //Manages layout of icons on the visual element that represents the floor
    //Handles pointer events related to the floor plan
    //Keeps track of icons on the floor
    //Keeps floor measurements and handles scaling
    //could refactor to remove Pointer events related to the icons to a separate class
    public class FloorPlan
    {
        private const string DRAG_AREA = "ve_layout_container";
        private const string START_DRAG = "ve_icon_container";
        
        private const string ICON_IMAGE = "ve_icon_image";
        private const string FLOOR = "ve_layout_area";


        private VisualElement _dragArea;

        //make a list of start drag elements and register callbacks for each
        private VisualElement _startDragElement;

        //need to add each icon's element and then assign it as the pointer icon on Pointer Down using evt.target
        private VisualElement _tableIcon;
        private VisualElement _trolleyIcon;
        private VisualElement _workerIcon;
        private VisualElement _conveyorIcon;
        private VisualElement _kanbanIcon;
        private VisualElement _machineIcon;
        private VisualElement _productFlowIcon;
        
        private VisualElement _pointerIcon;

        private VisualElement _floor;

        // used to calculate offset between potion icon and mouse pointer
        private Vector3 _iconStartPosition;
        private Vector3 _pointerStartPosition;

        private bool _isDragging;

        private Button _tableButton;

        private IconFactory<VisualElement> _iconFactory = new IconFactory<VisualElement>();



        public FloorPlan(VisualElement root)
        {
            SetVisualElements(root);
            RegisterCallbacks();
            _iconFactory.Factory = GetIcon;
            _iconFactory.PreReturn = ReturnIcon;
           
           
        }

        private void SetVisualElements(VisualElement root)
        {
            _dragArea = root.Q<VisualElement>(DRAG_AREA);
            _startDragElement = root.Q<VisualElement>(START_DRAG);
            _floor = root.Q<VisualElement>(FLOOR);
            _tableIcon = root.Q<VisualElement>(ICON_IMAGE);
            LayoutIconInfo info = new LayoutIconInfo()
            {
                Name = "table", 
                IsFloorIcon = false 
            };
            _tableIcon.userData = info;
            
        }

        private void SetIconUserData(VisualElement iconContainer)
        {
            string iconName = iconContainer.Q<Label>().text;
            switch(iconName)
            {
                case "Table":
                    _tableIcon = iconContainer.Q<VisualElement>(ICON_IMAGE);
                    _tableIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.table,
                        Name = "table",
                        IsFloorIcon = false
                    };
                    break;

                case "Trolley":
                    _trolleyIcon = iconContainer.Q<VisualElement>(ICON_IMAGE);
                    _trolleyIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.trolley,
                        Name = "trolley",
                        IsFloorIcon = false
                    };
                    break;
                case "Worker":
                    _workerIcon = iconContainer.Q<VisualElement>(ICON_IMAGE);
                    _workerIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.worker,
                        Name = "worker",
                        IsFloorIcon = false
                    };
                    break;
                case "Conveyor":
                    _conveyorIcon = iconContainer.Q<VisualElement>(ICON_IMAGE);
                    _conveyorIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.converyor,
                        Name = "conveyor",
                        IsFloorIcon = false
                    };
                    break;
                default:
                    break;
            }
        }

        private void RegisterCallbacks()
        {
            _startDragElement.RegisterCallback<PointerDownEvent>(PointerDownEventHandler);
            _dragArea.RegisterCallback<PointerMoveEvent>(PointerMoveEventHandler);
            _dragArea.RegisterCallback<PointerUpEvent>(PointerUpEventHandler);

            _floor.RegisterCallback<PointerEnterEvent>(PointerEnterEventHandler);
            _floor.RegisterCallback<PointerLeaveEvent>(PointerLeaveEventHandler);
        }

        private void PointerDownEventHandler(PointerDownEvent evt)
        {
            
            //assign pointer icon to the element that was clicked
            //register callbacks for pointer move and pointer up
            Debug.Log("pointer down");
            VisualElement target = evt.target as VisualElement;
            _pointerIcon = target.Q<VisualElement>(ICON_IMAGE);
            //LayoutIconInfo info = _pointerIcon.userData as LayoutIconInfo;

            _dragArea.CapturePointer(evt.pointerId);

            //set icon and pointer start positions
            _iconStartPosition = _pointerIcon.transform.position;
            _pointerStartPosition = evt.position;

            _isDragging = true;
            
        }

        private void PointerUpEventHandler(PointerUpEvent evt)
        {
            //Unregister callbacks for pointer move and pointer up
            //need to differentiate between existing and new floor items so that we only add a new icon if new
            Debug.Log("pointer up");
            _dragArea.ReleasePointer(evt.pointerId);
            _isDragging = false;
            LayoutIconInfo info = _pointerIcon.userData as LayoutIconInfo;
            var position = _dragArea.WorldToLocal(evt.position);
            bool floorContainsIcon = _floor.ContainsPoint(position);
            Debug.Log("floor contains icon = " + floorContainsIcon);
            if(info.IsFloorIcon && _floor.ContainsPoint(position))
            {
                float xOffset = _pointerIcon.resolvedStyle.width / 2;
                float yOffset = _pointerIcon.resolvedStyle.height / 2;

                _pointerIcon.style.translate = new Translate(position.x - xOffset, position.y - yOffset);
                _pointerIcon = null;
                
            }
            else if(!info.IsFloorIcon && _floor.ContainsPoint(position))
            {
                //create new icon at position
                //this icon needs to register pointer event callbacks so it can be moved on the floor
                //add to a list of icons on the floor
                //make events to keep track of any changes to the icons on the floor
                VisualElement icon = _iconFactory.GetIcon();
                float xOffset = _pointerIcon.resolvedStyle.width / 2;
                float yOffset = _pointerIcon.resolvedStyle.height / 2;
               
                icon.style.translate = new Translate(position.x - xOffset, position.y - yOffset);

                LayoutIconInfo  iconInfo = new LayoutIconInfo()
                {
                    Type = info.Type,
                    Name = info.Name,
                    IsFloorIcon = true
                };
                icon.userData = iconInfo;
                icon.RegisterCallback<PointerDownEvent>(PointerDownEventHandler);
                _pointerIcon.transform.position = _iconStartPosition;
                _pointerIcon = null;
            }
            else if(info.IsFloorIcon && !_floor.ContainsPoint(position))
            {
                Debug.Log("evt position not inside floor area...");
                _iconFactory.PreReturn(_pointerIcon);
                _pointerIcon = null;
            }
            else if (!info.IsFloorIcon && !_floor.ContainsPoint(position))
            {
                Debug.Log("evt position not inside floor area...");
                _pointerIcon.transform.position = _iconStartPosition;
                _pointerIcon = null;
            }
           
        }

        private void PointerMoveEventHandler(PointerMoveEvent evt)
        {

            //Debug.Log("pointer move");
            // offset icon to the current pointer position if active
            if (_isDragging && _dragArea.HasPointerCapture(evt.pointerId))
            {
                float newX = _iconStartPosition.x + (evt.position.x - _pointerStartPosition.x);
                float newY = _iconStartPosition.y + (evt.position.y - _pointerStartPosition.y);

                _pointerIcon.transform.position = new Vector2(newX, newY);
            }
        }

        

        private void PointerEnterEventHandler(PointerEnterEvent evt)
        {
            Debug.Log("pointer enter");
        }

        private void PointerLeaveEventHandler(PointerLeaveEvent evt)
        {
            Debug.Log("pointer leave");
        }


        //one factory for each type of icon...or...
        
        private VisualElement GetIcon()
        {


            VisualElement iconElement = new VisualElement();
            iconElement.AddToClassList("ve_table_icon");
            iconElement.style.position = new StyleEnum<Position>(Position.Absolute);
            iconElement.name = ICON_IMAGE;
            _floor.Add(iconElement);
            return iconElement;

            //Button button = new Button();
            //button.AddToClassList("button_table_icon");
            ////button.clicked += () => Debug.Log("clicked");
            //DraggableElement draggableElement = new DraggableElement(button, _iconFactory);
            //draggableElement.AddDropHandler(_floor, OnFloorDrop);
            //_floor.Add(button);
            //return button;
        }

        private void ReturnIcon(VisualElement icon)
        {
            if(icon.parent == _floor)
            {
                _floor.Remove(icon);
            }
            icon.AddToClassList("hidden");
            
        }

        private void AddIconToStyleClass(IconType type)
        {
            switch (type)
            {
                case IconType.table:
                    _tableIcon.AddToClassList("ve_table_icon");
                    break;
                case IconType.worker:
                    _tableIcon.AddToClassList("ve_worker_icon");
                    break;  
                case IconType.machine:
                    _tableIcon.AddToClassList("ve_machine_icon");
                    break;
                case IconType.trolley:
                    _tableIcon.AddToClassList("ve_trolley_icon");
                    break;
                default:
                    //do default stuff
                    break;
            }
        }
           

        private void OnFloorDrop(FloorDropEvent evt)
        {
            Debug.Log("dropping on floor");
        }
    }
}

