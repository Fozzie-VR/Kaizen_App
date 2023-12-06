using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class PageView : MonoBehaviour
    {
        //Event name constants
        private const string PAGE_STATE_CHANGE = "PageStateChange";
        private const string PAGE_SORT_ORDER_CHANGE = "PageSortOrderChange";

        //Event action string constants
        private const string PAGE_STATE_CHANGE_ACTION = "PageStateChangeAction";
        private const string PAGE_SORT_ORDER_CHANGE_ACTION = "PageSortOrderChangeAction";

        [SerializeField] private PageType _pageType;
        [SerializeField] private int _pageSortOrder;
        [SerializeField] private PageState _pageState;
        [SerializeField] private UIDocument _pageDocument;
        [SerializeField] private VisualTreeAsset _pageVisualTreeAsset;

        private VisualElement _pageRoot;

        public PageType PageType => _pageType;
        public int PageSortOrder => _pageSortOrder;
        public PageState PageState => _pageState;
        public VisualElement PageRoot => _pageRoot;


        private void Awake()
        {
            _pageDocument.visualTreeAsset = _pageVisualTreeAsset;
            _pageRoot = _pageDocument.rootVisualElement;
            EventManager.StartListening(PAGE_STATE_CHANGE, OnPageStateChange);
            EventManager.StartListening(PAGE_SORT_ORDER_CHANGE, OnPageSortOrderChange);

            TogglePageActivation(_pageState);
        }

        //page state change handler
        private void OnPageStateChange(Dictionary<string, object> evntMessage)
        {
            if(evntMessage.TryGetValue(PAGE_STATE_CHANGE_ACTION, out object states))
            {
                List<object> stateList = (List<object>)states;
                PageType pageType = (PageType)stateList[0];
                PageState pageState = (PageState)stateList[1];

                if(pageType == _pageType)
                {
                    _pageState = pageState;
                    TogglePageActivation(pageState);
                }
            }
        }

        //page sort order change handler
        private void OnPageSortOrderChange(Dictionary<string, object> evntMessage)
        {
            if(evntMessage.TryGetValue(PAGE_SORT_ORDER_CHANGE_ACTION, out object states))
            {
                List<object> stateList = (List<object>)states;
                PageType pageType = (PageType)stateList[0];
                int pageSortOrder = (int)stateList[1];

                if(pageType == _pageType)
                {
                    _pageSortOrder = pageSortOrder;
                    _pageDocument.sortingOrder = pageSortOrder;
                }
            }
        }

        public void TogglePageActivation(PageState pageState)
        {
            //switch statement to handle page state changes
            switch (pageState)
            {
                case PageState.Active:
                    _pageDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                    break;
                case PageState.Inactive:
                    _pageDocument.rootVisualElement.style.display = DisplayStyle.None;
                    break;
                default:
                    break;
            }
        }
    }

}

