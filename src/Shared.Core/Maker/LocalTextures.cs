using BepInEx.Configuration;
using KKAPI.Utilities;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
#if KK || KKS || EC
using ChaCustom;
#elif AI || HS2
using CharaCustom;
#endif
using TMPro;

namespace KKAPI.Maker {
    /// <summary>
    /// API for universal toggling of locally saved textures.
    /// </summary>
    public static class LocalTextures
    {
        /// <summary>
        /// Fired whenever the SaveType changes.
        /// </summary>
        public static System.EventHandler SaveTypeChangedEvent;

        /// <summary>
        /// The type of texture saving that plugins should use
        /// </summary>
        public static TextureSaveType SaveType
        {
            get
            {
                return ConfTexSaveType.Value;
            }
            set
            {
                saveTypeChanging = true;
                ConfTexSaveType.Value = value;
                var eLogger = ApiEventExecutionLogger.GetEventLogger();
                eLogger.Begin(nameof(SaveTypeChangedEvent), "");
                OnSaveTypeChanged(null, new LocalSaveChangedEventArgs(value), eLogger);
                eLogger.End();
                saveTypeChanging = false;
            }
        }

        /// <summary>
        /// Options for the type of texture saving that plugins should use
        /// </summary>
        public enum TextureSaveType
        {
            /// <summary>
            /// Textures should be bundled with the card
            /// </summary>
            Bundled = 0,
            /// <summary>
            /// Textures should be saved separately from the card in a local folder
            /// </summary>
            Local = 1,
        }

        internal static ConfigEntry<TextureSaveType> ConfTexSaveType { get; set; }
        private static bool saveTypeChanging = false;

        static LocalTextures()
        {
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", TextureSaveType.Bundled, new ConfigDescription("Whether external textures used by plugins should be bundled with the card or to a local folder.", null, new ConfigurationManagerAttributes { Browsable = false }));
            MakerAPI.MakerStartedLoading += (x, y) => { SetupUI(); };
            if (MakerAPI.InsideAndLoaded) SetupUI();
        }

        private static void SetupUI()
        {
            // Save new screen
            var panel1 = SetupUIPanel(out _);
            panel1.transform.localPosition += new Vector3(0, panel1.GetComponent<RectTransform>().sizeDelta.y + 10, 0);

            // Overwrite screen
            var panel2 = SetupUIPanel(out bool isNew);
            var panel2rect = panel2.GetComponent<RectTransform>();
            Vector2 panel2RectSize = panel2rect.sizeDelta;
            var checkWindow = Singleton<CustomBase>.Instance.GetComponentsInChildren<CustomCheckWindow>(true)[0];
            var overwriteWindow = checkWindow.checkInfo[(int)CustomCheckWindow.CheckType.CharaOverwrite].gameObject;
            panel2.transform.SetParent(overwriteWindow.transform);
            SetTfProps(panel2rect, 0, 0, 0, 0, 0, 0, 0, 0);
            panel2rect.sizeDelta = panel2RectSize;
            if (isNew)
                panel2.transform.localPosition = new Vector3(0, -panel2RectSize.y / 2, 0);
            else
                panel2.transform.localPosition = new Vector3(-panel2RectSize.x / 2, 0, 0);
        }

        private static GameObject SetupUIPanel(out bool isNewPanel)
        {
            GameObject root = Singleton<CustomBase>.Instance.customCtrl.objCaptureTop;
            ToggleGroup tmpTglGrp = root.GetComponentInChildren<ToggleGroup>(true);
            GameObject localSave;

            string tglText1 = "Bundled Textures";
            string tglText2 = "Local Textures";
            string warningText = "※ Local textures are saved by\n     each supporting plugin\n     to their own folders.";

            if (tmpTglGrp != null)
            {
                isNewPanel = false;
                localSave = Object.Instantiate(tmpTglGrp.gameObject, tmpTglGrp.transform.parent);
                localSave.name = "localSave";
                if (localSave.TryGetComponent<UI_ToggleGroupCtrl>(out var grpCtrl))
                    Object.DestroyImmediate(grpCtrl);
                localSave.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = warningText;
                localSave.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = tglText1;
                localSave.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = tglText2;
            }
            else
            {
                isNewPanel = true;
                TextMeshProUGUI fontSource = root.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(x => x != null);
                localSave = new GameObject("localSave", new[] { typeof(RectTransform), typeof(ToggleGroup), typeof(Image) });
                localSave.transform.SetParent(root.transform);
                SetTfProps(localSave.GetComponent<RectTransform>(), 0, 1, 0, 1, 1384, -772, 1688, -632);
                localSave.GetComponent<Image>().sprite = MakeSprite("local-frame.png", 12);
                localSave.GetComponent<Image>().type = Image.Type.Sliced;

                var text = new GameObject("text", new[] { typeof(RectTransform), typeof(TextMeshProUGUI) });
                text.transform.SetParent(localSave.transform);
                SetTfProps(text.GetComponent<RectTransform>(), 0, 1, 1, 1, 8, -136, 0, -88);
                var textUGUI = SetupText(text, warningText, TextAlignmentOptions.TopLeft);
                textUGUI.color = new Color(1, 0.3f, 0.3f);

                var tglGrp = localSave.GetComponent<ToggleGroup>();
                tglGrp.RegisterToggle(CreateCheck("imgRbFace", tglText1, TextureSaveType.Bundled, -40, -8));
                tglGrp.RegisterToggle(CreateCheck("imgRbCard", tglText2, TextureSaveType.Local, -80, -48));

                SetLayers(localSave.transform);
                
                Toggle CreateCheck(string name, string checkName, TextureSaveType type, float f, float h)
                {
                    var checkRoot = new GameObject(name, new[] { typeof(RectTransform), typeof(Image), typeof(Toggle) });
                    checkRoot.transform.SetParent(localSave.transform);
                    SetTfProps(checkRoot.GetComponent<RectTransform>(), 0, 1, 1, 1, 8, f, 0, h);
                    checkRoot.GetComponent<Image>().color = Color.clear;
                    var checkToggle = checkRoot.GetComponent<Toggle>();
                    checkToggle.group = tglGrp;
                    checkToggle.navigation = new Navigation() { mode = Navigation.Mode.None };

                    var checkBack = new GameObject("rb_back", new[] { typeof(RectTransform), typeof(Image) });
                    checkBack.transform.SetParent(checkRoot.transform.transform);
                    SetTfProps(checkBack.GetComponent<RectTransform>(), 0, 0, 0, 1, 0, 0, 32, 0);
                    checkBack.GetComponent<Image>().sprite = MakeSprite("local-back.png", 0);

                    var checkCheck = new GameObject("rb_check", new[] { typeof(RectTransform), typeof(Image) });
                    checkCheck.transform.SetParent(checkBack.transform.transform);
                    SetTfProps(checkCheck.GetComponent<RectTransform>(), 0, 0, 1, 1, 0, 0, 0, 0);
                    checkCheck.GetComponent<Image>().sprite = MakeSprite("local-check.png", 0);

                    var checkText = new GameObject("text", new[] { typeof(RectTransform), typeof(TextMeshProUGUI) });
                    checkText.transform.SetParent(checkRoot.transform);
                    SetTfProps(checkText.GetComponent<RectTransform>(), 0, 0, 1, 1, 36, 0, 0, 0);
                    SetupText(checkText, checkName);

                    checkToggle.graphic = checkCheck.GetComponent<Image>();

                    return checkToggle;
                }

                Sprite MakeSprite(string file, int border)
                {
                    Texture2D spriteTex = ResourceUtils.GetEmbeddedResource(file).LoadTexture();
                    Vector2 spriteSize = new Vector2(spriteTex.width, spriteTex.height);
                    Rect spriteRect = new Rect(Vector2.zero, spriteSize);
                    Vector4 spriteBorder = new Vector4(border, border, border, border);
                    return Sprite.Create(spriteTex, spriteRect, spriteSize / 2, 100, 0, SpriteMeshType.FullRect, spriteBorder);
                }

                void SetLayers(Transform rootTransform)
                {
                    rootTransform.gameObject.layer = 5; // UI
                    for (int i = 0; i < rootTransform.childCount; i++)
                    {
                        SetLayers(rootTransform.GetChild(i));
                    }
                }

                TextMeshProUGUI SetupText(GameObject go, string content, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
                {
                    TextMeshProUGUI tmpro = go.GetComponent<TextMeshProUGUI>();
                    if (fontSource != null) tmpro.font = fontSource.font;
                    tmpro.enableAutoSizing = true;
                    tmpro.fontSizeMin = 10;
                    tmpro.fontSizeMax = 16;
                    tmpro.alignment = alignment;
                    tmpro.text = content;

                    return tmpro;
                }
            }

            var validator = localSave.AddComponent<ToggleValidator>();

            Toggle tglBundled = localSave.transform.GetChild(1).GetComponent<Toggle>();
            tglBundled.isOn = SaveType == TextureSaveType.Bundled;
            tglBundled.onValueChanged = new Toggle.ToggleEvent();
            tglBundled.onValueChanged.AddListener((x) =>
            {
                if (x && !saveTypeChanging)
                    SaveType = TextureSaveType.Bundled;
            });

            Toggle tglLocal = localSave.transform.GetChild(2).GetComponent<Toggle>();
            tglLocal.isOn = SaveType == TextureSaveType.Local;
            tglLocal.onValueChanged = new Toggle.ToggleEvent();
            tglLocal.onValueChanged.AddListener((x) =>
            {
                if (x && !saveTypeChanging)
                    SaveType = TextureSaveType.Local;
            });

            validator.Register(tglBundled, tglLocal);

            SaveTypeChangedEvent += (x, y) =>
            {
                TextureSaveType newValue = (y as LocalSaveChangedEventArgs).NewSetting;
                if (newValue == TextureSaveType.Bundled && !tglBundled.isOn && tglBundled.isActiveAndEnabled)
                    tglBundled.isOn = true;
                else if (newValue == TextureSaveType.Local && !tglLocal.isOn && tglLocal.isActiveAndEnabled)
                    tglLocal.isOn = true;
            };

            return localSave;
        }

        private static void SetTfProps(RectTransform tf, float a, float b, float c, float d, float e, float f, float g, float h, Vector3? scale = null)
        {
            tf.localScale = Vector3.one;
            tf.anchorMin = new Vector2(a, b);
            tf.anchorMax = new Vector2(c, d);
            tf.offsetMin = new Vector2(e, f);
            tf.offsetMax = new Vector2(g, h);

            if (!scale.HasValue) tf.localScale = Vector3.one;
            else tf.localScale = scale.Value;
        }

        private static void OnSaveTypeChanged(object sender, System.EventArgs args, ApiEventExecutionLogger eventLogger)
        {
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(sender, args), nameof(SaveTypeChangedEvent), eventLogger);
        }

#if !KKS

        private static bool TryGetComponent<T>(this GameObject go, out T result) where T : Component
        {
            foreach (Component component in go.GetComponents<Component>())
            {
                if (component is T componentT)
                {
                    result = componentT;
                    return true;
                }
            }
            result = null;
            return false;
        }

#endif

        private class ToggleValidator : MonoBehaviour
        {
            private Toggle tglBundled;
            private Toggle tglLocal;

            public void Register(Toggle tglBundled, Toggle tglLocal)
            {
                this.tglBundled = tglBundled;
                this.tglLocal = tglLocal;
            }

            private void OnEnable()
            {
                if (tglBundled != null && SaveType == TextureSaveType.Bundled)
                    tglBundled.isOn = true;
                else if (tglLocal != null && SaveType == TextureSaveType.Local)
                    tglLocal.isOn = true;
            }
        }
    }
}
