using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class IconSpawner
    {
        private const string DRAG_AREA = "ve_layout_container";
        private const string ICON_DRAGGABLE = "ve_icon_container";
        private const string ICON_IMAGE = "ve_icon_image";
        private const string FLOOR = "ve_layout_area";
        private const string ICON_IMAGE_STYLE = "icon_image";
        private const string ICON_CONTAINER_STYLE = "icon_container";
        private const string ICON_LABEL_STYLE = "icon_label";

        private VisualElement _tableIcon;
        private VisualElement _trolleyIcon;
        private VisualElement _workerIcon;
        private VisualElement _conveyorIcon;
        private VisualElement _kanbanIcon;
        private VisualElement _machineIcon;
        private VisualElement _productFlowIcon;

        private VisualElement _dragArea;
        private VisualElement _floor;
        private List<VisualElement> _iconDraggables = new List<VisualElement>();
        private Dictionary<VisualElement, Vector3> _iconPositions = new Dictionary<VisualElement, Vector3>();
        private IconFactory<VisualElement> _iconFactory = new IconFactory<VisualElement>();

        public event Action<Vector2, VisualElement> DropIcon;
        public IconSpawner(VisualElement root)
        {
            DropIcon += OnIconDropped;
            SetVisualElements(root);
            _iconFactory.Factory += GetIcon;
            _iconFactory.PreReturn += ReturnIcon;
            
        }

        private void SetVisualElements(VisualElement root)
        {
            _dragArea = root.Q<VisualElement>(DRAG_AREA);

            _floor = root.Q<VisualElement>(FLOOR);

            _iconDraggables = root.Query<VisualElement>(ICON_DRAGGABLE).ToList();
            foreach (VisualElement iconContainer in _iconDraggables)
            {
                SetupSpawnerIcons(iconContainer);
            }

        }

        private void SetupSpawnerIcons(VisualElement iconContainer)
        {
            string iconName = iconContainer.Q<Label>().text;
            switch (iconName)
            {
                case "Table":
                    _tableIcon = iconContainer;
                   
                    _iconPositions.Add(_tableIcon, _tableIcon.transform.position);
                    _tableIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.table,
                        Name = "table",
                        IsFloorIcon = false
                    };
                    IconMover mover = new IconMover(_tableIcon, _dragArea, DropIcon);
                    break;

                case "Trolley":
                    _trolleyIcon = iconContainer;
                   
                    _iconPositions.Add(_trolleyIcon, _trolleyIcon.transform.position);
                    _trolleyIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.trolley,
                        Name = "trolley",
                        IsFloorIcon = false
                    };
                    IconMover mover2 = new IconMover(_trolleyIcon, _dragArea, DropIcon);
                    break;
                case "Worker":
                    _workerIcon = iconContainer;
                   
                    _iconPositions.Add(_workerIcon, _workerIcon.transform.position);
                    _workerIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.worker,
                        Name = "worker",
                        IsFloorIcon = false
                    };
                    IconMover mover3 = new IconMover(_workerIcon, _dragArea, DropIcon);
                    break;
                case "Conveyor":
                    _conveyorIcon = iconContainer;
                   
                    _iconPositions.Add(_conveyorIcon, _conveyorIcon.transform.position);
                    _conveyorIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.converyor,
                        Name = "conveyor",
                        IsFloorIcon = false
                    };
                    IconMover mover4 = new IconMover(_conveyorIcon, _dragArea, DropIcon);
                    break;
                case "Machine":
                    _machineIcon = iconContainer;
                    
                    _iconPositions.Add(_machineIcon, _machineIcon.transform.position);
                    _machineIcon.userData = new LayoutIconInfo()
                    {
                        Type = IconType.machine,
                        Name = "machine",
                        IsFloorIcon = false
                    };
                    IconMover mover5 = new IconMover(_machineIcon, _dragArea, DropIcon);
                    break;
                default:
                    break;
            }
        }

        private void OnIconDropped(Vector2 dropPosition, VisualElement droppedIcon)
        {
            Debug.Log("Icon dropped");
            //check if icon is dropped on floor and spawn floor icon if it is
           
            LayoutIconInfo info = droppedIcon.userData as LayoutIconInfo;
            float xOffset = droppedIcon.resolvedStyle.width / 2;
            float yOffset = droppedIcon.resolvedStyle.height / 2;
            var position = _dragArea.WorldToLocal(dropPosition);
          
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
                position.x -= xOffset;
                position.y -= yOffset;  
               SpawnFloorIcon(position, info);
                
            }

            //return to start position
            Vector3 startPosition = _iconPositions[droppedIcon];
            droppedIcon.transform.position = startPosition;
        }

        private void SpawnFloorIcon(Vector3 position, LayoutIconInfo info)
        {
            VisualElement icon = _iconFactory.GetIcon();
            VisualElement iconImage = icon.Q<VisualElement>(ICON_IMAGE);
            string imageClass = GetImageIconStyleClass(info.Type);
            iconImage.AddToClassList(imageClass);
            Label label = icon.Q<Label>();
            label.text = GetIconLabelText(info.Type);

            icon.style.translate = new Translate(position.x, position.y);

            LayoutIconInfo iconInfo = new LayoutIconInfo()
            {
                Type = info.Type,
                Name = info.Name,
                IsFloorIcon = true
            };
            icon.userData = iconInfo;
            FloorIcon floorIcon = new FloorIcon(icon, _dragArea, _floor, _iconFactory);
            KaizenAppManager._instance.KaizenEvents.OnFloorIconSpawned(floorIcon);
        }

        private VisualElement GetIcon()
        {
            VisualElement iconContainer = new VisualElement();
            iconContainer.AddToClassList("icon_container");
            iconContainer.style.position = new StyleEnum<Position>(Position.Absolute);
            iconContainer.name = ICON_DRAGGABLE;

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
            if (icon.parent == _floor)
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
            switch (type)
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


    }

}


