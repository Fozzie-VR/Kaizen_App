using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class IconSpawner
    {
        private const string DRAG_AREA = "ve_floor_plan_screen";
        private const string ICON_DRAGGABLE = "ve_icon_container";
        private const string ICON_IMAGE = "ve_icon_image";
        private const string FLOOR = "ve_layout_area";
        private const string ICON_IMAGE_STYLE = "icon_image";
        private const string ICON_CONTAINER_STYLE = "icon_container";
        private const string ICON_LABEL_STYLE = "icon_label";
        private const string ICON_SPAWNED_EVENT = "IconSpawned";
        private const string ICON_SPAWNED_EVENT_KEY = "floorIcon";
       
        private VisualElement _dragArea;
        private VisualElement _floor;
        private List<VisualElement> _iconDraggables = new List<VisualElement>();
        private Dictionary<VisualElement, Vector3> _iconPositions = new Dictionary<VisualElement, Vector3>();
        private IconFactory<VisualElement> _iconFactory = new IconFactory<VisualElement>();

        public event Action<Vector2, VisualElement> DropIcon;
        private IconMover _iconMover;
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
                iconContainer.RegisterCallback<PointerDownEvent>(OnIconSelected);
                //SetupSpawnerIcons(iconContainer);
            }
        }
        

        private void OnIconSelected(PointerDownEvent evt)
        {
            Debug.Log("Icon selected");
            VisualElement icon = evt.currentTarget as VisualElement;

            //Get icon from pool
            VisualElement draggableIcon = GetIcon();

            //Set icon position to mouse position
            float xOffset = icon.resolvedStyle.width / 2;
            float yOffset = icon.resolvedStyle.height / 2;
            Vector2 pos = new Vector2(evt.position.x - xOffset, evt.position.y - yOffset);
            draggableIcon.transform.position = _dragArea.WorldToLocal(pos);

            //Set icon info, image and label
            string iconName = icon.Q<Label>().text; 
            LayoutIconInfo info = GetIconInfo(iconName);
            draggableIcon.userData = info;
            VisualElement iconImage = draggableIcon.Q<VisualElement>(ICON_IMAGE);
            string imageClass = GetImageIconStyleClass(info.Type);
            iconImage.AddToClassList(imageClass);
            Label label = draggableIcon.Q<Label>();
            label.text = GetIconLabelText(info.Type);
            
            //setup icon mover
            _iconMover = new IconMover(draggableIcon, _dragArea, DropIcon);
            _iconMover.StartDragging(evt);
            
        }

        private void OnIconDropped(Vector2 dropPosition, VisualElement droppedIcon)
        {
            Debug.Log("spawner on icon dropped");
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

            if(!floorContainsIcon)
            {
                ReturnIcon(droppedIcon);
            }
            else
            {
                _iconMover.UnregisterDropAction(DropIcon);
                _iconMover = null;
                FloorIcon floorIcon = new FloorIcon(droppedIcon, _dragArea, _floor, _iconFactory);
                EventManager.TriggerEvent(ICON_SPAWNED_EVENT, new Dictionary<string, object> { { ICON_SPAWNED_EVENT_KEY, floorIcon } });
               
            }

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

            _dragArea.Add(iconContainer);
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

        private LayoutIconInfo GetIconInfo(string iconName)
        {
            LayoutIconInfo info = null;
            switch (iconName)
            {
                case "Table":
                    info = new LayoutIconInfo(IconType.Table);
                    break;
                case "Trolley":
                    info = new LayoutIconInfo(IconType.Trolley);
                    break;
                case "Worker":
                    info = new LayoutIconInfo(IconType.Worker);
                    break;
                case "Conveyor":
                    info = new LayoutIconInfo(IconType.Conveyor);
                    break;
                case "Machine":
                    info = new LayoutIconInfo(IconType.Machine);
                    break;
                case "Product":
                    info = new LayoutIconInfo(IconType.Product);
                    break;
                case "Kanban":
                    info = new LayoutIconInfo(IconType.Kanban);
                    break;
                case "Parts Shelf":
                    info = new LayoutIconInfo(IconType.PartsShelf);
                    break;
                case "Custom Item":
                    info = new LayoutIconInfo(IconType.CustomItem);
                    break;
                case "Custom Label":
                    info = new LayoutIconInfo(IconType.CustomLabel);
                    break;
                case "Product Flow":
                    info = new LayoutIconInfo(IconType.ProductFlow);
                    break;
                case "Worker Movement":
                    info = new LayoutIconInfo(IconType.WorkerMovement);
                    break;
                case "Transport Flow":
                    info = new LayoutIconInfo(IconType.TransportFlow);
                    break;
                default:
                    break;
            }
            return info;
        }

        private string GetImageIconStyleClass(IconType type)
        {
            switch (type)
            {
                case IconType.CustomItem:
                    return "custom_item_icon";
                    break;
                case IconType.CustomLabel:
                    return "custom_label_icon";
                    break;
                case IconType.ProductFlow:
                    return "flow_icon";
                    break;
                case IconType.WorkerMovement:
                    return "movement_icon";
                    break;
                case IconType.TransportFlow:
                    return "transport_icon";
                    break;
                case IconType.Product:
                    return "product_icon";
                    break;
                case IconType.Kanban:
                    return "kanban_icon";
                    break;
                case IconType.PartsShelf:
                    return "parts_shelf_icon";
                    break;
                case IconType.Table:
                    return "table_icon";
                    break;
                case IconType.Worker:
                    return "worker_icon";
                    break;
                case IconType.Machine:
                    return "machine_icon";
                    break;
                case IconType.Trolley:
                    return "trolley_icon";
                    break;
                case IconType.Conveyor:
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
                case IconType.CustomItem:
                    return "Custom Item";
                    break;
                case IconType.CustomLabel:
                    return "Label";
                    break;
                case IconType.ProductFlow:
                    return "Product Flow";
                    break;
                case IconType.WorkerMovement:
                    return "Movement";
                    break;
                case IconType.TransportFlow:
                    return "Transport";
                    break;
                case IconType.Product:
                    return "Product";
                    break;
                case IconType.Kanban:
                    return "Kanban";
                    break;
                case IconType.PartsShelf:
                    return "Parts Shelf";
                    break;
                case IconType.Table:
                    return "Table";
                    break;
                case IconType.Worker:
                    return "Worker";
                    break;
                case IconType.Machine:
                    return "Machine";
                    break;
                case IconType.Trolley:
                    return "Trolley";
                    break;
                case IconType.Conveyor:
                    return "Conveyor";
                    break;
                default:
                    return "Table";

            }
        }


    }

}


