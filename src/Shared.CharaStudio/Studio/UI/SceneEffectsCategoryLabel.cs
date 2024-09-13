#if KK || KKS || AI || HS2
#define TMP
#endif

using UnityEngine.UI;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// A container for the value of a ColorPicker, associated label and button, and the setter method that triggers on value change.
    /// </summary>
    public class SceneEffectsLabelSet
    {
        /// <summary>
        /// Label UI element.
        /// </summary>
        public Text Label { get; set; }
        /// <summary>
        /// Get or set the text of the label.
        /// </summary>
        public string Text
        {
            get => Label.text;
            set => Label.text = value;
        }

        /// <summary>
        /// Create a new Label. Typically, you want to use SceneEffectsCategory.AddLabel instead of creating these manually.
        /// </summary>
        /// <param name="label">Label UI element</param>
        /// <param name="text">Label text</param>
        public SceneEffectsLabelSet(Text label, string text)
        {
            Label = label;
            Text = text;

            Label.gameObject.name = $"Label {Text}";
        }
    }
}