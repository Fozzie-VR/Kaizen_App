using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    //manager initializes the page state model and keeps reference to the model
    public class PageManager: MonoBehaviour
    {
        
        private PageType _currentPageState;

        //list of all pages
        [SerializeField] List<PageView> _pages;

        MainMenuView _mainMenuView;

        //list of all active pages
        private List<UIDocument> _activePages = new ();

        //current page that is showing. This is the page that has highest sorting order
        private UIDocument _currentPage;

        private PageStateModel _pageStateModel;


        //Activate _landingPages and initialize landing page view classes such as MainMenuView
        private void Awake()
        {
            InitializePageStateModel();
            InitializeMainMenuView();
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
        //method to activate one page and deactivate all others

    }

}
