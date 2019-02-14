using BepInEx;
using ChaCustom;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Control that allows user to change a <see cref="Color"/> in a separate color selector window
    /// </summary>
    public class MakerColor : BaseEditableGuiEntry<Color>
    {
        private static Transform _colorCopy;

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="settingName">Text displayed next to the control</param>
        /// <param name="useAlpha">
        /// If true, the color selector will allow the user to change alpha of the color.
        /// If false, no color slider is shown and alpha is always 1f.
        /// </param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="initialValue">Color set to the control when it is created</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerColor(string settingName, bool useAlpha, MakerCategory category, Color initialValue, BaseUnityPlugin owner) : base(category, initialValue, owner)
        {
            SettingName = settingName;
            UseAlpha = useAlpha;
        }

        /// <summary>
        /// Name of the setting
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// If true, the color selector will allow the user to change alpha of the color.
        /// If false, no color slider is shown and alpha is always 1f.
        /// </summary>
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

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_colorCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        public override void Dispose() { }

        /// <inheritdoc />
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
