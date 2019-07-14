using System;
using Studio;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Studio.UI {
    /// <summary>
    /// Custom control that draws a single, circular button with an on/off state
    /// </summary>
    public class CurrentStateCategorySwitch : CurrentStateCategorySubItemBase {

        private readonly Func<OCIChar, bool> _updateSelectionCallback;
        /// <summary>
        /// Current state of the button, off (false) or on (true)
        /// </summary>
        public BehaviorSubject<bool> SelectedSetting { get; }

        /// <summary>
        /// A single button for the Chara > CurrentState studio menu.
        /// </summary>
        /// <param name="name">Name of the button, shown on left</param> 
        /// <param name="onUpdateSelection">Function called when the current character changes and the on/off state needs to be updated.
        /// <code>OCIChar</code> is the newly selected character. Return the new state. Can't be null.</param>
        public CurrentStateCategorySwitch(string name, Func<OCIChar, bool> onUpdateSelection) : base(name) {
            _updateSelectionCallback = onUpdateSelection ?? throw new ArgumentNullException(nameof(onUpdateSelection));
            SelectedSetting = new BehaviorSubject<bool>(false);            
        }

        /// <inheritdoc />
        protected internal override void CreateItem(GameObject categoryObject) {
            var original = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Etc/Son");

            var copy = Object.Instantiate(original, categoryObject.transform, true);
            copy.name = "CustomSwitch " + Name;
            copy.transform.localScale = Vector3.one;
            copy.gameObject.SetActive(true);
            var text = copy.transform.Find("Text");
            text.name = "Text " + Name;
            var text1 = text.GetComponent<Text>();
            text1.text = Name;

            Toggle toggle = copy.GetComponentInChildren<Toggle>(true);

            toggle.transform.name = $"Button {Name}";
            toggle.gameObject.SetActive(true);
            toggle.isOn = SelectedSetting.Value;
            SelectedSetting.Subscribe((bool newSet) => {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = newSet;
                toggle.onValueChanged.AddListener(delegate { SelectedSetting.OnNext(toggle.isOn); });
            });
        }

        /// <inheritdoc />
        protected internal override void OnUpdateInfo(OCIChar ociChar) {
            SelectedSetting.OnNext(_updateSelectionCallback.Invoke(ociChar));
        }
    }
}
