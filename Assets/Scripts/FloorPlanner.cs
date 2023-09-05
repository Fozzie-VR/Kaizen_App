using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    //Manages layout of icons on the visual element that represents the floor
    //Handles pointer events related to the floor plan
    //Keeps track of icons on the floor
    //Keeps floor measurements and handles scaling
    //could refactor to remove Pointer events related to the icons to a separate class

    public class FloorPlanner

    {
        private const string DRAG_AREA = "ve_layout_container";
        private const string START_DRAG = "ve_icon_container";
        
        private const string ICON_IMAGE = "ve_icon_image";
        private const string FLOOR = "ve_layout_area";

        private const string ICON_IMAGE_STYLE = "icon_image";
        private const string ICON_CONTAINER_STYLE = "icon_container";
        private const string ICON_LABEL_STYLE = "icon_label";
    

        private VisualElement _floor;

        private List<FloorIcon> _floorIcons = new();


        public FloorPlanner(VisualElement root)
        {
           _floor = root.Q(FLOOR);
            KaizenAppManager._instance.KaizenEvents.FloorIconSpawned += AddIcon;
        }

        public void AddIcon(FloorIcon icon)
        {
            _floorIcons.Add(icon);
        }

        public void RemoveIcon(FloorIcon icon)
        {
            _floorIcons.Remove(icon);
        }

       

       
    }
}

