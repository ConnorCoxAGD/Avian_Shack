using UnityEngine;
using System.Collections;

public static class TextureGenerator {

    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) 
    {
        Texture2D texture = new Texture2D (width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels (colourMap);
        texture.Apply ();
        return texture;
    }


    public static Texture2D TextureFromHeightMap(NoiseMap noiseMap) 
    {
        int width = noiseMap.values.GetLength (0);
        int height = noiseMap.values.GetLength (1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap [y * width + x] = Color.Lerp (Color.black, Color.white, Mathf.InverseLerp(noiseMap.minValue,noiseMap.maxValue,noiseMap.values [x, y]));
            }
        }

        return TextureFromColourMap (colorMap, width, height);
    }
}