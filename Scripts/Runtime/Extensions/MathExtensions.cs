using UnityEngine;

namespace HEVS.Extensions {
    /// <summary>
    /// An extension utility class.
    /// </summary>
    public static partial class MathematicsExtensions
    {
        /// <summary>
        /// Interpolates the color towards a target color.
        /// </summary>
        /// <param name="orig">The original color object.</param>
        /// <param name="target">The target color.</param>
        /// <param name="amount">The maximum amount to interpolate towards the target color.</param>
        /// <returns>Returns the interpolated color.</returns>
        public static Color MoveTowards(this Color orig, Color target, float amount)
        {
            Color result = new Color(orig.r, orig.g, orig.b, orig.a);
            result.r = Mathf.MoveTowards(result.r, target.r, amount);
            result.g = Mathf.MoveTowards(result.g, target.g, amount);
            result.b = Mathf.MoveTowards(result.b, target.b, amount);
            result.a = Mathf.MoveTowards(result.a, target.a, amount);
            return result;
        }

        /// <summary>
        /// A positive modulo for floats.
        /// </summary>
        /// <param name="x">The original float value.</param>
        /// <param name="m">The modulo float value. Default is 1.</param>
        /// <returns>Returns a positive modulo.</returns>
        public static float PositiveMod(this float x, float m = 1.0f)
        {
            float r = x % m;
            return r < 0f ? r + m : r;
        }

        /// <summary>
        /// A positive modulo for ints.
        /// </summary>
        /// <param name="x">The original integer value.</param>
        /// <param name="m">The modulo integer value.</param>
        /// <returns>Returns a positive modulo.</returns>
        public static int PositiveMod(this int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        #region Vector Swizzling
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 xx(this Vector2 v)
        {
            return new Vector2(v.x, v.x);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 yy(this Vector2 v)
        {
            return new Vector2(v.y, v.y);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 yx(this Vector2 v)
        {
            return new Vector2(v.y, v.x);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 xx(this Vector3 v)
        {
            return new Vector2(v.x, v.x);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector3 xxx(this Vector3 v)
        {
            return new Vector3(v.x, v.x, v.x);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 xy(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 xz(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 yz(this Vector3 v)
        {
            return new Vector2(v.y, v.z);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector4 xxyy(this Vector4 v)
        {
            return new Vector4(v.x, v.x, v.y, v.y);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector4 zzww(this Vector4 v)
        {
            return new Vector4(v.z, v.z, v.w, v.w);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 xy(this Vector4 v)
        {
            return new Vector2(v.x, v.y);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector2 xz(this Vector4 v)
        {
            return new Vector2(v.x, v.z);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector3 xyz(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        /// <summary>
        /// Sqizzles the vector.
        /// </summary>
        /// <param name="v">The vector to swizzle.</param>
        /// <returns>Returns a swizzled result.</returns>
        public static Vector3 rgb(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        #endregion

        /// <summary>
        /// Find the Quaternion's magnitude.
        /// </summary>
        /// <param name="q">The Quaternion to query.</param>
        /// <returns>Returns the Quaternion's magnitude.</returns>
        public static float magnitude(this Quaternion q)
        {
            return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        }

        /// <summary>
        /// Find the Quaternion's conjugate.
        /// </summary>
        /// <param name="q">The Quaternion to query.</param>
        /// <returns>Returns the Quaternion's Conjugate.</returns>
        public static Quaternion conjugate(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

        /// <summary>
        /// Returns a Vector3 whose components are the absolutes of this Vector3.
        /// </summary>
        /// <param name="v">The Vector3 to convert.</param>
        /// <returns>Returns an absolute version of the specified Vector3.</returns>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
    }
}
