using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace KaizenApp
{
    public class PageSortOrderChanger : ICommand
    {
        private const string PAGE_SORT_ORDER_CHANGE = "PageSortOrderChange";
        private const string PAGE_SORT_ORDER_CHANGE_ACTION = "PageSortOrderChangeAction";

        private int _newSortOrder;
        private int _oldSortOrder;
        private PageType _pageType;

        public PageSortOrderChanger(PageType pageType, int oldSortOrder, int newSortOrder)
        {
            _pageType = pageType;
            _oldSortOrder = oldSortOrder;
            _newSortOrder = newSortOrder;
        }

        public void Execute()
        {
            EventManager.TriggerEvent(PAGE_SORT_ORDER_CHANGE, new Dictionary<string, object>
            {
                {
                    PAGE_SORT_ORDER_CHANGE_ACTION,
                    new List<object> { _pageType, _newSortOrder }
                }
            });
        }

        public void Undo()
        {
            EventManager.TriggerEvent(PAGE_SORT_ORDER_CHANGE, new Dictionary<string, object>
            {
                {
                    PAGE_SORT_ORDER_CHANGE_ACTION,
                    new List<object> { _pageType, _oldSortOrder }
                }
            });
        }
    }
   
}

