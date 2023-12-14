using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizenApp
{
    public class LayoutChanger : ICommand
    {
        public const string LAYOUT_CHANGE_EVENT = "LayoutChangeEvent";
        public const string LAYOUT_CHANGE_EVENT_KEY = "LayoutChangeKey";

        public enum LayoutChangeType
        {
            IconAdded,
            IconRemoved,
            IconMoved,
            IconResized,
            IconRotated,
            FloorDimensionsChanged,
            PixelsPerMeterChanged
        }

        public void Execute()
        {
            
        }

        public void Undo()
        {
            
        }
       
    }

}
