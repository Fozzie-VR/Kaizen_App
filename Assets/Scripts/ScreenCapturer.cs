using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public static class ScreenCapturer
    {
        public static Texture2D GetScreenCapturer(VisualElement container, float widthMultiplier, float heightMultiplier)
        {
            int yOffset = GetYOffset(container);
            Vector2 captureAreaPos = container.worldBound.position;
            Vector2 captureAreaSize = container.worldBound.size;
            
            Texture2D screenTexture = new Texture2D((int)captureAreaSize.x, (int)captureAreaSize.y, TextureFormat.RGB24, false);
            Rect captureRect = new Rect(captureAreaPos.x, captureAreaPos.y - yOffset, captureAreaSize.x, captureAreaSize.y);
            screenTexture.ReadPixels(captureRect, 0, 0);
            screenTexture.Apply();
            return screenTexture;
        }

        //get the difference between the distance from the top of the screen to the top of the container
        //and the distance from the bottom of the screen to the bottom of the container
        //to account for the difference in how textures set zero y at the bottom and UI Elements set zero y at the top
        private static int GetYOffset(VisualElement container)
        {
            int screenHeight = Screen.height;
            Vector2 containerPos = container.worldBound.position;
            Vector2 containerSize = container.worldBound.size;
            int yOffset = (int)containerPos.y - (screenHeight - (int)containerSize.y - (int)containerPos.y);
            return yOffset;
        }
    }
}
