using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    //might want to use scriptable objects for this; likely to need a lot of types...
    public enum PageType
    {
        MainMenu,
        KaizenForm,
        FloorDimensionsPage,
        PreKaizenLayout,
        PostKaizenLayout,
        Presentation
    }

    public enum PageState
    {
        Active,
        Inactive
    }

    public class PageStateModel
    {
        //keeps track of the current page state and sorting order for each page type

        //issues commands to change the page state and sorting order based on events from the page view
        private const string PAGE_STATE_CHANGE_REQUEST = "PageStateChanged";
        private const string PAGE_SORT_ORDER_CHANGE_REQUEST = "PageSortOrderChanged";
        public const string PRE_KAIZEN_LAYOUT_CLICKED = "PreKaizenLayoutClicked";

        private const string PAGE_STATE_CHANGE_REQUEST_KEY = "PageStateChangedAction";
        private const string PAGE_SORT_ORDER_CHANGE_REQUEST_KEY = "PageSortOrderChangedAction";
        
        Dictionary<PageType, PageStateEntry> _pageStateModelEntries = new();

        private CommandHandler _commandHandler;

        public PageStateModel()
        {
            EventManager.StartListening(PAGE_STATE_CHANGE_REQUEST, OnPageStateChanged);
            EventManager.StartListening(PAGE_SORT_ORDER_CHANGE_REQUEST, OnPageSortOrderChanged);
            EventManager.StartListening(PRE_KAIZEN_LAYOUT_CLICKED, OnPreKaizenLayoutClicked);
            _commandHandler = new CommandHandler();
        }

        private void OnPreKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            if(_pageStateModelEntries.TryGetValue(PageType.FloorDimensionsPage, out PageStateEntry pageStateEntry))
            {
                PageState newPageState = PageState.Active;
                PageStateChanger pageStateChanger = new PageStateChanger(PageType.FloorDimensionsPage, pageStateEntry.PageState, newPageState);
                pageStateEntry.PageState = newPageState;
                _commandHandler.AddCommand(pageStateChanger);
            }
        }

        private void OnPageSortOrderChanged(Dictionary<string, object> evntMessage)
        {
            if (evntMessage.TryGetValue(PAGE_SORT_ORDER_CHANGE_REQUEST_KEY, out object states))
            {
                List<object> stateList = (List<object>)states;
                PageType pageType = (PageType)stateList[0];
                int newSortOrder = (int)stateList[1];

                if (_pageStateModelEntries.TryGetValue(pageType, out PageStateEntry pageStateEntry))
                {
                    PageSortOrderChanger pageSortOrderChanger = new PageSortOrderChanger(pageType, pageStateEntry.PageSortOrder, newSortOrder);
                    pageStateEntry.PageSortOrder = newSortOrder;
                    _commandHandler.AddCommand(pageSortOrderChanger);
                }
            }
        }

        private void OnPageStateChanged(Dictionary<string, object> evntMessage)
        {
            if(evntMessage.TryGetValue(PAGE_STATE_CHANGE_REQUEST_KEY, out object states))
            {
                List<object> stateList = (List<object>)states;
                PageType pageType = (PageType)stateList[0];
                PageState newPageState = (PageState)stateList[1];

                if(_pageStateModelEntries.TryGetValue(pageType, out PageStateEntry pageStateEntry))
                {
                    PageStateChanger pageStateChanger = new PageStateChanger(pageType, pageStateEntry.PageState, newPageState);
                    pageStateEntry.PageState = newPageState;
                    _commandHandler.AddCommand(pageStateChanger);
                }
            }
        }

        public void RegisterPage(PageType pageType, PageState pageState, int pageSortOrder)
        {
            PageStateEntry pageStateModelEntry = new PageStateEntry
            {
                PageState = pageState,
                PageSortOrder = pageSortOrder
            };
            _pageStateModelEntries.Add(pageType, pageStateModelEntry);
        }

        private struct PageStateEntry
        {
            public PageState PageState;
            public int PageSortOrder;
        }


    }

}
