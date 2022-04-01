using System;
using System.Collections;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Basic library of easing functions.
    /// Includes classic tween using t, b, c and d,
    /// and includes simplified 1D variants for modifying a delta [0,1] range.
    /// Also includes some utility coroutines.
    /// </summary>
    public class Ease
    {
        /// <summary>
        /// Interpolate a value from start to end for a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="start">Starting value.</param>
        /// <param name="end">Ending value.</param>
        /// <param name="duration">Duration of the ease.</param>
        /// <param name="function">Easing method to use.</param>
        /// <param name="tick">The tick method to call each iteration</param>
        /// <returns>Returns the IEnumerator for a coroutine.</returns>
        public static IEnumerator tween(float start, float end, float duration, 
                                        Func<float, float, float, float, float> function, 
                                        Action<float> tick)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                tick(function(t, start, end - start, duration));               
                yield return null;
            }
            tick(end);
        }

        /// <summary>
        /// Interpolate a GameObject's position to a new location for a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="newPosition"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator position(GameObject gameObject, Vector3 newPosition, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localPosition;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.LerpUnclamped(start, newPosition, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localPosition = newPosition;
        }

        /// <summary>
        /// Interpolate a GameObject's position a certain amount each step for a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="translation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator translate(GameObject gameObject, Vector3 translation, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localPosition;
            Vector3 end = start + translation;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localPosition = end;
        }

        /// <summary>
        /// Interpolate a GameObject's position a certain amount each step for a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator translate(GameObject gameObject, float X, float Y, float Z, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localPosition;
            Vector3 end = start + new Vector3(X, Y, Z);
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localPosition = end;
        }

        /// <summary>
        /// Interpolate a GameObject's position.x a certain amount each step for a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="translation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator translateX(GameObject gameObject, float translation, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localPosition;
            Vector3 end = start;
            end.x += translation;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localPosition = end;
        }

        /// <summary>
        /// Interpolate a GameObject's position.y a certain amount each step for a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="translation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator translateY(GameObject gameObject, float translation, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localPosition;
            Vector3 end = start;
            end.y += translation;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localPosition = end;
        }

        /// <summary>
        /// Interpolate a GameObject's position.z a certain amount each step for a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="translation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator translateZ(GameObject gameObject, float translation, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localPosition;
            Vector3 end = start;
            end.z += translation;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localPosition = end;
        }

        /// <summary>
        /// Interpolate a GameObject's local scale to a new scale over a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="newScale"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator scale(GameObject gameObject, Vector3 newScale, float duration, Func<float,float,float,float,float> function)
        {
            Vector3 start = gameObject.transform.localScale;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localScale = Vector3.LerpUnclamped(start, newScale, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localScale = newScale;
        }

        /// <summary>
        /// Interpolate a GameObject's local scale to a new scale over a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator scale(GameObject gameObject, float X, float Y, float Z, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localScale;
            Vector3 end = start;
            end.x *= X;
            end.y *= Y;
            end.z *= Z;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localScale = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localScale = end;
        }

        /// <summary>
        /// Interpolate a GameObject's local scale to a new scale over a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="scale"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator scale(GameObject gameObject, float scale, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localScale;
            Vector3 end = start * scale;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localScale = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localScale = end;
        }

        /// <summary>
        /// Interpolate a GameObject's local scale X to a new value over a set duration, using a specific Ease method, and call a tick method each step.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="scale"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator scaleX(GameObject gameObject, float scale, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localScale;
            Vector3 end = start;
            end.x *= scale;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localScale = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localScale = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="scale"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator scaleY(GameObject gameObject, float scale, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localScale;
            Vector3 end = start;
            end.y *= scale;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localScale = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localScale = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="scale"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator scaleZ(GameObject gameObject, float scale, float duration, Func<float, float, float, float, float> function)
        {
            Vector3 start = gameObject.transform.localScale;
            Vector3 end = start;
            end.z *= scale;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localScale = Vector3.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localScale = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="newRotation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator rotation(GameObject gameObject, Quaternion newRotation, float duration, Func<float, float, float, float, float> function)
        {
            Quaternion start = gameObject.transform.localRotation;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localRotation = Quaternion.LerpUnclamped(start, newRotation, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localRotation = newRotation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="euler"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator rotate(GameObject gameObject, Vector3 euler, float duration, Func<float, float, float, float, float> function)
        {
            Quaternion start = gameObject.transform.localRotation;
            Quaternion end = start * Quaternion.Euler(euler);
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localRotation = Quaternion.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localRotation = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator rotate(GameObject gameObject, float X, float Y, float Z, float duration, Func<float, float, float, float, float> function)
        {
            Quaternion start = gameObject.transform.localRotation;
            Quaternion end = start * Quaternion.Euler(X, Y, Z);
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localRotation = Quaternion.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localRotation = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="rotation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator rotateX(GameObject gameObject, float rotation, float duration, Func<float, float, float, float, float> function)
        {
            Quaternion start = gameObject.transform.localRotation;
            Quaternion end = start * Quaternion.Euler(rotation,0,0);
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localRotation = Quaternion.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localRotation = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="rotation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator rotateY(GameObject gameObject, float rotation, float duration, Func<float, float, float, float, float> function)
        {
            Quaternion start = gameObject.transform.localRotation;
            Quaternion end = start * Quaternion.Euler(0, rotation, 0);
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localRotation = Quaternion.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localRotation = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="rotation"></param>
        /// <param name="duration"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator rotateZ(GameObject gameObject, float rotation, float duration, Func<float, float, float, float, float> function)
        {
            Quaternion start = gameObject.transform.localRotation;
            Quaternion end = start * Quaternion.Euler(0, 0, rotation);
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gameObject.transform.localRotation = Quaternion.LerpUnclamped(start, end, function(t, 0.0f, 1.0f, duration));
                yield return null;
            }
            gameObject.transform.localRotation = end;
        }

        #region Classic Easing Functions
        /// <summary>
        /// Linear-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float linearIn(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }

        /// <summary>
        /// Linear-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float linearOut(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }

        /// <summary>
        /// Quadratic-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float quadIn(float t, float b, float c, float d)
        {
            t /= d;
            return c * t * t + b;
        }

        /// <summary>
        /// Quadratic-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float quadOut(float t, float b, float c, float d)
        {
            t /= d;
            return -c * t * (t - 2) + b;
        }

        /// <summary>
        /// Quadratic-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float quadInOut(float t, float b, float c, float d)
        {
            t /= d / 2;
            if (t < 1)
                return c / 2 * t * t + b;
            t--;
            return -c / 2 * (t * (t - 2) - 1) + b;
        }

        /// <summary>
        /// Cubic-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float cubicIn(float t, float b, float c, float d)
        {
            t /= d;
            return c * t * t * t + b;
        }

        /// <summary>
        /// Cubic-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float cubicOut(float t, float b, float c, float d)
        {
            t /= d;
            t--;
            return c * (t * t * t + 1) + b;
        }

        /// <summary>
        /// Cubic-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float cubicInOut(float t, float b, float c, float d)
        {
            t /= d / 2;
            if (t < 1)
                return c / 2 * t * t * t + b;
            t -= 2;
            return c / 2 * (t * t * t + 2) + b;
        }

        /// <summary>
        /// Sine-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float sineIn(float t, float b, float c, float d)
        {
            return -c * Mathf.Cos(t / d * (Mathf.PI / 2)) + c + b;
        }

        /// <summary>
        /// Sine-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float sineOut(float t, float b, float c, float d)
        {
            return c * Mathf.Sin(t / d * (Mathf.PI / 2)) + b;
        }

        /// <summary>
        /// Sine-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float sineInOut(float t, float b, float c, float d)
        {
            return -c / 2 * (Mathf.Cos(Mathf.PI * t / d) - 1) + b;
        }

        /// <summary>
        /// Exponential-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float expIn(float t, float b, float c, float d)
        {
            return (t == 0) ? b : c * Mathf.Pow(2, 10 * (t / d - 1)) + b;
        }

        /// <summary>
        /// Exponential-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float expOut(float t, float b, float c, float d)
        {
            return (t == d) ? b + c : c * (-Mathf.Pow(2, -10 * t / d) + 1) + b;
        }

        /// <summary>
        /// Exponential-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float expInOut(float t, float b, float c, float d)
        {
            if (t == 0) return b;
            if (t == d) return b + c;
            if ((t /= d / 2) < 1) return c / 2 * Mathf.Pow(2, 10 * (t - 1)) + b;
            return c / 2 * (-Mathf.Pow(2, -10 * --t) + 2) + b;
        }

        /// <summary>
        /// Circular-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float circIn(float t, float b, float c, float d)
        {
            return -c * (Mathf.Sqrt(1 - (t /= d) * t) - 1) + b;
        }

        /// <summary>
        /// Circular-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float circOut(float t, float b, float c, float d)
        {
            return c * Mathf.Sqrt(1 - (t = t / d - 1) * t) + b;
        }

        /// <summary>
        /// Circular-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float circInOut(float t, float b, float c, float d)
        {
            if ((t /= d / 2) < 1) return -c / 2 * (Mathf.Sqrt(1 - t * t) - 1) + b;
            return c / 2 * (Mathf.Sqrt(1 - (t -= 2) * t) + 1) + b;
        }

        /// <summary>
        /// Elastic-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float elasticIn(float t, float b, float c, float d)
        {
            float s = 1.70158f; float p = 0; float a = c;
            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (p == 0) p = d * 0.3f;
            if (a < Mathf.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Mathf.PI) * Mathf.Asin(c / a);
            return -(a * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + b;
        }

        /// <summary>
        /// Elastic-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float elasticOut(float t, float b, float c, float d)
        {
            float s = 1.70158f; float p = 0; float a = c;
            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (p == 0) p = d * 0.3f;
            if (a < Mathf.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Mathf.PI) * Mathf.Asin(c / a);
            return a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + c + b;
        }

        /// <summary>
        /// Elastic-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float elasticInOut(float t, float b, float c, float d)
        {
            float s = 1.70158f; float p = 0; float a = c;
            if (t == 0) return b; if ((t /= d / 2) == 2) return b + c; if (p == 0) p = d * (.3f * 1.5f);
            if (a < Mathf.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Mathf.PI) * Mathf.Asin(c / a);
            if (t < 1) return -.5f * (a * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + b;
            return a * Mathf.Pow(2, -10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) * .5f + c + b;
        }

        /// <summary>
        /// Back-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float backIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * ((1.70158f + 1) * t - 1.70158f) + b;
        }

        /// <summary>
        /// Back-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float backOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * ((1.70158f + 1) * t + 1.70158f) + 1) + b;
        }

        /// <summary>
        /// Back-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float backInOut(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
            return c / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
        }

        /// <summary>
        /// Bounce-In easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float bounceIn(float t, float b, float c, float d)
        {
            return c - bounceOut(d - t, 0, c, d) + b;
        }

        /// <summary>
        /// Bounce-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float bounceOut(float t, float b, float c, float d)
        {
            if ((t /= d) < (1 / 2.75))
            {
                return c * (7.5625f * t * t) + b;
            }
            else if (t < (2 / 2.75f))
            {
                return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
            }
            else if (t < (2.5f / 2.75f))
            {
                return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
            }
            else
            {
                return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
            }
        }

        /// <summary>
        /// Bounce-In-Out easing function.
        /// </summary>
        /// <param name="t">The current delta-step of the ease, range between 0 and d.</param>
        /// <param name="b">The starting value.</param>
        /// <param name="c">The delta of the value, i.e. the amount of change to apply to the ease.</param>
        /// <param name="d">The total duration of the ease.</param>
        /// <returns>Returns the interpolated value.</returns>
        public static float bounceInOut(float t, float b, float c, float d)
        {
            if (t < d / 2) return bounceIn(t * 2, 0, c, d) * .5f + b;
            return bounceOut(t * 2 - d, 0, c, d) * .5f + c * .5f + b;
        }
        #endregion

        #region Simple 1D Ease Functions
        /// <summary>
        /// Back-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float backIn(float t)
        {
            return t * t * (2.70158f * t - 1.70158f);
        }

        /// <summary>
        /// Back-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float backOut(float t)
        {
            return 1.0f - backIn(1.0f - t);
        }

        /// <summary>
        /// Back-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float backInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return backIn(t) * 0.5f;
            else
                return 1.0f - backIn(2.0f - t) * 0.5f;
        }

        /// <summary>
        /// Cubic-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float cubicIn(float t)
        {
            return Mathf.Pow(t, 3.0f);
        }

        /// <summary>
        /// Cubic-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float cubicOut(float t)
        {
            return 1.0f - cubicIn(1.0f - t);
        }

        /// <summary>
        /// Cubic-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float cubicInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return cubicIn(t) * 0.5f;
            else
                return 1.0f - cubicIn(2.0f - t) * 0.5f;
        }

        /// <summary>
        /// Quadratic-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float quadIn(float t)
        {
            return t * t;
        }

        /// <summary>
        /// Quadratic-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float quadOut(float t)
        {
            return 1.0f - quadIn(1.0f - t);
        }

        /// <summary>
        /// Quadratic-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float quadInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return quadIn(t) * 0.5f;
            else
                return 1.0f - quadIn(2.0f - t) * 0.5f;
        }

        /// <summary>
        /// Sine-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float sineIn(float t)
        {
            return -1 * Mathf.Cos(t * (Mathf.PI / 2)) + 1;
        }

        /// <summary>
        /// Sine-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float sineOut(float t)
        {
            return 1.0f - sineIn(1.0f - t);
        }

        /// <summary>
        /// Sine-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float sineInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return sineIn(t) * 0.5f;
            else
                return 1.0f - sineIn(2.0f - t) * 0.5f;
        }

        /// <summary>
        /// Linear-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float linearIn(float t)
        {
            return t;
        }

        /// <summary>
        /// Linear-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float linearOut(float t)
        {
            return 1.0f - t;
        }

        /// <summary>
        /// Linear-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float linearInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return linearIn(t) * 0.5f;
            else
                return 1.0f - linearIn(2.0f - t) * 0.5f;
        }

        /// <summary>
        /// Elastic-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float elasticIn(float t)
        {
            return Mathf.Pow(2.0f, (10.0f * (t - 1.0f))) * Mathf.Sin(t * Mathf.PI * 6.5f);
        }

        /// <summary>
        /// Elastic-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float elasticOut(float t)
        {
            return 1.0f - elasticIn(1.0f - t);
        }

        /// <summary>
        /// Elastic-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float elasticInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return elasticIn(t) * 0.5f;
            else
                return 1.0f - elasticIn(2.0f - t) * 0.5f;
        }

        /// <summary>
        /// Exponential-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float expIn(float t)
        {
            if (t == 0.0f)
                return 0.0f;
            return Mathf.Pow(2.0f, 10.0f * (t - 1.0f));
        }

        /// <summary>
        /// Exponential-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float expOut(float t)
        {
            return 1.0f - expIn(1.0f - t);
        }

        /// <summary>
        /// Exponential-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float expInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return expIn(t) * 0.5f;
            else
                return 1.0f - expIn(2.0f - t) * 0.5f;
        }

        /// <summary>
        /// Bounce-In ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float bounceIn(float t)
        {
            if ((1.0f - t) < (1.0f / 2.75f)) return 1.0f - 7.5625f * Mathf.Pow(1.0f - t, 2.0f);
            if ((1.0f - t) < (2.0f / 2.75f)) return 1.0f - (7.5625f * Mathf.Pow(1.0f - t - 1.5f / 2.75f, 2.0f) + 0.75f);
            if ((1.0f - t) < (2.5f / 2.75f)) return 1.0f - (7.5625f * Mathf.Pow(1.0f - t - 2.25f / 2.75f, 2.0f) + 0.9375f);
            return 1.0f - (7.5625f * Mathf.Pow(1.0f - t - 2.625f / 2.75f, 2.0f) + 0.984375f);
        }

        /// <summary>
        /// Bounce-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float bounceOut(float t)
        {
            return 1.0f - bounceIn(1.0f - t);
        }

        /// <summary>
        /// Bounce-In-Out ease function to modify a 1D delta value from 0-to-1.
        /// </summary>
        /// <param name="t">The 0-to-1 range to modify.</param>
        /// <returns>Returns the modified value.</returns>
        static public float bounceInOut(float t)
        {
            t *= 2.0f;
            if (t < 1.0f)
                return bounceIn(t) * 0.5f;
            else
                return 1.0f - bounceIn(2.0f - t) * 0.5f;
        }
        #endregion
    }
}