using BepInEx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MakerAPI
{
    public class MakerRadioButtons : ValueMakerGuiEntry<int>
    {
        private readonly string _settingName;
        private readonly string _button1;
        private readonly string _button2;
        private readonly string _button3;

        private static Transform _radioCopy;

        public MakerRadioButtons(MakerCategory category, string settingName, string button1, string button2, string button3, BaseUnityPlugin owner) : base(category, 0, owner)
        {
            _settingName = settingName;
            _button1 = button1;
            _button2 = button2;
            _button3 = button3;
        }

        protected internal override void CreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(RadioCopy, subCategoryList, true);

            tr.name = "rb" + GuiApiNameAppendix;

            var settingName = tr.Find("textTglTitle").GetComponent<TextMeshProUGUI>();
            settingName.text = _settingName;
            settingName.color = TextColor;

            var t1 = tr.Find("rb00").GetComponent<Toggle>();
            var t2 = tr.Find("rb01").GetComponent<Toggle>();
            var t3 = tr.Find("rb02").GetComponent<Toggle>();

            t1.GetComponentInChildren<TextMeshProUGUI>().text = _button1;
            t2.GetComponentInChildren<TextMeshProUGUI>().text = _button2;
            t3.GetComponentInChildren<TextMeshProUGUI>().text = _button3;

            t1.onValueChanged.AddListener(a =>
            {
                if (a)
                    SetNewValue(0);
            });
            t2.onValueChanged.AddListener(a =>
            {
                if (a)
                    SetNewValue(1);
            });
            t3.onValueChanged.AddListener(a =>
            {
                if (a)
                    SetNewValue(2);
            });

            BufferedValueChanged.Subscribe(i =>
            {
                switch (i)
                {
                    case 0:
                        t1.isOn = true;
                        break;
                    case 1:
                        t2.isOn = true;
                        break;
                    case 2:
                        t3.isOn = true;
                        break;
                }
            });

            tr.gameObject.SetActive(true);
        }

        private static Transform RadioCopy
        {
            get
            {
                if (_radioCopy == null)
                {
                    // Exists in male and female maker
                    var originalSlider = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglEye02/Eye02Top/rbEyeSettingType").transform;

                    _radioCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, true);
                    _radioCopy.gameObject.SetActive(false);
                    _radioCopy.name = "rbSide" + GuiApiNameAppendix;

                    foreach (var toggle in _radioCopy.GetComponentsInChildren<Toggle>())
                    {
                        toggle.onValueChanged.RemoveAllListeners();
                        toggle.image.raycastTarget = true;
                        toggle.graphic.raycastTarget = true;
                        toggle.gameObject.name = "rb" + toggle.gameObject.name.Substring(8);
                    }
                }

                return _radioCopy;
            }
        }
    }
}