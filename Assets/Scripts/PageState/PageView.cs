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
        [SerializeField] private PageSortOrder _pageSortOrder;
        [SerializeField] private PageState _pageState;
        [SerializeField] private UIDocument _pageDocument;
        [SerializeField] private VisualTreeAsset _pageVisualTreeAsset;


        private void Awake()
        {
            
        }

        //page state change handler
        private void OnPageStateChange(Dictionary<string, object> evntMessage)
        {
            
        }

        //page sort order change handler
        private void OnPageSortOrderChange(Dictionary<string, object> evntMessage)
        {
            
        }
    }

}

