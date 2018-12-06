using BepInEx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MakerAPI
{
    public class MakerToggle : BaseEditableGuiEntry<bool>
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

        protected internal override void Initialize()
        {
            if (_toggleCopy == null)
                MakeCopy();
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