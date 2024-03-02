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
        public const string POST_KAIZEN_LAYOUT_INITIALIZED = "post_kaizen_layout_initialized";

        FloorDimensions _floorDimensions;
        VisualElement _preKaizenLayoutContainer = null;
        VisualElement _postKaizenLayoutContainer = null;
        LayoutView _preKaizenLayoutView = null;
        LayoutView _postKaizenLayoutView = null;
        LayoutModel _preKaizenLayoutModel = null;
        LayoutModel _postKaizenLayoutModel = null;

        SelectionInspector _preKaizenSelectionInspector = null;
        SelectionInspector _postKaizenSelectionInspector = null;
        bool _preKaizenLayoutActive = false;
        bool _postKaizenLayoutActive = false;
        

        public LayoutInitializer(PageView preKaizenLayout, PageView postKaizenLayout)
        {
            _preKaizenLayoutModel = new LayoutModel(true);
            _postKaizenLayoutModel = new LayoutModel(false);
            _preKaizenLayoutView = new LayoutView(true);
            _postKaizenLayoutView = new LayoutView(false);
            EventManager.StartListening(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT, OnPreKaizenLayoutEvent);
            EventManager.StartListening(PageManager.POST_KAIZEN_LAYOUT_PAGE_EVENT, OnPostKaizenLayoutEvent);
            EventManager.StartListening(KaizenFormView.PRE_KAIZEN_LAYOUT_CLICKED, OnPreKaizenLayoutClicked);
            EventManager.StartListening(KaizenFormView.POST_KAIZEN_LAYOUT_CLICKED, OnPostKaizenLayoutClicked);
            EventManager.StartListening(LayoutView.BACK_TO_KAIZEN_FORM_EVENT, OnBackToKaizenFormClicked);
        }

        private void OnBackToKaizenFormClicked(Dictionary<string, object> obj)
        {
            if (_preKaizenLayoutActive)
            {
                _preKaizenSelectionInspector.UnregisterCallbacks();
            }
            
            if (_postKaizenLayoutActive)
            {
                _postKaizenSelectionInspector.UnregisterCallbacks();
            }
        }
        
        


        private void OnPreKaizenLayoutEvent(Dictionary<string, object> eventArgs)
        {
            if(eventArgs.TryGetValue(PageManager.PRE_KAIZEN_LAYOUT_PAGE_EVENT_KEY, out object pageViewObject))
            {
                VisualElement pageRoot = pageViewObject as VisualElement;
                _preKaizenLayoutContainer = pageRoot;
                _preKaizenLayoutView.BindElements(_preKaizenLayoutContainer);
                new IconSpawner(pageRoot);
                _preKaizenSelectionInspector =  new SelectionInspector(pageRoot, _preKaizenLayoutView);
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
                _postKaizenLayoutView.SetFloorSize(_preKaizenLayoutView.FloorWidthMeters, _preKaizenLayoutView.FloorHeightMeters);
                _postKaizenLayoutView.BindElements(_postKaizenLayoutContainer);
                _postKaizenLayoutView.Floor.RegisterCallback<GeometryChangedEvent>(OnFloorGeometryChanged);
                new IconSpawner(pageRoot);
                Debug.Log("initializing post kaizen selection inspector");
                _postKaizenSelectionInspector = new SelectionInspector(pageRoot, _postKaizenLayoutView);
                new UndoRedoView(pageRoot);
            }
            
        }

        private void OnPostKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            _preKaizenLayoutActive = false;
            _postKaizenLayoutActive = true;
           
        }

        private void OnPreKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            _preKaizenLayoutActive = true;
            _postKaizenLayoutActive = false;
        }


        private void OnFloorGeometryChanged(GeometryChangedEvent evt)
        {
            EventManager.TriggerEvent(POST_KAIZEN_LAYOUT_INITIALIZED, null);
            EventManager.StopListening(PageManager.POST_KAIZEN_LAYOUT_PAGE_EVENT, OnPostKaizenLayoutEvent);
            _postKaizenLayoutView.Floor.UnregisterCallback<GeometryChangedEvent>(OnFloorGeometryChanged);
        }
    }

}
