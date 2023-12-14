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
    public struct IconModel
    {
        public int Id;
        public IconType Type;
        public float Width;
        public float Height;
    }

}

