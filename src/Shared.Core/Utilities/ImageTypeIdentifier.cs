using System.Collections.Generic;
using System.Linq;
using System;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Tool for identifying likely file extension for byte arrays representing images.
    /// </summary>
    public static class ImageTypeIdentifier
    {
      private static readonly Dictionary<string, byte[][]> patterns = new Dictionary<string, byte[][]>
        {
            { "png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { "jpg", new[]
                {
                    new byte[] { 0xFF, 0xD8, 0xFF },
                    new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A },
                    new byte[] { 0xFF, 0x4F, 0xFF, 0x51 }
                }
            },
            // webp hack: several other filetypes begin like that but none other are images
            { "webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
            { "bmp", new[] { new byte[] { 0x42, 0x4D } } },
            { "gif", new[]
                {
                    new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },
                    new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }
                }
            },
            { "avif", new[]
                {
                    new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66 },
                    new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66 }
                }
            },
            { "tif", new[]
                {
                    new byte[] { 0x49, 0x49, 0x2A, 0x00 },
                    new byte[] { 0x4D, 0x4D, 0x00, 0x2A },
                    new byte[] { 0x49, 0x49, 0x2B, 0x00 },
                    new byte[] { 0x4D, 0x4D, 0x00, 0x2B }
                }
            },
        };

        /// <summary>
        /// Identify the file extension of a byte array representing an image.
        /// Returns "bin" by default if the array is shorter than 20 bytes, or identification is unsuccessful.
        /// </summary>
        /// <param name="bytes">Byte array representing the image</param>
        /// <param name="defReturn">Default return value if identification is unsuccessful</param>
        /// <returns>The identified lowercase file extension</returns>
        public static string Identify(byte[] bytes, string defReturn = "bin")
        {
            if (bytes == null || bytes.Length < 20) return defReturn;
            bool found;
            int i;
            foreach (var kvp in patterns)
                foreach (var pattern in kvp.Value)
                {
                    found = true;
                    for (i = 0; i < pattern.Length; i++)
                    {
                        if (bytes[i] != pattern[i])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found) return kvp.Key;
                }
            return defReturn;
        }
    }
}
