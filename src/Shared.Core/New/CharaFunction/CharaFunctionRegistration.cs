using System;
using BepInEx;

namespace ModdingAPI
{
    public class CharaFunctionRegistration
    {
        public PluginInfo Owner { get; }
        public Type ControllerType { get; }
        public string ExtDataGuid { get; }
        public int Priority { get; set; }

        internal CharaFunctionRegistration(PluginInfo owner, string extDataGuid, Type controllerType)
        {
            if (controllerType is null)
                throw new ArgumentNullException(nameof(controllerType));
            if (!typeof(CharaFunctionControllerBase).IsAssignableFrom(controllerType))
                throw new ArgumentException("Invalid controller type, it has to inherit from CharaFunctionControllerBase", nameof(controllerType));

            if (owner == null && extDataGuid == null)
                throw new ArgumentNullException(nameof(owner), "both owner and extDataGuid are null, at least one is needed");

            Owner = owner;
            ControllerType = controllerType;
            ExtDataGuid = extDataGuid ?? owner.Metadata.GUID;
        }
    }
}