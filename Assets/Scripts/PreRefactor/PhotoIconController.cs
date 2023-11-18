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
        private Image _overlayImageElement;
        private Button _takePhotoButton;
        private Button _closeWindowButton;
        private WebCamTexture _cameraTexture;
        private UIDocument _cameraDocument;
        private VisualElement _imageContainer;
        private VisualElement _cameraContainer;
        private Slider _opacitySlider;

        private LayoutIconInfo _iconInfo;

        private Button _rotateButton;
        private Button _scaleButton;
        int _imageRotation = 0;
        Vector2 _imageScale = new Vector2(1f, 1f);
        bool _rotationNegative = false;
        bool _scaleInverted = false;


        public PhotoIconController(UIDocument cameraDocument)
        {
            _cameraDocument = cameraDocument;
            _imageElement = new Image();
            _imageElement.name = "img_camera";
            _overlayImageElement = new Image();
            _overlayImageElement.name = "img_overlay";
            EventManager.StartListening(TAKE_PHOTO_EVENT, OnTakePhotoEvent);
        }

        private void OnTakePhotoEvent(Dictionary<string, object> dictionary)
        {
            Debug.Log("subscribing to image container geometry changed event");
            _cameraDocument.enabled = true;
            _cameraContainer = _cameraDocument.rootVisualElement;

            _imageContainer = _cameraContainer.Q<VisualElement>("ve_image_container");
            _imageContainer.Add(_imageElement);
            _imageContainer.Add(_overlayImageElement);
            _imageContainer.RegisterCallback<GeometryChangedEvent>(ImageContainerGeometryChanged);
            _imageElement.style.position = Position.Absolute;
            _overlayImageElement.style.position = Position.Absolute;

            _iconInfo = dictionary[TAKE_PHOTO_EVENT_KEY] as LayoutIconInfo;
            _cameraDocument.enabled = true;
            _takePhotoButton = _cameraContainer.Q<Button>("btn_take_picture");
            _takePhotoButton.clicked += TakePhoto;

            _closeWindowButton = _cameraContainer.Q<Button>("btn_close");
            _closeWindowButton.clicked += CloseWindow;

            _rotateButton = _cameraContainer.Q<Button>("btn_rotate");
            _rotateButton.clicked += RotateImageElement;

            _scaleButton = _cameraContainer.Q<Button>("btn_scale");
            _scaleButton.clicked += ScaleImageElement;

            _opacitySlider = _cameraContainer.Q<Slider>("slider_opacity");
            _opacitySlider.RegisterValueChangedCallback(OnOpacitySliderChanged);
            _opacitySlider.style.visibility = Visibility.Hidden;

            OnIconPressed();
        }

       
        private void ImageContainerGeometryChanged(GeometryChangedEvent evt)
        {
            //set container size to a 4:3 ratio
            float width = _imageContainer.resolvedStyle.width;
            float height = _imageContainer.resolvedStyle.height;
            int heightUnit = Mathf.RoundToInt(height / 3f);
            int adjustedWidth = heightUnit * 4;
            int adjustedHeight = heightUnit * 3;

            _imageContainer.style.width = new Length(adjustedWidth);
            _imageContainer.style.height = new Length(adjustedHeight);
            _imageElement.style.width = new Length(adjustedWidth);
            _imageElement.style.height = new Length(adjustedHeight);
            _overlayImageElement.style.width = new Length(adjustedWidth);
            _overlayImageElement.style.height = new Length(adjustedHeight);

            Debug.Log("screen orientation = " + Screen.orientation);
            //if(Application.isEditor)
            //{
            //    return;
            //}
           

            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                _imageRotation = 90;
                
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                _imageRotation = 0;
                
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                _imageRotation = 180;
               
            }
            else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            {
                _imageRotation = 270;
            }

            _imageElement.style.rotate = new Rotate(_imageRotation);
            _overlayImageElement.style.rotate = new Rotate(_imageRotation);

            _imageElement.style.scale = new Scale(_imageScale);
            _overlayImageElement.style.scale = new Scale(_imageScale);

            _rotateButton.text = _imageRotation.ToString();
            _scaleButton.text = _imageScale.x.ToString();
        }

        private void ScaleImageElement()
        {
            if(!_scaleInverted)
            {
                _imageScale.x = -1f;
                _scaleInverted = true;
            }
            else
            {
                _imageScale.x = 1f;
                _scaleInverted = false;
            }
            _imageElement.style.scale = new Scale(_imageScale);
            _overlayImageElement.style.scale = new Scale(_imageScale);
            _scaleButton.text = _imageScale.x.ToString();
        }

        private void RotateImageElement()
        {
            if(_imageRotation < 270 && !_rotationNegative )
            {
                _imageRotation += 90;
            }
            else if(_imageRotation == 270)
            {
                _rotationNegative = true;
                _imageRotation = 0;
            }
            else if(_imageRotation > -270 && _rotationNegative)
            {
                _imageRotation -= 90;
               
            }
            else if(_imageRotation == -270)
            {
                _rotationNegative = false;
                _imageRotation = 0;
            }
            _imageElement.style.rotate = new Rotate(_imageRotation);
            _overlayImageElement.style.rotate = new Rotate(_imageRotation);
            _rotateButton.text = _imageRotation.ToString();
            
            
        }

        private void OnOpacitySliderChanged(ChangeEvent<float> evt)
        {
            float opacity = evt.newValue;
            _overlayImageElement.style.opacity = opacity;
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
               
                if(_iconInfo.PhotoTexture != null && KaizenAppManager.Instance.IsPostKaizenLayout)
                {
                    //_overlayImageElement.style.backgroundImage = _iconInfo.PhotoTexture;
                    _overlayImageElement.style.opacity = _opacitySlider.value;
                    //_opacitySlider.RemoveFromClassList("hidden");
                    _opacitySlider.style.visibility = Visibility.Visible;
                    _opacitySlider.BringToFront();
                }
                else if(!KaizenAppManager.Instance.IsPostKaizenLayout)
                {
                    _overlayImageElement.style.opacity = 0f;
                    _opacitySlider.style.visibility = Visibility.Hidden;

                }
                _cameraTexture.Play();
                
                _imageElement.style.width = new Length(Screen.width);
                _imageElement.style.height = new Length(Screen.height);
               
                _overlayImageElement.style.width = new Length(Screen.width);
                _overlayImageElement.style.height = new Length(Screen.height);
               
            }
            else
            {
                Debug.LogError("User has not granted permission to use the camera");
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
            _overlayImageElement.style.backgroundImage = photo;
            _overlayImageElement.style.backgroundSize = new StyleBackgroundSize();
            _iconInfo.PhotoTexture = photo;
            _iconInfo.Rotation = _imageRotation;
            //ToDo:pass on photo rotation and scale so it shows up correctly in the layout
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
