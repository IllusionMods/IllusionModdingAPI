using BepInEx;
using TMPro;
using UnityEngine;

namespace MakerAPI
{
    public class MakerText : BaseGuiEntry
    {
        private static Transform _textCopy;

        public MakerText(string text, MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
            Text = text;
        }

        public string Text { get; }

        private static Transform TextCopy
        {
            get
            {
                if (_textCopy == null)
                {
                    var original = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/tglConfig/ConfigTop/txtExplanation").transform;

                    _textCopy = Object.Instantiate(original, GuiCacheTransfrom, true);
                    _textCopy.gameObject.SetActive(false);
                    _textCopy.name = "txtCustom" + GuiApiNameAppendix;
                }
                return _textCopy;
            }
        }

        public override void Dispose() { }

        protected internal override void CreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(TextCopy, subCategoryList, true);

            var settingName = tr.GetComponentInChildren<TextMeshProUGUI>();
            settingName.text = Text;
            settingName.color = TextColor;

            tr.gameObject.SetActive(true);
        }
    }
}
