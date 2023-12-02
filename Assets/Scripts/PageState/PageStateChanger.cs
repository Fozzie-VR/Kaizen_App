using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class PageStateChanger: ICommand
    {
        //Event name constants
        private const string PAGE_STATE_CHANGE = "PageStateChange";
        private const string PAGE_SORT_ORDER_CHANGE = "PageSortOrderChange";

        //Event action string constants
        private const string PAGE_STATE_CHANGE_ACTION = "PageStateChangeAction";
        private const string PAGE_SORT_ORDER_CHANGE_ACTION = "PageSortOrderChangeAction";

        private PageType _pageState;
        

        public PageStateChanger(PageType _pageType, PageState _currentPageState, PageState _newPageState)
        {
            
           
        }

        public void Execute()
        { 
            
        }

        public void Undo()
        {
            
        }
    }
    
}
