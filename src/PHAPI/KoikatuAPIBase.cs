using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KKAPI
{
    /// <summary>
    /// Provides overall information about the game and the API itself, and provides some useful tools.
    /// More information is available in project wiki at https://github.com/ManlyMarco/KKAPI/wiki
    /// </summary>
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ExtensibleSaveFormat.ExtendedSave.GUID)]
    [BepInIncompatibility("com.bepis.makerapi")]
    public partial class KoikatuAPI
    {
        /// <summary>
        /// Version of this assembly/plugin.
        /// WARNING: This is a const field, therefore it will be copied to your assembly!
        /// Use this field to check if the installed version of the plugin is up to date by adding this attribute to your plugin class:
        /// <code>[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]</code>
        /// THIS VALUE WILL NOT BE READ FROM THE INSTALLED VERSION, YOU WILL READ THE VALUE FROM THIS VERSION THAT YOU COMPILE YOUR PLUGIN AGAINST!
        /// More info: https://stackoverflow.com/questions/55984/what-is-the-difference-between-const-and-readonly
        /// </summary>
        public const string VersionConst = "1.15.1";

        /// <summary>
        /// GUID of this plugin, use for checking dependancies with <see cref="BepInDependency"/>."/>
        /// </summary>
        public const string GUID = "marco.kkapi";

        /// <summary>
        /// Enables display of additional log messages when certain events are triggered within KKAPI. 
        /// Useful for plugin devs to understand when controller messages are fired.
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public static bool EnableDebugLogging
        {
            get => EnableDebugLoggingSetting.Value;
            set => EnableDebugLoggingSetting.Value = value;
        }

        private static ConfigEntry<bool> EnableDebugLoggingSetting { get; set; }

        internal static KoikatuAPI Instance { get; private set; }
        internal static new ManualLogSource Logger { get; private set; }

        /// <summary>
        /// Don't use manually
        /// </summary>
        public KoikatuAPI()
        {
            Instance = this;
            Logger = base.Logger;

            EnableDebugLoggingSetting = Config.Bind("Debug", "Show debug messages", false, "Enables display of additional log messages when certain events are triggered within KKAPI. Useful for plugin devs to understand when controller messages are fired. Changes take effect after game restart.");

            Logger.LogDebug($"Game version {GetGameVersion()} running under {System.Threading.Thread.CurrentThread.CurrentCulture.Name} culture");

            var abdata = Path.Combine(Paths.GameRootPath, "abdata");
            if (Directory.Exists(abdata))
            {
                var addFiles = Directory.GetFiles(abdata, "add*", SearchOption.TopDirectoryOnly);
                if (addFiles.Any())
                {
                    var addFileNumbers = addFiles.Select(Path.GetFileName)
                        .Where(x => x?.Length > 3)
                        .Select(x => x.Substring(3))
                        .OrderBy(x => x, new WindowsStringComparer())
                        .ToArray();

                    Logger.LogDebug("Installed DLC: " + string.Join(" ", addFileNumbers));
                }
            }

            Logger.LogDebug($"Processor: {SystemInfo.processorType} ({SystemInfo.processorCount} threads @ {SystemInfo.processorFrequency}MHz); RAM: {SystemInfo.systemMemorySize}MB ({MemoryInfo.GetCurrentStatus()?.dwMemoryLoad.ToString() ?? "--"}% used); OS: {SystemInfo.operatingSystem}");

            SceneManager.sceneLoaded += (scene, mode) => Logger.LogDebug($"SceneManager.sceneLoaded - {scene.name} in {mode} mode");
            SceneManager.sceneUnloaded += scene => Logger.LogDebug($"SceneManager.sceneUnloaded - {scene.name}");
            SceneManager.activeSceneChanged += (prev, next) => Logger.LogDebug($"SceneManager.activeSceneChanged - from {prev.name} to {next.name}");
        }
    }
}
