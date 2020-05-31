using System;
#if AI || HS2
using AIChara;
#endif

#pragma warning disable 1591

namespace KKAPI.Maker
{
    public sealed class ChaFileLoadedEventArgs : EventArgs
    {
        public ChaFileLoadedEventArgs(string filename, byte sex, bool face, bool body, bool hair, bool parameter, bool coordinate, ChaFileControl characterInstance, ChaFile loadedChaFile)
        {
            Filename = filename;
            Sex = sex;
            Face = face;
            Body = body;
            Hair = hair;
            Parameter = parameter;
            Coordinate = coordinate;
            CharacterInstance = characterInstance;
            LoadedChaFile = loadedChaFile;
        }

        public string Filename { get; }
        public byte Sex { get; }
        public bool Face { get; }
        public bool Body { get; }
        public bool Hair { get; }
        public bool Parameter { get; }
        public bool Coordinate { get; }
        public ChaFileControl CharacterInstance { get; }

        /// <summary>
        /// Use this to get extended data on the character
        /// </summary>
        public ChaFile LoadedChaFile { get; }
    }
}
