using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace KaizenApp
{
    public class KaizenFormView
    {
        public const string PRE_KAIZEN_LAYOUT_CLICKED = "pre_kaizen_layout_CLICKED";
        public const string POST_KAIZEN_LAYOUT_CLICKED = "post_kaizen_layout_CLICKED";
       
        private TextField _kaizenTheme;
        private TextField _jobDetails;
        private TextField _issues;
        private TextField _kaizenDescription;
        private TextField _otherItems;
        private TextField _kaizenResults;

        private VisualElement _rootElement;
        private VisualElement _preKaizenLayout;
        private VisualElement _postKaizenLayout;
       
        public KaizenFormView(VisualElement container)
        {
            _rootElement = container;
            BindElements();
            RegisterCallbacks();
        }

        private void BindElements()
        {
            _kaizenTheme = _rootElement.Q<TextField>("txt_theme");
            _jobDetails = _rootElement.Q<TextField>("txt_details");
            _issues = _rootElement.Q<TextField>("txt_issues");
            _kaizenDescription = _rootElement.Q<TextField>("txt_description");
            _otherItems = _rootElement.Q<TextField>("txt_other");
            _kaizenResults = _rootElement.Q<TextField>("txt_results");
            _preKaizenLayout = _rootElement.Q<VisualElement>("ve_pre_kaizen_layout");
            _postKaizenLayout = _rootElement.Q<VisualElement>("ve_post_kaizen_layout");
        }

        private void RegisterCallbacks()
        {
            _preKaizenLayout.RegisterCallback<PointerUpEvent>(PointerUpEvent => OnPreKaizenLayoutClicked(PointerUpEvent));
            _postKaizenLayout.RegisterCallback<PointerUpEvent>(PointerUpEvent => OnPostKaizenLayoutClicked(PointerUpEvent));
            _kaizenTheme.RegisterCallback<ChangeEvent<string>>(evt => OnKaizenThemeChanged(evt));
            _jobDetails.RegisterCallback<ChangeEvent<string>>(evt => OnJobDetailsChanged(evt));
            _issues.RegisterCallback<ChangeEvent<string>>(evt => OnIssuesChanged(evt));
            _kaizenDescription.RegisterCallback<ChangeEvent<string>>(evt => OnKaizenDescriptionChanged(evt));
            _otherItems.RegisterCallback<ChangeEvent<string>>(evt => OnOtherItemsChanged(evt));
            _kaizenResults.RegisterCallback<ChangeEvent<string>>(evt => OnKaizenResultsChanged(evt));
        }
       
        private void OnPreKaizenLayoutClicked(PointerUpEvent pointerUpEvent)
        {
           EventManager.TriggerEvent(PRE_KAIZEN_LAYOUT_CLICKED, null);
        }

        private void OnPostKaizenLayoutClicked(PointerUpEvent pointerUpEvent)
        {
            Debug.Log("PostKaizenLayoutClicked");
            EventManager.TriggerEvent(POST_KAIZEN_LAYOUT_CLICKED, null);
        }

        private void OnKaizenThemeChanged(ChangeEvent<string> evt)
        {
            
        }

        private void OnJobDetailsChanged(ChangeEvent<string> evt)
        {
            
        }

        private void OnIssuesChanged(ChangeEvent<string> evt)
        {
            
        }

        private void OnKaizenDescriptionChanged(ChangeEvent<string> evt)
        {
            
        }

        private void OnOtherItemsChanged(ChangeEvent<string> evt)
        {
            
        }

        private void OnKaizenResultsChanged(ChangeEvent<string> evt)
        {
            
        }

    }

}
