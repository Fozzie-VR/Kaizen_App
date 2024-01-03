using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// This class is used to control the Undo and Redo buttons in the UI
namespace KaizenApp
{
    public class UndoRedoView
    {
        private const string UNDO_BUTTON = "btn_undo";
        private const string REDO_BUTTON = "btn_redo";

        public const string UNDO_EVENT = "undo";
        public const string REDO_EVENT = "redo";
        public UndoRedoView(VisualElement root)
        {
            Button undoButton = root.Q<Button>(UNDO_BUTTON);
            undoButton.clicked += Undo;

            Button redoButton = root.Q<Button>(REDO_BUTTON);
            redoButton.clicked += Redo;
        }

        public void Undo()
        {
            EventManager.TriggerEvent(UNDO_EVENT, null);
        }

        public void Redo()
        {
            EventManager.TriggerEvent(REDO_EVENT, null);
        }
    }
}
