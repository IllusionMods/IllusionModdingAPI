using System;
using BepInEx;
using ChaCustom;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if KK
using Harmony;
#else
using HarmonyLib;
#endif

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Control that allows user to change a <see cref="Color"/> in a separate color selector window
    /// </summary>
    public class MakerColor : BaseEditableGuiEntry<Color>
    {
        private static Transform _colorCopy;

        private readonly BehaviorSubject<int> _colorBoxWidth = new BehaviorSubject<int>(276);

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

        /// <summary>
        /// Width of the color box. Can adjust this to allow for longer label text.
        /// Default width is 276 and might need to get lowered to allow longer labels.
        /// The default color boxes in accessory window are 230 wide.
        /// </summary>
        public int ColorBoxWidth
        {
            get => _colorBoxWidth.Value;
            set => _colorBoxWidth.OnNext(value);
        }

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
            var original = GetExistingControl("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglMole", "btnMoleColor");

            _colorCopy = Object.Instantiate(original, GuiCacheTransfrom, true);
            _colorCopy.gameObject.SetActive(false);
            _colorCopy.name = "btnColor";

            var button = _colorCopy.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.targetGraphic.raycastTarget = true;

            RemoveLocalisation(_colorCopy.gameObject);
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_colorCopy == null)
                MakeCopy();
        }

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
                        var m = AccessTools.Method(typeof(CvsColor), nameof(CvsColor.Setup));
                        switch (m.GetParameters().Length)
                        {
                            case 5:
                                m.Invoke(cvsColor, new object[] { SettingName, connectColorKind, Value, (Action<Color>)SetValue, UseAlpha });
                                break;
                            case 6:
                                m.Invoke(cvsColor, new object[] { SettingName, connectColorKind, Value, (Action<Color>)SetValue, (Action)(() => { }), UseAlpha });
                                break;
                            default:
                                throw new InvalidOperationException("Where am I what is this help me");
                        }
                    }
                });

            var previewImg = tr.Find("backrect/alpha/imgColorSample").GetComponent<Image>();
            BufferedValueChanged.Subscribe(c => previewImg.color = c);

            var boxRt = tr.Find("backrect").GetComponent<RectTransform>();
            _colorBoxWidth.Subscribe(width => boxRt.offsetMin = new Vector2(width * -1, boxRt.offsetMin.y));

            return tr.gameObject;
        }
    }
}
