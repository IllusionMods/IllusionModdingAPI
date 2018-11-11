using BepInEx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MakerAPI
{
    public class MakerToggle : ValueMakerGuiEntry<bool>
    {
        private static Transform _toggleCopy;

        public MakerToggle(MakerCategory category, string displayName, BaseUnityPlugin owner) : base(category, false, owner)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }

        public static Transform ToggleCopy
        {
            get
            {
                if (_toggleCopy == null)
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

                    _toggleCopy = copy;
                }
                return _toggleCopy;
            }
        }

        protected internal override void CreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(ToggleCopy, subCategoryList, true);

            var tgl = tr.GetComponentInChildren<Toggle>();
            tgl.onValueChanged.AddListener(SetNewValue);

            BufferedValueChanged.Subscribe(b => tgl.isOn = b);

            var text = tr.GetComponentInChildren<TextMeshProUGUI>();
            text.text = DisplayName;
            text.color = TextColor;
            tr.gameObject.SetActive(true);
        }
    }
}