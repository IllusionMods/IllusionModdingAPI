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
        private static readonly Dictionary<string, byte[][]> patterns = new Dictionary<string, byte[][]>();
        private static readonly Dictionary<string, string[]> patternsRaw = new Dictionary<string, string[]>
            {
                { "png", new[] { "89 50 4E 47 0D 0A 1A 0A" } },
                { "jpg", new[] { "FF D8 FF", "00 00 00 0C 6A 50 20 20 0D 0A 87 0A", "FF 4F FF 51" } },
                { "webp", new[] { "52 49 46 46" } },
                { "bmp", new[] { "42 4D" } },
                { "gif", new[] { "47 49 46 38 37 61", "47 49 46 38 39 61" } },
                { "avif", new[] { "00 00 00 1C 66 74 79 70 61 76 69 66", "00 00 00 20 66 74 79 70 61 76 69 66" } },
                { "tif", new[] { "49 49 2A 00", "4D 4D 00 2A", "49 49 2B 00", "4D 4D 00 2B" } },
            };

        static ImageTypeIdentifier()
        {
            List<byte[]> patternList = new List<byte[]>();
            foreach (var kvp in patternsRaw)
            {
                patternList.Clear();
                foreach (var byteString in kvp.Value)
                    patternList.Add(byteString.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray());
                patterns.Add(kvp.Key, patternList.ToArray());
            }
        }

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
