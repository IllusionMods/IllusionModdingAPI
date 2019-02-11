using System;
using System.Collections.Generic;
using ExtensibleSaveFormat;

namespace MakerAPI.Chara
{
    public sealed class CoordinateEventArgs : EventArgs
    {
        public CoordinateEventArgs(ChaControl character, ChaFileCoordinate loadedCoordinate)
        {
            Character = character;
            LoadedCoordinate = loadedCoordinate;
        }

        /// <summary>
        /// Character the coordinate was loaded to
        /// </summary>
        public ChaControl Character { get; }

        /// <summary>
        /// The loaded coordinate
        /// </summary>
        public ChaFileCoordinate LoadedCoordinate { get; }

        public Dictionary<string, PluginData> GetCoordinateExtData() => ExtendedSave.GetAllExtendedData(LoadedCoordinate);
        public void SetCoordinateExtData(string dataId, PluginData data) => ExtendedSave.SetExtendedDataById(LoadedCoordinate, dataId, data);
    }
}
