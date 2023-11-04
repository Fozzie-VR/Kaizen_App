using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class LayoutComparisonPage
    {
        private const string PRE_KAIZEN_LAYOUT_CONTAINER_NAME = "ve_pre_kaizen_container";
        private const string POST_KAIZEN_LAYOUT_CONTAINER_NAME = "ve_post_kaizen_container";

        private VisualElement _preKaizenLayoutContainer;
        private VisualElement _postKaizenLayoutContainer;

        private Button _backToLayoutButton;

        public LayoutComparisonPage(VisualElement container)
        {
            container.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _preKaizenLayoutContainer = container.Q<VisualElement>(PRE_KAIZEN_LAYOUT_CONTAINER_NAME);
            _postKaizenLayoutContainer = container.Q<VisualElement>(POST_KAIZEN_LAYOUT_CONTAINER_NAME);

            _backToLayoutButton = container.Q<Button>("btn_back");
            _backToLayoutButton.clicked += ReturnToPostKaizenLayout;
           
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _preKaizenLayoutContainer.style.width = new Length(80, LengthUnit.Percent);
            _preKaizenLayoutContainer.style.height = _preKaizenLayoutContainer.style.width;
            _postKaizenLayoutContainer.style.width = _preKaizenLayoutContainer.style.width;
            _postKaizenLayoutContainer.style.height = _preKaizenLayoutContainer.style.height;

            _preKaizenLayoutContainer.style.backgroundImage = KaizenAppManager._instance.PreKaizenLayout;
            _postKaizenLayoutContainer.style.backgroundImage = KaizenAppManager._instance.PostKaizenLayout;
        }


        private void ReturnToPostKaizenLayout()
        {
            KaizenAppManager._instance.BackFromComparisonPage();
        }
    }

}
