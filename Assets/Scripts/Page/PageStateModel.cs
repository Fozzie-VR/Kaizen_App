using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    //might want to use scriptable objects for this; likely to need a lot of types...
    public enum PageType
    {
        DailyInspiration,
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
        //public const string PRE_KAIZEN_LAYOUT_CLICKED = "PreKaizenLayoutClicked";
        private const string FLOOR_DIMENSIONS_SET_EVENT = "floor_dimensions_set";
       

        private const string PAGE_STATE_CHANGE_REQUEST_KEY = "PageStateChangedAction";
        private const string PAGE_SORT_ORDER_CHANGE_REQUEST_KEY = "PageSortOrderChangedAction";
        private const string FLOOR_DIMENSIONS_SET_EVENT_KEY = "floor_dimensions";

        Dictionary<PageType, PageStateEntry> _pageStateModelEntries = new();

        private CommandHandler _commandHandler;

        public PageStateModel()
        {
            EventManager.StartListening(PAGE_STATE_CHANGE_REQUEST, OnPageStateChanged);
            EventManager.StartListening(PAGE_SORT_ORDER_CHANGE_REQUEST, OnPageSortOrderChanged);
            EventManager.StartListening(DailyInspiration.INSPIRATION_PAGE_CLICKED, OnInspirationClicked);
            EventManager.StartListening(MainMenuView.MAKE_KAIZEN_FORM_CLICKED, OnMakeKaizenFormClicked);
            EventManager.StartListening(KaizenFormView.PRE_KAIZEN_LAYOUT_CLICKED, OnPreKaizenLayoutClicked);
            EventManager.StartListening(FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
            _commandHandler = new CommandHandler();
        }

        private void OnInspirationClicked(Dictionary<string, object> dictionary)
        {
            DeactivatePage(PageType.DailyInspiration);
            ActivatePage(PageType.MainMenu);
        }

        private void OnMakeKaizenFormClicked(Dictionary<string, object> dictionary)
        {
            DeactivatePage(PageType.MainMenu);
            ActivatePage(PageType.KaizenForm);
        }

        private void OnPreKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            Debug.Log("PreKaizenLayoutClicked");
            DeactivatePage(PageType.KaizenForm);
            ActivatePage(PageType.FloorDimensionsPage);
        }

        private void OnFloorDimensionsSet(Dictionary<string, object> dictionary)
        {
            DeactivatePage(PageType.FloorDimensionsPage);
            ActivatePage(PageType.PreKaizenLayout);
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

        private void ActivatePage(PageType pageType)
        {
            if (_pageStateModelEntries.TryGetValue(pageType, out PageStateEntry pageStateEntry))
            {
                Debug.Log("activate page command for type " + pageType);
                PageState newPageState = PageState.Active;
                PageStateChanger pageStateChanger = new PageStateChanger(pageType, pageStateEntry.PageState, newPageState);
                pageStateEntry.PageState = newPageState;
                _commandHandler.AddCommand(pageStateChanger);
            }
        }

        private void DeactivatePage(PageType pageType)
        {
            if (_pageStateModelEntries.TryGetValue(pageType, out PageStateEntry pageStateEntry))
            {
                Debug.Log("deactivate page command for type " + pageType);
                PageState newPageState = PageState.Inactive;
                PageStateChanger pageStateChanger = new PageStateChanger(pageType, pageStateEntry.PageState, newPageState);
                pageStateEntry.PageState = newPageState;
                _commandHandler.AddCommand(pageStateChanger);
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
