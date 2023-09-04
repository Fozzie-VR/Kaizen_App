using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    //Manages layout of icons on the visual element that represents the floor
    //Handles pointer events related to the floor plan
    //Keeps track of icons on the floor
    //Keeps floor measurements and handles scaling
    //could refactor to remove Pointer events related to the icons to a separate class

    public class FloorPlanner

    {
        private const string DRAG_AREA = "ve_layout_container";
        private const string START_DRAG = "ve_icon_container";
        
        private const string ICON_IMAGE = "ve_icon_image";
        private const string FLOOR = "ve_layout_area";

        private const string ICON_IMAGE_STYLE = "icon_image";
        private const string ICON_CONTAINER_STYLE = "icon_container";
        private const string ICON_LABEL_STYLE = "icon_label";


        private VisualElement _dragArea;
        private List<VisualElement> _startDragElements = new List<VisualElement>();

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


        public FloorPlanner(VisualElement root)
        {
            SetVisualElements(root);
            RegisterCallbacks();
            _iconFactory.Factory = GetIcon;
            _iconFactory.PreReturn = ReturnIcon;
        }

        private void SetVisualElements(VisualElement root)
        {
            _dragArea = root.Q<VisualElement>(DRAG_AREA);
            _floor = root.Q<VisualElement>(FLOOR);

            _startDragElements = root.Query<VisualElement>(START_DRAG).ToList();
            foreach(VisualElement iconContainer in _startDragElements)
            {
                SetIconUserData(iconContainer);
            }

        }

        private void SetIconUserData(VisualElement iconContainer)
        {
            string iconName = iconContainer.Q<Label>().text;
            switch(iconName)
            {
                case "Table":
                    _tableIcon = iconContainer;
                    _tableIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.table,
                        Name = "table",
                        IsFloorIcon = false
                    };
                    break;

                case "Trolley":
                    _trolleyIcon = iconContainer;
                    _trolleyIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.trolley,
                        Name = "trolley",
                        IsFloorIcon = false
                    };
                    break;
                case "Worker":
                    _workerIcon = iconContainer;
                    _workerIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.worker,
                        Name = "worker",
                        IsFloorIcon = false
                    };
                    break;
                case "Conveyor":
                    _conveyorIcon = iconContainer;
                    _conveyorIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.converyor,
                        Name = "conveyor",
                        IsFloorIcon = false
                    };
                    break;
                case "Machine":
                    _machineIcon = iconContainer;
                    _machineIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.machine,
                        Name = "machine",
                        IsFloorIcon = false
                    };
                    break;
                default:
                    break;
            }
        }

        private void RegisterCallbacks()
        {
            foreach(VisualElement iconContainer in _startDragElements)
            {
                iconContainer.RegisterCallback<PointerDownEvent>(PointerDownEventHandler);
            }
            _dragArea.RegisterCallback<PointerMoveEvent>(PointerMoveEventHandler);
            _dragArea.RegisterCallback<PointerUpEvent>(PointerUpEventHandler);

            _floor.RegisterCallback<PointerEnterEvent>(PointerEnterEventHandler);
            _floor.RegisterCallback<PointerLeaveEvent>(PointerLeaveEventHandler);
        }

        private void PointerDownEventHandler(PointerDownEvent evt)
        {
            VisualElement target = evt.target as VisualElement;
            _pointerIcon = target;
            _dragArea.CapturePointer(evt.pointerId);

            //set icon and pointer start positions
            _iconStartPosition = _pointerIcon.transform.position;
            _pointerStartPosition = evt.position;

            _isDragging = true;
        }

        private void PointerUpEventHandler(PointerUpEvent evt)
        {
            //Debug.Log("pointer up");
            _dragArea.ReleasePointer(evt.pointerId);
            _isDragging = false;
            LayoutIconInfo info = _pointerIcon.userData as LayoutIconInfo;
            float xOffset = _pointerIcon.resolvedStyle.width / 2;
            float yOffset = _pointerIcon.resolvedStyle.height / 2;
            var position = _dragArea.WorldToLocal(evt.position);
            // floor 0, 0 is top left
            //need to get current quadrant from position
            //check for overlap based on quadrant
            Debug.Log(position);
            Debug.Log("floor width " + _floor.resolvedStyle.width);
            Debug.Log("floor height " + _floor.resolvedStyle.height);
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

            if(position.y + yOffset > _floor.resolvedStyle.height)
            {
                floorContainsIcon = false;
            }

           
            //Debug.Log("floor contains icon = " + floorContainsIcon);
            if(info.IsFloorIcon && floorContainsIcon)
            {
                _pointerIcon.style.translate = new Translate(position.x - xOffset, position.y - yOffset);
                _pointerIcon = null;
                
            }
            else if(!info.IsFloorIcon && floorContainsIcon)
            {
               
                VisualElement icon = _iconFactory.GetIcon();
                VisualElement iconImage = icon.Q<VisualElement>(ICON_IMAGE);
                string imageClass = GetImageIconStyleClass(info.Type);
                iconImage.AddToClassList(imageClass);
                Label label = icon.Q<Label>();
                label.text = GetIconLabelText(info.Type);
               
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
            else if(info.IsFloorIcon && !floorContainsIcon)
            {
                //Debug.Log("evt position not inside floor area...");
                _iconFactory.PreReturn(_pointerIcon);
                _pointerIcon = null;
            }
            else if (!info.IsFloorIcon && !floorContainsIcon)
            {
                //Debug.Log("evt position not inside floor area...");
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
            //Debug.Log("pointer enter");
        }

        private void PointerLeaveEventHandler(PointerLeaveEvent evt)
        {
            //Debug.Log("pointer leave");
        }


        //one factory for each type of icon...or...
        
        private VisualElement GetIcon()
        {
            VisualElement iconContainer = new VisualElement();
            iconContainer.AddToClassList("icon_container");
            iconContainer.style.position = new StyleEnum<Position>(Position.Absolute);
            iconContainer.name = START_DRAG;

            VisualElement iconImage = new VisualElement();
            iconImage.AddToClassList(ICON_IMAGE_STYLE);
            iconImage.name = ICON_IMAGE;
            iconImage.pickingMode = PickingMode.Ignore;
            iconContainer.Add(iconImage);

            Label iconLabel = new Label();
            iconLabel.AddToClassList(ICON_LABEL_STYLE);
            iconContainer.Add(iconLabel);

            _floor.Add(iconContainer);
            return iconContainer;

        }

        private void ReturnIcon(VisualElement icon)
        {
            if(icon.parent == _floor)
            {
                _floor.Remove(icon);
            }
            icon.AddToClassList("hidden");
            
        }

        private string GetImageIconStyleClass(IconType type)
        {
            switch (type)
            {
                case IconType.table:
                    return "table_icon";
                    break;
                case IconType.worker:
                    return "worker_icon";
                    break;  
                case IconType.machine:
                    return "machine_icon";
                    break;
                case IconType.trolley:
                    return "trolley_icon";
                    break;
                case IconType.converyor:
                    return "conveyor_icon";
                    break;
                default:
                    return "table_icon";
                    
            }
        }

        private string GetIconLabelText(IconType type)
        {
            switch(type)
            {
                case IconType.table:
                    return "Table";
                    break;
                case IconType.worker:
                    return "Worker";
                    break;
                case IconType.machine:
                    return "Machine";
                    break;
                case IconType.trolley:
                    return "Trolley";
                    break;
                case IconType.converyor:
                    return "Conveyor";
                    break;
                default:
                    return "Table";

            }   
        }
           

        private void OnFloorDrop(FloorDropEvent evt)
        {
            Debug.Log("dropping on floor");
        }
    }
}

