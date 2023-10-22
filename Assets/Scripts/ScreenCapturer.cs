using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public static class ScreenCapturer
    {
        //TODO: This is a hack to get the correct y offset for the screen capture.  It should be calculated based on the height of the toolbar.
       
        public static Texture2D GetScreenCapturer(VisualElement container, float widthMultiplier, float heightMultiplier)
        {
            //Debug.Log("height multiplier: " + heightMultiplier);
            //Debug.Log("width multiplier: " + widthMultiplier);
            int _yOffset = Mathf.RoundToInt(heightMultiplier * 40);
            //Debug.Log("y offset = " + _yOffset);
            Vector2 captureAreaPos = container.worldBound.position;
            captureAreaPos.x *= widthMultiplier;
            captureAreaPos.y *= heightMultiplier;
            Vector2 captureAreaSize = container.worldBound.size;
            captureAreaSize.x *= widthMultiplier;
            captureAreaSize.y *= heightMultiplier;
            
            Texture2D screenTexture = new Texture2D((int)captureAreaSize.x, (int)captureAreaSize.y, TextureFormat.RGB24, false);
            Rect captureRect = new Rect(captureAreaPos.x, captureAreaPos.y + _yOffset, captureAreaSize.x, captureAreaSize.y);
            //Debug.Log($"captureRect: {captureRect}");
            screenTexture.ReadPixels(captureRect, 0, 0);
            screenTexture.Apply();
            return screenTexture;
        }
    }
}