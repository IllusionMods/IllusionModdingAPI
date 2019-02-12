using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    public class MakerButton : BaseGuiEntry
    {
        private static Transform _buttonCopy;

        public MakerButton(string text, MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
            Text = text;
            OnClick = new Button.ButtonClickedEvent();
        }

        public Button.ButtonClickedEvent OnClick { get; }
        public string Text { get; }

        private static Transform ButtonCopy
        {
            get
            {
                if (_buttonCopy == null)
                    MakeCopy();
                return _buttonCopy;
            }
        }

        private static void MakeCopy()
        {
            var original = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/02_HairTop/tglBack/BackTop/grpBtn").transform;

            _buttonCopy = Object.Instantiate(original, GuiCacheTransfrom, true);
            _buttonCopy.gameObject.SetActive(false);
            _buttonCopy.name = "btnCustom" + GuiApiNameAppendix;

            var button = _buttonCopy.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.targetGraphic.raycastTarget = true;
        }

        protected internal override void Initialize()
        {
            if (_buttonCopy == null)
                MakeCopy();
        }

        public override void Dispose()
        {
            OnClick.RemoveAllListeners();
        }

        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(ButtonCopy, subCategoryList, true);

            var button = tr.GetComponentInChildren<Button>();
            button.onClick.AddListener(OnClick.Invoke);

            var text = tr.GetComponentInChildren<TextMeshProUGUI>();
            text.text = Text;
            text.color = TextColor;

            return tr.gameObject;
        }
    }
}
