using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
   
    public class PhotoIconController
    {
        private const string TAKE_PHOTO_EVENT = "TakePhoto";
        private const string TAKE_PHOTO_EVENT_KEY = "iconInfo";

        private const string PHOTO_TAKEN_EVENT = "PhotoTaken";
        private const string PHOTO_TAKEN_EVENT_KEY = "photoTexture";

        private Image _imageElement;
        private Button _takePhotoButton;
        private Button _closeWindowButton;
        private WebCamTexture _cameraTexture;
        private UIDocument _cameraDocument;
        private VisualElement _imageContainer;
        private VisualElement _cameraContainer;

        private LayoutIconInfo _iconInfo;

        public PhotoIconController(UIDocument cameraDocument)
        {
            _cameraDocument = cameraDocument;
            _imageElement = new Image();
            EventManager.StartListening(TAKE_PHOTO_EVENT, OnTakePhotoEvent);
        }

        private void OnTakePhotoEvent(Dictionary<string, object> dictionary)
        {
            _cameraDocument.enabled = true;
            _cameraContainer = _cameraDocument.rootVisualElement;

            _imageContainer = _cameraContainer.Q<VisualElement>("ve_image_container");
            _imageContainer.Add(_imageElement);

            _iconInfo = dictionary[TAKE_PHOTO_EVENT_KEY] as LayoutIconInfo;
            _cameraDocument.enabled = true;
            _takePhotoButton = _cameraContainer.Q<Button>("btn_take_picture");
            _takePhotoButton.clicked += TakePhoto;

            _closeWindowButton = _cameraContainer.Q<Button>("btn_close");
            _closeWindowButton.clicked += CloseWindow;

            OnIconPressed();
        }

        public void OnIconPressed()
        {
            _cameraDocument.sortingOrder = 2;
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            // Check if the user has granted permission to use the camera
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                // Create a new instance of the device camera
                _cameraTexture = new WebCamTexture();
                _imageElement.image = _cameraTexture;
                _cameraTexture.Play();
                _imageElement.style.width = new Length(Screen.width);
                _imageElement.style.height = new Length(Screen.height);
                //_imageContainer.style.width = new StyleLength(640);
                //_imageContainer.style.height = new StyleLength(720);
               
            }
            else
            {
                Debug.LogError("User has not granted permission to use the camera.");
            }
        }

        private void TakePhoto()
        {
            // Take a photo
            Texture2D photo = new Texture2D(_cameraTexture.width, _cameraTexture.height);
            photo.SetPixels(_cameraTexture.GetPixels());
            photo.Apply();
            _cameraTexture.Stop();
            Debug.Log("Camera texture width: " + _cameraTexture.width);
            Debug.Log("Camera texture height: " + _cameraTexture.height);
            _imageElement.style.backgroundImage = photo;
            _imageElement.style.backgroundSize = new StyleBackgroundSize();
            _iconInfo.PhotoTexture = photo;
            EventManager.TriggerEvent(PHOTO_TAKEN_EVENT, 
                new Dictionary<string, object> { { PHOTO_TAKEN_EVENT_KEY, photo } });

        }

        private void CloseWindow()
        {
            _cameraDocument.enabled = false;
            _cameraTexture.Stop();
            _cameraTexture = null;
            //_cameraContainer.Remove(_imageElement);
            _cameraContainer = null;
            _imageContainer = null;
            _iconInfo = null;
            _takePhotoButton.clicked -= TakePhoto;
            _closeWindowButton.clicked -= CloseWindow;
        }
    }
}
