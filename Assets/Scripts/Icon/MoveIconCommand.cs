using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class MoveIconCommand : ICommand
    {
        public const string ICON_MOVE_COMMAND = "iconMoveCommand";
        public const string ICON_MOVE_COMMAND_KEY = "iconMoveCommandKey";


        private int _iconID;
        private Vector3 _newPosition;
        private Vector2 _newLocalPosition;
        private Vector3 _oldPosition;
        private Vector2 _oldLocalPosition;
        

        public MoveIconCommand(int iconID, Vector3 position, Vector2 localPosition,
            Vector3 oldPosition, Vector2 oldLocalPosition)
        {
            _iconID = iconID;
            _newPosition = position;
            _newLocalPosition = localPosition;
            _oldLocalPosition = oldLocalPosition;
            _oldPosition = oldPosition;
        }

        public void Execute()
        {
            object[] args = new object[] {_iconID, _newPosition, _newLocalPosition};
            EventManager.TriggerEvent(ICON_MOVE_COMMAND, 
                new Dictionary<string, object> { { ICON_MOVE_COMMAND_KEY,args } });
        }

        public void Undo()
        {
            object[] args = new object[] {_iconID, _oldPosition, _oldLocalPosition};
            EventManager.TriggerEvent(ICON_MOVE_COMMAND, 
                               new Dictionary<string, object> { { ICON_MOVE_COMMAND_KEY,args } });
        }
    }

}
