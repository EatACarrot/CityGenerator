using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNoise
{
    //creating perlin noise for more natural randomness
    //https://www.youtube.com/watch?v=bG0uEXV6aHQ - tutorial used
    static public Texture2D GenNoiseTexture(int width, int height, float scale, float offsetX, float offsetY)
    {         

        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = GenColor(x, y , width, height, scale, offsetX, offsetY);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    static Color GenColor(int x, int y, int width, int height, float scale, float offsetX, float offsetY)
    {
        float xCoord = (float)x / (float)width * scale + offsetX;
        float yCoord = (float)y / (float)height * scale + offsetY;  

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }
}
