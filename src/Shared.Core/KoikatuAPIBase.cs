#if !PH && !KK && !EC
#define QUITTING_EVENT_AVAILABLE
#endif

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
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
    [BepInDependency(ExtensibleSaveFormat.ExtendedSave.GUID, ExtensibleSaveFormat.ExtendedSave.Version)]
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
        public const string VersionConst = "1.28";

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
        /// Can be used to detect if application is currently quitting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if application is quitting; otherwise, <c>false</c>.
        /// </value>
        public static bool IsQuitting { get; internal set; }

        /// <summary>
        /// Occurs when application is quitting.
        /// Plugins can use this to do things like write config files and caches, or stop outstanding coroutines to prevent shutdown delays.
        /// Note: This event might not fire if the game isn't closed cleanly (hard crashes, killed process, closing the console window, etc.).
        /// </summary>
        public event EventHandler Quitting;

        /// <summary>
        /// Needs to be called at the start of Awake
        /// </summary>
        private void BaseAwake()
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

            if (!TomlTypeConverter.CanConvert(typeof(Rect)))
            {
                TomlTypeConverter.AddConverter(typeof(Rect), new TypeConverter
                {
                    ConvertToObject = (s, type) =>
                    {
                        var result = new Rect();
                        if (s != null)
                        {
                            var cleaned = s.Trim('{', '}').Replace(" ", "");
                            foreach (var part in cleaned.Split(','))
                            {
                                var parts = part.Split(':');
                                if (parts.Length == 2 && float.TryParse(parts[1], out var value))
                                {
                                    var id = parts[0].Trim('"');
                                    if (id == "x") result.x = value;
                                    else if (id == "y") result.y = value;
                                    // Check z and w in case something was using Vector4 to serialize a Rect before
                                    else if (id == "width" || id == "z") result.width = value;
                                    else if (id == "height" || id == "w") result.height = value;
                                }
                            }
                        }
                        return result;
                    },
                    ConvertToString = (o, type) =>
                    {
                        var rect = (Rect)o;
                        return string.Format(CultureInfo.InvariantCulture,
                            "{{ \"x\":{0}, \"y\":{1}, \"width\":{2}, \"height\":{3} }}",
                            rect.x, rect.y, rect.width, rect.height);
                    }
                });
            }

#if QUITTING_EVENT_AVAILABLE
            UnityEngine.Application.quitting += () => OnQuitting(EventArgs.Empty);
#else
            Harmony.CreateAndPatchAll(typeof(SharedHooks), $"{GetType().FullName}.{nameof(SharedHooks)}");
#endif
        }

        /// <summary>
        /// Check if a plugin is loaded and has at least the minimum version. 
        /// If the plugin is missing or older than minimumVersion, user is shown an error message on screen and false is returned.
        /// Warning: Run only from Start, not from constructor or Awake because some plugins might not be loaded yet!
        /// </summary>
        /// <param name="origin">Your plugin</param>
        /// <param name="guid">Guid of the plugin your plugin is dependant on</param>
        /// <param name="minimumVersion">Minimum version of the required plugin</param>
        /// <param name="level">Level of the issue - <code>Error</code> if plugin can't work, <code>Warning</code> if there might be issues, or <code>None</code> to not show any message.</param>
        /// <returns>True if plugin exists and it's version equals or is newer than minimumVersion, otherwise false</returns>
        [Obsolete("Use the new overload of BepInDependency attribute with version number")]
        public static bool CheckRequiredPlugin(BaseUnityPlugin origin, string guid, Version minimumVersion, BepInEx.Logging.LogLevel level = BepInEx.Logging.LogLevel.Error)
        {
            var target = BepInEx.Bootstrap.Chainloader.Plugins
                .Where(x => x != null)
                .Select(MetadataHelper.GetMetadata)
                .FirstOrDefault(x => x.GUID == guid);
            if (target == null)
            {
                if (level != BepInEx.Logging.LogLevel.None)
                {
                    Logger.LogMessage($"{level.ToString().ToUpper()}: Plugin \"{guid}\" required by \"{MetadataHelper.GetMetadata(origin).GUID}\" was not found!");
                }

                return false;
            }
            if (minimumVersion > target.Version)
            {
                if (level != BepInEx.Logging.LogLevel.None)
                {
                    Logger.LogMessage($"{level.ToString().ToUpper()}: Plugin \"{guid}\" required by \"{MetadataHelper.GetMetadata(origin).GUID}\" is outdated! At least v{minimumVersion} is needed!");
                }

                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if a plugin that is not compatible with your plugin is loaded. 
        /// If the plugin is loaded, user is shown a warning message on screen and true is returned.
        /// Warning: Run only from Start, not from constructor or Awake because some plugins might not be loaded yet!
        /// </summary>
        /// <param name="origin">Your plugin</param>
        /// <param name="guid">Guid of the plugin your plugin is incompatible with</param>
        /// <param name="level">Level of the issue - <code>Error</code> if plugin can't work, <code>Warning</code> if there might be issues, or <code>None</code> to not show any message.</param>
        /// <returns>True if plugin exists, otherwise false</returns>
        [Obsolete("Use the new attribute BepInIncompatibility")]
        public static bool CheckIncompatiblePlugin(BaseUnityPlugin origin, string guid, BepInEx.Logging.LogLevel level = BepInEx.Logging.LogLevel.Warning)
        {
            var target = BepInEx.Bootstrap.Chainloader.Plugins
                .Where(x => x != null)
                .Select(MetadataHelper.GetMetadata)
                .FirstOrDefault(x => x.GUID == guid);
            if (target != null)
            {
                if (level != BepInEx.Logging.LogLevel.None)
                {
                    Logger.LogMessage($"{level.ToString().ToUpper()}: Plugin \"{guid}\" is incompatible with \"{MetadataHelper.GetMetadata(origin).GUID}\"!");
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Invoke the Action on the main unity thread. Use to synchronize your threads.
        /// </summary>
        [Obsolete("Use ThreadingHelper.Instance.StartSyncInvoke instead")]
        public static void SynchronizedInvoke(Action action)
        {
            ThreadingHelper.Instance.StartSyncInvoke(action);
        }

        //[System.Diagnostics.Conditional("DEBUG")]
        internal static void Assert(bool success, string error)
        {
            if (success) return;
            //todo uncomment? or keep commented for easier future debugging? 
            //if (!EnableDebugLogging) return;
            Logger.LogWarning("Assertion failed: " + error + "\nat: " + new System.Diagnostics.StackTrace(1));
        }

        internal void OnQuitting(EventArgs e)
        {
            if (EnableDebugLogging) Logger.LogWarning("KoikatuAPI - OnQuitting");
            IsQuitting = true;
            Quitting?.Invoke(this, e);
        }


#if !QUITTING_EVENT_AVAILABLE
        private static class SharedHooks
        {

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Manager.Scene), nameof(Manager.Scene.GameExit))]
            private static void GameExitPrefix()
            {
                try
                {
                    Instance.OnQuitting(EventArgs.Empty);
                }
                catch (Exception err)
                {
                    Logger.LogError($"Unexpected error during game exit: {err.Message}\n at {err.StackTrace}");
                }
            }
        }
#endif
    }
}
