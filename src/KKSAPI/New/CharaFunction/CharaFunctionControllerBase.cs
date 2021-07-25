using ExtensibleSaveFormat;
using UnityEngine;

namespace ModdingAPI
{
    public abstract class CharaFunctionControllerBase
    {
        #region Init

        internal CharaFunctionControllerBase(CharaFunctionRegistration source, CharaFunctionManager owner)
        {
            Owner = owner;
            Source = source;
        }

        public CharaFunctionRegistration Source { get; }
        public CharaFunctionManager Owner { get; }
        public Transform CharacterRoot => Owner.ChaControl.transform;

        public bool Initialized { get; private set; }
        public bool Destroyed => Owner == null;

        internal void OnInitialize()
        {
            if(Initialized) throw new System.InvalidOperationException("OnInitialize called for the second time");
            Initialize(Owner);
            Initialized = true;
        }

        protected abstract void Initialize(CharaFunctionManager owner);

        #endregion

        #region ExtData

        /// <summary>
        /// Opens the clothes extended data. The data is saved automatically so it doesn't have to be set manually.
        /// Simply add keys to the data dictionary and they will be saved to the card on next card save.
        /// You should call this method every time you need to read a value in case it changes because of a clothing change.
        /// By default opens ext data for the currently loaded coordinate.
        /// </summary>
        /// <param name="coordinateId">The coordinate number to open extened data for if the game supports multiple coords (0-indexed)</param>
        public PluginData OpenClothesExtData(int coordinateId = -1)
        {
            var clothes = (coordinateId < 0 ? Owner.ChaControl.nowCoordinate : Owner.ChaFileControl.coordinate[coordinateId]).clothes;
            clothes.TryGetExtendedDataById(Source.ExtDataGuid, out var data);
            if (data == null)
            {
                data = new PluginData();
                clothes.SetExtendedDataById(Source.ExtDataGuid, data);
            }
            return data;
        }
        public PluginData OpenAccessoryExtData(int accessoryPartId, int coordinateId = -1)
        {
            var coord = (coordinateId < 0 ? Owner.ChaControl.nowCoordinate : Owner.ChaFileControl.coordinate[coordinateId]);
            var accessoryPart = coord.accessory.parts[accessoryPartId];
            accessoryPart.TryGetExtendedDataById(Source.ExtDataGuid, out var data);
            if (data == null)
            {
                data = new PluginData();
                accessoryPart.SetExtendedDataById(Source.ExtDataGuid, data);
            }
            return data;
        }
        public PluginData OpenBodyExtData()
        {
            Owner.ChaFileControl.custom.body.TryGetExtendedDataById(Source.ExtDataGuid, out var data);
            if (data == null)
            {
                data = new PluginData();
                Owner.ChaFileControl.custom.body.SetExtendedDataById(Source.ExtDataGuid, data);
            }
            return data;
        }
        public PluginData OpenFaceExtData()
        {
            Owner.ChaFileControl.custom.face.TryGetExtendedDataById(Source.ExtDataGuid, out var data);
            if (data == null)
            {
                data = new PluginData();
                Owner.ChaFileControl.custom.face.SetExtendedDataById(Source.ExtDataGuid, data);
            }
            return data;
        }
        public PluginData OpenParameterExtData()
        {
            Owner.ChaFileControl.parameter.TryGetExtendedDataById(Source.ExtDataGuid, out var data);
            if (data == null)
            {
                data = new PluginData();
                Owner.ChaFileControl.parameter.SetExtendedDataById(Source.ExtDataGuid, data);
            }
            return data;
        }

        //todo global extdata

        #endregion
    }
}