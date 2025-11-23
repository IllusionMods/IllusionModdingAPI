using BepInEx;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// TODO: Hide and show buttons? Maybe right click context menu?
namespace KKAPI.Studio.UI.Toolbars
{
    /// <summary>
    /// Base class for custom toolbar buttons in the studio UI.
    /// </summary>
    public abstract class ToolbarControlBase : IDisposable
    {
        private static GameObject _existingButtonGo;
        private static Transform _allButtonParentTr;
        private static Vector2 _originPosition;
        private static readonly Vector2 _positionOffset = new Vector2(40, 40f);

        private readonly GlobalTooltips.Tooltip _tooltip;
        private RectTransform _rectTransform;
        private protected RectTransform RectTransform
        {
            get => _rectTransform;
            set
            {
                if (_rectTransform != value)
                {
                    _rectTransform = value;
                    if (_tooltip != null)
                        GlobalTooltips.RegisterTooltip(_rectTransform.gameObject, _tooltip);
                }
            }
        }

        private readonly Func<Texture2D> _iconGetter;
        private Texture2D _iconTex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarControlBase" /> class.
        /// </summary>
        /// <param name="buttonID">Unique button ID.</param>
        /// <param name="hoverText">Hover text for the button. null to disable.</param>
        /// <param name="iconGetter">
        /// Function to get the icon texture. It should be a 32x32 icon used for the button.
        /// You can find a template here
        /// https://gitgoon.dev/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
        /// For best performance and smallest size save your thumbnail as 8bit grayscale png (or indexed if you need colors) with
        /// no alpha channel.
        /// This func is always called on main thread, it may not be called if button is hidden by user.
        /// </param>
        /// <param name="owner">Plugin that owns the button.</param>
        protected ToolbarControlBase(string buttonID, string hoverText, Func<Texture2D> iconGetter, BaseUnityPlugin owner)
        {
            ButtonID = buttonID ?? throw new ArgumentNullException(nameof(buttonID));
            ButtonID = ButtonID.Trim();
            if (string.IsNullOrEmpty(ButtonID)) throw new ArgumentException("buttonID can't be empty", nameof(buttonID));
            // Check if buttonID contains any characters that can't be used in a GameObject name
            if (ButtonID.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("buttonID contains invalid characters", nameof(buttonID));
            _iconGetter = iconGetter ?? throw new ArgumentNullException(nameof(iconGetter));
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
#if DEBUG
            hoverText = $"{GetType().FullName}\nOwner={owner.Info.Metadata.Name}\nName={buttonID}\n\n{hoverText ?? "<NULL>"}";
#endif
            HoverText = hoverText;
            if (!string.IsNullOrEmpty(hoverText))
                _tooltip = new GlobalTooltips.Tooltip(hoverText);
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
                if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarControlBase));
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
        /// Which row and column to place the button in.
        /// For the bottom left toolbar, this is counted from bottom left corner of the screen.
        /// If null, the button position will be chosen automatically.
        /// </summary>
        public ToolbarPosition? DesiredPosition { get; internal set; }

        /// <summary>
        /// The Unity UI Button object for this toolbar button. Null until the button is created in the UI.
        /// </summary>
        public Button ButtonObject { get; protected set; }

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
            IsDisposed = true;

            ToolbarManager.RemoveControl(this);

            Visible?.Dispose();
            Interactable?.Dispose();

            var button = ButtonObject;
            if (button) Object.Destroy(button.gameObject);
            Object.Destroy(_iconTex);
        }

        /// <summary>
        /// Create the actual button control in the studio UI.
        /// </summary>
        protected internal virtual void CreateControl()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarControlBase));
            if (ButtonObject) return; // already created

            var iconTex = IconTex;

            var btnIconSprite = Sprite.Create(iconTex, new Rect(0f, 0f, 32f, 32f), new Vector2(16f, 16f));

            // Create new button
            var newGobject = Object.Instantiate(_existingButtonGo, _allButtonParentTr, false);
            newGobject.name = $"Button {ButtonID} (StudioAPI)";
            RectTransform = newGobject.GetComponent<RectTransform>();
            RectTransform.localScale = Vector3.one;

            var copyBtn = RectTransform.GetComponent<Button>();
            copyBtn.onClick.ActuallyRemoveAllListeners();
            var btnIcon = copyBtn.image;
            btnIcon.sprite = btnIconSprite;
            btnIcon.color = Color.white;
            ButtonObject = copyBtn;

            Interactable.SubscribeOnMainThread().Subscribe(b => ButtonObject.interactable = b);
            Visible.SubscribeOnMainThread().Subscribe(b =>
            {
                var gameObject = ButtonObject.gameObject;
                if (gameObject.activeSelf != b)
                {
                    gameObject.SetActive(b);
                    ToolbarManager.RequestToolbarRelayout();
                }
            });

            DragHelper.SetUpDragging(this, newGobject);

            ControlCreated?.Invoke(newGobject);
        }

        internal event Action<GameObject> ControlCreated;

        /// <summary>
        /// Set the desired position of the button in the toolbar.
        /// If another button has already requested this position, this button may be moved to the right.
        /// </summary>
        public void SetDesiredPosition(int row, int column) => SetDesiredPosition(new ToolbarPosition(row, column));

        /// <inheritdoc cref="SetDesiredPosition(int,int)"/>
        public void SetDesiredPosition(ToolbarPosition position)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarControlBase));

            DesiredPosition = position;
            ToolbarManager.RequestToolbarRelayout();
        }

        /// <summary>
        /// Clear the desired position and let the button be positioned automatically.
        /// </summary>
        public void ResetDesiredPosition()
        {
            DesiredPosition = null;
            ToolbarManager.RequestToolbarRelayout();
        }

        /// <summary>
        /// Set the actual transform position of the button.
        /// Returns true if the position is within screen bounds.
        /// </summary>
        internal bool SetActualPosition(ToolbarPosition position, bool setDesired)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarControlBase));

            var newPos = new Vector2(_originPosition.x + position.Column * _positionOffset.x,
                                     _originPosition.y + position.Row * _positionOffset.y);
            RectTransform.anchoredPosition = newPos;

            var positionIsInBounds = RectTransform.IsInsideScreenBounds(-5);
            if (setDesired)
                DesiredPosition = positionIsInBounds ? position : (ToolbarPosition?)null;
            return positionIsInBounds;
        }

        /// <summary>
        /// Gets the current position of the actual button object as placed in the toolbar.
        /// Use to see where the button ended up after layout (e.g. when desired position is null).
        /// </summary>
        protected internal ToolbarPosition GetActualPosition() => GetActualPosition(RectTransform.anchoredPosition);

        /// <summary>
        /// Gets the toolbar position of the specified anchoredPosition as if it was in the toolbar.
        /// </summary>
        internal static ToolbarPosition GetActualPosition(Vector2 anchoredPosition)
        {
            var y = Mathf.RoundToInt(anchoredPosition.y);
            var row = Mathf.RoundToInt((y - _originPosition.y) / _positionOffset.y);

            var x = Mathf.RoundToInt(anchoredPosition.x);
            var column = Mathf.RoundToInt((x - _originPosition.x) / _positionOffset.x);

            //Console.WriteLine($"GetActualPosition: x={anchoredPosition.x} -> col={column} y={anchoredPosition.y} -> row={row}");
            return new ToolbarPosition(row, column);
        }

        /// <summary>
        /// Initializes the toolbar and finds the origin position for custom buttons.
        /// </summary>
        internal static IEnumerable<ToolbarControlAdapter> InitToolbar()
        {
            if (_existingButtonGo) return Enumerable.Empty<ToolbarControlAdapter>();

            _existingButtonGo = GameObject.Find("StudioScene/Canvas System Menu/01_Button/Button Center");
            _allButtonParentTr = _existingButtonGo.transform.parent;
            var allStockButtons = _allButtonParentTr.OfType<RectTransform>().ToList();

            // Find bottom-left-most button to use as origin
            var origin = allStockButtons.OrderBy(x => Mathf.RoundToInt(x.anchoredPosition.y)).ThenBy(x => Mathf.RoundToInt(x.anchoredPosition.x)).First();
            _originPosition = origin.anchoredPosition;

            return allStockButtons.Select(stockButton => new ToolbarControlAdapter(stockButton.GetComponent<Button>()));
        }

        /// <summary>
        /// Helper class to enable drag-and-drop reordering of toolbar buttons.
        /// </summary>
        protected sealed class DragHelper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            /// <summary>
            /// Sets up drag handling for a toolbar button.
            /// </summary>
            /// <param name="owner">The <see cref="ToolbarControlBase"/> instance that owns the button.</param>
            /// <param name="buttonObject">The button <see cref="GameObject"/> to enable dragging for.</param>
            /// <returns>The created <see cref="DragHelper"/> component.</returns>
            public static DragHelper SetUpDragging(ToolbarControlBase owner, GameObject buttonObject)
            {
                if (owner == null) throw new ArgumentNullException(nameof(owner));
                if (buttonObject == null) throw new ArgumentNullException(nameof(buttonObject));
                // The template might already have a DragHelper component, so check first
                var dh = buttonObject.GetComponent<DragHelper>() ?? buttonObject.AddComponent<DragHelper>();
                dh._owner = owner;
                return dh;
            }

            private ToolbarControlBase _owner;
            private ToolbarPosition _currentDragPosition;

            void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
            {
                _currentDragPosition = _owner.GetActualPosition();

                var button = _owner.ButtonObject;
                if (button == null) throw new ArgumentNullException(nameof(button));

                // Move to front so it's not hidden behind other buttons while dragging
                button.transform.SetAsLastSibling();

                // Disable while dragging so it doesn't trigger click events
                button.enabled = false;
            }

            void IDragHandler.OnDrag(PointerEventData eventData)
            {
                // Move the object with the mouse
                var rt = _owner.RectTransform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rt.parent, eventData.position, eventData.pressEventCamera, out var localPoint);
                // Adjust position so button is centered under mouse
                localPoint.x -= rt.rect.width / 2;
                localPoint.y += rt.rect.height / 2;
                rt.anchoredPosition = localPoint;
                _currentDragPosition = GetActualPosition(rt.anchoredPosition);
            }

            void IEndDragHandler.OnEndDrag(PointerEventData eventData)
            {
                _owner.ButtonObject.enabled = true;

                // Move other buttons to make room if necessary (to next column in the same row)

                // Get all buttons in the same row that are visible and not this button
                var allButtons = ToolbarManager.GetAllButtons(false);
                var sameRowButtons = allButtons.Where(b => b.DesiredPosition != null && b.DesiredPosition.Value.Row == _currentDragPosition.Row && b != _owner)
                                               .ToDictionary(b => b.DesiredPosition.Value.Column, b => b);

                // If the position is already taken, try to move the other button to make space
                if (sameRowButtons.TryGetValue(_currentDragPosition.Column, out var otherButtonToMove))
                {
                    // Move by one to the left if there is space
                    if (_currentDragPosition.Column >= 1 && !sameRowButtons.ContainsKey(_currentDragPosition.Column - 1))
                    {
                        otherButtonToMove.DesiredPosition = new ToolbarPosition(_currentDragPosition.Row, _currentDragPosition.Column - 1);
                    }
                    else
                    {
                        // Otherwise move all buttons right until a free space is found
                        for (int c = _currentDragPosition.Column + 1; ; c++)
                        {
                            otherButtonToMove.DesiredPosition = new ToolbarPosition(_currentDragPosition.Row, c);

                            if (!sameRowButtons.TryGetValue(c, out otherButtonToMove))
                                break;
                        }
                    }
                }

                // Place the dragged button at the new position and trigger relayout
                _owner.SetDesiredPosition(_currentDragPosition);
            }
        }
    }
}
