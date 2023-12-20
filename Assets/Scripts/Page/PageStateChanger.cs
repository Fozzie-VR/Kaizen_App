using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class PageStateChanger: ICommand
    {
        //Event name constants
        public const string PAGE_STATE_CHANGE = "PageStateChange";
        public const string PAGE_STATE_CHANGE_ACTION = "PageStateChangeAction";
       

        private PageType _pageType;
        private PageState _previousPageState;
        private PageState _newPageState;
        

        public PageStateChanger(PageType pageType, PageState currentPageState, PageState newPageState)
        {
            _pageType = pageType;
            _previousPageState = currentPageState;
            _newPageState = newPageState;
           
        }

        public void Execute()
        {
            object states = new List<object> { _pageType, _newPageState };

            EventManager.TriggerEvent(PAGE_STATE_CHANGE, new Dictionary<string, object>
            {
                { 
                    PAGE_STATE_CHANGE_ACTION,
                    states
                }
            });
        }

        public void Undo()
        {
            object states = new List<object> { _pageType, _previousPageState };

            EventManager.TriggerEvent(PAGE_STATE_CHANGE, new Dictionary<string, object>
            {
                {
                    PAGE_STATE_CHANGE_ACTION,
                    states
                }
            });
        }
    }
    
}
