using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{

    public enum IconType { table, trolley, worker, converyor, kanban, machine, product_flow }
    public class LayoutIconInfo
    {
        public string Name;
        public IconType Type;
        public bool IsFloorIcon;
        public float Width;
        public float Length;
    }

}
