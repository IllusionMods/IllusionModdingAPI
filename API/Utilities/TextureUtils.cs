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
    }
}