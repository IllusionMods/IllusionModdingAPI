using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI.Maker;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
#if !EC
using KKAPI.Studio;
#endif

namespace KKAPI.Utilities
{
    /// <summary>
    /// Template class for handling texture saving.
    /// </summary>
    public abstract class TextureSaveHandlerBase
    {
        /// <summary>
        /// Where local textures should be saved
        /// </summary>
        public string LocalTexturePath;
        /// <summary>
        /// Prefix for locally saved texture files
        /// </summary>
        public readonly string LocalTexPrefix;
        /// <summary>
        /// Prefix for local identification data in PluginData
        /// </summary>
        public readonly string LocalTexSavePrefix;
        /// <summary>
        /// Prefix for deduped identification data in PluginData
        /// </summary>
        public readonly string DedupedTexSavePrefix;
        /// <summary>
        /// Postfix for deduped texture data in PluginData
        /// </summary>
        public readonly string DedupedTexSavePostfix;
        /// <summary>
        /// Name of folder within local texture folder where unused textures can be automatically placed
        /// </summary>
        public readonly string LocalTexUnusedFolder;

        // Audit variables
        private static readonly List<TextureSaveHandlerBase> auditPlugins = new List<TextureSaveHandlerBase>();
        private static readonly Dictionary<TextureSaveHandlerBase, string> auditPluginNames = new Dictionary<TextureSaveHandlerBase, string>();
        private static readonly Dictionary<TextureSaveHandlerBase, byte[]> auditPluginSearchBytes = new Dictionary<TextureSaveHandlerBase, byte[]>();
        private static int auditAllFiles = 0;
        private static int auditProcessedFiles = 0;
        private static int auditRunningThread = 0;
        private static readonly Dictionary<TextureSaveHandlerBase, Dictionary<string, string>> auditUnusedTextures = new Dictionary<TextureSaveHandlerBase, Dictionary<string, string>>();
        private static readonly Dictionary<TextureSaveHandlerBase, Dictionary<string, List<string>>> auditMissingTextures = new Dictionary<TextureSaveHandlerBase, Dictionary<string, List<string>>>();
        private static readonly Dictionary<TextureSaveHandlerBase, Dictionary<string, List<string>>> auditFoundHashToFiles = new Dictionary<TextureSaveHandlerBase, Dictionary<string, List<string>>>();
        private static readonly object auditLock = new object();
        private static bool auditShow = false;
        private static int auditNowPlugin = 0;
        private static Coroutine auditDoneCoroutine = null;
        private static Rect auditRect = new Rect();
        private static Vector2 auditUnusedScroll = Vector2.zero;
        private static Vector2 auditMissingScroll = Vector2.zero;
        private static GUIStyle _auditLabel = null;
        private static GUIStyle AuditLabel
        {
            get
            {
                if (_auditLabel == null)
                {
                    _auditLabel = new GUIStyle(GUI.skin.label)
                    {
                        font = new Font(new[] { GUI.skin.font.name }, Mathf.RoundToInt(GUI.skin.font.fontSize * 1.25f))
                    };
                }
                return _auditLabel;
            }
        }
        private static GUIStyle _auditButton = null;
        private static GUIStyle AuditButton
        {
            get
            {
                if (_auditButton == null)
                {
                    _auditButton = new GUIStyle(GUI.skin.button)
                    {
                        font = AuditLabel.font
                    };
                }
                return _auditButton;
            }
        }
        private static GUIStyle _auditWindow = null;
        private static GUIStyle AuditWindow
        {
            get
            {
                if (_auditWindow == null)
                {
#if PH
                    _auditWindow = new GUIStyle(GUI.skin.window);
#else
                    _auditWindow = new GUIStyle(IMGUIUtils.SolidBackgroundGuiSkin.window);
#endif
                }
                return _auditWindow;
            }
        }
        private static GUIStyle _auditBigText = null;
        private static GUIStyle AuditBigText
        {
            get
            {
                if (_auditBigText == null)
                {
                    _auditBigText = new GUIStyle(AuditLabel)
                    {
                        font = new Font(new[] { AuditLabel.font.name }, Mathf.RoundToInt(AuditLabel.font.fontSize * 1.5f))
                    };
                }
                return _auditBigText;
            }
        }
        private static GUIStyle _auditWarnButton = null;
        private static GUIStyle AuditWarnButton
        {
            get
            {
                if (_auditWarnButton == null)
                {
                    _auditWarnButton = new GUIStyle(AuditButton)
                    {
                        font = AuditButton.font
                    };
                    var warnColor = new Color(1, 0.25f, 0.20f);
                    _auditWarnButton.normal.textColor = warnColor;
                    _auditWarnButton.active.textColor = warnColor;
                    _auditWarnButton.hover.textColor = warnColor;
                    _auditWarnButton.focused.textColor = warnColor;
                }
                return _auditWarnButton;
            }
        }

        /// <summary>
        /// Create and initialise the Texture Handler
        /// </summary>
        /// <param name="localTexturePath">Where local textures should be saved</param>
        /// <param name="localTexPrefix">Prefix for locally saved texture files</param>
        /// <param name="localTexSavePrefix">Prefix for local identification data in PluginData</param>
        /// <param name="dedupedTexSavePrefix">Prefix for deduped identification data in PluginData</param>
        /// <param name="dedupedTexSavePostfix">Postfix for deduped texture data in PluginData</param>
        /// <param name="localTexUnusedFolder">Name of folder where unused textures can be automatically placed</param>
        public TextureSaveHandlerBase(
            string localTexturePath,
            string localTexPrefix,
            string localTexSavePrefix = "LOCAL_",
            string dedupedTexSavePrefix = "DEDUPED_",
            string dedupedTexSavePostfix = "_DATA",
            string localTexUnusedFolder = "_Unused"
        ) {
            LocalTexturePath = localTexturePath;
            LocalTexPrefix = localTexPrefix;
            LocalTexSavePrefix = localTexSavePrefix;
            DedupedTexSavePrefix = dedupedTexSavePrefix;
            DedupedTexSavePostfix = dedupedTexSavePostfix;
            LocalTexUnusedFolder = localTexUnusedFolder;
        }

        /// <summary>
        /// Register the Texture Handler instance for auditing via the API audit function.
        /// To be eligible for automatic auditing, the locally saved data must be in the form of an
        /// (int, string) Dictionary, where the key is the ID of the texture, and the string is its
        /// hash encoded in hex, which is also used in the local file's name. The saved data has to
        /// be serialised via MessagePackSerializer.
        /// </summary>
        /// <param name="name">Name of the plugin for display purposes</param>
        /// <param name="searchString">Key of the hash dictionary used when saving to PluginData, ASCII only</param>
        public void RegisterForAudit(string name, string searchString)
        {
            if (auditPlugins.Contains(this)) return;
            auditPlugins.Add(this);
            auditPluginNames.Add(this, name);
            auditPluginSearchBytes.Add(this, System.Text.Encoding.ASCII.GetBytes(searchString));
        }

        internal static void AuditOptionDrawer(ConfigEntryBase configEntry)
        {
            if (GUILayout.Button("Audit Local Files", GUILayout.ExpandWidth(true)))
            {
                AuditLocalFiles();
                try
                {
                    if (Chainloader.PluginInfos.TryGetValue("com.bepis.bepinex.configurationmanager", out var cfgMgrInfo) && cfgMgrInfo != null)
                    {
                        var displaying = cfgMgrInfo.Instance.GetType().GetProperty("DisplayingWindow", AccessTools.all);
                        displaying.SetValue(cfgMgrInfo.Instance, false, null);
                    }
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError($"Error closing ConfigurationManager window!\n{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private static void AuditLocalFiles()
        {
            if (auditPlugins.Count == 0)
            {
                KoikatuAPI.Logger.LogMessage("No plugins have registered for auditing!");
                return;
            }

            foreach (var plugin in auditPlugins)
            {
                auditUnusedTextures[plugin] = new Dictionary<string, string>();
                auditMissingTextures[plugin] = new Dictionary<string, List<string>>();
                auditFoundHashToFiles[plugin] = new Dictionary<string, List<string>>();

                if (Directory.Exists(plugin.LocalTexturePath))
                {
                    string[] localTexFolderFiles = Directory.GetFiles(plugin.LocalTexturePath, plugin.LocalTexPrefix + "*", SearchOption.TopDirectoryOnly);
                    foreach (string file in localTexFolderFiles)
                        auditUnusedTextures[plugin].Add(Regex.Match(file, $"(?<={plugin.LocalTexPrefix})[A-F0-9]+(?=\\.)").Value, file.Split(Path.DirectorySeparatorChar).Last());
                }
                else
                {
                    KoikatuAPI.Logger.LogMessage($"No local texture folder found for {auditPluginNames[plugin]}!");
                }
            }

            var pngs = new List<string>();
            pngs.AddRange(Directory.GetFiles(Path.Combine(Paths.GameRootPath, @"UserData\chara"), "*.png", SearchOption.AllDirectories));
            pngs.AddRange(Directory.GetFiles(Path.Combine(Paths.GameRootPath, @"UserData\Studio\scene"), "*.png", SearchOption.AllDirectories));
            auditAllFiles = pngs.Count;
            auditProcessedFiles = 0;
            auditNowPlugin = 0;
            auditRect = new Rect();
            auditShow = true;

            int numThreads = Environment.ProcessorCount;
            auditRunningThread = numThreads;
            auditDoneCoroutine = KoikatuAPI.Instance.StartCoroutine(AuditLocalFilesDone());
            for (int i = 0; i < numThreads; i++)
            {
                int nowOffset = i;
                ThreadingHelper.Instance.StartAsyncInvoke(delegate
                {
                    AuditLocalFilesProcessor(pngs, numThreads, nowOffset);
                    --auditRunningThread;
                    return null;
                });
            }
        }

        private static void AuditLocalFilesProcessor(List<string> pngs, int period, int offset)
        {
            lock (KoikatuAPI.Logger)
                KoikatuAPI.Logger.LogDebug($"Starting new local file processor with period {period} and offset {offset}!");

            // There was no local saving before this point in time, so we can skip files not modified since
            DateTime cutoff = new DateTime(2025, 10, 21);

            string file;
            int i = offset;
            while (i < pngs.Count)
            {
                if (auditDoneCoroutine == null) return;

                file = pngs[i];
                if (
                    file != null && File.Exists(file)
                    && File.GetLastWriteTime(file) > cutoff
                    && new FileInfo(file).Length <= int.MaxValue
                )
                {
                    byte[] fileData = File.ReadAllBytes(file);
                    foreach (var plugin in auditPlugins)
                        ScourData(file, fileData, plugin);
                }

                lock (auditLock)
                    ++auditProcessedFiles;
                i += period;
            }

            lock (KoikatuAPI.Logger)
                KoikatuAPI.Logger.LogDebug($"Local file processor with offset {offset} done!");
        }

        private static void ScourData(string file, byte[] fileData, TextureSaveHandlerBase plugin)
        {
            int readingAt = 0;
            while (true)
            {
                int patternStart = FindPosition(fileData, auditPluginSearchBytes[plugin], readingAt);
                if (patternStart < 0) break;
                int posLenByte = patternStart + auditPluginSearchBytes[plugin].Length;

                int length = -1;
                int offset = 0;
                switch (fileData[posLenByte])
                {
                    case 0xC4:
                        length = fileData[posLenByte + 1];
                        offset = 2;
                        break;
                    case 0xC5:
                        length = (fileData[posLenByte + 1] << 8) + fileData[posLenByte + 2];
                        offset = 3;
                        break;
                    // I've never seen this case, it's only an extrapolation from the first two, but getting here would imply having
                    // more than 65535 bytes worth of local texture hashes in a single controller, which is unlikely anyway
                    case 0xC6:
                        length = (fileData[posLenByte + 1] << 16) + (fileData[posLenByte + 2] << 8) + fileData[posLenByte + 3];
                        offset = 4;
                        break;
                }
                if (length == -1) break;

                try
                {
                    byte[] dicBytes = fileData.Subset(posLenByte + offset, length).ToArray();
                    Dictionary<int, string> hashDict = MessagePackSerializer.Deserialize<Dictionary<int, string>>(dicBytes);
                    if (hashDict != null && hashDict.Count > 0)
                        foreach (var kvp in hashDict)
                            lock (auditFoundHashToFiles[plugin])
                                if (!auditFoundHashToFiles[plugin].TryGetValue(kvp.Value, out var fileList))
                                    auditFoundHashToFiles[plugin].Add(kvp.Value, new List<string> { file });
                                else
                                    lock (fileList)
                                        fileList.Add(file);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError($"Error deserialising hash dictionary!\n{e.Message}\n{e.StackTrace}");
                    break;
                }
                readingAt = posLenByte + length;
            }
        }

        private static int FindPosition(byte[] data, byte[] pattern, int startPos)
        {
            int pos = startPos - 1;
            int foundPosition = -1;
            int at = 0;

            while (++pos < data.Length)
            {
                if (data[pos] == pattern[at])
                {
                    at++;
                    if (at == 1) foundPosition = pos;
                    if (at == pattern.Length) return foundPosition;
                }
                else
                {
                    at = 0;
                }
            }
            return -1;
        }

        private static IEnumerator AuditLocalFilesDone()
        {
            while (auditRunningThread > 0)
                yield return null;

            foreach (var plugin in auditPlugins)
                foreach (var kvp in auditFoundHashToFiles[plugin])
                    if (!auditUnusedTextures[plugin].Remove(kvp.Key))
                        auditMissingTextures[plugin].Add(kvp.Key, kvp.Value);

            auditDoneCoroutine = null;

            yield break;
        }

        private static void AuditWindowFunction(int windowID)
        {
            if (auditDoneCoroutine != null)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); GUILayout.FlexibleSpace();
                {
                    GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                    {
                        GUILayout.BeginVertical(GUI.skin.box);
                        {
                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace(); GUILayout.Label("Processing cards and scenes...", AuditBigText); GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace(); GUILayout.Label($"{auditProcessedFiles} / {auditAllFiles}", AuditLabel); GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace(); GUILayout.Label($"{Math.Round((double)auditProcessedFiles / auditAllFiles, 3) * 100:0.0}%", AuditLabel); GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Cancel", AuditButton, GUILayout.Width(100), GUILayout.Height(30)))
                            {
                                auditShow = false;
                                KoikatuAPI.Instance.StopCoroutine(auditDoneCoroutine);
                                auditDoneCoroutine = null;
                            }
                            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                            GUILayout.Space(10);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                }
                GUILayout.FlexibleSpace(); GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5);
                        if (GUILayout.Button("<", AuditButton, GUILayout.Width(30), GUILayout.Height(30)))
                        {
                            --auditNowPlugin;
                            if (auditNowPlugin < 0)
                                auditNowPlugin = auditPlugins.Count - 1;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"({auditNowPlugin + 1}/{auditPlugins.Count}) {auditPluginNames[auditPlugins[auditNowPlugin]]} local file audit results", AuditBigText);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(">", AuditButton, GUILayout.Width(30), GUILayout.Height(30)))
                        {
                            ++auditNowPlugin;
                            if (auditNowPlugin == auditPlugins.Count)
                                auditNowPlugin = 0;
                        }
                        GUILayout.Space(5);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(300));
                        {
                            if (auditUnusedTextures[auditPlugins[auditNowPlugin]] == null || auditUnusedTextures[auditPlugins[auditNowPlugin]].Count == 0)
                            {
                                GUILayout.BeginVertical(); GUILayout.FlexibleSpace();
                                GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                                GUILayout.Label("No unused textures found!", AuditLabel);
                                GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                                GUILayout.FlexibleSpace(); GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                                GUILayout.Label("Unused textures", AuditLabel);
                                GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                                GUILayout.Space(5);
                                auditUnusedScroll = GUILayout.BeginScrollView(auditUnusedScroll, false, true, GUI.skin.label, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.ExpandHeight(true));
                                {
                                    GUILayout.BeginVertical();
                                    {
                                        foreach (var kvp in auditUnusedTextures[auditPlugins[auditNowPlugin]])
                                            GUILayout.Label(kvp.Value);
                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndScrollView();
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
                        GUILayout.BeginVertical(GUI.skin.box);
                        {
                            if (auditMissingTextures[auditPlugins[auditNowPlugin]] == null || auditMissingTextures[auditPlugins[auditNowPlugin]].Count == 0)
                            {
                                GUILayout.BeginVertical(); GUILayout.FlexibleSpace();
                                GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                                GUILayout.Label("No missing textures found!", AuditLabel);
                                GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                                GUILayout.FlexibleSpace(); GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                                GUILayout.Label("Missing textures", AuditLabel);
                                GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
                                GUILayout.Space(5);
                                auditMissingScroll = GUILayout.BeginScrollView(auditMissingScroll, false, true, GUI.skin.label, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.ExpandHeight(true));
                                {
                                    GUILayout.BeginVertical();
                                    {
                                        foreach (var kvp in auditMissingTextures[auditPlugins[auditNowPlugin]])
                                        {
                                            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
                                            GUILayout.Label($"Missing texture hash: {kvp.Key}", AuditLabel);
                                            GUILayout.Label($"Used by:\n{string.Join(",\n", kvp.Value.ToArray())}", AuditLabel);
                                            GUILayout.EndVertical();
                                            GUILayout.Space(3);
                                        }
                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndScrollView();
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(4);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Delete unused files", AuditWarnButton, GUILayout.Height(30)))
                        {
                            foreach (var kvp in auditUnusedTextures[auditPlugins[auditNowPlugin]])
                                File.Delete(Path.Combine(auditPlugins[auditNowPlugin].LocalTexturePath, kvp.Value));
                            auditUnusedTextures[auditPlugins[auditNowPlugin]].Clear();
                        }
                        GUILayout.Space(5);
                        if (GUILayout.Button($"Move unused files to '{auditPlugins[auditNowPlugin].LocalTexUnusedFolder}' folder", AuditButton, GUILayout.Height(30)))
                        {
                            string unusedFolder = Path.Combine(auditPlugins[auditNowPlugin].LocalTexturePath, auditPlugins[auditNowPlugin].LocalTexUnusedFolder);
                            if (!Directory.Exists(unusedFolder))
                                Directory.CreateDirectory(unusedFolder);
                            foreach (var kvp in auditUnusedTextures[auditPlugins[auditNowPlugin]])
                                File.Move(Path.Combine(auditPlugins[auditNowPlugin].LocalTexturePath, kvp.Value), Path.Combine(unusedFolder, kvp.Value));
                            auditUnusedTextures[auditPlugins[auditNowPlugin]].Clear();
                        }
                        GUILayout.Space(5);
                        if (GUILayout.Button("Close", AuditButton, GUILayout.Height(30)))
                        {
                            auditUnusedTextures.Clear();
                            auditMissingTextures.Clear();
                            auditFoundHashToFiles.Clear();
                            auditShow = false;
                        }
                        GUILayout.Space(4);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }
                GUILayout.EndVertical();
            }
        }

        internal static void DoOnGUI()
        {
            if (auditShow)
            {
                Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
                for (int i = 0; i < 4; i++) GUI.Box(screenRect, "");
                auditRect.position = new Vector2((Screen.width - auditRect.size.x) / 2, (Screen.height - auditRect.size.y) / 2);
                float minWidth = Mathf.Clamp(Screen.width / 2, 960, 1280);
                auditRect = GUILayout.Window(42069, auditRect, AuditWindowFunction, "", AuditWindow, GUILayout.MinWidth(minWidth), GUILayout.MinHeight(Screen.height * 4 / 5));
                IMGUIUtils.EatInputInRect(screenRect);
            }
        }

        /// <summary>
        /// Save the provided data with the current settings
        /// </summary>
        /// <param name="pluginData">PluginData being populated in the save process</param>
        /// <param name="key">Key to use to save textures</param>
        /// <param name="data">Texture data to be saved</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        public virtual void Save(PluginData pluginData, string key, object data, bool isCharaController)
        {
            switch (DetermineSaveType())
            {
                case (int)CharaTextureSaveType.Local:
                    SaveLocal(pluginData, key, data, isCharaController);
                    break;
#if !EC
                case (int)SceneTextureSaveType.Deduped:
                    SaveDeduped(pluginData, key, data, isCharaController);
                    break;
#endif
                default:
                    SaveBundled(pluginData, key, data, isCharaController);
                    break;
            }
        }

        /// <summary>
        /// Determine save type and then load data from the provided PluginData
        /// </summary>
        /// <param name="pluginData">PluginData being loaded</param>
        /// <param name="key">Key to use to acquire data from PluginData</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        public virtual T Load<T>(PluginData pluginData, string key, bool isCharaController)
        {
            if (IsBundled(pluginData, key, out var dataBundled))
                return (T)LoadBundled(pluginData, key, dataBundled, isCharaController);
#if !EC
            else if (IsDeduped(pluginData, key, out var dataDeduped))
                return (T)LoadDeduped(pluginData, key, dataDeduped, isCharaController);
#endif
            else if (IsLocal(pluginData, key, out var dataLocal))
                return (T)LoadLocal(pluginData, key, dataLocal, isCharaController);
            return (T)DefaultData();
        }

        /// <summary>
        /// What to return when no data is able to be loaded.
        /// </summary>
        protected abstract object DefaultData();

        /// <summary>
        /// Determines whether the provided data was saved as bundled
        /// </summary>
        /// <param name="pluginData">ExtSave data being loaded</param>
        /// <param name="key">Texture data identifier</param>
        /// <param name="data">Bundled data extracted from pluginData</param>
        protected abstract bool IsBundled(PluginData pluginData, string key, out object data);

        /// <summary>
        /// Save the provided data with the provided key as bundled
        /// </summary>
        /// <param name="pluginData">PluginData being populated in the save process</param>
        /// <param name="key">Key to use to save textures</param>
        /// <param name="data">Texture data to be saved</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        protected abstract void SaveBundled(PluginData pluginData, string key, object data, bool isCharaController = false);

        /// <summary>
        /// Load bundled data from the provided PluginData
        /// </summary>
        /// <param name="pluginData">PluginData being loaded</param>
        /// <param name="key">Key to use to acquire data from PluginData</param>
        /// <param name="data">Data acquired from IsLocal immediately before this function</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        protected abstract object LoadBundled(PluginData pluginData, string key, object data, bool isCharaController = false);

#if !EC

        /// <summary>
        /// Determines whether the provided data was saved as deduped
        /// </summary>
        /// <param name="pluginData">ExtSave data being loaded</param>
        /// <param name="key">Texture data identifier</param>
        /// <param name="data">Deduped data extracted from pluginData</param>
        protected abstract bool IsDeduped(PluginData pluginData, string key, out object data);

        /// <summary>
        /// Save the provided data with the provided key as deduped
        /// </summary>
        /// <param name="pluginData">PluginData being populated in the save process</param>
        /// <param name="key">Key to use to save textures</param>
        /// <param name="data">Texture data to be saved</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        protected abstract void SaveDeduped(PluginData pluginData, string key, object data, bool isCharaController = false);

        /// <summary>
        /// Load deduped data from the provided PluginData
        /// </summary>
        /// <param name="pluginData">PluginData being loaded</param>
        /// <param name="key">Key to use to acquire data from PluginData</param>
        /// <param name="data">Data acquired from IsLocal immediately before this function</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        protected abstract object LoadDeduped(PluginData pluginData, string key, object data, bool isCharaController = false);

#endif

        /// <summary>
        /// Determines whether the provided data was saved as local
        /// </summary>
        /// <param name="pluginData">ExtSave data being loaded</param>
        /// <param name="key">Texture data identifier</param>
        /// <param name="data">Local data extracted from pluginData</param>
        protected abstract bool IsLocal(PluginData pluginData, string key, out object data);

        /// <summary>
        /// Save the provided data with the provided key as local
        /// </summary>
        /// <param name="pluginData">PluginData being populated in the save process</param>
        /// <param name="key">Key to use to save textures</param>
        /// <param name="data">Texture data to be saved</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        protected abstract void SaveLocal(PluginData pluginData, string key, object data, bool isCharaController = false);

        /// <summary>
        /// Load local data from the provided PluginData
        /// </summary>
        /// <param name="pluginData">PluginData being loaded</param>
        /// <param name="key">Key to use to acquire data from PluginData</param>
        /// <param name="data">Data acquired from IsLocal immediately before this function</param>
        /// <param name="isCharaController">Whether a CharaCustomFunctionController (true), or a SceneCustomFunctionController (false) is calling the save method</param>
        protected abstract object LoadLocal(PluginData pluginData, string key, object data, bool isCharaController = false);

#if !PH

        internal static string AddLocalPrefixToCard(string current)
        {
            if (!IsAutoSave() && CharaLocalTextures.SaveType == CharaTextureSaveType.Local)
                return "LOCAL_" + current;
            return current;
        }

        private static bool IsAutoSave()
        {
            if (Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.autosave", out PluginInfo pluginInfo) && pluginInfo?.Instance != null)
                return (bool)(pluginInfo.Instance.GetType().GetField("Autosaving")?.GetValue(null) ?? false);
            return false;
        }

#endif

        private static int DetermineSaveType()
        {
#if !EC
            if (StudioAPI.InsideStudio)
                return (int)SceneLocalTextures.SaveType;
#endif
            if (MakerAPI.InsideMaker)
                return (int)CharaLocalTextures.SaveType;
            throw new ArgumentException("Not inside Studio or Maker!");
        }
    }
}
