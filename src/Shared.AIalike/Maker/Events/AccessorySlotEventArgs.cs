using System;
using AIChara;
using CharaCustom;
#pragma warning disable 612

namespace KKAPI.Maker
{
    /// <summary>
    /// Event args for events that are related to accessory slot indexes.
    /// </summary>
    public class AccessorySlotEventArgs : EventArgs
    {
        /// <inheritdoc />
        public AccessorySlotEventArgs(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        /// <summary>
        /// Currently opened accessory slot index. 0-indexed.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// Get accessory UI entry in maker.
        /// </summary>
        public CustomAcsCorrectSet CvsAccessory => AccessoriesApi.GetCvsAccessory();

        /// <summary>
        /// Get accessory component.
        /// </summary>
        public CmpAccessory AccessoryComponent => MakerAPI.GetCharacterControl().GetAccessory(SlotIndex);
    }
}
