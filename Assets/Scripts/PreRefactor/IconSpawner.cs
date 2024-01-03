using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class IconSpawner
    {
        //visual element strings
        private const string DRAG_AREA = "ve_floor_plan_screen";
        private const string ICON_DRAGGABLE = "ve_icon_container";
        private const string ICON_IMAGE = "ve_icon_image";
        private const string FLOOR_ELEMENT = "ve_floor";
        private const string POST_KAIZEN_FLOOR = "ve_post_kaizen_layout_area";
       
        //event strings
        public const string ICON_SPAWN_REQUESTED = "IconSpawned";
        public const string ICON_SPAWN_REQUESTED_KEY = "floorIcon";
        public const string ICON_CLONE_EVENT = "SpawnIcon";
        public const string ICON_CLONE_EVENT_KEY = "icon";

        private const string SELECTION_EVENT = "IconSelected";
        private const string ICON_INFO = "iconInfo";

        private const string ICON_REMOVED_EVENT = "IconRemoved";
        private const string FLOOR_ICON_EVENT_KEY = "floorIcon";

        //page state now tracked another way
        private const string SWITCH_KAIZEN_LAYOUT_CLICKED = "post_kaizen_layout_clicked";
        private const string POST_KAIZEN_LAYOUT_EVENT_KEY = "post_kaizen";

        //Visual Elements
        private VisualElement _dragArea;
        private VisualElement _floor;
        private List<VisualElement> _iconDraggables = new List<VisualElement>();
        
        private IconFactory<VisualElement> _iconFactory = new IconFactory<VisualElement>();

        public event Action<Vector2, VisualElement> DropIcon;
        
        //Icon mover is a controller that handles dragging and dropping icons
        private IconMover _iconMover;
        

        public IconSpawner(VisualElement root)
        {
            DropIcon += OnIconDropped;
            SetVisualElements(root);

            _iconFactory.Factory += GetIcon;
            _iconFactory.PreGet += PreGetIcon;
            _iconFactory.PreReturn += ReturnIcon;

            EventManager.StartListening(ICON_REMOVED_EVENT, OnIconRemoved);
            EventManager.StartListening(SWITCH_KAIZEN_LAYOUT_CLICKED, OnSwitchKaizenLayoutClicked);
            EventManager.StartListening(ICON_CLONE_EVENT, OnIconClone);
            
        }
      
        private void OnIconClone(Dictionary<string, object> eventDictionary)
        {
           var icon = eventDictionary[ICON_CLONE_EVENT_KEY] as VisualElement;
           CloneIcon(icon);
        }

        private void OnSwitchKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            bool isPostKaizenLayout = (bool)dictionary[POST_KAIZEN_LAYOUT_EVENT_KEY];
            if (isPostKaizenLayout)
            {
                _floor = _dragArea.Q<VisualElement>(POST_KAIZEN_FLOOR);
            }
            else
            {
                _floor = _dragArea.Q<VisualElement>(FLOOR_ELEMENT);
            }
        }

        private void OnIconRemoved(Dictionary<string, object> dictionary)
        {
            var icon = dictionary[FLOOR_ICON_EVENT_KEY] as FloorIcon;
            var iconInfo = icon.IconInfo;
            var iconElement = iconInfo.IconElement;
            ReturnIcon(iconElement);
        }

        private void SetVisualElements(VisualElement root)
        {
            _dragArea = root.Q<VisualElement>(DRAG_AREA);

            _floor = root.Q<VisualElement>(FLOOR_ELEMENT);

            _iconDraggables = root.Query<VisualElement>(ICON_DRAGGABLE).ToList();
            foreach (VisualElement iconContainer in _iconDraggables)
            {
                iconContainer.RegisterCallback<PointerDownEvent>(OnIconSelected);
                SetUserData(iconContainer);
            }
        }

        private void SetUserData(VisualElement iconContainer)
        {
            string iconName = iconContainer.Q<Label>().text;
            IconType iconType = GetIconType(iconName);
            iconContainer.userData = iconType;

        }


        private void OnIconSelected(PointerDownEvent evt)
        {
            VisualElement icon = evt.currentTarget as VisualElement;
            string iconName = icon.Q<Label>().text;
            VisualElement iconImage = icon.Q<VisualElement>(ICON_IMAGE);

            //Get icon from pool
            VisualElement draggableIcon = _iconFactory.GetIcon();

            //set user data
            IconType iconType = (IconType)icon.userData;
            draggableIcon.userData = iconType;

            //set styles; should only need one style per icon type
            if (iconName == "Custom Label")
            {
                TextField textField = new TextField();
                draggableIcon.Add(textField);
                textField.AddToClassList("label_text_field");
            }

            VisualElement container = draggableIcon.Q<VisualElement>(ICON_DRAGGABLE);
            string containerClass = GetFloorIconContainerStyle(iconType);
            
            container.AddToClassList(containerClass);

            //Set icon position to mouse position
            Vector2 position = _dragArea.WorldToLocal(evt.position);
            float xOffset = iconImage.resolvedStyle.width / 2;
            float yOffset = iconImage.resolvedStyle.height / 2;
            draggableIcon.transform.position = new Vector2(position.x - xOffset, position.y - yOffset);

           //create icon mover
            _iconMover = new IconMover(draggableIcon, _dragArea, DropIcon);
            _iconMover.StartDragging(evt);
        }
        

        private void CloneIcon(VisualElement icon)
        {
            VisualElement clone = GetIcon();
            LayoutIconInfo iconInfo = icon.userData as LayoutIconInfo;
            LayoutIconInfo cloneInfo = iconInfo.GetClone();
            clone.userData = cloneInfo;
            clone.transform.position = icon.transform.position;
            clone.transform.rotation = icon.transform.rotation;
            clone.style.width = icon.resolvedStyle.width;
            clone.style.height = icon.resolvedStyle.height;
            //Debug.Log("clone width: " + clone.style.width.value);
            clone.AddToClassList(iconInfo.styleClass);

            FloorIcon floorIcon = new FloorIcon(clone, _dragArea, _floor);
            EventManager.TriggerEvent(ICON_SPAWN_REQUESTED, new Dictionary<string, object> { { ICON_SPAWN_REQUESTED_KEY, floorIcon } });
        }
      
        private void OnIconDropped(Vector2 dropPosition, VisualElement droppedIcon)
        {
            var localPos = _floor.WorldToLocal(dropPosition);
            bool floorContainsIcon = _floor.ContainsPoint(localPos);
            float xOffset = droppedIcon.resolvedStyle.width / 2;
            float yOffset = droppedIcon.resolvedStyle.height / 2;

            if (localPos.x - xOffset < 0)
            {
                floorContainsIcon = false;
            }

            if (localPos.x + xOffset > _floor.resolvedStyle.width)
            {
                floorContainsIcon = false;
            }

            if (localPos.y - yOffset < 0)
            {
                floorContainsIcon = false;
            }

            if (localPos.y + yOffset > _floor.resolvedStyle.height)
            {
                floorContainsIcon = false;
            }

            if(!floorContainsIcon)
            {
               _iconFactory.ReturnIcon(droppedIcon);
            }
            else
            {
                _iconMover.UnregisterDropAction(DropIcon);
                _iconMover = null;
                //FloorIcon floorIcon = new FloorIcon(droppedIcon, _dragArea, _floor);

                int iconWidth = (int)droppedIcon.resolvedStyle.width;
                int iconHeight = (int)droppedIcon.resolvedStyle.height;
                
                IconType iconType = (IconType)droppedIcon.userData;
               
                var pos = droppedIcon.transform.position;
                object[] args = new object[] {iconType, pos, localPos, iconWidth, iconHeight };

                EventManager.TriggerEvent(ICON_SPAWN_REQUESTED, new Dictionary<string, object> { { ICON_SPAWN_REQUESTED_KEY, args } });
                
                ReturnIcon(droppedIcon);
            }

        }

        private void PreGetIcon(VisualElement icon)
        {
            icon.RemoveFromClassList("hidden");
        }
       
        private VisualElement GetIcon()
        {
            VisualElement iconContainer = new VisualElement();
            iconContainer.style.position = new StyleEnum<Position>(Position.Absolute);
            iconContainer.name = ICON_DRAGGABLE;
            iconContainer.usageHints = UsageHints.DynamicTransform;

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

        #region IconSwitchGetters
        private IconType GetIconType(string iconName)
        {
            IconType iconType = IconType.Table;
            switch (iconName)
            {
                case "Table":
                    iconType = IconType.Table;
                    break;
                case "Trolley":
                    iconType = IconType.Trolley;
                    break;
                case "Worker":
                    iconType = IconType.Worker;
                    break;
                case "Conveyor":
                    iconType = IconType.Conveyor;
                    break;
                case "Machine":
                    iconType = IconType.Machine;
                    break;
                case "Product":
                    iconType = IconType.Product;
                    break;
                case "Kanban":
                    iconType = IconType.Kanban;
                    break;
                case "Parts Shelf":
                    iconType = IconType.PartsShelf;
                    break;
                case "Custom Item":
                    iconType = IconType.CustomItem;
                    break;
                case "Custom Label":
                    iconType = IconType.CustomLabel;
                    break;
                case "Product Flow":
                    iconType = IconType.ProductFlow;
                    break;
                case "Worker Movement":
                    iconType = IconType.WorkerMovement;
                    break;
                case "Transport Flow":
                    iconType = IconType.TransportFlow;
                    break;
                case "Photo":
                    iconType = IconType.Photo;
                    break;
                default:
                    break;
            }
            return iconType;
        }

        private string GetFloorIconContainerStyle(IconType iconType)
        {
            switch (iconType)
            {
                case IconType.CustomItem:
                    return "floor_custom_item";
                    break;
                case IconType.CustomLabel:
                    return "floor_custom_label";
                    break;
                case IconType.ProductFlow:
                    return "floor_product_flow";
                    break;
                case IconType.WorkerMovement:
                    return "floor_worker_movement";
                    break;
                case IconType.TransportFlow:
                    return "floor_transport_flow";
                    break;
                case IconType.Product:
                    return "floor_product";
                    break;
                case IconType.Kanban:
                    return "floor_kanban";
                    break;
                case IconType.PartsShelf:
                    return "floor_parts_shelf";
                    break;
                case IconType.Table:
                    return "floor_table";
                    break;
                case IconType.Worker:
                    return "floor_worker";
                    break;
                case IconType.Machine:
                    return "floor_machine";
                    break;
                case IconType.Trolley:
                    return "floor_trolley";
                    break;
                case IconType.Conveyor:
                    return "floor_conveyor";
                    break;
                case IconType.Photo:
                    return "floor_photo";
                    break;
                default:
                    return "floor_table";

            }
        }
        #endregion

    }

}

