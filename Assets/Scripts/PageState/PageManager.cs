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
        [SerializeField] private UIDocument _mainMenuPage;
        [SerializeField] private UIDocument _kaizenFormPage;
        [SerializeField] private UIDocument _floorDimensionsPage;
        [SerializeField] private UIDocument _preKaizenLayoutPage;
        [SerializeField] private UIDocument _postKaizenLayoutPage;
        [SerializeField] private UIDocument _presentationPage;

        MainMenuView _mainMenuView;

        //list of all active pages
        private List<UIDocument> _activePages = new ();

        //current page that is showing. This is the page that has highest sorting order
        private UIDocument _currentPage;


        //Activate _landingPages and initialize landing page view classes such as MainMenuView
        private void Awake()
        {
            
            InitializeLandingPages();
        }

        private void InitializeLandingPages()
        {
            _currentPageState = PageType.Landing;
            ActivatePage(_currentPageState);
        }


        //method to activate one page and deactivate all others
        public void ActivatePage(PageType pageState)
        {
            //deactivate all pages
            foreach (var page in _activePages)
            {
                page.enabled = false;
            }

            //clear the list of active pages
            _activePages.Clear();

            //activate the page that was passed in
            switch (pageState)
            {
                case PageType.Landing:
                    _mainMenuPage.enabled = true;
                    _activePages.Add(_mainMenuPage);
                    break;
                case PageType.KaizenForm:
                    _kaizenFormPage.enabled = true;
                    _activePages.Add(_kaizenFormPage);
                    break;
                case PageType.PreKaizenLayout:
                    _preKaizenLayoutPage.enabled = true;
                    _activePages.Add(_preKaizenLayoutPage);
                    break;
                case PageType.PostKaizenLayout:
                    _postKaizenLayoutPage.enabled = true;
                    _activePages.Add(_postKaizenLayoutPage);
                    break;
                case PageType.Presentation:
                    _presentationPage.enabled = true;
                    _activePages.Add(_presentationPage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pageState), pageState, null);
            }
        }   
    }

}
