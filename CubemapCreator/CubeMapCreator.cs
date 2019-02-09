using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class CubeMapCreator
{
    public static Texture2D CreateCubemapTexture(Camera camera, int textureSize)
    {
        Texture2D texture = new Texture2D(textureSize * 6, textureSize, TextureFormat.RGB24, false);

        texture.SetPixels(CreateCubemapPixels(camera, textureSize));
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Width = 6 * textureSize, height = textureSize
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="textureSize"></param>
    /// <returns></returns>
    public static Color[] CreateCubemapPixels(Camera camera, int textureSize)
    {
        Cubemap cubemap = new Cubemap(textureSize, TextureFormat.RGB24, false);
        camera.RenderToCubemap(cubemap);

        var newPixels = new Color[textureSize * textureSize * 6];

        List<Color[]> pixels = new List<Color[]>
        {
            cubemap.GetPixels(CubemapFace.PositiveX),
            cubemap.GetPixels(CubemapFace.NegativeX),
            cubemap.GetPixels(CubemapFace.PositiveY),
            cubemap.GetPixels(CubemapFace.NegativeY),
            cubemap.GetPixels(CubemapFace.PositiveZ),
            cubemap.GetPixels(CubemapFace.NegativeZ)
        };

        int whidth, height, n;
        for (int i = 0; i < newPixels.Length; i++)
        {
            whidth = i % textureSize;
            height = i / textureSize / 6;
            n = i / textureSize % 6;

            try
            {
                if (n == 3)
                {
                    newPixels[i] = pixels[n][textureSize * whidth + (textureSize - height - 1)];
                }
                else if (n == 2)
                {
                    newPixels[i] = pixels[2][textureSize * (textureSize - whidth - 1) + height];
                }
                else
                {
                    newPixels[newPixels.Length - i - 1] = pixels[n][textureSize * height + whidth];
                }
            }
            catch
            {
                Debug.LogError(i + " " + whidth + " " + height + " " + n);
                break;
            }
        }

        return newPixels;
    }
}