using System;
using System.Collections.Generic;
using BepInEx;

namespace ModdingAPI
{
    public abstract class CharaFunctionAPI
    {
        protected internal CharaFunctionAPI()
        {
            KKAPI.Chara.CharacterApi.RegisterExtraBehaviour<CharaFunctionManager>(nameof(CharaFunctionManager));
        }

        public Dictionary<string, CharaFunctionRegistration> RegisteredControllers { get; } = new Dictionary<string, CharaFunctionRegistration>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type of the controller</typeparam>
        /// <param name="owner">The plugin that owns this controller.</param>
        /// <param name="customExtDataGuid">Custom guid for use with ext data. If null, the owning plugin's guid will be used.</param>
        /// <returns></returns>
        public CharaFunctionRegistration RegisterCharaFunctionController<T>(PluginInfo owner, string customExtDataGuid = null) where T : CharaFunctionControllerBase
        {
            return RegisterCharaFunctionController(typeof(T), owner, customExtDataGuid);
        }

        public CharaFunctionRegistration RegisterCharaFunctionController(Type controllerType, PluginInfo owner, string customExtDataGuid = null)
        {
            var reg = new CharaFunctionRegistration(owner, customExtDataGuid, controllerType);
            RegisteredControllers.Add(reg.ExtDataGuid, reg);
            return reg;
        }
    }
}