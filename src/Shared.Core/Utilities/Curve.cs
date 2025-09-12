using MonoMod.Cil;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Provides a comprehensive set of utilities for performing operations on curves, including interpolation via Catmull-Rom splines and Bezier curves.
    /// </summary>
    public class Curve
    {
        /// <summary>
        /// A collection of predefined interpolation templates used for calculating positions along curves with various behaviors.
        /// </summary>
        /// <remarks>
        /// This struct includes methods for linear interpolation, easing (in, out, and in-out), smooth stepping, and step functions.
        /// </remarks>
        public struct Templates
        {
            /// <summary>
            /// Performs linear interpolation between two points to calculate a position at a specific interpolation value.
            /// </summary>
            /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the linear curve.</param>
            /// <returns>A <see cref="Vector2"/> representing the interpolated position along the linear curve.</returns>
            public static Vector2 Linear(float t) => Bezier(Vector2.zero, Vector2.one, Vector2.zero, Vector2.one, t);
            /// <summary>
            /// Performs easing interpolation between two points to calculate a position at a specific interpolation value, using a predefined ease-in-out curve.
            /// </summary>
            /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the easing curve.</param>
            /// <returns>A <see cref="Vector2"/> representing the interpolated position along the easing curve.</returns>
            public static Vector2 Ease(float t) => Bezier(Vector2.zero, Vector2.one, new Vector2(0.25f, 0.1f), new Vector2(0.25f, 1f), t);
            /// <summary>
            /// Performs an ease-in interpolation between two points to calculate a position at a specific interpolation value, resulting in a slower start and faster end.
            /// </summary>
            /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the ease-in curve.</param>
            /// <returns>A <see cref="Vector2"/> representing the interpolated position along the ease-in curve.</returns>
            public static Vector2 EaseIn(float t) => Bezier(Vector2.zero, Vector2.one, new Vector2(0.42f, 0f), Vector2.one, t);
            /// <summary>
            /// Calculates a position on an ease-out curved trajectory for a given interpolation value.
            /// </summary>
            /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the ease-out curve.</param>
            /// <returns>A <see cref="Vector2"/> representing the interpolated position along the ease-out curve.</returns>
            public static Vector2 EaseOut(float t) => Bezier(Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0.58f, 1f), t);
            /// <summary>
            /// Calculates a position along an ease-in-out interpolation curve at a specific interpolation value.
            /// </summary>
            /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the ease-in-out curve.</param>
            /// <returns>A <see cref="Vector2"/> representing the interpolated position along the ease-in-out curve.</returns>
            public static Vector2 EaseInOut(float t) => Bezier(Vector2.zero, Vector2.one, new Vector2(0.42f, 0), new Vector2(0.58f, 1), t);
            /// <summary>
            /// Computes a smooth step interpolation between two points to calculate a position at a specific interpolation value.
            /// </summary>
            /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the smooth curve.</param>
            /// <returns>A <see cref="Vector2"/> representing the interpolated position along the smooth step curve.</returns>
            public static Vector2 SmoothStep(float t) => Bezier(Vector2.zero, Vector2.one, new Vector2(0.5f, 0), new Vector2(0.5f, 1), t);
            /// <summary>
            /// Evaluates a step function to return a discrete position based on the input value.
            /// </summary>
            /// <param name="t">The input value to evaluate, typically ranging from 0 to 1. Values less than 0.5 will return <see cref="Vector2.zero"/>, and values greater or equal to 0.5 will return <see cref="Vector2.one"/>.</param>
            /// <returns>A <see cref="Vector2"/> that represents the output of the step function.</returns>
            public static Vector2 Step(float t) => t < 0.5f ? Vector2.zero : Vector2.one;
            /// <summary>
            /// Computes a step interpolation between two values, returning either the starting or ending point based on the comparison with a threshold.
            /// </summary>
            /// <param name="t">A value in the range [0, 1] which is compared against the threshold to determine the return value.</param>
            /// <param name="mid">The threshold value, defaulting to 0.5, which determines the transition point between the two outputs.</param>
            /// <returns>A <see cref="Vector2"/> representing either the starting point (Vector2.zero) if below the threshold or the ending point (Vector2.one) if above the threshold.</returns>
            public static Vector2 Step(float t, float mid = 0.5f) => t < mid ? Vector2.zero : Vector2.one;
            /// <summary>
            /// Evaluates a step function to determine a position based on a threshold at the start of the interpolation range.
            /// </summary>
            /// <param name="t">The input value for the step function, ranging from 0 to 1. Values less than 0.01 return zero; otherwise, one.</param>
            /// <returns>A <see cref="Vector2"/> that is either zero or one based on the evaluation of the input value.</returns>
            public static Vector2 StepStart(float t) => t < 0.01f ? Vector2.zero : Vector2.one;
            /// <summary>
            /// Determines whether the input interpolation factor is near the end of the range, returning either zero or one.
            /// </summary>
            /// <param name="t">The interpolation factor, generally ranging from 0 to 1, used to determine the step output.</param>
            /// <returns>A <see cref="Vector2"/> that returns zero if the input is less than 0.99, and one otherwise.</returns>
            public static Vector2 StepEnd(float t) => t < 0.99f ? Vector2.zero : Vector2.one;
        }

        /// <summary>
        /// Performs Catmull-Rom spline interpolation between four points to calculate a position at a specific interpolation value.
        /// </summary>
        /// <param name="p0">The control point preceding the starting point of the interpolation segment.</param>
        /// <param name="p1">The starting point of the interpolation segment.</param>
        /// <param name="p2">The ending point of the interpolation segment.</param>
        /// <param name="p3">The control point following the ending point of the interpolation segment.</param>
        /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the segment.</param>
        /// <returns>A <see cref="Vector4"/> representing the interpolated position along the Catmull-Rom spline.</returns>
        public static Vector4 CatmullRom(Vector4 p0, Vector4 p1, Vector4 p2, Vector4 p3, float t)
        {
            var tSquared = t * t;
            var tCubed = tSquared * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * tSquared + (-p0 + 3f * p1 - 3f * p2 + p3) * tCubed);
        }
        /// <summary>
        /// Performs Catmull-Rom spline interpolation between four points to calculate a position at a specific interpolation value.
        /// </summary>
        /// <param name="p0">The control point preceding the starting point of the interpolation segment.</param>
        /// <param name="p1">The starting point of the interpolation segment.</param>
        /// <param name="p2">The ending point of the interpolation segment.</param>
        /// <param name="p3">The control point following the ending point of the interpolation segment.</param>
        /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the segment.</param>
        /// <returns>A <see cref="Vector3"/> representing the interpolated position along the Catmull-Rom spline.</returns>
        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var tSquared = t * t;
            var tCubed = tSquared * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * tSquared + (-p0 + 3f * p1 - 3f * p2 + p3) * tCubed);
        }
        /// <summary>
        /// Performs Catmull-Rom spline interpolation between four points to calculate a position at a specific interpolation value.
        /// </summary>
        /// <param name="p0">The control point preceding the starting point of the interpolation segment.</param>
        /// <param name="p1">The starting point of the interpolation segment.</param>
        /// <param name="p2">The ending point of the interpolation segment.</param>
        /// <param name="p3">The control point following the ending point of the interpolation segment.</param>
        /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the segment.</param>
        /// <returns>A <see cref="Vector2"/> representing the interpolated position along the Catmull-Rom spline.</returns>
        public static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            var tSquared = t * t;
            var tCubed = tSquared * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * tSquared + (-p0 + 3f * p1 - 3f * p2 + p3) * tCubed);
        }
        /// <summary>
        /// Calculates a position on a cubic Bezier curve for a given interpolation value.
        /// </summary>
        /// <param name="p0">The starting point of the curve.</param>
        /// <param name="p1">The first control point that influences the shape of the curve near the starting point.</param>
        /// <param name="p2">The second control point that influences the shape of the curve near the ending point.</param>
        /// <param name="p3">The ending point of the curve.</param>
        /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the curve.</param>
        /// <returns>A <see cref="Vector4"/> representing the interpolated position along the Bezier curve.</returns>
        public static Vector4 Bezier(Vector4 p0, Vector4 p1, Vector4 p2, Vector4 p3, float t)
        {
            float u = 1 - t;
            float tSquared = t * t;
            float uSquared = u * u;
            float uCubed = uSquared * u;
            float tCubed = tSquared * t;
            return uCubed * p0 + 3 * uSquared * t * p1 + 3 * u * tSquared * p2 + tCubed * p3;
        }
        /// <summary>
        /// Calculates a position on a cubic Bezier curve for a given interpolation value.
        /// </summary>
        /// <param name="p0">The starting point of the curve.</param>
        /// <param name="p1">The first control point that influences the shape of the curve near the starting point.</param>
        /// <param name="p2">The second control point that influences the shape of the curve near the ending point.</param>
        /// <param name="p3">The ending point of the curve.</param>
        /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the curve.</param>
        /// <returns>A <see cref="Vector3"/> representing the interpolated position along the Bezier curve.</returns>
        public static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            float tSquared = t * t;
            float uSquared = u * u;
            float uCubed = uSquared * u;
            float tCubed = tSquared * t;
            return uCubed * p0 + 3 * uSquared * t * p1 + 3 * u * tSquared * p2 + tCubed * p3;
        }
        /// <summary>
        /// Calculates a position on a cubic Bezier curve for a given interpolation value.
        /// </summary>
        /// <param name="p0">The starting point of the curve.</param>
        /// <param name="p1">The first control point that influences the shape of the curve near the starting point.</param>
        /// <param name="p2">The second control point that influences the shape of the curve near the ending point.</param>
        /// <param name="p3">The ending point of the curve.</param>
        /// <param name="t">The interpolation factor, ranging from 0 (start) to 1 (end), that determines the position along the curve.</param>
        /// <returns>A <see cref="Vector2"/> representing the interpolated position along the Bezier curve.</returns>
        public static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            float tSquared = t * t;
            float uSquared = u * u;
            float uCubed = uSquared * u;
            float tCubed = tSquared * t;
            return uCubed * p0 + 3 * uSquared * t * p1 + 3 * u * tSquared * p2 + tCubed * p3;
        }

        /// <summary>
        /// Resamples a polyline represented by a set of 3D points into a specified number of evenly spaced points, with optional smoothing.
        /// </summary>
        /// <param name="points">An array of <see cref="Vector3"/> representing the original polyline points to be resampled.</param>
        /// <param name="count">The desired number of points in the resulting resampled polyline.</param>
        /// <param name="smooth">Indicates whether smoothing should be applied using interpolation. Defaults to false.</param>
        /// <param name="solver">
        /// The interpolation function to use for smoothing, if enabled. If null, the default Catmull-Rom interpolation is used.
        /// The function must have the signature: Func(Vector3, Vector3, Vector3, Vector3, float, Vector3).
        /// </param>
        /// <returns>An array of <see cref="Vector3"/> containing the resampled polyline points.</returns>
        /// <remarks>Does not work with <see cref="Bezier(UnityEngine.Vector4,UnityEngine.Vector4,UnityEngine.Vector4,UnityEngine.Vector4,float)"/> for <paramref name="solver"/></remarks>
        public static Vector3[] ResamplePoly(Vector3[] points, int count, bool smooth = false,
            RuntimeILReferenceBag.FastDelegateInvokers.Func<Vector3, Vector3, Vector3, Vector3, float, Vector3> solver = null)
        {
            if (points.Length == count) return points;

            Vector3[] result = new Vector3[count];
            float step = (points.Length - 1) / (float)(count - 1);

            if (solver == null) solver = CatmullRom;

            for (int i = 0; i < count; i++)
            {
                float index = i * step;
                int floor = Mathf.FloorToInt(index);
                float t = index - floor;

                if (floor >= points.Length - 1)
                {
                    result[i] = points[points.Length - 1];
                }
                else if (smooth)
                {
                    Vector3 p0, p1, p2, p3;

                    if (floor == 0)
                    {
                        p0 = points[0];
                        p1 = points[0];
                        p2 = points[1];
                        p3 = points.Length > 2 ? points[2] : points[1];
                    }
                    else if (floor >= points.Length - 2)
                    {
                        p0 = points.Length > 2 ? points[points.Length - 3] : points[points.Length - 2];
                        p1 = points[points.Length - 2];
                        p2 = points[points.Length - 1];
                        p3 = points[points.Length - 1];
                    }
                    else
                    {
                        p0 = points[floor - 1];
                        p1 = points[floor];
                        p2 = points[floor + 1];
                        p3 = points[floor + 2];
                    }

                    result[i] = solver(p0, p1, p2, p3, t);
                }
                else
                {
                    result[i] = Vector3.Lerp(points[floor], points[floor + 1], t);
                }
            }

            return result;
        }
    }
}