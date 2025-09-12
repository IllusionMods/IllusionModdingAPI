using System;
using JetBrains.Annotations;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Represents a smart rectangle that provides advanced manipulation of a rectangle's dimensions and position.
    /// </summary>
    public class SmartRect
    {
        public event EventHandler OnAnimationComplete; 
        private Rect _animateFrom;
        private Rect _animateTo;
        private float _animationDuration;
        private float _elapsedTime;

        public static float DefaultOffsetX => 20;
        public static float DefaultOffsetY => 5f;

        public readonly float DefaultHeight;
        public readonly float DefaultWidth;
        public readonly float DefaultX;
        public readonly float DefaultY;

        private float _moveX;
        private float _moveY;
        private readonly float _offsetX;
        private readonly float _offsetY;
        private Rect _source;

        public float OffsetX => _offsetX;
        public float OffsetY => _offsetY;

        public float Height
        {
            get => _source.height;
            set
            {
                _source.height = value;
                _moveY = value + _offsetY;
            }
        }

        public float TotalWidth => Width + _offsetX;
        public float TotalHeight => Height + _offsetY;

        public float Width
        {
            get => _source.width;
            set
            {
                _source.width = value;
                _moveX = value + _offsetX;
            }
        }

        public float X
        {
            get => _source.x;
            set => _source.x = value;
        }

        public float Y
        {
            get => _source.y;
            set => _source.y = value;
        }

        /// <summary>
        /// Represents a smart rectangle that provides advanced manipulation of a rectangle's dimensions and position.
        /// </summary>
        /// <param name="src">The default <seealso cref="Rect"/> to use.</param>
        public SmartRect(Rect src) : this(src, DefaultOffsetX, DefaultOffsetY)
        {
        }


        /// <summary>
        /// Represents a smart rectangle that provides advanced manipulation of a rectangle's dimensions and position.
        /// </summary>
        /// <param name="src">The default <seealso cref="Rect"/> to use.</param>
        /// <param name="offX">The offset in pixels towards the X coordinate.</param>
        /// <param name="offY">The offset in pixels towards the Y coordinate.</param>
        public SmartRect(Rect src, float offX, float offY)
        {
            _source = new Rect(src.x, src.y, src.width, src.height);
            _offsetX = offX;
            _offsetY = offY;
            _moveX = _source.width + _offsetX;
            _moveY = _source.height + _offsetY;
            DefaultHeight = src.height;
            DefaultWidth = src.width;
            DefaultX = src.x;
            DefaultY = src.y;
        }

        public SmartRect(float x, float y, float width, float height) : this(new Rect(x, y, width, height))
        {
        }

        public SmartRect(float x, float y, float width, float height, float offX, float offY) : this(
            new Rect(x, y, width, height), offX, offY)
        {
        }

        /// <summary>
        /// Divides the current width of the rectangle into equal segments for horizontal layout.
        /// Adjusts the width of each segment based on the total number of elements and specified horizontal offsets.
        /// </summary>
        /// <param name="elementCount">The number of elements to divide the rectangle horizontally into.</param>
        public SmartRect BeginHorizontal(int elementCount)
        {
            Width = (Width - _offsetX * (elementCount - 1)) / elementCount;

            return this;
        }

        /// <summary>
        /// Synonymous to <see cref="ResetX"/>
        /// </summary>
        public SmartRect EndHorizontal()
        {
            return ResetX();
        }

        /// <summary>
        /// Moves the SmartRect by the specified vector values.
        /// </summary>
        /// <param name="vec">A <see cref="Vector2"/> specifying the change in X and Y coordinates.</param>
        /// <returns>The updated <see cref="SmartRect"/> after applying the movement.</returns>
        public SmartRect Move(Vector2 vec)
        {
            _source.x += vec.x;
            _source.y += vec.y;
            return this;
        }


        /// <summary>
        /// Moves the rectangle by the specified x and y offsets and returns the updated <see cref="SmartRect"/>.
        /// </summary>
        /// <param name="x">The offset to move the rectangle along the x-axis.</param>
        /// <param name="y">The offset to move the rectangle along the y-axis.</param>
        /// <returns>The updated <see cref="SmartRect"/> after being moved.</returns>
        public SmartRect Move(int x, int y)
        {
            _source.x += x;
            _source.y += y;
            return this;
        }

        /// <summary>
        /// Moves the rectangle's X position by the specified offset and adjusts its width accordingly.
        /// </summary>
        /// <param name="off">The offset value to move the rectangle's X position.</param>
        public SmartRect MoveOffsetX(float off)
        {
            _source.x += off;
            _source.width -= off;

            return this;
        }

        /// <summary>
        /// Adjusts the Y position and height of the smart rectangle by the specified offset.
        /// </summary>
        /// <param name="off">The offset to apply to the Y position and height.</param>
        public SmartRect MoveOffsetY(float off)
        {
            _source.y += off;
            _source.height -= off;

            return this;
        }

        /// <summary>
        /// Adjusts the X position of the current rectangle to align with the right edge of the specified rectangle, taking into account the given width.
        /// </summary>
        /// <param name="box">The rectangle to align with.</param>
        /// <param name="width">The width to use for alignment.</param>
        public SmartRect MoveToEndX(Rect box, float width)
        {
            _source.x += box.x + box.width - _source.x - width;

            return this;
        }

        /// <summary>
        /// Moves the Y position of the rectangle represented by the current <see cref="SmartRect"/> instance
        /// to the bottom of the specified 'box' plus the specified 'height'.
        /// </summary>
        /// <param name="box">The reference rectangle used to determine the new Y position.</param>
        /// <param name="height">The height to be considered when moving to the end.</param>
        public SmartRect MoveToEndY(Rect box, float height)
        {
            _source.y += box.y + box.height - _source.y - height;

            return this;
        }

        /// <summary>
        /// Moves the rectangle horizontally by a predefined offset and returns the updated <see cref="SmartRect"/> instance.
        /// </summary>
        /// <returns>The updated <see cref="SmartRect"/> instance.</returns>
        public SmartRect MoveX()
        {
            _source.x += _moveX;
            return this;
        }

        /// <summary>
        /// Moves the rectangle along the X-axis by the specified offset and optionally considers its width during the move.
        /// </summary>
        /// <param name="off">The offset by which to move the rectangle along the X-axis.</param>
        /// <param name="considerWidth">Determines whether the rectangle's width should be added to the offset.</param>
        /// <returns>The current instance of <see cref="SmartRect"/> after applying the movement.</returns>
        public SmartRect MoveX(float off, bool considerWidth = false)
        {
            _source.x += off;
            if (considerWidth)
            {
                _source.x += _source.width;
            }

            return this;
        }

        /// <summary>
        /// Moves the <seealso cref="SmartRect"/> by it's own height.
        /// </summary>
        public SmartRect MoveY()
        {
            _source.y += _moveY;

            return this;
        }

        /// <summary>
        /// Moves the <seealso cref="SmartRect"/> by a specified offset.
        /// </summary>
        /// <param name="offset">The amount to move the <seealso cref="SmartRect"/> by.</param>
        /// <param name="considerHeight">If true will also move the <seealso cref="SmartRect"/> by its own height, else only by <paramref name="offset"/></param>
        public SmartRect MoveY(float offset, bool considerHeight = false)
        {
            _source.y += offset;
            if (considerHeight)
            {
                _source.y += _source.height;
            }

            return this;
        }

        public SmartRect SetWidth(float width)
        {
            _source.width = width;
            return this;
        }

        public SmartRect SetHeight(float height)
        {
            _source.height = height;
            return this;
        }

        /// <summary>
        /// Sets the width of the rectangle such that its right edge aligns with the specified x-coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate to which the right edge of the rectangle should align.</param>
        /// <returns>The current instance of <see cref="SmartRect"/> to allow method chaining.</returns>
        public SmartRect WidthToEnd(float x)
        {
            _source.width = x - _source.x;
            return this;
        }

        /// <summary>
        /// Sets the height of the rectangle such that its bottom edge aligns with the specified y-coordinate.
        /// </summary>
        /// <param name="y">The y-coordinate to which the bottom edge of the rectangle should align.</param>
        /// <returns>The current instance of <see cref="SmartRect"/> to allow method chaining.</returns>
        public SmartRect HeightToEnd(float y)
        {
            _source.height = y - _source.y;
            return this;
        }

        /// <summary>
        /// Updates the animation of the rectangle independently using a provided solver function based on progress.
        /// </summary>
        /// <param name="solver">A function that takes progress (a value between 0 and 1) and returns a vector where the y-component determines the animation factor.</param>
        /// <returns>
        /// A boolean indicating whether the animation is still in progress.
        /// Returns <c>true</c> if the animation is ongoing, <c>false</c> if the animation has completed.
        /// </returns>
        public bool UpdateAnimationIndependent(Func<float, Vector2> solver)
        {
            if (_animationDuration == 0)
                return false;

            var xDiff = _animateTo.x - _source.x;
            var yDiff = _animateTo.y - _source.y;
            var widthDiff = _animateTo.width - _source.width;
            var heightDiff = _animateTo.height - _source.height;

            if (Mathf.Abs(xDiff) <= 0.01f && Mathf.Abs(yDiff) <= 0.01f && Mathf.Abs(widthDiff) <= 0.01f &&
                Mathf.Abs(heightDiff) <= 0.01f)
            {
                _source = _animateTo;
                _animationDuration = 0;
                _elapsedTime = 0;
                OnAnimationComplete?.Invoke(this, null);
                return false;
            }

            var progress = Mathf.Clamp01(_elapsedTime / _animationDuration);
            var f = solver(progress).y;

            _source.x = Mathf.Ceil(f * (_animateTo.x - _animateFrom.x) + _animateFrom.x);
            _source.y = Mathf.Ceil(f * (_animateTo.y - _animateFrom.y) + _animateFrom.y);
            _source.width = Mathf.Ceil(f * (_animateTo.width - _animateFrom.width) + _animateFrom.width);
            _source.height = Mathf.Ceil(f * (_animateTo.height - _animateFrom.height) + _animateFrom.height);

            _elapsedTime += Time.deltaTime;

            var updateAnimation = progress < 1f;
            if (!updateAnimation)
            {
                _source = _animateTo;
                _elapsedTime = 0;
                _animationDuration = 0;
                OnAnimationComplete?.Invoke(this, null);
            }

            return updateAnimation;
        }

        /// <summary>
        /// Sets the starting and target rectangles, along with the duration for an animation.
        /// </summary>
        /// <param name="from">The starting <see cref="Rect"/> of the animation.</param>
        /// <param name="to">The target <see cref="Rect"/> to animate to.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <returns>The current instance of <see cref="SmartRect"/> to allow method chaining.</returns>
        public SmartRect SetAnimation(Rect from, Rect to, float duration)
        {
            _elapsedTime = 0f;
            _animateFrom = from;
            _animateTo = to;
            _source = from;
            _animationDuration = duration;

            return this;
        }

        /// <summary>
        /// Sets the target rectangle and duration for an animation.
        /// </summary>
        /// <param name="to">The target <see cref="Rect"/> to animate to.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <returns>The current instance of <see cref="SmartRect"/> to allow method chaining.</returns>
        public SmartRect SetAnimateTo(Rect to, float duration)
        {
            _elapsedTime = 0f;
            _animateFrom = _source;
            _animateTo = to;
            _animationDuration = duration;

            return this;
        }

        /// <summary>
        /// Moves to the next column by shifting the rectangle horizontally by a predefined offset.
        /// </summary>
        /// <returns>An updated instance of <see cref="SmartRect"/> representing the next column.</returns>
        public SmartRect NextColumn()
        {
            MoveX();
            return this;
        }

        public SmartRect Col(int col) =>
            new SmartRect(_source.x + _moveX * col, _source.y, _source.width, _source.height);

        public SmartRect Row(int row) =>
            new SmartRect(_source.x, _source.y + _moveY * row, _source.width, _source.height);

        /// <summary>
        /// Moves the rectangle to the next row, optionally resetting the column position.
        /// </summary>
        /// <param name="resetColumn">Indicates whether the column position should be reset to the default X position.</param>
        /// <returns>The updated instance of <see cref="SmartRect"/> after moving to the next row.</returns>
        public SmartRect NextRow(bool resetColumn = true)
        {
            if (resetColumn)
                _source.x = DefaultX;
            MoveY();
            return this;
        }

        /// <summary>
        /// Resets the <see cref="SmartRect"/> to its default position and dimensions.
        /// </summary>
        public SmartRect Reset()
        {
            _source.x = DefaultX;
            _source.y = DefaultY;
            Height = DefaultHeight;
            Width = DefaultWidth;
            return this;
        }

        /// <summary>
        /// Resets the x-coordinate of the rectangle to its default value.
        /// Optionally resets the width to its default value.
        /// </summary>
        /// <param name="includeWidth">If true, the width will also be reset to its default value.</param>
        public SmartRect ResetX(bool includeWidth = true)
        {
            _source.x = DefaultX;
            if (includeWidth)
            {
                _source.width = DefaultWidth;
            }

            return this;
        }

        /// <summary>
        /// Resets the y-coordinate of the rectangle to its default value.
        /// Optionally resets the height to its default value.
        /// </summary>
        /// <param name="includeHeight">If true, the height will also be reset to its default value.</param>
        public SmartRect ResetY(bool includeHeight = false)
        {
            _source.y = DefaultY;
            if (includeHeight)
            {
                _source.height = DefaultHeight;
            }

            return this;
        }


        /// <summary>
        /// Converts the current <see cref="SmartRect"/> instance into a Rect object.
        /// </summary>
        /// <returns>A Rect object representing the current <see cref="SmartRect"/>.</returns>
        public Rect ToRect()
        {
            return _source;
        }

        /// <summary>
        /// Defines an implicit conversion from a <see cref="SmartRect"/> instance to a UnityEngine.Rect instance.
        /// </summary>
        /// <param name="r">The <see cref="SmartRect"/> instance to convert.</param>
        /// <returns>A Rect instance representing the same dimensions and position as the <see cref="SmartRect"/>.</returns>
        public static implicit operator Rect(SmartRect r)
        {
            return r._source;
        }
    }
}