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
        private static Transform _originalToggle;
        private static GameObject _loadTop;

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

            // Present but disabled by default
            _loadTop.GetComponent<GridLayoutGroup>().enabled = true;

            foreach (var toggle in Toggles)
                toggle.CreateControl(_loadTop.transform);
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform loadTop)
        {
            var toggle = this;
            var copy = Object.Instantiate(_originalToggle, loadTop.transform, false);
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

            return copy.gameObject;
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

            _loadTop = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinOption/SystemWin/O_Load/LoadSetting");
            _originalToggle = _loadTop.transform.Find("TglLoadType01");
        }
    }
}
