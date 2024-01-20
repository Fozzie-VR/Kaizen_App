using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizenApp
{
    public class KaizenFormModel
    {
        
        private string _kaizenTheme;
        private string _jobDetails;
        private string _issues;
        private string _kaizenDescription;
        private string _otherItems;
        private string _kaizenResults;

        private Texture2D _preKaizenLayout;
        private Texture2D _postKaizenLayout;

        private bool _preKaizenLayoutActive;

        public KaizenFormModel()
        {
           RegisterCallbacks();
        }
        private void RegisterCallbacks()
        {
            EventManager.StartListening(LayoutView.LAYOUT_CAPTURED_EVENT, OnLayoutCaptured);
            EventManager.StartListening(KaizenFormView.PRE_KAIZEN_LAYOUT_CLICKED, OnPreKaizenLayoutClicked);
        }

        private void OnPreKaizenLayoutClicked(Dictionary<string, object> dictionary)
        {
            _preKaizenLayoutActive = true;
        }

        private void OnLayoutCaptured(Dictionary<string, object> eventArgs)
        {
            Texture2D layout = (Texture2D)eventArgs[LayoutView.LAYOUT_CAPTURED_EVENT_KEY];
            if (_preKaizenLayoutActive)
            {
                _preKaizenLayout = layout;
            }
            else
            {
                _postKaizenLayout = layout;
            }
        }

        


    }

}
