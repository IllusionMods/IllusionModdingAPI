using System;
using System.IO;
using System.Linq;
using BepInEx;
using Illusion.Extensions;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Base class for custom toolbar buttons in the studio UI.
    /// </summary>
    public abstract class CustomToolbarControlBase : IDisposable
    {
        private static GameObject _existingButton;
        private static Transform _allButtonParent;
        private static Vector2 _originPosition;
        private static readonly Vector2 _positionOffset = new Vector2(40, 40f);
        private readonly Func<Texture2D> _iconGetter;

        private Texture2D _iconTex;
        private protected RectTransform RectTransform;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomToolbarControlBase" /> class.
        /// </summary>
        /// <param name="buttonID">Unique button ID.</param>
        /// <param name="hoverText">Hover text for the button. null to disable.</param>
        /// <param name="iconGetter">
        /// Function to get the icon texture. It should be a 32x32 icon used for the button.
        /// You can find a template here
        /// https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
        /// For best performance and smallest size save your thumbnail as 8bit grayscale png (or indexed if you need colors) with
        /// no alpha channel.
        /// This func is always called on main thread, it may not be called if button is hidden by user.
        /// </param>
        /// <param name="owner">Plugin that owns the button.</param>
        protected CustomToolbarControlBase(string buttonID, string hoverText, Func<Texture2D> iconGetter, BaseUnityPlugin owner)
        {
            ButtonID = buttonID ?? throw new ArgumentNullException(nameof(buttonID));
            ButtonID = ButtonID.Trim();
            if (string.IsNullOrEmpty(ButtonID)) throw new ArgumentException("buttonID can't be empty", nameof(buttonID));
            // Check if buttonID contains any characters that can't be used in a GameObject name
            if (ButtonID.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("buttonID contains invalid characters", nameof(buttonID));

            HoverText = hoverText;
            _iconGetter = iconGetter ?? throw new ArgumentNullException(nameof(iconGetter));
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Unique identifier for the button.
        /// </summary>
        public string ButtonID { get; }

        /// <summary>
        /// Text to display when hovering over the button.
        /// </summary>
        public string HoverText { get; }

        /// <summary>
        /// The plugin that owns this button.
        /// </summary>
        public BaseUnityPlugin Owner { get; }

        /// <summary>
        /// The icon texture for the button. Must be 32x32.
        /// </summary>
        public Texture2D IconTex
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(CustomToolbarControlBase));
                if (_iconTex != null)
                    return _iconTex;

                _iconTex = _iconGetter();
                if (_iconTex == null)
                    throw new InvalidOperationException($"Icon texture getter for {ButtonID} returned null");
                if (_iconTex.width != 32 || _iconTex.height != 32)
                    KoikatuAPI.Logger.LogWarning($"Icon texture passed to {ButtonID} has wrong size, it should be 32x32 but is {_iconTex.width}x{_iconTex.height}");
                return _iconTex;
            }
        }

        /// <summary>
        /// Which row to place the button in.
        /// For the bottom left toolbar, this is counted from bottom left corner of the screen.
        /// Set to -1 to automatically add to the end of the toolbar.
        /// </summary>
        public int DesiredRow { get; private protected set; } = -1;

        /// <summary>
        /// Which column to place the button in.
        /// For the bottom left toolbar, this is counted from bottom left corner of the screen.
        /// Set to -1 to automatically add to the end of the toolbar.
        /// </summary>
        public int DesiredColumn { get; private protected set; } = -1;

        /// <summary>
        /// The Unity UI Button object for this toolbar button.
        /// </summary>
        public BehaviorSubject<Button> ButtonObject { get; } = new BehaviorSubject<Button>(null);

        /// <summary>
        /// Whether the button is interactable.
        /// </summary>
        public BehaviorSubject<bool> Interactable { get; } = new BehaviorSubject<bool>(true);

        /// <summary>
        /// Whether the button is visible.
        /// </summary>
        public BehaviorSubject<bool> Visible { get; } = new BehaviorSubject<bool>(true);
        
        /// <summary>
        /// True if the button has been removed and needs to be recreated to be used again.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Destroys the button and cleans up resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed) return;

            lock (CustomToolbarButtons.Buttons)
            {
                CustomToolbarButtons.Buttons.Remove(this);
            }

            IsDisposed = true;

            Visible?.Dispose();
            Interactable?.Dispose();

            var button = ButtonObject.Value;
            if (button) Object.Destroy(button.gameObject);
            Object.Destroy(_iconTex);
        }

        /// <summary>
        /// Create the actual button control in the studio UI.
        /// </summary>
        protected internal virtual void CreateControl()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(CustomToolbarControlBase));
            if (ButtonObject.Value) return; // already created

            var iconTex = IconTex;

            var btnIconSprite = Sprite.Create(iconTex, new Rect(0f, 0f, 32f, 32f), new Vector2(16f, 16f));

            // Create new button
            var copyButton = Object.Instantiate(_existingButton, _allButtonParent, false);
            copyButton.name = $"Button {ButtonID} (StudioAPI)";
            RectTransform = copyButton.GetComponent<RectTransform>();
            RectTransform.localScale = Vector3.one;

            var copyBtn = RectTransform.GetComponent<Button>();
            copyBtn.onClick.ActuallyRemoveAllListeners();
            var btnIcon = copyBtn.image;
            btnIcon.sprite = btnIconSprite;
            btnIcon.color = Color.white;
            ButtonObject.OnNext(copyBtn);

            Interactable.Subscribe(b => ButtonObject.Value.interactable = b);
            Visible.Subscribe(b =>
            {
                var gameObject = ButtonObject.Value.gameObject;
                if(gameObject.activeSelf != b)
                {
                    gameObject.SetActive(b);
                    CustomToolbarButtons.RequestToolbarRelayout();
                }
            });
        }

        /// <summary>
        /// Set the desired position of the button in the toolbar.
        /// If another button has already requested this position, this button will be moved to the right.
        /// </summary>
        /// <param name="row">Row index. 0 indexed.</param>
        /// <param name="column">Column index. 0 indexed.</param>
        public void SetDesiredPosition(int row, int column)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(CustomToolbarControlBase));

            DesiredRow = row;
            DesiredColumn = column;

            CustomToolbarButtons.RequestToolbarRelayout();
        }

        /// <summary>
        /// Set the actual transform position of the button.
        /// </summary>
        internal void SetActualPosition(int row, int column)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(CustomToolbarControlBase));

            DesiredRow = row;
            DesiredColumn = column;

            var newPos = new Vector2(_originPosition.x + column * _positionOffset.x,
                                     _originPosition.y + row * _positionOffset.y);
            RectTransform.anchoredPosition = newPos;
        }

        /// <summary>
        /// Gets the actual position of the button in the toolbar.
        /// </summary>
        internal void GetActualPosition(out int row, out int col)
        {
            // Calculate row and column based on position
            var pos = RectTransform.anchoredPosition;
            var x = Mathf.RoundToInt(pos.x);
            var y = Mathf.RoundToInt(pos.y);

            row = Mathf.RoundToInt((y - _originPosition.y) / _positionOffset.y);
            col = Mathf.RoundToInt((x - _originPosition.x) / _positionOffset.x);

#if DEBUG
            Console.WriteLine($"GetActualPosition: {ButtonID} x={pos.x} col={col} y={pos.y} row={row}");
#endif
        }

        /// <summary>
        /// Initializes the toolbar and finds the origin position for custom buttons.
        /// </summary>
        internal static void InitToolbar()
        {
            if (_existingButton) return;

            _existingButton = GameObject.Find("StudioScene/Canvas System Menu/01_Button/Button Center");
            _allButtonParent = _existingButton.transform.parent;
            var allStockButtons = _allButtonParent.OfType<RectTransform>().ToList();

            // Find bottom-left-most button to use as origin
            var origin = allStockButtons.OrderBy(x => Mathf.RoundToInt(x.anchoredPosition.y)).ThenBy(x => Mathf.RoundToInt(x.anchoredPosition.x)).First();
            _originPosition = origin.anchoredPosition;

            foreach (var allStockButton in allStockButtons)
            {
                var wrapper = new ToolbarControlPlaceholder(allStockButton.GetComponent<Button>());
                CustomToolbarButtons.Buttons.Add(wrapper);
            }
        }
    }
}
