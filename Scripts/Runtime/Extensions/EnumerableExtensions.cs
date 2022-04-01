using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace HEVS.Extensions {
    /// <summary>
    /// Extension methods for <see cref="IEnumerable"/> and <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions {
        /// <summary>
        /// Check whether all elements of a sequence of <see cref="bool"/> are true.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool All(this IEnumerable<bool> self) {
            return self.All(x => x);
        }

        /// <summary>
        /// Check whether any element of a sequence of <see cref="bool"/> is true.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool Any(this IEnumerable<bool> self) {
            return self.Any(x => x);
        }

        /// <summary>
        /// Computes the product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int Product(this IEnumerable<int> self) {
            return self.Aggregate(1, (acc, val) => acc * val);
        }

        /// <summary>
        /// Computes the product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long Product(this IEnumerable<long> self) {
            return self.Aggregate(1L, (acc, val) => acc * val);
        }

        /// <summary>
        /// Computes the product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static double Product(this IEnumerable<float> self) {
            return self.Aggregate(1.0f, (acc, val) => acc * val);
        }

        /// <summary>
        /// Computes the product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static double Product(this IEnumerable<double> self) {
            return self.Aggregate(1.0, (acc, val) => acc * val);
        }

        /// <summary>
        /// Cumulatively applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TAccumulate"></typeparam>
        /// <param name="self"></param>
        /// <param name="seed"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<TAccumulate> CumulativeAggregate<TSource, TAccumulate>(this IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) {
            TAccumulate result = seed;
            foreach (var item in self) {
                result = func(result, item);
                yield return result;
            }
        }

        /// <summary>
        /// Computes the cumulative sum of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<int> CumulativeSum(this IEnumerable<int> self) {
            return self.CumulativeAggregate(0, (acc, val) => acc + val);
        }

        /// <summary>
        /// Computes the cumulative sum of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<long> CumulativeSum(this IEnumerable<long> self) {
            return self.CumulativeAggregate(0L, (acc, val) => acc + val);
        }

        /// <summary>
        /// Computes the cumulative sum of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<float> CumulativeSum(this IEnumerable<float> self) {
            return self.CumulativeAggregate(0.0f, (acc, val) => acc + val);
        }

        /// <summary>
        /// Computes the cumulative sum of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<double> CumulativeSum(this IEnumerable<double> self) {
            return self.CumulativeAggregate(0.0, (acc, val) => acc + val);
        }

        /// <summary>
        /// Computes the cumulative product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<int> CumulativeProduct(this IEnumerable<int> self) {
            return self.CumulativeAggregate(1, (acc, val) => acc * val);
        }

        /// <summary>
        /// Computes the cumulative product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<long> CumulativeProduct(this IEnumerable<long> self) {
            return self.CumulativeAggregate(1L, (acc, val) => acc * val);
        }

        /// <summary>
        /// Computes the cumulative product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<float> CumulativeProduct(this IEnumerable<float> self) {
            return self.CumulativeAggregate(1.0f, (acc, val) => acc * val);
        }

        /// <summary>
        /// Computes the cumulative product of a sequence of numeric values.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<double> CumulativeProduct(this IEnumerable<double> self) {
            return self.CumulativeAggregate(1.0, (acc, val) => acc * val);
        }

        /// <summary>
        /// Computes the cartesian product of sequences.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> self) {
            if (self == null) { return null; }

            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return self.Aggregate(emptyProduct, (accumulator, sequence) => accumulator.SelectMany(accseq => sequence, (acc, val) => acc.Append(val)));
        }

        /// <summary>
        /// Creates a <see cref="OrderedDictionary"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static OrderedDictionary ToOrderedDictionary<T, TKey, TValue>(this IEnumerable<T> self, Func<T, TKey> key, Func<T, TValue> value) {
            var result = new OrderedDictionary();
            foreach (var item in self) {
                result.Add(key(item), value(item));
            }
            return result;
         }
    }
}
