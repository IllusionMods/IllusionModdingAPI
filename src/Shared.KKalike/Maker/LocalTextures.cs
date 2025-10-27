using KKAPI.Utilities;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using ChaCustom;
using TMPro;

namespace KKAPI.Maker
{
    public static partial class LocalTextures
    {
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
            GameObject copyFrom = root.GetComponentInChildren<ToggleGroup>(true).gameObject;
            GameObject localSave;

            string tglText1 = "Bundled Textures";
            string tglText2 = "Local Textures";
            string warningText = "Cards with local textures save storage space but cannot be shared.";

            if (copyFrom != null)
            {
                isNewPanel = false;
                localSave = Object.Instantiate(copyFrom.gameObject, copyFrom.transform.parent);
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
                localSave.GetComponent<Image>().sprite = MakeSprite("local-frame.png", 12);
                localSave.GetComponent<Image>().type = Image.Type.Sliced;
                SetTfProps(localSave.GetComponent<RectTransform>(), 0, 1, 0, 1, 1384, -772, 1688, -632);

                var text = new GameObject("text", new[] { typeof(RectTransform), typeof(TextMeshProUGUI) });
                text.transform.SetParent(localSave.transform);
                var textUGUI = SetupText(text, warningText, TextAlignmentOptions.TopLeft);
                textUGUI.color = new Color(1, 0.3f, 0.3f);
                SetTfProps(text.GetComponent<RectTransform>(), 0, 1, 1, 1, 8, -136, 0, -88);

                var tglGrp = localSave.GetComponent<ToggleGroup>();
                tglGrp.RegisterToggle(CreateCheck("imgRbFace", tglText1, -40, -8));
                tglGrp.RegisterToggle(CreateCheck("imgRbCard", tglText2, -80, -48));

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
    }
}