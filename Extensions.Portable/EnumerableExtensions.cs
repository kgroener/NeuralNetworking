using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class EnumerableExtensions
    {

        static Random _random = new Random();

        public static IEnumerable<T> GetRandomizedEnumerable<T>(this IEnumerable<T> input, uint size)
        {
            int index = 0;

            while (index++ < size)
            {
                var randomIndex = _random.Next(0, input.Count());
                var randomIndexObject = input.ElementAt(randomIndex);

                yield return randomIndexObject;
            }
        }

        public static T GetRandomElement<T>(this IEnumerable<T> input)
        {
            var index = _random.Next(0, input.Count());

            return input.ElementAt(index);
        }

        public static void MapAction<T>(this IEnumerable<T> first, IEnumerable<T> second, Action<T, T> action)
        {
            if (first.Count() != second.Count())
            {
                throw new ArgumentException("First and second enumerable are not of equal length");
            }

            var enumerator = second.GetEnumerator();
            foreach(var a in first)
            {
                enumerator.MoveNext();
                var b = enumerator.Current;
                action(a, b);
            }
        }

    }
}
