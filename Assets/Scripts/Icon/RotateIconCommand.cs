using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class RotateIconCommand : ICommand
    {
        public const string ICON_ROTATE_COMMAND = "iconRotateCommand";
        public const string ICON_ROTATE_COMMAND_KEY = "iconRotateCommandKey";

        public const string ICON_ROTATE_COMMAND_UNDO = "iconRotateCommandUndo";
        public const string ICON_ROTATE_COMMAND_UNDO_KEY = "iconRotateCommandUndoKey";

        private IconViewInfo _newInfo;
        private IconViewInfo _oldInfo;

        public RotateIconCommand(IconViewInfo newInfo, IconViewInfo oldInfo)
        {
            _newInfo = newInfo;
            _oldInfo = oldInfo;
        }

        public void Execute()
        {
           EventManager.TriggerEvent(ICON_ROTATE_COMMAND, new Dictionary<string, object> 
           { 
               { ICON_ROTATE_COMMAND_KEY, _newInfo } 
           });
        }

        public void Undo()
        {
            EventManager.TriggerEvent(ICON_ROTATE_COMMAND, new Dictionary<string, object>
           {
               { ICON_ROTATE_COMMAND_KEY, _oldInfo }
           });
        }
    }

}
