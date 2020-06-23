using System;
using System.Collections.Generic;
using System.Linq;
#if KK || EC
using UniRx;
#elif AI || HS2
using AIChara;
#endif

namespace KKAPI.Chara
{
    public static partial class CharacterApi
    {
        /// <summary>
        /// Information about a single kind of a <see cref="CharaCustomFunctionController"/>. New kind is created every time
        /// you call <see cref="CharacterApi.RegisterExtraBehaviour{T}(string)"/>.
        /// </summary>
        public sealed class ControllerRegistration
        {
            private readonly List<CharaCustomFunctionController> _instances = new List<CharaCustomFunctionController>();

            /// <summary>
            /// All currently existing instances of this kind of controller.
            /// </summary>
            public IEnumerable<CharaCustomFunctionController> Instances => _instances.Where(x => x != null);

            /// <summary>
            /// Type of the custom controller kind.
            /// </summary>
            public Type ControllerType { get; }

            /// <summary>
            /// ID of the extended data used by this controller kind.
            /// </summary>
            public string ExtendedDataId { get; }

            /// <summary>
            /// Method used to copy extended data used by this controller in case that's necessary.
            /// </summary>
            public CopyExtendedDataFunc ExtendedDataCopier { get; }

            /// <summary>
            /// If true, the current state of all controllers of this kind should be preserved inside <see cref="CharaCustomFunctionController.OnReload(GameMode,bool)"/>.
            /// New extended data will not be read, instead currently data will be reused, or the reload will not happen at all.
            /// </summary>
            public bool MaintainState { get; set; }

            /// <summary>
            /// If true, the current state of all controllers of this kind should be preserved inside <see cref="CharaCustomFunctionController.OnCoordinateBeingLoaded(ChaFileCoordinate,bool)"/>.
            /// New extended data will not be read, instead currently data will be reused, or the load will not happen at all.
            /// </summary>
            public bool MaintainCoordinateState { get; set; }

            internal ControllerRegistration(Type controllerType, string extendedDataId, CopyExtendedDataFunc extendedDataCopier)
            {
                ControllerType = controllerType;
                ExtendedDataId = extendedDataId;
                ExtendedDataCopier = extendedDataCopier;
            }

            internal void CreateInstance(ChaControl target)
            {
                var newBehaviour = (CharaCustomFunctionController) target.gameObject.AddComponent(ControllerType);
                newBehaviour.ControllerRegistration = this;

                _instances.Add(newBehaviour);
                _instances.RemoveAll(x => x == null);
            }
        }
    }
}
