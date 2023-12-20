using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class RemoveIconCommand: ICommand
    {

        public const string REMOVE_ICON_COMMAND = "RemoveIconCommand";
        public const string REMOVE_ICON_COMMAND_KEY = "RemoveIconCommandArgs";
        private object _commandArgs;

        //command args need all icon data for undo
        public RemoveIconCommand(object commandArgs)
        {
            _commandArgs = commandArgs;
        }

        public void Execute()
        {
            EventManager.TriggerEvent(REMOVE_ICON_COMMAND, new Dictionary<string, object>
            {
                {
                    REMOVE_ICON_COMMAND_KEY,
                    _commandArgs
                }
            });
        }

        public void Undo()
        {
            EventManager.TriggerEvent(AddIconCommand.ADD_ICON_COMMAND, new Dictionary<string, object>
            {
                {
                    AddIconCommand.ADD_ICON_COMMAND_KEY,
                    _commandArgs
                }
            });
        }
    }
}

