using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class SetFloorSizeCommand : ICommand
    {
        public const string FLOOR_SIZE_SET_EVENT = "floorSizeSet";
        public const string FLOOR_SIZE_SET_EVENT_KEY = "floorSizeSetKey";

        private FloorDimensions _newDimensions;
        private FloorDimensions _oldDimensions;
        
        public SetFloorSizeCommand(FloorDimensions newDimensions, FloorDimensions oldDimensions)
        {
            Debug.Log("SetFloorSizeCommand");
            _newDimensions = newDimensions;
            _oldDimensions = oldDimensions;
        }
        public void Execute()
        {
            EventManager.TriggerEvent(FLOOR_SIZE_SET_EVENT, new Dictionary<string, object>()
            {
                {FLOOR_SIZE_SET_EVENT_KEY, _newDimensions}
            });
        }

        public void Undo()
        {
            EventManager.TriggerEvent(FLOOR_SIZE_SET_EVENT, new Dictionary<string, object>()
            {
                {FLOOR_SIZE_SET_EVENT_KEY, _oldDimensions}
            });
        }
    }

}
