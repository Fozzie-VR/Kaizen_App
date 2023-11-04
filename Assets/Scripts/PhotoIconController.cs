using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
   
    public class PhotoIconController
    {
        private Image _imageElement;
        private Button _takePhotoButton;
        private WebCamTexture _cameraTexture;
        private UIDocument _cameraDocument;
        private VisualElement _imageContainer;

        public PhotoIconController(UIDocument cameraDocument)
        {
            //photoIcon.RegisterCallback<PointerDownEvent>(OnIconPressed);
            _cameraDocument = cameraDocument;
            _imageElement = new Image();

            VisualElement cameraContainer = cameraDocument.rootVisualElement;
            _imageContainer = cameraContainer.Q<VisualElement>("ve_image_container");
            _takePhotoButton = cameraContainer.Q<Button>("btn_take_picture");
            _takePhotoButton.clicked += TakePhoto;
            _imageContainer.Add(_imageElement);
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
                _imageContainer.style.width = _imageContainer.parent.resolvedStyle.width / 3;
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
            _imageElement.style.backgroundImage = photo;
        }

    }

}
