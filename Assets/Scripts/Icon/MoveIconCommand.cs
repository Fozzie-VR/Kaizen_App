using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class MoveIconCommand : ICommand
    {
        public const string ICON_MOVE_COMMAND = "iconMoveCommand";
        public const string ICON_MOVE_COMMAND_KEY = "iconMoveCommandKey";
       
        private IconViewInfo _newInfo;
        private IconViewInfo _oldInfo;
        

        public MoveIconCommand(IconViewInfo newInfo, IconViewInfo oldInfo)
        {
            _newInfo = newInfo;
            _oldInfo = oldInfo;
        }

        public void Execute()
        {
            
            EventManager.TriggerEvent(ICON_MOVE_COMMAND, 
                new Dictionary<string, object> { { ICON_MOVE_COMMAND_KEY,_newInfo } });
        }

        public void Undo()
        {
            EventManager.TriggerEvent(ICON_MOVE_COMMAND,
                new Dictionary<string, object> { { ICON_MOVE_COMMAND_KEY, _oldInfo } });
        }
    }

}
