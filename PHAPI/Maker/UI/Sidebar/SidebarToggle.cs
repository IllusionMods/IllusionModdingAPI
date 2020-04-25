using System;
using BepInEx;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
        protected internal override void Initialize() { }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform child)
        {
            child.name = "Toggle custom PHAPI";

            Object.DestroyImmediate(child.GetComponent<MoveableToggleButton>());
            Object.DestroyImmediate(child.GetComponent<OverlapSwitch>());

            var tgl = child.GetComponent<ToggleButton>();
            tgl.SetText(Text, Text);
            foreach (var image in tgl.GetComponentsInChildren<Image>()) image.raycastTarget = true;
            tgl.action.ActuallyRemoveAllListeners();

            ValueChanged.Subscribe(b => tgl.Value = b);
            tgl.action.AddListener(SetValue);

            return child.gameObject;
        }

        /// <summary>
        /// Text displayed next to the checkbox
        /// </summary>
        public string Text { get; }
    }
}
