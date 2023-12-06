using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class FloorDimensionsPage: IView
    {
        private const string FLOOR_DIMENSIONS_SET_EVENT = "floor_dimensions_set";
        private const string FLOOR_DIMENSIONS_SET_EVENT_KEY = "floor_dimensions";

        private FloatField _floorHeight;
        private FloatField _floorWidth;

        private float _floorWidthMeters;
        public float FloorWidthMeters => _floorWidthMeters;

        private float _floorHeightMeters;
        public float FloorHeightMeters => _floorHeightMeters;

        public FloorDimensionsPage(VisualElement root)
        {
            _floorHeight = root.Q<FloatField>("float_floor_length");
            _floorWidth = root.Q<FloatField>("float_floor_width");

            Button finished = root.Q<Button>("btn_finished");
            finished.clicked += OnFinishedClicked;
        }

        private void OnFinishedClicked()
        {
           FloorDimensions floorDimensions = new FloorDimensions 
           { 
                FloorHeightMeters = _floorHeight.value,
                FloorWidthMeters = _floorWidth.value
           };
           EventManager.TriggerEvent(FLOOR_DIMENSIONS_SET_EVENT, new Dictionary<string, object> { { FLOOR_DIMENSIONS_SET_EVENT_KEY, floorDimensions } });
        }
    }

    public struct FloorDimensions
    {
        public float FloorWidthMeters;
        public float FloorHeightMeters;
    }   
}
