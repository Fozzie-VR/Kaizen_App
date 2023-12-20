using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    public class GridDrawer
    {
        public const string GRID_DRAWN_EVENT = "GridDrawn";
        public const string GRID_DRAWN_EVENT_KEY = "gridTexture";

        public void DrawGrid(float width, float height, float pixelsPerMeter)
        {
            //int gridWidth = Mathf.RoundToInt(_floor.resolvedStyle.width);
            int gridWidth = Mathf.RoundToInt(width * pixelsPerMeter);

            int gridHeight = Mathf.RoundToInt(height * pixelsPerMeter);

            Texture2D gridTexture = new Texture2D(gridWidth, gridHeight);

            //create array of colors to fill texture
            Color[] pixels = gridTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            for (int y = 0; y < gridHeight; y++)
            {

                for (int x = 0; x < gridWidth; x++)
                {
                    if (x % pixelsPerMeter == 0 && y % 2 == 0 || y % pixelsPerMeter == 0 && x % 2 == 0 || y == gridHeight - 1 && x % 2 == 0 || x == gridWidth - 1 && y % 2 == 0)
                    {
                        pixels[x + y * gridWidth] = Color.black;
                        //Debug.Log(x + y * gridWidth);
                    }

                }
            }

            gridTexture.SetPixels(pixels);
            gridTexture.Apply();

            //FloorPlan listens for this event and sets the background image of the floor
            EventManager.TriggerEvent(GRID_DRAWN_EVENT, 
                new Dictionary<string, object> { { GRID_DRAWN_EVENT_KEY, gridTexture } });

            //_floor.style.backgroundImage = _gridTexture;
            //_floor.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

        }
    }

}
