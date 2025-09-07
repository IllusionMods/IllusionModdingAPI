using MonoMod.Cil;
using UnityEngine;

namespace KKAPI.Utilities
{
    class Curve
    {
        public struct Templates
        {
            public static Vector2 Linear(float t) => Bezier(Vector2.zero, Vector2.one, Vector2.zero, Vector2.one, t);
            public static Vector2 Ease(float t) => Bezier(Vector2.zero, Vector2.one, new Vector2(0.25f, 0.1f), new Vector2(0.25f, 1f), t);
            public static Vector2 EaseIn(float t) => Bezier(Vector2.zero, Vector2.one, new Vector2(0.42f, 0f), Vector2.one, t);
            public static Vector2 EaseOut(float t) => Bezier(Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0.58f, 1f), t);
            public static Vector2 EaseInOut(float t) => Bezier(Vector2.zero, Vector2.one, new Vector2(0.42f, 0), new Vector2(0.58f, 1), t);
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
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
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
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
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
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
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
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
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
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
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
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
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