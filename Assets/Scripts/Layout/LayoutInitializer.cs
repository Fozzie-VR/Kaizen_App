using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{

    public class LayoutInitializer
    {

        FloorDimensions _floorDimensions;
        VisualElement _preKaizenLayoutContainer = null;
        LayoutView _layoutView = null;

        public LayoutInitializer()
        {
            EventManager.StartListening(FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT, OnFloorDimensionsSet);
            EventManager.StartListening(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT, OnPreKaizenLayoutEvent);
        }
        private void OnFloorDimensionsSet(Dictionary<string, object> dictionary)
        {
            _floorDimensions = (FloorDimensions)dictionary[FloorDimensionsPage.FLOOR_DIMENSIONS_SET_EVENT_KEY];

            //IconSpawner iconSpawner = new IconSpawner(pageView.PageRoot);
        }

        private void OnPreKaizenLayoutEvent(Dictionary<string, object> eventArgs)
        {
            if(eventArgs.TryGetValue(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT_KEY, out object pageViewObject))
            {
                PageView pageView = (PageView)pageViewObject;
                _preKaizenLayoutContainer = pageView.PageRoot;
                IconSpawner iconSpawner = new IconSpawner(pageView.PageRoot);

            }
        }

    }

}
