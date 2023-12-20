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
            LayoutModel layoutModel = new LayoutModel();
            _layoutView = new LayoutView();
            EventManager.StartListening(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT, OnPreKaizenLayoutEvent);
        }

        private void OnPreKaizenLayoutEvent(Dictionary<string, object> eventArgs)
        {
            if(eventArgs.TryGetValue(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT_KEY, out object pageViewObject))
            {
                VisualElement pageRoot = pageViewObject as VisualElement;
                _preKaizenLayoutContainer = pageRoot;
                _layoutView.BindElements(_preKaizenLayoutContainer);
                IconSpawner iconSpawner = new IconSpawner(pageRoot);
            }
        }
    }

}
