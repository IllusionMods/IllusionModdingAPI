using BepInEx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that displays a toggle
    /// </summary>
    public class MakerToggle : BaseEditableGuiEntry<bool>
    {
        private static Transform _toggleCopy;

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="displayName">Text shown next to the checkbox</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerToggle(MakerCategory category, string displayName, BaseUnityPlugin owner) : this(category, displayName, false, owner)
        {
        }

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="displayName">Text shown next to the checkbox</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="initialValue">Initial value of the toggle</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerToggle(MakerCategory category, string displayName, bool initialValue, BaseUnityPlugin owner) : base(category, initialValue, owner)
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Text shown next to the checkbox
        /// </summary>
        public string DisplayName { get; }

        private static Transform ToggleCopy
        {
            get
            {
                if (_toggleCopy == null)
                    MakeCopy();
                return _toggleCopy;
            }
        }

        private static void MakeCopy()
        {
            // Exists in male and female maker
            var original = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglMouth/MouthTop/tglCanine").transform;

            var copy = Object.Instantiate(original, GuiCacheTransfrom, true);
            copy.gameObject.SetActive(false);
            copy.name = "tglSingle" + GuiApiNameAppendix;

            var toggle = copy.GetComponentInChildren<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();
            toggle.image.raycastTarget = true;
            toggle.graphic.raycastTarget = true;

            toggle.GetComponent<RectTransform>().offsetMax = new Vector2(460, -8);

            _toggleCopy = copy;
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_toggleCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(ToggleCopy, subCategoryList, true);

            var tgl = tr.GetComponentInChildren<Toggle>();
            tgl.onValueChanged.AddListener(SetNewValue);

            BufferedValueChanged.Subscribe(b => tgl.isOn = b);

            var text = tr.GetComponentInChildren<TextMeshProUGUI>();
            text.text = DisplayName;
            text.color = TextColor;

            return tr.gameObject;
        }
    }
}