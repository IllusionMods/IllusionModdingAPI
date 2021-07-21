using System;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Utility methods for working with texture objects.
    /// </summary>
    public static class TextureUtils
    {
        /// <summary>
        /// Copy this texture inside a new editable Texture2D.
        /// </summary>
        /// <param name="tex">Texture to copy</param>
        /// <param name="format">Format of the copy</param>
        /// <param name="mipMaps">Copy has mipmaps</param>
        public static Texture2D ToTexture2D(this Texture tex, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        {
            var rt = RenderTexture.GetTemporary(tex.width, tex.height);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            GL.Clear(true, true, Color.clear);

            Graphics.Blit(tex, rt);

            var t = new Texture2D(tex.width, tex.height, format, mipMaps);
            t.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            t.Apply(false);

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return t;
        }

        /// <summary>
        /// Create texture from an image stored in a byte array, for example a png file read from disk.
        /// </summary>
        public static Texture2D LoadTexture(this byte[] texBytes, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        {
            if (texBytes == null) throw new ArgumentNullException(nameof(texBytes));

            var tex = new Texture2D(2, 2, format, mipMaps);
            tex.LoadImage(texBytes);
            return tex;
        }

        /// <summary>
        /// Create a sprite based on this texture.
        /// </summary>
        public static Sprite ToSprite(this Texture2D texture)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Filtering to use when resizing a texture
        /// </summary>
        public enum ImageFilterMode
        {
            /// <summary>
            /// Nearest pixel, fastest but gets aliased
            /// </summary>
            Nearest = 0,
            /// <summary>
            /// Use bilinear scaling
            /// </summary>
            Biliner = 1,
            /// <summary>
            /// Average of all nearby pixels
            /// </summary>
            Average = 2
        }

        /// <summary>
        /// Create a resized copy of this texture.
        /// http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/
        /// </summary>
        /// <param name="pSource">Texture to resize</param>
        /// <param name="pFilterMode">How to resize</param>
        /// <param name="pScale">New scale</param>
        public static Texture2D ResizeTexture(this Texture2D pSource, ImageFilterMode pFilterMode, float pScale)
        {
            //*** Variables
            int i;

            //*** Get All the source pixels
            Color[] aSourceColor = pSource.GetPixels(0);
            Vector2 vSourceSize = new Vector2(pSource.width, pSource.height);

            //*** Calculate New Size
            float xWidth = Mathf.RoundToInt((float)pSource.width * pScale);
            float xHeight = Mathf.RoundToInt((float)pSource.height * pScale);

            //*** Make New
            Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

            //*** Make destination array
            int xLength = (int)xWidth * (int)xHeight;
            Color[] aColor = new Color[xLength];

            Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

            //*** Loop through destination pixels and process
            Vector2 vCenter = new Vector2();
            for (i = 0; i < xLength; i++)
            {

                //*** Figure out x&y
                float xX = (float)i % xWidth;
                float xY = Mathf.Floor((float)i / xWidth);

                //*** Calculate Center
                vCenter.x = (xX / xWidth) * vSourceSize.x;
                vCenter.y = (xY / xHeight) * vSourceSize.y;

                //*** Do Based on mode
                //*** Nearest neighbour (testing)
                if (pFilterMode == ImageFilterMode.Nearest)
                {

                    //*** Nearest neighbour (testing)
                    vCenter.x = Mathf.Round(vCenter.x);
                    vCenter.y = Mathf.Round(vCenter.y);

                    //*** Calculate source index
                    int xSourceIndex = (int)((vCenter.y * vSourceSize.x) + vCenter.x);

                    //*** Copy Pixel
                    aColor[i] = aSourceColor[xSourceIndex];
                }

                //*** Bilinear
                else if (pFilterMode == ImageFilterMode.Biliner)
                {

                    //*** Get Ratios
                    float xRatioX = vCenter.x - Mathf.Floor(vCenter.x);
                    float xRatioY = vCenter.y - Mathf.Floor(vCenter.y);

                    //*** Get Pixel index's
                    int xIndexTL = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                    int xIndexTR = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));
                    int xIndexBL = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                    int xIndexBR = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));

                    //*** Calculate Color
                    aColor[i] = Color.Lerp(
                        Color.Lerp(aSourceColor[xIndexTL], aSourceColor[xIndexTR], xRatioX),
                        Color.Lerp(aSourceColor[xIndexBL], aSourceColor[xIndexBR], xRatioX),
                        xRatioY
                    );
                }

                //*** Average
                else if (pFilterMode == ImageFilterMode.Average)
                {

                    //*** Calculate grid around point
                    int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
                    int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
                    int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
                    int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

                    //*** Loop and accumulate
                    Color oColorTemp = new Color();
                    float xGridCount = 0;
                    for (int iy = xYFrom; iy < xYTo; iy++)
                    {
                        for (int ix = xXFrom; ix < xXTo; ix++)
                        {

                            //*** Get Color
                            oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];

                            //*** Sum
                            xGridCount++;
                        }
                    }

                    //*** Average Color
                    aColor[i] = oColorTemp / (float)xGridCount;
                }
            }

            //*** Set Pixels
            oNewTex.SetPixels(aColor);
            oNewTex.Apply();

            //*** Return
            return oNewTex;
        }
    }
}
