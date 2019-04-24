using BepInEx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI.Sidebar
{
    /// <summary>
    /// A toggle to be used in the right "Control Panel" sidebar in character maker.
    /// The space is limited so use sparingly.
    /// </summary>
    public class SidebarToggle : BaseEditableGuiEntry<bool>, ISidebarControl
    {
        /// <inheritdoc />
        public SidebarToggle(string text, bool initialValue, BaseUnityPlugin owner) : base(null, initialValue, owner)
        {
            Text = text;
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var origTgl = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CvsDraw/Top/tglBlink");
            var copy = Object.Instantiate(origTgl, origTgl.transform.parent);

            var tgl = copy.GetComponentInChildren<Toggle>();
            tgl.graphic.raycastTarget = true;
            tgl.onValueChanged.RemoveAllListeners();

            tgl.onValueChanged.AddListener(SetValue);
            BufferedValueChanged.Subscribe(val => tgl.isOn = val);

            var txt = copy.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = Text;
            txt.color = TextColor;
            txt.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 145);

            return copy;
        }

        /// <summary>
        /// Text displayed next to the checkbox
        /// </summary>
        public string Text { get; }
    }
}
