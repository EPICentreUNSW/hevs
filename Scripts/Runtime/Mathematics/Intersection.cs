using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// A utility class for simple intersection tests.
    /// </summary>
    public class Intersection
    {
        // treats ray as an infinite ray in both directions
        /// <summary>
        /// Intersect an infinite ray with a sphere.
        /// </summary>
        /// <param name="ray">The intersecting Ray.</param>
        /// <param name="center">The sphere's center.</param>
        /// <param name="radius">The sphere's radius.</param>
        /// <returns>Returns the entry/exit intersection points, or null if no intersection.</returns>
        public static Vector3[] RaySphereIntersection(Ray ray, Vector3 center, float radius)
        {
            // find the two points of intersection
            // the first is the entry point, the second is the exit point
            float t0 = 0, t1 = 0;

            Vector3 L = ray.origin - center;

            float a = Vector3.Dot(ray.direction,ray.direction);
            float b = 2 * Vector3.Dot(ray.direction, L);
            float c = Vector3.Dot(L, L) - radius * radius;

            if (!SolveQuadratic(a, b, c, ref t0, ref t1))
                return null;

            return new Vector3[] { ray.GetPoint(t0), ray.GetPoint(t1) };
        }

        /// <summary>
        /// Intersect an infinite ray with a cylinder.
        /// </summary>
        /// <param name="ray">The intersecting Ray.</param>
        /// <param name="cylBottom">The center top of the cylinder.</param>
        /// <param name="cylTop">The center bottom of the cylinder.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <returns>Returns the entry/exit intersection points, or null if no intersection.</returns>
        public static Vector3[] RayCylinderIntersection(Ray ray, Vector3 cylBottom, Vector3 cylTop, float radius)
        {
            // find the two points of intersection
            // the first is the entry point, the second is the exit point
            float t0 = 0, t1 = 0;

            var ab = cylTop - cylBottom;
            var ao = ray.origin - cylBottom;
            var aoxab = Vector3.Cross(ao, ab);
            var vxab = Vector3.Cross(ray.direction, ab);
            float ab2 = Vector3.Dot(ab, ab);
            float a = Vector3.Dot(vxab, vxab);
            float b = 2 * Vector3.Dot(vxab, aoxab);
            float c = Vector3.Dot(aoxab, aoxab) - radius * radius * ab2;

            if (!SolveQuadratic(a, b, c, ref t0, ref t1))
                return null;

            return new Vector3[]{ ray.GetPoint(t0), ray.GetPoint(t1) };
        }

        static bool SolveQuadratic(float a, float b, float c, ref float x0, ref float x1) 
        { 
            float discr = b * b - 4 * a * c; 
            if (discr < 0)
                return false; 
            else if (discr == 0)
                x0 = x1 = - 0.5f * b / a; 
            else { 
                float q = (b > 0) ?
                    -0.5f * (b + Mathf.Sqrt(discr)) :
                    -0.5f * (b - Mathf.Sqrt(discr));
                x0 = q / a; 
                x1 = c / q; 
            } 

            // should swap
            if (x0 > x1) {
                float temp = x0;
                x0 = x1;
                x1 = temp;
            }
 
            return true; 
        }
    }
}