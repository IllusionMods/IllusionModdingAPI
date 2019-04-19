using System;
using ChaCustom;

namespace KKAPI.Maker
{
    public sealed class AccessorySlotChangeEventArgs : EventArgs
    {
        public AccessorySlotChangeEventArgs(int slotIndex)
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
        public CvsAccessory CvsAccessory => AccessoriesApi.GetCvsAccessory(SlotIndex);

        /// <summary>
        /// Get accessory component.
        /// </summary>
        public ChaAccessoryComponent AccessoryComponent => MakerAPI.GetCharacterControl().GetAccessory(SlotIndex);
    }
}