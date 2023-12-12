using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    //manager initializes the page state model and keeps reference to the model
    public class PageManager : MonoBehaviour
    {

        private const string PAGE_STATE_CHANGE = "PageStateChange";
        private const string PAGE_STATE_CHANGE_ACTION = "PageStateChangeAction";

        //list of all pages
        [SerializeField] List<PageView> _pages;

        MainMenuView _mainMenuView;

        //list of all active pages
        private List<UIDocument> _activePages = new();

        //current page that is showing. This is the page that has highest sorting order
        private UIDocument _currentPage;

        private PageStateModel _pageStateModel;


        //Activate _landingPages and initialize landing page view classes such as MainMenuView
        private void Awake()
        {
            InitializePageStateModel();
            //InitializeMainMenuView();
            EventManager.StartListening(PAGE_STATE_CHANGE, OnPageStateChange);
        }

        private void Start()
        {
            InitializeMainMenuView();
        }

        private void OnPageStateChange(Dictionary<string, object> evntMessage)
        {
            if (evntMessage.TryGetValue(PAGE_STATE_CHANGE_ACTION, out object states))
            {
                List<object> stateList = (List<object>)states;
                PageType pageType = (PageType)stateList[0];
                PageState pageState = (PageState)stateList[1];
                if (pageState == PageState.Active)
                {
                    InitializePage(pageType);
                }
            }
        }

        private void InitializePageStateModel()
        {
            _pageStateModel = new PageStateModel();
            foreach (PageView page in _pages)
            {
                _pageStateModel.RegisterPage(page.PageType, page.PageState, page.PageSortOrder);
            }
        }

        private void InitializeMainMenuView()
        {
            PageView mainMenuPage = _pages.Find(page => page.PageType == PageType.MainMenu);
            _mainMenuView = new MainMenuView(mainMenuPage.PageRoot);
        }

        //Initialize FloorDimensionsPageView
        private void InitializeFloorDimensionsPageView()
        {
            PageView floorDimensionsPage = _pages.Find(page => page.PageType == PageType.FloorDimensionsPage);
            FloorDimensionsPage floorDimensionsPageView = new FloorDimensionsPage(floorDimensionsPage.PageRoot);
        }

        //method to initialize a page based on page type, use switch statement
        private void InitializePage(PageType pageType)
        {
            switch (pageType)
            {
                case PageType.FloorDimensionsPage:
                    InitializeFloorDimensionsPageView();
                    break;
                case PageType.PreKaizenLayout:
                    InitializePreKaizenLayoutPageView();
                    break;
                case PageType.PostKaizenLayout:
                    InitializePostKaizenLayoutView();
                    break;
                case PageType.Presentation:
                    InitializeComparisonPageView();
                    break;
                default:
                    break;
            }
        }

        private void InitializePostKaizenLayoutView()
        {
            throw new NotImplementedException();
        }

        private void InitializePreKaizenLayoutPageView()
        {
           //initialize icon spawner, layout view, layout model, grid drawer, icon view, navigation view, undo/redo view

        }

        private void InitializeComparisonPageView()
        {
            throw new NotImplementedException();
        }


    }

        

}
