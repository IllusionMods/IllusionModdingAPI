using System.Collections.Generic;
using BepInEx.Configuration;
using System.Collections;
using BepInEx.Bootstrap;
using KKAPI.Utilities;
using UnityEngine.UI;
using UnityEngine;
using HarmonyLib;
using BepInEx;
using Studio;
#if !PH
using TMPro;
#endif

namespace KKAPI.Studio
{
    /// <summary>
    /// API for global toggling of locally saved textures in Studio.
    /// The module is only activated if Activate is called, SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
    /// </summary>
    public static class SceneLocalTextures
    {
        /// <summary>
        /// Fired whenever SaveType changes
        /// </summary>
        public static System.EventHandler<SceneTextureSaveTypeChangedEventArgs> SaveTypeChangedEvent;

        /// <summary>
        /// The type of texture saving that plugins should use
        /// </summary>
        public static SceneTextureSaveType SaveType
        {
            get
            {
                // If local texture support is disabled, always return Bundled
                if (!Maker.CharaLocalTextures.EnableLocalTextureSupport.Value)
                    return SceneTextureSaveType.Bundled;
                return ConfTexSaveType.Value;
            }
            set
            {
                if (ConfTexSaveType.Value == value) return;
                ConfTexSaveType.Value = value;
            }
        }

        /// <summary>
        /// Activates the LocalTextures API
        /// </summary>
        public static bool Activate()
        {
            try
            {
                var hello = Maker.CharaLocalTextures.SaveType;
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static ConfigEntry<SceneTextureSaveType> ConfTexSaveType { get; set; }

        private static bool oldBrowserFolders = false;
        private static bool saveTypeChanging = false;
        private static readonly Harmony harmony = null;

        static SceneLocalTextures()
        {
            string description = "Whether external textures used by plugins should be bundled with the scene, deduped and then saved to the scene, or saved to a local folder.\nWARNING: Scenes with deduped textures save some space but cannot be loaded by earlier plugin versions. Scenes with local textures save more space but cannot be shared.";
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Scene Save Type", SceneTextureSaveType.Bundled, new ConfigDescription(description, null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
            ConfTexSaveType.SettingChanged += OnSaveTypeChanged;
            if (StudioAPI.InsideStudio)
                harmony = Harmony.CreateAndPatchAll(typeof(SceneLocalTextures));

            KoikatuAPI.Instance.StartCoroutine(CheckFolderBrowsersLater());
            IEnumerator CheckFolderBrowsersLater()
            {
                yield return null;
                if (Chainloader.PluginInfos.TryGetValue("marco.FolderBrowser", out PluginInfo browserFoldersInfo))
                {
                    System.Version version = browserFoldersInfo.Metadata.Version;
                    if (version.Major < 3 || (version.Major == 3 && version.Minor < 1))
                        oldBrowserFolders = true;
                }
            }

            // Activates Maker LocalTexture API
            Maker.CharaLocalTextures.SaveType.ToString();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SceneLoadScene), nameof(SceneLoadScene.Awake))]
        private static void SetupUI(SceneLoadScene __instance)
        {
            // Don't show UI if local texture support is disabled
            if (!Maker.CharaLocalTextures.EnableLocalTextureSupport.Value)
                return;
                
            SetupUIPanel(__instance);
        }

        private static GameObject SetupUIPanel(SceneLoadScene root)
        {
            GameObject localSave;

            string titleText = "Texture Save Type";
            string warningText = "Scenes with deduped textures save some storage space but cannot be loaded by earlier plugin versions.\nScenes with local textures save more storage space but cannot be shared.";

            // Build UI
            {
#if !PH
                TextMeshProUGUI fontSource = Object.FindObjectOfType<TextMeshProUGUI>();
#endif
                localSave = new GameObject("localSave", new[] { typeof(RectTransform), typeof(ToggleGroup), typeof(Image) });
                localSave.transform.SetParent(root.transform.GetComponentInChildren<StudioNode>(true).GetComponentInParent<Canvas>().transform);
                localSave.GetComponent<Image>().sprite = MakeSprite("local-frame.png", 12);
                localSave.GetComponent<Image>().type = Image.Type.Sliced;
                if (oldBrowserFolders)
                    SetTfProps(localSave.GetComponent<RectTransform>(), 0, 1, 0, 1, 940, -225, 1420, -50);
                else
                    SetTfProps(localSave.GetComponent<RectTransform>(), 0, 1, 0, 1, 195, -225, 675, -50);

                var title = new GameObject("title", new[] { typeof(RectTransform) });
                title.transform.SetParent(localSave.transform);
#if !PH
                SetupText(title, titleText, TextAlignmentOptions.TopLeft, 24);
#else
                SetupText(title, titleText, TextAnchor.UpperLeft, 24);
#endif
                SetTfProps(title.GetComponent<RectTransform>(), 0, 1, 1, 1, 12, -48, 0, -5);

                var tglGrp = localSave.GetComponent<ToggleGroup>();
                tglGrp.RegisterToggle(CreateCheck("tglBundled", "Bundled", -80, -48));
                tglGrp.RegisterToggle(CreateCheck("tglDeduped", "Deduped", -120, -88));
                tglGrp.RegisterToggle(CreateCheck("tglLocal", "Local", -160, -128));

                var warning = new GameObject("warning", new[] { typeof(RectTransform) });
                warning.transform.SetParent(localSave.transform);
#if !PH
                var warningUGUI = SetupText(warning, warningText, TextAlignmentOptions.TopLeft);
#else
                var warningUGUI = SetupText(warning, warningText, TextAnchor.UpperLeft);
#endif
                warningUGUI.color = new Color(1, 0.3f, 0.3f);
                SetTfProps(warning.GetComponent<RectTransform>(), 0, 0, 1, 1, 138, 8, -10, -46);

                SetLayers(localSave.transform);

                Toggle CreateCheck(string name, string checkName, float f, float h)
                {
                    var checkRoot = new GameObject(name, new[] { typeof(RectTransform), typeof(Image), typeof(Toggle) });
                    checkRoot.transform.SetParent(localSave.transform);
                    SetTfProps(checkRoot.GetComponent<RectTransform>(), 0, 1, 0, 1, 8, f, 128, h);
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

                    var checkText = new GameObject("text", new[] { typeof(RectTransform) });
                    checkText.transform.SetParent(checkRoot.transform);
                    SetupText(checkText, checkName);
                    SetTfProps(checkText.GetComponent<RectTransform>(), 0, 0, 1, 1, 36, 0, 0, 0);

                    checkToggle.graphic = checkCheck.GetComponent<Image>();

                    return checkToggle;
                }

#if !PH
                TextMeshProUGUI SetupText(GameObject go, string content, TextAlignmentOptions alignment = TextAlignmentOptions.Left, int maxSize = 16)
#else
                Text SetupText(GameObject go, string content, TextAnchor alignment = TextAnchor.MiddleLeft, int maxSize = 16)
#endif
                {
#if !PH
                    TextMeshProUGUI textComp = go.GetOrAddComponent<TextMeshProUGUI>();
                    if (fontSource != null) textComp.font = fontSource.font;
                    textComp.enableAutoSizing = true;
                    textComp.fontSizeMin = 10;
                    textComp.fontSizeMax = maxSize;
#else
                    Text textComp = go.GetOrAddComponent<Text>();
                    textComp.resizeTextForBestFit = true;
                    textComp.resizeTextMinSize = 10;
                    textComp.resizeTextMaxSize = maxSize;
#endif
                    textComp.alignment = alignment;
                    textComp.text = content;

                    return textComp;
                }
            }

            var validator = localSave.AddComponent<ToggleValidator>();

            List<Toggle> toggles = new List<Toggle>();
            foreach (SceneTextureSaveType option in System.Enum.GetValues(typeof(SceneTextureSaveType)))
            {
                Toggle nowToggle = localSave.transform.GetChild(1 + (int)option).GetComponent<Toggle>();
                nowToggle.isOn = SaveType == option;
                nowToggle.onValueChanged = new Toggle.ToggleEvent();
                nowToggle.onValueChanged.AddListener((x) =>
                {
                    if (x && !saveTypeChanging)
                        SaveType = option;
                });

                toggles.Add(nowToggle);
            }

            validator.Register(toggles);

            SaveTypeChangedEvent += (x, y) =>
            {
                if (toggles == null) return;
                SceneTextureSaveType newValue = y.NewSetting;
                foreach (SceneTextureSaveType option in System.Enum.GetValues(typeof(SceneTextureSaveType)))
                {
                    if (toggles.Count < 1 + (int)option) break;
                    Toggle toggle = toggles[(int)option];
                    if (toggle != null && newValue == option && !toggle.isOn && toggle.isActiveAndEnabled)
                        toggle.isOn = true;
                }
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

            if (scale.HasValue) tf.localScale = scale.Value;
        }

        private static void SetLayers(Transform rootTransform)
        {
            rootTransform.gameObject.layer = 5; // UI
            for (int i = 0; i < rootTransform.childCount; i++)
            {
                SetLayers(rootTransform.GetChild(i));
            }
        }

        private static Sprite MakeSprite(string file, int border)
        {
            Texture2D spriteTex = ResourceUtils.GetEmbeddedResource(file).LoadTexture();
            Vector2 spriteSize = new Vector2(spriteTex.width, spriteTex.height);
            Rect spriteRect = new Rect(Vector2.zero, spriteSize);
            Vector4 spriteBorder = new Vector4(border, border, border, border);
            return Sprite.Create(spriteTex, spriteRect, spriteSize / 2, 100, 0, SpriteMeshType.FullRect, spriteBorder);
        }

        private static void OnSaveTypeChanged(object x, System.EventArgs y)
        {
            saveTypeChanging = true;
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(SaveTypeChangedEvent), "");
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(null, new SceneTextureSaveTypeChangedEventArgs(SaveType)), nameof(SaveTypeChangedEvent), eLogger);
            eLogger.End();
            saveTypeChanging = false;
        }

        private class ToggleValidator : MonoBehaviour
        {
            private List<Toggle> toggles;

            public void Register(List<Toggle> toggles)
            {
                this.toggles = toggles;
            }

            private void OnEnable()
            {
                if (toggles == null) return;
                foreach (SceneTextureSaveType option in System.Enum.GetValues(typeof(SceneTextureSaveType)))
                {
                    var toggle = toggles[(int)option];
                    if (toggle != null && SaveType == option && !toggle.isOn)
                        toggle.isOn = true;
                }
            }
        }
    }
}
