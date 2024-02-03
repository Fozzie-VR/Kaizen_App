using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class LayoutInitializer
    {

        FloorDimensions _floorDimensions;
        VisualElement _preKaizenLayoutContainer = null;
        VisualElement _postKaizenLayoutContainer = null;
        LayoutView _preKaizenLayoutView = null;
        LayoutView _postKaizenLayoutView = null;
        LayoutModel _preKaizenLayoutModel = null;
        LayoutModel _postKaizenLayoutModel = null;

        public LayoutInitializer()
        {
            _preKaizenLayoutModel = new LayoutModel(true);
            _postKaizenLayoutModel = new LayoutModel(false);
            _preKaizenLayoutView = new LayoutView(true);
            _postKaizenLayoutView = new LayoutView(false);
            EventManager.StartListening(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT, OnPreKaizenLayoutEvent);
            EventManager.StartListening(PageManager.POST_KAIZEN_LAYOUT_PAGE_EVENT, OnPostKaizenLayoutEvent);
        }

      

        private void OnPreKaizenLayoutEvent(Dictionary<string, object> eventArgs)
        {
            if(eventArgs.TryGetValue(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT_KEY, out object pageViewObject))
            {
                VisualElement pageRoot = pageViewObject as VisualElement;
                _preKaizenLayoutContainer = pageRoot;
                _preKaizenLayoutView.BindElements(_preKaizenLayoutContainer);
                new IconSpawner(pageRoot);
                new SelectionInspector(pageRoot, _preKaizenLayoutView);
                new UndoRedoView(pageRoot);
            }
            EventManager.StopListening(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT, OnPreKaizenLayoutEvent);
        }

        private void OnPostKaizenLayoutEvent(Dictionary<string, object> eventArgs)
        {
            if (eventArgs.TryGetValue(PageManager.POST_KAIZEN_LAYOUT_PAGE_EVENT_KEY, out object pageViewObject))
            {
                VisualElement pageRoot = pageViewObject as VisualElement;
                _postKaizenLayoutContainer = pageRoot;
                _postKaizenLayoutView.BindElements(_postKaizenLayoutContainer);
                new IconSpawner(pageRoot);
                new SelectionInspector(pageRoot, _postKaizenLayoutView);
                new UndoRedoView(pageRoot);
            }
            EventManager.StopListening(PageManager.POST_KAIZEN_LAYOUT_PAGE_EVENT, OnPostKaizenLayoutEvent);
        }
    }

}
