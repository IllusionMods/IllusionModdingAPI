using BepInEx;
using KKAPI.Utilities;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a single-line text box.
    /// </summary>
    public class MakerTextbox : BaseEditableGuiEntry<string>
    {
        private static Transform _textboxCopy;

        private readonly string _settingName;
        private readonly string _defaultValue;

        /// <summary>
        /// Type of content filtering to do on the input.
        /// </summary>
        public InputField.ContentType ContentType { get; set; } = InputField.ContentType.Standard;

        /// <summary>
        /// Maximum number of characters, about 22 are visible at once.
        /// </summary>
        public int CharacterLimit { get; set; } = 200;

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="settingName">Text displayed next to the slider</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        /// <param name="defaultValue">Value the textbox will be set to after creation</param>
        public MakerTextbox(MakerCategory category, string settingName, string defaultValue, BaseUnityPlugin owner) : base(category, defaultValue, owner)
        {
            _settingName = settingName;

            _defaultValue = defaultValue;
        }

        private static Transform TextboxCopy
        {
            get
            {
                if (_textboxCopy == null)
                    MakeCopy();

                return _textboxCopy;
            }
        }

        private static void MakeCopy()
        {
            // Exists in male and female maker
            var originalSlider = GetExistingControl("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll", "sldTemp");

            _textboxCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, false);
            _textboxCopy.gameObject.SetActive(false);
            _textboxCopy.name = "textboxTemp";

            var slider = _textboxCopy.Find("Slider").gameObject;
            Object.Destroy(slider);

            var inputFieldObj = _textboxCopy.Find("InputField");
            var inputFieldRt = inputFieldObj.GetComponent<RectTransform>();
            inputFieldRt.offsetMin = new Vector2(-275, 6);

            var inputField = inputFieldObj.GetComponent<TMP_InputField>();
            inputField.onValueChanged.ActuallyRemoveAllListeners();
            inputField.onSubmit.ActuallyRemoveAllListeners();
            inputField.onEndEdit.ActuallyRemoveAllListeners();

#if KKS
            var resetButton = _textboxCopy.Find("btnReset").GetComponent<Button>();
#else
            var resetButton = _textboxCopy.Find("Button").GetComponent<Button>();
#endif
            resetButton.onClick.ActuallyRemoveAllListeners();

            foreach (var renderer in _textboxCopy.GetComponentsInChildren<Image>())
                renderer.raycastTarget = true;

            RemoveLocalisation(_textboxCopy.gameObject);
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_textboxCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(TextboxCopy, subCategoryList, false);

            tr.name = "sldTextbox";

            var textMesh = tr.Find("textShape").GetComponent<TextMeshProUGUI>();
            textMesh.text = _settingName;
            textMesh.color = TextColor;

            var inputField = tr.Find("InputField").GetComponent<TMP_InputField>();
            inputField.contentType = (TMP_InputField.ContentType)ContentType;
            inputField.characterLimit = CharacterLimit;
            switch (ContentType)
            {
                case InputField.ContentType.Autocorrected:
                    inputField.inputType = TMP_InputField.InputType.AutoCorrect;
                    break;
                case InputField.ContentType.Password:
                    inputField.inputType = TMP_InputField.InputType.Password;
                    break;
                default:
                    inputField.inputType = TMP_InputField.InputType.Standard;
                    break;
            }

            // Update either on every keystroke or only at end
            //inputField.onValueChanged.AddListener(SetValue);
            inputField.onEndEdit.AddListener(SetValue);

            if (MakerAPI.InsideMaker) Singleton<ChaCustom.CustomBase>.Instance.lstTmpInputField.Add(inputField);

#if KKS
            var resetButton = tr.Find("btnReset").GetComponent<Button>();
#else
            var resetButton = tr.Find("Button").GetComponent<Button>();
#endif
            resetButton.onClick.AddListener(() => SetValue(_defaultValue));

            BufferedValueChanged.Subscribe(text => inputField.text = text);

            return tr.gameObject;
        }
    }
}
