using UnityEngine;
using BitMiracle.LibTiff.Classic;
using System;

namespace HEVS.Extensions
{
    /// <summary>
    /// A utility class for extension methods.
    /// </summary>
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Loads a 32bit floating point RGB Tiff into the texture
        /// </summary>
        /// <param name="tex">The texture to load into.</param>
        /// <param name="filename">The file to load.</param>
        /// <returns>Returns true if display adapter was activated and set, false otherwise.</returns>
        public static bool LoadFloatingpointTiff(this UnityEngine.Texture2D tex, string filename)
        {
            int x, y;

            using (Tiff tif = Tiff.Open(filename, "r"))
            {
                if (tif == null)
                {
                    Debug.LogError("HEVS: Unable to open TIFF: " + filename);
                    return false;
                }

                int width = tif.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = tif.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                //Debug.Log("Loading float TIFF. Dimensions : " + width + " x " + height);

                // we only support Float RGA TIFFs with 1 row per scan line for now
                if ((tif.ScanlineSize() / 3 / sizeof(float)) != width)
                {
                    Debug.LogError("HEVS: TIFF format not supported. Only Float RGB with 1 row per scan line is supported.");
                    return false;
                }

                byte[] buffer = new byte[tif.ScanlineSize()];
                float[] color_ptr = new float[buffer.Length / 3];

                Texture2D newtex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
                for (y = 0; y < height; y++)
                {
                    tif.ReadScanline(buffer, (height - y - 1));
                    Buffer.BlockCopy(buffer, 0, color_ptr, 0, buffer.Length);
                    for (x = 0; x < width; x++)
                    {
                        newtex.SetPixel(x, y, new Color(color_ptr[x * 3 + 0], color_ptr[x * 3 + 1], color_ptr[x * 3 + 2]));
                    }
                }

                tex.Reinitialize(width, height);

                // this SetPixels will change the textureformat to the format of newtex
                tex.SetPixels(newtex.GetPixels());
                tex.Apply();
            }

            return true;

            /*
            tex = new Texture2D(256, 256, TextureFormat.RGBAFloat, false);
            int x, y;

            for (x = 0; x <256; x++)
            {
                for (y = 0; y < 256; y++)
                {
                    if (x < 6400)
                        tex.SetPixel(x, y, new Color((float)x / 255.0f, (float)y / 255.0f, 1));
                    else
                        //black
                        tex.SetPixel(x, y, new Color(0, 0, 0));
                }
            }
            tex.Apply();
            */
        }
    }
}
