using BepInEx;
using ChaCustom;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MakerAPI
{
    public class MakerColor : BaseEditableGuiEntry<Color>
    {
        private static Transform _colorCopy;

        public MakerColor(string settingName, bool useAlpha, MakerCategory category, Color initialValue, BaseUnityPlugin owner) : base(category, initialValue, owner)
        {
            SettingName = settingName;
            UseAlpha = useAlpha;
        }

        public string SettingName { get; }
        public bool UseAlpha { get; }

        private static Transform ColorCopy
        {
            get
            {
                if (_colorCopy == null)
                    MakeCopy();
                return _colorCopy;
            }
        }

        private static void MakeCopy()
        {
            var original = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglMole/MoleTop/btnMoleColor").transform;

            _colorCopy = Object.Instantiate(original, GuiCacheTransfrom, true);
            _colorCopy.gameObject.SetActive(false);
            _colorCopy.name = "btnColor" + GuiApiNameAppendix;

            var button = _colorCopy.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.targetGraphic.raycastTarget = true;
        }

        protected internal override void Initialize()
        {
            if (_colorCopy == null)
                MakeCopy();
        }

        public override void Dispose() { }

        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(ColorCopy, subCategoryList, true);

            var settingName = tr.GetComponentInChildren<TextMeshProUGUI>();
            settingName.text = SettingName;
            settingName.color = TextColor;

            var cvsColor = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsColor/Top").GetComponent<CvsColor>();

            var button = tr.GetComponentInChildren<Button>();
            var connectColorKind = (CvsColor.ConnectColorKind)SettingName.GetHashCode();
            button.onClick.AddListener(
                () =>
                {
                    if (cvsColor.isOpen && cvsColor.connectColorKind == connectColorKind)
                        cvsColor.Close();
                    else
                    {
                        // TODO is history callback useful?
                        cvsColor.Setup(SettingName, connectColorKind, Value, SetNewValue, () => { }, UseAlpha);
                    }
                });

            var previewImg = tr.Find("backrect/alpha/imgColorSample").GetComponent<Image>();
            BufferedValueChanged.Subscribe(c => previewImg.color = c);

            return tr.gameObject;
        }
    }
}
