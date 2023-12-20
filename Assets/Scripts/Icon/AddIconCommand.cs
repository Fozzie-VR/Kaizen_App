using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizenApp
{
    public class AddIconCommand: ICommand
    {

        public const string ADD_ICON_COMMAND = "AddIconCommand";
        public const string ADD_ICON_COMMAND_KEY = "AddIconCommandArgs";
        private object _commandArgs;

        public AddIconCommand(object commandArgs)
        {
            _commandArgs = commandArgs;
        }

        public void Execute()
        {
            EventManager.TriggerEvent(ADD_ICON_COMMAND, new Dictionary<string, object>
            {
                {
                    ADD_ICON_COMMAND_KEY,
                    _commandArgs
                }
            });
        }

        public void Undo()
        {
            EventManager.TriggerEvent(RemoveIconCommand.REMOVE_ICON_COMMAND, new Dictionary<string, object>
            {
                {
                    RemoveIconCommand.REMOVE_ICON_COMMAND_KEY,
                    _commandArgs
                }
            });
        }
    }

}
