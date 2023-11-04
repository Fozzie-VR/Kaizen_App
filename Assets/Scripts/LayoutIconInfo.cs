using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{

    public enum IconType { Worker, CustomItem, CustomLabel, ProductFlow, WorkerMovement, 
        TransportFlow, Product, Table, Trolley, Machine, Conveyor, Kanban, PartsShelf, Photo }
    public class LayoutIconInfo
    {
        public IconType Type;
        public string styleClass;
        public float Width;
        public float Height;
        public float Rotation;
        public Vector2 Position;
        public Vector2 LocalPosition;
        public VisualElement IconElement;
        public FloorIcon FloorIcon;
        public Texture2D PhotoTexture;

        public LayoutIconInfo(IconType iconType)
        {
            Type = iconType;
        }

        public LayoutIconInfo GetClone()
        {
            LayoutIconInfo clone = new LayoutIconInfo(Type);
            clone.styleClass = styleClass;
            clone.Width = Width;
            clone.Height = Height;
            clone.Rotation = Rotation;
            clone.Position = Position;
            clone.LocalPosition = LocalPosition;
            clone.IconElement = null;
            clone.FloorIcon = null;
            clone.PhotoTexture = null;
            return clone;
        }
    }

}
