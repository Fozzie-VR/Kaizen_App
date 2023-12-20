using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public enum IconType
    {
        Worker, CustomItem, CustomLabel, ProductFlow, WorkerMovement,
        TransportFlow, Product, Table, Trolley, Machine, Conveyor, Kanban, PartsShelf, Photo
    }
    public struct IconInfo
    {
        public int Id;
        public IconType Type;
        public float Width;
        public float Height;
        public Vector3 Position;
        public Vector3 LocalPosition;
        public float Rotation;
    }

}

