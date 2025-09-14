using KKAPI.Maker.UI;
using System;
using System.Reflection;
using UniRx;
using UnityEngine;

namespace KKAPI.Studio.UI
{
    public static partial class CustomToolbarButtons
    {
        /// <summary>
        /// Add a toggle button to the top of the left studio toolbar.
        /// Clicking on the button will toggle it between being on and off.
        /// </summary>
        /// <param name="iconTex">
        /// A 32x32 icon used for the button.
        /// You can find a template here
        /// https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
        /// For best performance and smallest size save your thumbnail as 8bit grayscale png (or indexed if you need colors) with no alpha channel.
        /// </param>
        /// <param name="initialValue">Initial state of the toggle.</param>
        /// <param name="onValueChanged">
        /// Action fired each time user clicks on the toggle button.
        /// The value is true if the toggle is enabled by the click, false if disabled.
        /// </param>
        [Obsolete("Use other AddLeftToolbarToggle overloads")]
        public static ToolbarToggle AddLeftToolbarToggle(Texture2D iconTex, bool initialValue = false, Action<bool> onValueChanged = null)
        {
            if (iconTex == null) throw new ArgumentNullException(nameof(iconTex));
            var tgl = new ToolbarToggleControl(Assembly.GetCallingAssembly().GetName().Name, "", () => iconTex, initialValue, onValueChanged, KoikatuAPI.Instance);
            AddLeftToolbarControl(tgl);
            return new ToolbarToggle(tgl);
        }

        /// <summary>
        /// Add a simple button to the top of the left studio toolbar.
        /// </summary>
        /// <param name="iconTex">
        /// A 32x32 icon used for the button.
        /// You can find a template here
        /// https://github.com/ManlyMarco/QuickAccessBox/blob/master/Shared_QuickAccessBox/UI/toolbar-icon.png
        /// </param>
        /// <param name="onClicked">Action fired each time user clicks on the button</param>
        [Obsolete("Use other AddLeftToolbarToggle overloads")]
        public static ToolbarButton AddLeftToolbarButton(Texture2D iconTex, Action onClicked = null)
        {
            if (iconTex == null) throw new ArgumentNullException(nameof(iconTex));
            var btn = new ToolbarButtonControl(Assembly.GetCallingAssembly().GetName().Name, "", () => iconTex, onClicked, KoikatuAPI.Instance);
            AddLeftToolbarControl(btn);
            return new ToolbarButton(btn);
        }
    }

    /// <summary>
    /// Custom toolbar toggle button. Add using <see cref="CustomToolbarButtons.AddLeftToolbarToggle"/>.
    /// </summary>
    [Obsolete("Use other AddLeftToolbarToggle overloads with ToggleToolbarButton")]
    public class ToolbarToggle : BaseEditableGuiEntry<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarToggle"/> class.
        /// </summary>
        /// <param name="target">The target toggle toolbar button.</param>
        public ToolbarToggle(ToolbarToggleControl target) : base(null, target.Value.Value, null)
        {
            target.Value.Subscribe(b =>
            {
                if (b != Value)
                    Value = b;
            });
            ValueChanged.Subscribe(b =>
            {
                if (b != target.Value.Value)
                    target.Value.OnNext(b);
            });
            target.ButtonObject.Subscribe(b =>
            {
                _controlObjects.Clear();
                if (b != null) _controlObjects.Add(b.gameObject);
            });
        }

        /// <inheritdoc />
        protected internal override void Initialize() { }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList) => null;
    }

    /// <summary>
    /// Custom toolbar button. Add using <see cref="CustomToolbarButtons.AddLeftToolbarButton"/>.
    /// </summary>
    [Obsolete("Use other AddLeftToolbarToggle overloads with SimpleToolbarButton")]
    public class ToolbarButton : BaseGuiEntry
    {
        private readonly Subject<Unit> _clicked = new Subject<Unit>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarButton"/> class.
        /// </summary>
        /// <param name="target">The target simple toolbar button.</param>
        internal ToolbarButton(ToolbarButtonControl target) : base(null, null)
        {
            target.OnClicked.Subscribe(_ => _clicked.OnNext(Unit.Default));
            target.ButtonObject.Subscribe(b =>
            {
                _controlObjects.Clear();
                if (b != null) _controlObjects.Add(b.gameObject);
            });
        }

        /// <summary>
        /// Triggered when the button is clicked.
        /// </summary>
        public IObservable<Unit> Clicked => _clicked;

        /// <inheritdoc />
        protected internal override void Initialize() { }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList) => null;
    }
}