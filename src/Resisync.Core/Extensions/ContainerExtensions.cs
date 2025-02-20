/*
 * MIT License
 * 
 * Copyright (c) 2025 Julian Albrecht
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Resisync.Core.Extensions;

public static class ContainerExtensions
{
    public static IOrderedEnumerable<TSource> Sort<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool descending) =>
        descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);

    public static int IndexOf<T>(this IList<T> values, Predicate<T> predicate)
    {
        int index = 0;
        foreach (T item in values)
        {
            if (predicate(item))
                return index;
            ++index;
        }
        return -1;
    }

    public static int IndexOf<T>(this IList<T> values, Predicate<T> predicate, out T? value)
    {
        int index = 0;
        foreach (T item in values)
        {
            if (predicate(item))
            {
                value = item;
                return index; 
            }
            ++index;
        }
        value = default;
        return -1;
    }

    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        IEnumerator<T> enumerator = values.GetEnumerator();
        while (enumerator.MoveNext())
            action(enumerator.Current);
    }

    public static void Fill<T>(this IList<T> values, T value)
    {
        for(int i = 0; i < values.Count; ++i) 
            values[i] = value;
    }

    public static void Remove<T>(this IList<T> values, Predicate<T> predicate)
    {
        foreach (var item in values)
        {
            if(predicate(item))
                values.Remove(item);
        }
    }
}
