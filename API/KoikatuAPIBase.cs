using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using KKAPI.Maker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KKAPI
{
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class KoikatuAPI
    {
        private static readonly object _invokeLock = new object();
        private static Action _invokeList;

        /// <summary>
        /// Version of this assembly/plugin.
        /// WARNING: This is a const field, therefore it will be copied to your assembly!
        /// Use this field to check if the installed version of the plugin is up to date by doing this:
        /// <code>KoikatuAPI.CheckRequiredPlugin(this, KoikatuAPI.GUID, new Version(KoikatuAPI.VersionConst), LogLevel.Warning)</code>
        /// THIS VALUE WILL NOT BE READ FROM THE INSTALLED VERSION, YOU WILL READ THE VALUE FROM THIS VERSION THAT YOU COMPILE YOUR PLUGIN AGAINST!
        /// More info: https://stackoverflow.com/questions/55984/what-is-the-difference-between-const-and-readonly
        /// </summary>
        public const string VersionConst = "1.3.7";

        /// <summary>
        /// GUID of this plugin, use for checking dependancies with <see cref="BepInDependency"/> and <see cref="CheckRequiredPlugin"/>
        /// </summary>
        public const string GUID = "marco.kkapi";

        /// <summary>
        /// Enables display of additional log messages when certain events are triggered within KKAPI. 
        /// Useful for plugin devs to understand when controller messages are fired.
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public static bool EnableDebugLogging
        {
            get
            {
#if DEBUG
                return true;
#endif
                return EnableDebugLoggingSetting.Value;
            }
            set
            {
                EnableDebugLoggingSetting.Value = value;
            }
        }

        internal static KoikatuAPI Instance { get; private set; }

        /// <summary>
        /// Don't use manually
        /// </summary>
        public KoikatuAPI()
        {
            Instance = this;

            Log(LogLevel.Debug, $"Game version {GetGameVersion()} running under {System.Threading.Thread.CurrentThread.CurrentCulture.Name} culture");
            Log(LogLevel.Debug, $"Processor: {SystemInfo.processorType} ({SystemInfo.processorCount} cores @ {SystemInfo.processorFrequency}MHz); RAM: {SystemInfo.systemMemorySize}MB; OS: {SystemInfo.operatingSystem}");

            SceneManager.sceneLoaded += (scene, mode) => Log(LogLevel.Debug, $"SceneManager.sceneLoaded - {scene.name} in {mode} mode");
            SceneManager.sceneUnloaded += scene => Log(LogLevel.Debug, $"SceneManager.sceneUnloaded - {scene.name}");
            SceneManager.activeSceneChanged += (prev, next) => Log(LogLevel.Debug, $"SceneManager.activeSceneChanged - from {prev.name} to {next.name}");
        }

        private void Start()
        {
            // Needs to be called after moreaccessories has a chance to load
            AccessoriesApi.Init();
        }

        private bool CheckIncompatibilities()
        {
            if (CheckIncompatiblePlugin(this, "com.bepis.makerapi", LogLevel.Error))
            {
                Log(LogLevel.Error | LogLevel.Message, "MakerAPI is no longer supported and is preventing KKAPI from loading!");
                Log(LogLevel.Error | LogLevel.Message, "Remove MakerAPI.dll and update all mods that used it to fix this.");
                return false;
            }
            return true;
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
        public static bool CheckRequiredPlugin(BaseUnityPlugin origin, string guid, Version minimumVersion, LogLevel level = LogLevel.Error)
        {
            var target = BepInEx.Bootstrap.Chainloader.Plugins
                .Select(MetadataHelper.GetMetadata)
                .FirstOrDefault(x => x.GUID == guid);
            if (target == null)
            {
                if (level != LogLevel.None)
                {
                    KoikatuAPI.Log(LogLevel.Message | level,
                        $"{level.ToString().ToUpper()}: Plugin \"{guid}\" required by \"{MetadataHelper.GetMetadata(origin).GUID}\" was not found!");
                }

                return false;
            }
            if (minimumVersion > target.Version)
            {
                if (level != LogLevel.None)
                {
                    KoikatuAPI.Log(LogLevel.Message | level,
                        $"{level.ToString().ToUpper()}: Plugin \"{guid}\" required by \"{MetadataHelper.GetMetadata(origin).GUID}\" is outdated! At least v{minimumVersion} is needed!");
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
        public static bool CheckIncompatiblePlugin(BaseUnityPlugin origin, string guid, LogLevel level = LogLevel.Warning)
        {
            var target = BepInEx.Bootstrap.Chainloader.Plugins
                .Select(MetadataHelper.GetMetadata)
                .FirstOrDefault(x => x.GUID == guid);
            if (target != null)
            {
                if (level != LogLevel.None)
                {
                    KoikatuAPI.Log(LogLevel.Message | level,
                        $"{level.ToString().ToUpper()}: Plugin \"{guid}\" is incompatible with \"{MetadataHelper.GetMetadata(origin).GUID}\"!");
                }

                return true;
            }
            return false;
        }

        #region Synchronization

        /// <summary>
        /// Invoke the Action on the main unity thread. Use to synchronize your threads.
        /// </summary>
        public static void SynchronizedInvoke(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_invokeLock) _invokeList += callback;
        }

        private void Update()
        {
            // Safe to do outside of lock because nothing can remove callbacks, at worst we execute with 1 frame delay
            if (_invokeList == null) return;

            Action toRun;
            lock (_invokeLock)
            {
                toRun = _invokeList;
                _invokeList = null;
            }

            // Need to execute outside of the lock in case the callback itself calls Invoke we could deadlock
            // The invocation would also block any threads that call Invoke
            toRun();
        }

        #endregion
    }
}