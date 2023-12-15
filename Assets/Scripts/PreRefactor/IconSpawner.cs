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
        private const string PRE_KAIZEN_FLOOR = "ve_pre_kaizen_layout_area";
        private const string POST_KAIZEN_FLOOR = "ve_post_kaizen_layout_area";
       
        //event strings
        public const string ICON_SPAWNED_EVENT = "IconSpawned";
        public const string ICON_SPAWNED_EVENT_KEY = "floorIcon";
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
                _floor = _dragArea.Q<VisualElement>(PRE_KAIZEN_FLOOR);
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

            _floor = root.Q<VisualElement>(PRE_KAIZEN_FLOOR);

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
            IconInfo iconModel = GetIconModel(iconName);
            iconContainer.userData = iconModel;

        }


        private void OnIconSelected(PointerDownEvent evt)
        {
            VisualElement icon = evt.currentTarget as VisualElement;
            string iconName = icon.Q<Label>().text;
            VisualElement iconImage = icon.Q<VisualElement>(ICON_IMAGE);

            //Get icon from pool
            VisualElement draggableIcon = GetIcon();

            //set user data
            IconInfo templateInfo = (IconInfo)icon.userData;
            IconType iconType = templateInfo.Type;
            IconInfo draggableInfo = new IconInfo { Type = iconType };
            
            draggableIcon.userData = draggableInfo;

            //set styles; should only need one style per icon type
            if (iconName == "Custom Label")
            {
                TextField textField = new TextField();
                draggableIcon.Add(textField);
                textField.AddToClassList("label_text_field");
            }

            VisualElement container = draggableIcon.Q<VisualElement>(ICON_DRAGGABLE);
            string containerClass = GetFloorIconContainerStyle(draggableInfo.Type);
            //info.styleClass = containerClass;
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
            EventManager.TriggerEvent(ICON_SPAWNED_EVENT, new Dictionary<string, object> { { ICON_SPAWNED_EVENT_KEY, floorIcon } });
        }
      
        private void OnIconDropped(Vector2 dropPosition, VisualElement droppedIcon)
        {
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
                //FloorIcon floorIcon = new FloorIcon(droppedIcon, _dragArea, _floor);

                int iconWidth = (int)droppedIcon.resolvedStyle.width;
                int iconHeight = (int)droppedIcon.resolvedStyle.height;
                //LayoutIconInfo iconInfo = droppedIcon.userData as LayoutIconInfo;
                IconInfo iconInfo = (IconInfo)droppedIcon.userData;
                iconInfo.Height = iconHeight;
                iconInfo.Width = iconWidth;
               
                var pos = droppedIcon.transform.position;
                object[] args = new object[] {iconInfo, pos };

                EventManager.TriggerEvent(ICON_SPAWNED_EVENT, new Dictionary<string, object> { { ICON_SPAWNED_EVENT_KEY, args } });
                EventManager.TriggerEvent(SELECTION_EVENT, new Dictionary<string, object> { { ICON_INFO, args } });
            }

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
                case "Photo":
                    info = new LayoutIconInfo(IconType.Photo);
                    break;
                default:
                    break;
            }
            return info;
        }


        private IconInfo GetIconModel (string iconName)
        {
            IconInfo iconModel;
            switch (iconName)
            {
                case "Table":
                    iconModel = new IconInfo { Type = IconType.Table };
                    break;
                case "Trolley":
                    iconModel = new IconInfo{ Type = IconType.Trolley };
                    break;
                case "Worker":
                    iconModel = new IconInfo{ Type = IconType.Worker };
                    break;
                case "Conveyor":
                    iconModel = new IconInfo{ Type = IconType.Conveyor };
                    break;
                case "Machine":
                    iconModel = new IconInfo{ Type = IconType.Machine };
                    break;
                case "Product":
                    iconModel = new IconInfo{ Type = IconType.Product };
                    break;
                case "Kanban":
                    iconModel = new IconInfo{ Type = IconType.Kanban };
                    break;
                case "Parts Shelf":
                    iconModel = new IconInfo{ Type = IconType.PartsShelf };
                    break;
                case "Custom Item":
                    iconModel = new IconInfo{ Type = IconType.CustomItem };
                    break;
                case "Custom Label":
                    iconModel = new IconInfo{ Type = IconType.CustomLabel };
                    break;
                case "Product Flow":
                    iconModel = new IconInfo{ Type = IconType.ProductFlow };
                    break;
                case "Worker Movement":
                    iconModel = new IconInfo{ Type = IconType.WorkerMovement };
                    break;
                case "Transport Flow":
                    iconModel = new IconInfo{ Type = IconType.TransportFlow };
                    break;
                case "Photo":
                    iconModel = new IconInfo{ Type = IconType.Photo };
                    break;
                default:
                    return new IconInfo();

            }
            return iconModel;
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

