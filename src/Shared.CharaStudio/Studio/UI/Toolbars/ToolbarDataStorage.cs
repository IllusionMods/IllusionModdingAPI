using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BepInEx.Configuration;

namespace KKAPI.Studio.UI.Toolbars
{
    internal static class ToolbarDataStorage
    {
        private static ConfigEntry<string> _positionSetting;
        private static readonly Dictionary<string, ToolbarPosition> _InitialPositions = new Dictionary<string, ToolbarPosition>();

        private static ConfigEntry<string> _hiddenButtonIdsConfig;
        private static readonly HashSet<string> _HiddenIdsCache = new HashSet<string>();

        /// <summary>
        /// Initializes the storage with the plugin configuration file.
        /// Must be called before accessing edit mode or hidden buttons logic.
        /// </summary>
        static ToolbarDataStorage()
        {
            var config = KoikatuAPI.Instance.Config;

            _positionSetting = config.Bind("Toolbars", "LeftToolbarButtonPositions", "", new ConfigDescription("Stores desired positions of custom toolbar buttons. Remove to reset all button positions on next studio start.", null, new BrowsableAttribute(false)));
            _positionSetting.SettingChanged += (sender, args) =>
            {
                RefreshButtonPositionCache();
            };
            RefreshButtonPositionCache();

            _hiddenButtonIdsConfig = config.Bind("Toolbars", "LeftToolbarHiddenButtons", "", new ConfigDescription("List of Button IDs to hide.", null, new BrowsableAttribute(false)));
            _hiddenButtonIdsConfig.SettingChanged += (sender, args) =>
            {
                RefreshHiddenCache();
                ToolbarManager.RequestToolbarRelayout();
            };

            RefreshHiddenCache();
        }

        public static void ResetHidden() => _hiddenButtonIdsConfig.Value = "";
        public static void ResetPositions() => _positionSetting.Value = "";

        private static void RefreshHiddenCache()
        {
            _HiddenIdsCache.Clear();
            if (_hiddenButtonIdsConfig == null || string.IsNullOrEmpty(_hiddenButtonIdsConfig.Value)) return;

            var ids = _hiddenButtonIdsConfig.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in ids)
            {
                var trimmed = id.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    _HiddenIdsCache.Add(trimmed);
                }
            }
        }

        public static bool IsHidden(ToolbarControlBase button)
        {
            return _HiddenIdsCache.Contains(GetUniqueName(button));
        }

        public static void ToggleHidden(ToolbarControlBase button)
        {
            var name = GetUniqueName(button);
            if (_HiddenIdsCache.Remove(name))
            {
                KoikatuAPI.Logger.LogInfo($"Unhiding button: {button}");
            }
            else
            {
                _HiddenIdsCache.Add(name);
                KoikatuAPI.Logger.LogInfo($"Hiding button: {button}");
            }

            // SaveKey|SaveKey2|...
            _hiddenButtonIdsConfig.Value = string.Join("|", _HiddenIdsCache.OrderBy(id => id).ToArray());
        }

        private static string GetUniqueName(ToolbarControlBase button)
        {
            var guid = button is ToolbarControlAdapter ? "BaseGame" : button.Owner == KoikatuAPI.Instance ? "Unknown" : button.Owner.Info.Metadata.GUID;
            return $"{guid}_{button.ButtonID}".ReplaceChars(null, Path.GetInvalidFileNameChars()).ReplaceChars("_", ' ', ':', '|');
        }

        private static string ReplaceChars(this string filename, string replacement, params char[] charsToReplace)
        {
            return replacement != null ? string.Join(replacement, filename.Split(charsToReplace)) : string.Concat(filename.Split(charsToReplace));
        }

        /// <summary>
        /// Applies saved position to a button if it exists in the initial config.
        /// </summary>
        public static void ApplyInitialPosition(ToolbarControlBase btn)
        {
            if (_InitialPositions.TryGetValue(GetUniqueName(btn), out var pos))
                btn.DesiredPosition = pos;
        }

        private static void RefreshButtonPositionCache()
        {
            var value = _positionSetting.Value;
            var dict = _InitialPositions;
            dict.Clear();
            if (string.IsNullOrEmpty(value)) return;

            // SaveKey:row:col|SaveKey2:row:col|...
            var entries = value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var posParts = entry.Split(new[] { ':' }, StringSplitOptions.None);
                if (posParts.Length == 3)
                {
                    var saveKey = posParts[0];
                    if (saveKey.Length > 0 && int.TryParse(posParts[1], out int row) && int.TryParse(posParts[2], out int col))
                    {
                        if (!dict.ContainsKey(saveKey))
                            dict[saveKey] = new ToolbarPosition(row, col);
                        else
                            KoikatuAPI.Logger.LogWarning($"Duplicate toolbar button position entry found during loading for ID: {saveKey}");

                        continue;
                    }
                }

                KoikatuAPI.Logger.LogWarning($"Could not parse toolbar button position entry: {entry}");
            }
        }

        /// <summary>
        /// Write to config
        /// </summary>
        public static void SaveButtonPositions(ICollection<ToolbarControlBase> buttons)
        {
            if (_positionSetting == null)
                return;

            var duplicateIds = new HashSet<string>();
            var entries = new List<string>(buttons.Count);

            foreach (var b in buttons)
            {
                if (!b.DesiredPosition.HasValue) continue;

                var saveKey = GetUniqueName(b);
                if (entries.Contains(saveKey))
                {
                    duplicateIds.Add(saveKey);
                    continue;
                }

                entries.Add($"{saveKey}:{b.DesiredPosition.Value.Row}:{b.DesiredPosition.Value.Column}");
            }

            if (duplicateIds.Count > 0)
            {
                KoikatuAPI.Logger.LogWarning($"Duplicate toolbar button IDs detected when saving: {string.Join(", ", duplicateIds.ToArray())}. These settings will not be saved until the IDs are changed to be unique.");
                _positionSetting.Value = string.Join("|", entries.Except(duplicateIds).OrderBy(x => x).ToArray());
            }
            else
            {
                _positionSetting.Value = string.Join("|", entries.OrderBy(x => x).ToArray());
            }
        }
    }
}
