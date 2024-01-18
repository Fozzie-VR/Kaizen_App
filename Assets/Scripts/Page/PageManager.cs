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
        public const string PRE_KAIZEN_LAYOUT_PAGE_EVENT = "PreKaizenLayoutEvent";
        public const string PRE_KAIZEN_LAYOUT_PAGE_EVENT_KEY = "PreKaizenLayoutEventKey";

        public const string POST_KAIZEN_LAYOUT_PAGE_EVENT = "PostKaizenLayoutEvent";
        public const string POST_KAIZEN_LAYOUT_PAGE_EVENT_KEY = "PostKaizenLayoutEventKey";

        //list of all pages
        [SerializeField] List<PageView> _pages;

        MainMenuView _mainMenuView;

        //list of all active pages
        private List<UIDocument> _activePages = new();

        //current page that is showing. This is the page that has highest sorting order
        private UIDocument _currentPage;

        private PageStateModel _pageStateModel;
        private FloorDimensions _floorDimensions;

        //Activate _landingPages and initialize landing page view classes such as MainMenuView
        private void Awake()
        {
            InitializePageStateModel();
            LayoutInitializer layoutInitializer = new LayoutInitializer();
            EventManager.StartListening(PageStateChanger.PAGE_STATE_CHANGE, OnPageStateChange);
        }
        
        private void Start()
        {
            //InitializeMainMenuView();
        }

        private void OnPageStateChange(Dictionary<string, object> evntMessage)
        {
            if (evntMessage.TryGetValue(PageStateChanger.PAGE_STATE_CHANGE_ACTION, out object states))
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
                case PageType.MainMenu:
                    InitializeMainMenuView();
                    break;
                case PageType.KaizenForm:
                    InitializeKaizenFormView();
                    break;
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


        private void InitializeKaizenFormView()
        {
            PageView pageView = _pages.Find(page => page.PageType == PageType.KaizenForm);
            VisualElement pageRoot = pageView.PageRoot;
            pageRoot.RegisterCallback<GeometryChangedEvent>(KaizenFormGeometryChanged);
        }

        private void KaizenFormGeometryChanged(GeometryChangedEvent evt)
        {
            VisualElement pageRoot = (VisualElement)evt.target;
            KaizenFormView kaizenFormView = new KaizenFormView(pageRoot);
           
        }
        private void InitializePostKaizenLayoutView()
        {
            throw new NotImplementedException();
        }

        private void InitializePreKaizenLayoutPageView()
        {
           //initialize icon spawner, layout view, layout model, grid drawer, icon view, navigation view, undo/redo view
           PageView pageView = _pages.Find(page => page.PageType == PageType.PreKaizenLayout);
           VisualElement pageRoot = pageView.PageRoot;
           pageRoot.RegisterCallback<GeometryChangedEvent>(PreKaizenGeometryChanged);
           
        }

        private void PreKaizenGeometryChanged(GeometryChangedEvent evt)
        {
            VisualElement pageRoot = (VisualElement)evt.target;
            EventManager.TriggerEvent(PRE_KAIZEN_LAYOUT_PAGE_EVENT, new Dictionary<string, object>
           {
               {
                   PRE_KAIZEN_LAYOUT_PAGE_EVENT_KEY,
                   pageRoot
               }
           });
        }

        private void InitializeComparisonPageView()
        {
            throw new NotImplementedException();
        }


    }

        

}
