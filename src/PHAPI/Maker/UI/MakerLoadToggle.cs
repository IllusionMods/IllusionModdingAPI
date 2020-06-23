using System;
using System.Collections.Generic;
using System.Linq;
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
            var loadUi = MakerAPI.GetMakerBase().GetComponentInChildren<CharaLoadUI>(true);

            // Make space for new toggles
            var tfrt = loadUi.transform.Find("UI/TextFrame").GetComponent<RectTransform>();
            tfrt.offsetMin = new Vector2(135, -60);

            var chrt = loadUi.transform.Find("UI/Checks").GetComponent<RectTransform>();
            chrt.offsetMax = new Vector2(130, 225);

            var allBtn = loadUi.transform.Find("UI/ShortCutButtons/All/Button").GetComponent<Button>();
            allBtn.onClick.AddListener(OnAllOn);

            var noneBtn = loadUi.transform.Find("UI/ShortCutButtons/None/Button").GetComponent<Button>();
            noneBtn.onClick.AddListener(OnAllOff);

            foreach (var toggle in Toggles)
                toggle.CreateControl(chrt);

            foreach (var text in tfrt.parent.GetComponentsInChildren<Text>(true))
                SetTextAutosize(text);
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform loadBoxTransform)
        {
            var copy = Object.Instantiate(loadBoxTransform.Find("Toggle Acce").gameObject, loadBoxTransform);
            copy.name = "Toggle " + Text + GuiApiNameAppendix;

            RemoveLocalisation(copy.gameObject);

            var tgl = copy.GetComponent<Toggle>();
            tgl.onValueChanged.AddListener(SetValue);
            BufferedValueChanged.Subscribe(b => tgl.isOn = b);
            tgl.image.raycastTarget = true;
            tgl.graphic.raycastTarget = true;

            var txt = copy.GetComponentInChildren<Text>();
            txt.text = Text;

            copy.gameObject.SetActive(true);

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
        }

        private static void OnAllOff()
        {
            foreach (var toggle in Toggles)
            {
                if (toggle != null)
                    toggle.Value = false;
            }
        }

        private static void OnAllOn()
        {
            foreach (var toggle in Toggles)
            {
                if (toggle != null)
                    toggle.Value = true;
            }
        }
    }
}
