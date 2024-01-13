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
            int yOffset = Screen.height;
            Vector2 captureAreaPos = container.worldBound.position;
            yOffset -= (int)captureAreaPos.y;
            yOffset -= (int)captureAreaPos.y;
            //Debug.Log("container position = " + container.style.translate.value);
            //Debug.Log($"capture Area Pos: {captureAreaPos}");
            //captureAreaPos.x *= widthMultiplier;
            //captureAreaPos.y *= heightMultiplier;
            Vector2 captureAreaSize = container.worldBound.size;
            yOffset -= (int)captureAreaSize.y;
            Debug.Log("y offset = " + yOffset);
            //Debug.Log($"capture Area Size: {captureAreaSize}");
            //Debug.Log("capture area resolved width: " + container.resolvedStyle.width);
            //Debug.Log("capture area resolved height: " + container.resolvedStyle.height);
            //captureAreaSize.x *= widthMultiplier;
            //captureAreaSize.y *= heightMultiplier;
            
            Texture2D screenTexture = new Texture2D((int)captureAreaSize.x, (int)captureAreaSize.y, TextureFormat.RGB24, false);
            Rect captureRect = new Rect(captureAreaPos.x, captureAreaPos.y + yOffset, captureAreaSize.x, captureAreaSize.y);
            //Debug.Log($"captureRect position: {captureRect.position}");
            screenTexture.ReadPixels(captureRect, 0, 0);
            screenTexture.Apply();
            return screenTexture;
        }
    }
}
