using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{

    public enum IconType { Worker, CustomItem, CustomLabel, ProductFlow, WorkerMovement, 
        TransportFlow, Product, Table, Trolley, Machine, Conveyor, Kanban, PartsShelf }
    public class LayoutIconInfo
    {
        public IconType Type;
        public float Width;
        public float Height;
        public float Rotation;
        public Vector2 Position;
        public Vector2 LocalPosition;
        public VisualElement IconElement;
        public FloorIcon FloorIcon;

        public LayoutIconInfo(IconType iconType)
        {
            Type = iconType;
           
        }
    }

}
