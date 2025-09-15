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

namespace KKAPI.Studio.UI.Toolbars
{
    // TODO:
    // - hide and show buttons?
    // - maybe right click context menu?

    /// <summary>
    /// Base class for custom toolbar buttons in the studio UI.
    /// </summary>
    public abstract class ToolbarControlBase : IDisposable
    {
        private static GameObject _existingButtonGo;
        private static Transform _allButtonParentTr;
        private static Vector2 _originPosition;
        private static readonly Vector2 _positionOffset = new Vector2(40, 40f);

        private RectTransform _rectTransform;
        private GlobalTooltips.Tooltip _tooltip;
        private protected RectTransform RectTransform
        {
            get => _rectTransform;
            set
            {
                if (_rectTransform != value)
                {
                    _rectTransform = value;
                    if (_tooltip != null) _tooltip.Destroy();
                    if (!string.IsNullOrEmpty(HoverText)) _tooltip = GlobalTooltips.RegisterTooltip(_rectTransform, HoverText);
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
        /// https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
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
        /// Which row to place the button in.
        /// For the bottom left toolbar, this is counted from bottom left corner of the screen.
        /// Set to -1 to automatically add to the end of the toolbar.
        /// </summary>
        public int DesiredRow { get; internal set; } = -1;

        /// <summary>
        /// Which column to place the button in.
        /// For the bottom left toolbar, this is counted from bottom left corner of the screen.
        /// Set to -1 to automatically add to the end of the toolbar.
        /// </summary>
        public int DesiredColumn { get; internal set; } = -1;

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
            IsDisposed = true;

            ToolbarManager.RemoveControl(this);

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
            if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarControlBase));
            if (ButtonObject.Value) return; // already created

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
            ButtonObject.OnNext(copyBtn);

            Interactable.Subscribe(b => ButtonObject.Value.interactable = b);
            Visible.Subscribe(b =>
            {
                var gameObject = ButtonObject.Value.gameObject;
                if (gameObject.activeSelf != b)
                {
                    gameObject.SetActive(b);
                    ToolbarManager.RequestToolbarRelayout();
                }
            });

            DragHelper.SetUpDragging(this, newGobject);
        }

        /// <summary>
        /// Set the desired position of the button in the toolbar.
        /// If another button has already requested this position, this button may be moved to the right.
        /// </summary>
        /// <param name="row">Row index. 0 indexed.</param>
        /// <param name="column">Column index. 0 indexed.</param>
        public void SetDesiredPosition(int row, int column)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarControlBase));

            DesiredRow = row;
            DesiredColumn = column;

            ToolbarManager.RequestToolbarRelayout();
        }

        /// <summary>
        /// Set the actual transform position of the button.
        /// </summary>
        internal void SetActualPosition(int row, int column, bool setDesired)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarControlBase));

            if (setDesired)
            {
                DesiredRow = row;
                DesiredColumn = column;
            }

            var newPos = new Vector2(_originPosition.x + column * _positionOffset.x,
                                     _originPosition.y + row * _positionOffset.y);
            RectTransform.anchoredPosition = newPos;
        }

        /// <summary>
        /// Gets the actual row and column position of the button in the toolbar.
        /// </summary>
        internal void GetActualPosition(out int row, out int col) => GetActualPosition(RectTransform.anchoredPosition, out row, out col);

        /// <summary>
        /// Gets row and column of a position in the toolbar.
        /// </summary>
        internal static void GetActualPosition(Vector2 anchoredPosition, out int row, out int column)
        {
            var y = Mathf.RoundToInt(anchoredPosition.y);
            row = Mathf.RoundToInt((y - _originPosition.y) / _positionOffset.y);

            var x = Mathf.RoundToInt(anchoredPosition.x);
            column = Mathf.RoundToInt((x - _originPosition.x) / _positionOffset.x);

            //Console.WriteLine($"GetActualPosition: x={anchoredPosition.x} -> col={column} y={anchoredPosition.y} -> row={row}");
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
            private int _originalRow, _originalCol, _currentDragRow, _currentDragCol;

            void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
            {
                _owner.GetActualPosition(out _originalRow, out _originalCol);
                _currentDragRow = _originalRow;
                _currentDragCol = _originalCol;

                var button = _owner.ButtonObject.Value;
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
                GetActualPosition(rt.anchoredPosition, out _currentDragRow, out _currentDragCol);
            }

            void IEndDragHandler.OnEndDrag(PointerEventData eventData)
            {
                _owner.ButtonObject.Value.enabled = true;

                // Move other buttons to make room if necessary (to next column in the same row)

                // Get all buttons in the same row that are visible and not this button
                var allButtons = ToolbarManager.GetAllButtons();
                var sameRowButtons = allButtons.Where(b => b.DesiredRow == _currentDragRow && b.DesiredColumn >= 0 && b != _owner && b.Visible.Value)
                                               .ToDictionary(b => b.DesiredColumn, b => b);

                // If the position is already taken, try to move the other button to make space
                if (sameRowButtons.TryGetValue(_currentDragCol, out var otherButtonToMove))
                {
                    // Move by one to the left if there is space
                    if (_currentDragCol >= 1 && !sameRowButtons.ContainsKey(_currentDragCol - 1))
                    {
                        otherButtonToMove.DesiredColumn = _currentDragCol - 1;
                    }
                    else
                    {
                        // Otherwise move all buttons right until a free space is found
                        for (int c = _currentDragCol + 1; ; c++)
                        {
                            otherButtonToMove.DesiredColumn = c;

                            if (!sameRowButtons.TryGetValue(c, out otherButtonToMove))
                                break;
                        }
                    }
                }

                // Place the dragged button at the new position and trigger relayout
                _owner.SetDesiredPosition(_currentDragRow, _currentDragCol);
            }
        }
    }
}
