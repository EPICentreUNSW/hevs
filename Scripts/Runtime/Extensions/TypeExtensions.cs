using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HEVS.Extensions {
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions {
        /// <summary>
        /// Runtime equivalent of <c>default(T)</c>.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type self) {
            if (self.IsValueType) {
                return Activator.CreateInstance(self);
            }

            return null;
        }

        /// <summary>
        /// Gets the size of the Type in bytes.
        /// </summary>
        /// <param name="self">The Type to query.</param>
        /// <returns>The size of the queried Type.</returns>
        public static int GetSize(this Type self) {
            return Marshal.SizeOf(self);
        }

        private static bool IsUnmanagedImpl(this Type self) {
            var result = false;
            if (self.IsPrimitive || self.IsPointer || self.IsEnum) {
                result = true;
            } else if (self.IsGenericType || !self.IsValueType) {
                result = false;
            } else {
                result = self.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).All(x => x.FieldType.IsUnManaged());
            }
            return result;
        }

        private static readonly Dictionary<Type, bool> _isUnManagedCache = new Dictionary<Type, bool>();

        /// <summary>
        /// Queries if a Type is unmanaged.
        /// </summary>
        /// <param name="self">The Type to query.</param>
        /// <returns>Returns true if the queried Type is unmanaged.</returns>
        public static bool IsUnManaged(this Type self) {
            if (_isUnManagedCache.ContainsKey(self))
                return _isUnManagedCache[self];

            var result = IsUnmanagedImpl(self);
            _isUnManagedCache.Add(self, result);
            return result;
        }
    }
}
