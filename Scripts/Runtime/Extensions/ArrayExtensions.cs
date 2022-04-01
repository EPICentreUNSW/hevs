using System;
using System.Collections.Generic;
using System.Linq;

namespace HEVS.Extensions {
    /// <summary>
    /// Extension methods for <see cref="Array"/>.
    /// </summary>
    public static class ArrayExtensions {
        /// <summary>
        /// Returns the shape of the array as an array of <see cref="int"/>.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int[] GetShape(this Array self) {
            return Enumerable.Range(0, self.Rank).Select(i => self.GetLength(i)).ToArray();
        }

        /// <summary>
        /// Returns a sequence of all valid indices for the array.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<int[]> GetIndices(this Array self) {
            return GetShape(self).Select(i => Enumerable.Range(0, i)).CartesianProduct().Select(i => i.ToArray());
        }
    }
}
