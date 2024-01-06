using System.Collections.Generic;

namespace KaizenApp
{
    public class ResizeIconCommand : ICommand
    {
        public const string RESIZE_ICON_COMMAND = "ResizeIconCommand";
        public const string RESIZE_ICON_COMMAND_KEY = "ResizeIconCommandKey";

        public const string UNDO_RESIZE_ICON_COMMAND = "UndoResizeIconCommand";
        public const string UNDO_RESIZE_ICON_COMMAND_KEY = "UndoResizeIconCommandKey";

        private IconViewInfo _newInfo;
        private IconViewInfo _oldInfo;

        public ResizeIconCommand(IconViewInfo newInfo, IconViewInfo oldInfo)
        {
            _newInfo = newInfo;
            _oldInfo = oldInfo;
        }

        public void Execute()
        {
            EventManager.TriggerEvent(RESIZE_ICON_COMMAND, new Dictionary<string, object>{
                {
                    RESIZE_ICON_COMMAND_KEY, _newInfo
                }
            });
            
        }

        public void Undo()
        {
            EventManager.TriggerEvent(RESIZE_ICON_COMMAND, new Dictionary<string, object>{
                {
                    RESIZE_ICON_COMMAND_KEY, _oldInfo
                }
            });

            EventManager.TriggerEvent(UNDO_RESIZE_ICON_COMMAND, new Dictionary<string, object>
            {
                {
                    UNDO_RESIZE_ICON_COMMAND_KEY, _oldInfo
                }
            });

        }
    }
}