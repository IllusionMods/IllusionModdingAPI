using System;
using ExtensibleSaveFormat;
using UniRx;
using UnityEngine;

namespace MakerAPI.Chara
{
    /// <summary>
    /// Base type for custom character extensions.
    /// Is automatically instantiated by the API on root gameObjects of all characters, next to their <code>ChaControl</code>s.
    /// </summary>
    public abstract class CharaCustomFunctionController : MonoBehaviour
    {
        public ChaControl ChaControl { get; private set; }
        public ChaFileControl ChaFileControl => ChaControl.chaFile;

        public string ExtendedDataId { get; internal set; }
        public bool Started { get; private set; }

        public PluginData GetExtendedData()
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            return ExtendedSave.GetExtendedDataById(ChaFileControl, ExtendedDataId);
        }

        public void SetExtendedData(PluginData data)
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            ExtendedSave.SetExtendedDataById(ChaFileControl, ExtendedDataId, data);
        }

        /// <summary>
        /// Card is about to be saved. Write any extended data now by using <code>SetExtendedData</code>.
        /// Only fires in character maker, since that's the only time when a card can be modified.
        /// </summary>
        protected internal abstract void OnCardBeingSaved(GameMode currentGameMode);
        
        /// <summary>
        /// The character is being reloaded. Reset, load and set up your modifications here.
        /// Called automatically on start, and whenever the character was changed in some way
        /// </summary>
        protected internal abstract void OnReload(GameMode currentGameMode);

        public BehaviorSubject<ChaFileDefine.CoordinateType> CurrentCoordinate { get; private set; }

        protected virtual void Update()
        {
            // TODO change into a separate trigger component?
            var currentCoordinate = (ChaFileDefine.CoordinateType)ChaControl.fileStatus.coordinateType;
            if (currentCoordinate != CurrentCoordinate.Value)
                CurrentCoordinate.OnNext(currentCoordinate);
        }

        protected virtual void OnDestroy()
        {
            CurrentCoordinate.Dispose();
        }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void OnEnable()
        {
            // Order is Awake - OnEnable - Start, so need to check if we started yet
            if (Started)
                OnReload(MakerAPI.Instance.GetCurrentGameMode());
        }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void Start()
        {
            ChaControl = GetComponent<ChaControl>();
            CurrentCoordinate = new BehaviorSubject<ChaFileDefine.CoordinateType>((ChaFileDefine.CoordinateType)ChaControl.fileStatus.coordinateType);
            Started = true;
            OnReload(MakerAPI.Instance.GetCurrentGameMode());
        }
    }
}
