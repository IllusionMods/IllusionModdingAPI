using BepInEx;
using KKAPI.Utilities;
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

        private static GameObject _cachedToggle;

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if(_cachedToggle != null) return;

            var orig = GameObject.Find("CharaCustom/CustomControl/CanvasDraw/DrawWindow/dwCoorde/clothes/items/tgl01");
            var copy = Object.Instantiate(orig, GuiCacheTransfrom, false);
            copy.name = "plugTgl_AIAPI";

            var t = copy.transform.GetComponentInChildren<Text>();
            t.lineSpacing = 0.65f;
            SetTextAutosize(t);
            
            var tgl = copy.GetComponent<Toggle>();
            tgl.group = null;
            tgl.onValueChanged.ActuallyRemoveAllListeners();
            tgl.graphic.raycastTarget = true;

            copy.SetActive(false);

            _cachedToggle = copy;
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var copy = Object.Instantiate(_cachedToggle, subCategoryList);
    
            RemoveLocalisation(copy);
    
            var tgl = copy.GetComponent<Toggle>();
            tgl.onValueChanged.AddListener(SetValue);
            BufferedValueChanged.Subscribe(val => tgl.isOn = val);
    
            var txt = copy.GetComponentInChildren<Text>();
            txt.text = Text;
            txt.color = TextColor;

            return copy;
        }
    
        /// <summary>
        /// Text displayed next to the checkbox
        /// </summary>
        public string Text { get; }
    }
}
