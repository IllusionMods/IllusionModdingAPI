using System.Collections;
using BepInEx.Bootstrap;
using UnityEngine.UI;
using CharaCustom;
using UnityEngine;
using HarmonyLib;
using TMPro;
using BepInEx;

namespace KKAPI.Maker
{
    public static partial class LocalTextures
    {
        private static void SetupUI()
        {
            // Save new screen
            var panel1 = SetupUIPanel(false, out bool isNew);
            if (isNew)
            {
                panel1.transform.SetParent(Singleton<CustomBase>.Instance.cvsCapMenu.transform.parent);
                SetTfProps(panel1.GetComponent<RectTransform>(), 0, 0, 0, 0, 358, 10, 662, 150);
            }

            // Overwrite screen
            if (Chainloader.PluginInfos.TryGetValue("mikke.gravureAI", out var pluginInfo))
                KoikatuAPI.Instance.StartCoroutine(ModifyOverwriteScreen(pluginInfo));

            IEnumerator ModifyOverwriteScreen(PluginInfo info)
            {
                yield return null;
                yield return null;
                var fieldInfo = info.Instance.GetType().GetField("confCanvas", AccessTools.all);
                Canvas confCanvas = (Canvas)fieldInfo?.GetValue(info.Instance);
                if (confCanvas == null) yield break;
                var panel2 = SetupUIPanel(true, out _);
                var panel2rect = panel2.GetComponent<RectTransform>();
                panel2.transform.SetParent(confCanvas.transform.GetChild(0));
                SetTfProps(panel2rect, 0.5f, 0, 0.5f, 0, -152, 178, 152, 318);
            }
        }

        private static GameObject SetupUIPanel(bool forceNew, out bool isNewPanel)
        {
            GameObject root = Singleton<CustomBase>.Instance.cvsCapMenu.gameObject;
            Transform copyFrom = root.transform.Find("menuTop/bg");
            GameObject localSave;

            string warningText = "Cards with local textures save storage space but cannot be shared.";

            if (copyFrom != null && !forceNew)
            {
                isNewPanel = false;
                localSave = Object.Instantiate(copyFrom.gameObject, copyFrom.transform.parent);
                localSave.name = "localSave";
                localSave.transform.SetSiblingIndex(copyFrom.GetSiblingIndex() + 1);
                Object.DestroyImmediate(localSave.transform.Find("separate").gameObject);
                Object.DestroyImmediate(localSave.transform.Find("bgcolor").gameObject);
                Object.DestroyImmediate(localSave.transform.Find("index").gameObject);
                localSave.transform.Find("type/textTitle").GetComponent<Text>().text = "Textures";
                localSave.transform.Find("type/textTitle").GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                var toggles = localSave.transform.Find("type/items");
                toggles.GetChild(0).GetComponentInChildren<Text>().text = "Bundled";
                toggles.GetChild(0).GetComponentInChildren<Text>().fontSize = 15;
                toggles.GetChild(1).GetComponentInChildren<Text>().text = "Local";

                var warning = Object.Instantiate(localSave.transform.GetChild(0), localSave.transform);
                warning.name = "warning";
                Object.DestroyImmediate(warning.transform.GetChild(1).gameObject);
                warning.transform.GetChild(0).gameObject.name = "warnText";
                var warnText = warning.GetComponentInChildren<Text>();
                SetTfProps(warnText.gameObject.GetComponent<RectTransform>(), 0, 0, 1, 1, 25, -1, -20, 12);
                warnText.alignment = TextAnchor.UpperLeft;
                warnText.fontSize = 15;
                warnText.color = Color.red;
                warnText.text = warningText;
            }
            else
            {
                isNewPanel = true;
                localSave = new GameObject("localSave", new[] { typeof(RectTransform), typeof(ToggleGroup), typeof(Image) });
                localSave.transform.SetParent(root.transform);
                localSave.GetComponent<Image>().sprite = MakeSprite("local-frame.png", 12);
                localSave.GetComponent<Image>().type = Image.Type.Sliced;
                SetTfProps(localSave.GetComponent<RectTransform>(), 0, 1, 0, 1, 1384, -772, 1688, -632);

                var text = new GameObject("text", new[] { typeof(RectTransform), typeof(TextMeshProUGUI) });
                text.transform.SetParent(localSave.transform);
                var textUGUI = SetupText(text, warningText, TextAlignmentOptions.TopLeft);
                textUGUI.color = new Color(1, 0.3f, 0.3f);
                SetTfProps(text.GetComponent<RectTransform>(), 0, 1, 1, 1, 8, -136, 0, -88);

                var tglGrp = localSave.GetComponent<ToggleGroup>();
                tglGrp.RegisterToggle(CreateCheck("imgRbFace", "Bundled Textures", -40, -8));
                tglGrp.RegisterToggle(CreateCheck("imgRbCard", "Local Textures", -80, -48));

                SetLayers(localSave.transform);
                
                Toggle CreateCheck(string name, string checkName, float f, float h)
                {
                    var checkRoot = new GameObject(name, new[] { typeof(RectTransform), typeof(Image), typeof(Toggle) });
                    checkRoot.transform.SetParent(localSave.transform);
                    checkRoot.GetComponent<Image>().color = Color.clear;
                    var checkToggle = checkRoot.GetComponent<Toggle>();
                    checkToggle.group = tglGrp;
                    checkToggle.navigation = new Navigation() { mode = Navigation.Mode.None };
                    SetTfProps(checkRoot.GetComponent<RectTransform>(), 0, 1, 1, 1, 8, f, 0, h);

                    var checkBack = new GameObject("rb_back", new[] { typeof(RectTransform), typeof(Image) });
                    checkBack.transform.SetParent(checkRoot.transform.transform);
                    checkBack.GetComponent<Image>().sprite = MakeSprite("local-back.png", 0);
                    SetTfProps(checkBack.GetComponent<RectTransform>(), 0, 0, 0, 1, 0, 0, 32, 0);

                    var checkCheck = new GameObject("rb_check", new[] { typeof(RectTransform), typeof(Image) });
                    checkCheck.transform.SetParent(checkBack.transform.transform);
                    checkCheck.GetComponent<Image>().sprite = MakeSprite("local-check.png", 0);
                    SetTfProps(checkCheck.GetComponent<RectTransform>(), 0, 0, 1, 1, 0, 0, 0, 0);

                    var checkText = new GameObject("text", new[] { typeof(RectTransform), typeof(TextMeshProUGUI) });
                    checkText.transform.SetParent(checkRoot.transform);
                    SetupText(checkText, checkName);
                    SetTfProps(checkText.GetComponent<RectTransform>(), 0, 0, 1, 1, 36, 0, 0, 0);

                    checkToggle.graphic = checkCheck.GetComponent<Image>();

                    return checkToggle;
                }

                TextMeshProUGUI SetupText(GameObject go, string content, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
                {
                    TextMeshProUGUI tmpro = go.GetComponent<TextMeshProUGUI>();
                    tmpro.enableAutoSizing = true;
                    tmpro.fontSizeMin = 10;
                    tmpro.fontSizeMax = 16;
                    tmpro.alignment = alignment;
                    tmpro.text = content;

                    return tmpro;
                } 
            }

            var validator = localSave.AddComponent<ToggleValidator>();

            Toggle tglBundled;
            if (isNewPanel)
                tglBundled = localSave.transform.GetChild(1).GetComponent<Toggle>();
            else
                tglBundled = localSave.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Toggle>();
            tglBundled.isOn = SaveType == TextureSaveType.Bundled;
            tglBundled.onValueChanged = new Toggle.ToggleEvent();
            tglBundled.onValueChanged.AddListener((x) =>
            {
                if (x && !saveTypeChanging)
                    SaveType = TextureSaveType.Bundled;
            });

            Toggle tglLocal;
            if (isNewPanel)
                tglLocal = localSave.transform.GetChild(2).GetComponent<Toggle>();
            else
                tglLocal = localSave.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Toggle>();
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
    }
}