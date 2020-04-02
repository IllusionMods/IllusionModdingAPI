using System;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Adds a toggle to the bottom of the character card load window in character maker.
    /// Use to allow user to not load data related to your mod.
    /// Use with <see cref="AddLoadToggle"/>
    /// </summary>
    public class MakerLoadToggle : BaseEditableGuiEntry<bool>
    {
        private static readonly List<MakerLoadToggle> Toggles = new List<MakerLoadToggle>();

        /// <summary>
        /// Create a new load toggle. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/> 
        /// with <see cref="RegisterCustomControlsEvent.AddLoadToggle"/>.
        /// </summary>
        /// <param name="text">Text displayed next to the checkbox</param>
        /// <param name="initialValue">Initial value of the toggle</param>
        public MakerLoadToggle(string text, bool initialValue = true) : base(null, initialValue, null)
        {
            Text = text;
        }

        /// <summary>
        /// Text displayed next to the toggle
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Check if any of the custom toggles are checked
        /// </summary>
        public static bool AnyEnabled => Toggles.Any(x => x.Value);

        internal static void CreateCustomToggles()
        {
            if (!Toggles.Any()) return;

            //CharaCustom/CustomControl/CanvasSub/SettingWindow/WinOption/SystemWin/O_Load/LoadSetting/TglLoadType05
            var loadTop = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinOption/SystemWin/O_Load/LoadSetting");
            // Present but disabled by default
            loadTop.GetComponent<GridLayoutGroup>().enabled = true;

            var orig = loadTop.transform.Find("TglLoadType01");
            foreach (var toggle in Toggles)
            {
                var copy = Object.Instantiate(orig, loadTop.transform, false);
                copy.name = "TglLoadType_AIAPI_" + toggle.Text;

                var t = copy.GetComponent<Toggle>();
                t.onValueChanged.ActuallyRemoveAllListeners();
                toggle.BufferedValueChanged.Subscribe(b => t.isOn = b);
                t.onValueChanged.AddListener(b => toggle.Value = b);

                var txt = copy.GetComponentInChildren<Text>();
                RemoveLocalisation(txt.gameObject);
                SetTextAutosize(txt);
                txt.lineSpacing = 0.7f;
                txt.text = toggle.Text;
            }
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform loadBoxTransform)
        {
            KoikatuAPI.Logger.LogWarning("MakerLoadToggles are not implemented yet");
            Value = true;
            return null;
        }

        /// <inheritdoc />
        protected internal override void Initialize() { }

        internal static MakerLoadToggle AddLoadToggle(MakerLoadToggle toggle)
        {
            if (toggle == null) throw new ArgumentNullException(nameof(toggle));
            toggle.ThrowIfDisposed(nameof(toggle));

            Toggles.Add(toggle);
            return toggle;
        }

        internal static void Reset()
        {
            foreach (var toggle in Toggles)
                toggle.Dispose();
            Toggles.Clear();
        }

        internal static void Setup()
        {
            Reset();
        }
    }
}
