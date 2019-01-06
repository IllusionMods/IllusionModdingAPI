using ExtensibleSaveFormat;
using UnityEngine;

namespace MakerAPI
{
    /// <summary>
    /// Base type for custom character extensions.
    /// Is automatically instantiated by the API on root gameObjects of all characters, next to their <code>ChaControl</code>s.
    /// </summary>
    public abstract class CharaExtraBehaviour : MonoBehaviour
    {
        protected CharaExtraBehaviour(string extendedDataId)
        {
            ExtendedDataId = extendedDataId;
        }

        public ChaControl ChaControl => GetComponent<ChaControl>();
        public ChaFileControl ChaFileControl => ChaControl.chaFile;

        public string ExtendedDataId { get; }
        public bool Started { get; private set; }

        public PluginData GetExtendedData()
        {
            return ExtendedSave.GetExtendedDataById(ChaFileControl, ExtendedDataId);
        }

        public void SetExtendedData(PluginData data)
        {
            ExtendedSave.SetExtendedDataById(ChaFileControl, ExtendedDataId, data);
        }

        /// <summary>
        /// Card is about to be saved. Write any extended data now by using <code>SetExtendedData</code>.
        /// Only fires in character maker, since that's the only time when a card can be modified.
        /// </summary>
        protected internal abstract void OnCardBeingSaved();

        /// <summary>
        /// Override to supply custom extended data copying logic.
        /// By default copies all data under <code>ExtendedDataId</code> by reference.
        /// </summary>
        /// <param name="copyTo">Copy current character's ext data to this character</param>
        protected internal virtual void OnCopyExtendedData(ChaFile copyTo)
        {
            var extendedData = ExtendedSave.GetExtendedDataById(ChaFileControl, ExtendedDataId);
            if (extendedData != null)
                ExtendedSave.SetExtendedDataById(copyTo, ExtendedDataId, extendedData);
        }

        /// <summary>
        /// The character is being reloaded. Reset, load and set up your modifications here.
        /// Called automatically on start, and whenever the character was changed in some way
        /// </summary>
        protected internal abstract void OnReload();

        protected virtual void OnEnable()
        {
            // Order is Awake - OnEnable - Start, so need to check if we started yet
            if (Started)
                OnReload();
        }

        protected virtual void Start()
        {
            Started = true;
            OnReload();
        }
    }
}
