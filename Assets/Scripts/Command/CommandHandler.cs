using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizenApp
{
    public class CommandHandler
    {
        //list of ICommands
        private List<ICommand> _commandBuffer = new List<ICommand>();

        //index of the current command
        private int _commandIndex = 0;


        //Execute the current command
        public void AddCommand(ICommand command)
        {
            if(_commandIndex < _commandBuffer.Count)
            {
                _commandBuffer.RemoveRange(_commandIndex, _commandBuffer.Count - _commandIndex);
            }

            command.Execute();
            _commandBuffer.Add(command);
            _commandIndex++;
        }


        public void UndoCommand()
        {
            //Undo the last command and update the index
            if (_commandIndex < 0) return;
            _commandBuffer[_commandIndex].Undo();

            _commandIndex--;

        }

        public void RedoCommand()
        {
            //Redo the last command and update the index
            if (_commandIndex >= _commandBuffer.Count - 1) return;

            _commandIndex++;
            _commandBuffer[_commandIndex - 1].Execute();
        }

    }

}
