using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
   
    public class PhotoIconController
    {
        Image imageElement;
        Button photoTestButton;
        WebCamTexture cameraTexture;

        public PhotoIconController(VisualElement container)
        {
            photoTestButton = container.Q<Button>("btn_photo_test");
            photoTestButton.clicked += OnIconPressed;
            //photoIcon.RegisterCallback<PointerDownEvent>(OnIconPressed);
            imageElement = new Image();
            imageElement.BringToFront();
            container.Add(imageElement);

        }

        private void OnIconPressed()
        {
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            // Check if the user has granted permission to use the camera
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                // Create a new instance of the device camera
                cameraTexture = new WebCamTexture();
                
                imageElement.image = cameraTexture;
                imageElement.style.backgroundImage = new Texture2D(1, 1);
                cameraTexture.Play();
            }
            else
            {
                Debug.LogError("User has not granted permission to use the camera.");
            }
            photoTestButton.clicked -= OnIconPressed;
            photoTestButton.clicked += TakePhoto;
        }

        private void TakePhoto()
        {
            // Take a photo
            Texture2D photo = new Texture2D(cameraTexture.width, cameraTexture.height);
            photo.SetPixels(cameraTexture.GetPixels());
            photo.Apply();
            cameraTexture.Stop();
            imageElement.style.backgroundImage = photo;

        }

    }

}
