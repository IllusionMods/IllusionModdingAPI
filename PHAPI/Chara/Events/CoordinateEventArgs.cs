using System;
using System.Collections.Generic;
using Character;
using ExtensibleSaveFormat;

#pragma warning disable 1591

namespace KKAPI.Chara
{
    /// <summary>
    /// Fired in events that deal with coordinate cards
    /// </summary>
    public sealed class CoordinateEventArgs : EventArgs
    {
        public CoordinateEventArgs(Human character, CustomParameter loadedCoordinate)
        {
            Character = character;
            LoadedCoordinate = loadedCoordinate;
        }

        /// <summary>
        /// Character the coordinate was loaded to
        /// </summary>
        public Human Character { get; }

        /// <summary>
        /// The loaded coordinate
        /// </summary>
        public CustomParameter LoadedCoordinate { get; }

        /// <summary>
        /// Get all exrtended data assigned to this coordinate card
        /// </summary>
        public Dictionary<string, PluginData> GetCoordinateExtData() => ExtendedSave.GetAllExtendedData(LoadedCoordinate);

        /// <summary>
        /// Set extended data for this coordinate card
        /// </summary>
        /// <param name="dataId">Key to save the data under (usually plugin GUID)</param>
        /// <param name="data">Data to set</param>
        public void SetCoordinateExtData(string dataId, PluginData data) => ExtendedSave.SetExtendedDataById(LoadedCoordinate, dataId, data);
    }
}
