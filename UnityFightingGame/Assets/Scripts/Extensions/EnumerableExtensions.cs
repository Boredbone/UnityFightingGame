using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.Extensions
{
    public static class EnumelableExtensions
    {
        /// <summary>
        /// 二つのDictionaryを結合
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>
            (this Dictionary<TKey, TValue> first, IEnumerable<KeyValuePair<TKey, TValue>> second)
        {
            if (first == null && second == null)
            {
                return null;
            }
            else if (first == null)
            {
                return second.ToDictionary(x => x.Key, x => x.Value);
            }
            else if (second == null)
            {
                return first;
            }

            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

            foreach (var x in first.Concat(second).Where(x => x.Value != null))
            {
                dictionary[x.Key] = x.Value;//重複していたら上書き
            }

            return dictionary;

            //return first.Concat(second).Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
        }
        

        public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct
        {
            foreach (var item in source)
            {
                return item;
            }
            return null;
        }

        public static T? FirstOrNull<T>
            (this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
            return null;
        }

        /*
        public static int FirstIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int count = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return count;
                }
                count++;
            }
            return -1;
        }*/

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int index = 0;
            foreach (var item in source)
            {
                action(item, index);
                index++;
            }
        }

        public static IEnumerable<Tout> Convert<Tin, Tout>
            (this IEnumerable<Tin> source, Func<Tin, Tout> converter)
        {
            foreach (var item in source)
            {
                yield return converter(item);
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, params T[] second)
        {
            //--- first.Concat(second)って書くとStackOverflowException!!
            return Enumerable.Concat(first, second);
        }

        /// <summary>
        /// 要素を指定の個数ごとにまとめる
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Buffer<T>(this IEnumerable<T> source, int count)
        {
            var result = new List<T>(count);
            foreach (var item in source)
            {
                result.Add(item);
                if (result.Count == count)
                {
                    yield return result;
                    result = new List<T>(count);
                }
            }
            if (result.Count != 0)
            {
                yield return result.ToArray();
            }
        }

        /// <summary>
        /// シーケンスを指定されたサイズのチャンクに分割します.
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> self, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than 0.", "chunkSize");
            }

            while (self.Any())
            {
                yield return self.Take(chunkSize);
                self = self.Skip(chunkSize);
            }
        }

        /// <summary>
        /// 要素とインデックスを格納するクラス
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class IndexedItem<T>
        {
            public T Value { get; private set; }
            public int Index { get; private set; }
            public IndexedItem(T value, int index)
            {
                this.Value = value;
                this.Index = index;
            }
        }

        /// <summary>
        /// インデックスつき要素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<IndexedItem<T>> Indexed<T>(this IEnumerable<T> source)
        {
            return source.Select((x, i) => new IndexedItem<T>(x, i));
        }

        /// <summary>
        /// シーケンス全体を指定回数繰り返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="souce"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> souce, int count)
        {
            return Enumerable.Range(0, count).SelectMany(_ => souce);
        }

        /// <summary>
        /// シーケンスの各要素ごとに指定回数ずつ繰り返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="souce"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> Stretch<T>(this IEnumerable<T> souce, int count)
        {
            return souce.SelectMany(value => Enumerable.Range(0, count).Select(_ => value));
        }



        public static IList<T> Imitate<T>
            (this IList<T> source, IEnumerable<T> reference) where T : class
        {
            return source.Imitate(reference, (x, y) => object.ReferenceEquals(x, y), x => x);
        }

        public static IList<T> Imitate<T>
            (this IList<T> source, IEnumerable<T> reference, Func<T, T, bool> match)
        {
            return source.Imitate(reference, match, x => x);
        }

        public static IList<T1> Imitate<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match, Func<T2, T1> converter)
        {
            source.Absorb(reference, match, converter);
            return source.FilterStrictlyBy(reference, match);

            //source.FilterBy(reference, match);
            //source.AbsorbStrictly(reference, match, converter);
        }

        private static void AbsorbStrictly<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match, Func<T2, T1> converter)
        {

            int startIndex = 0;

            //新しいアイテムを追加

            foreach (var item in reference)
            {
                var length = source.Count;

                if (startIndex >= length)
                {
                    source.Add(converter(item));
                    startIndex = source.Count;
                    continue;
                }

                if (!match(source[startIndex], item))
                {
                    source.Insert(startIndex, converter(item));
                }
                startIndex++;
            }
        }


        public static void Absorb<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match, Func<T2, T1> converter)
        {

            int startIndex = 0;

            //新しいアイテムを追加

            foreach (var item in reference)
            {

                var length = source.Count;
                var existance = false;

                for (int i = startIndex; i < length; i++)
                {
                    var checkItem = source[i];

                    if (match(checkItem, item))
                    {
                        existance = true;
                        startIndex = i + 1;
                        break;
                    }
                }

                if (!existance)
                {
                    if (startIndex >= length)
                    {
                        source.Add(converter(item));
                        startIndex = source.Count;
                    }
                    else
                    {
                        source.Insert(startIndex, converter(item));
                        startIndex++;
                    }
                }
            }
        }

        private static List<T1> FilterStrictlyBy<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match)
        {
            //消えたアイテムの削除

            //int referenceIndex = 0;
            var removedItems = new List<T1>();


            using (var e = reference.GetEnumerator())
            {
                var usable = e.MoveNext();
                var currentReference = e.Current;

                foreach (var item in source)
                {
                    if (!usable || !match(item, currentReference))
                    {
                        removedItems.Add(item);
                    }
                    else
                    {
                        usable = e.MoveNext();
                        currentReference = e.Current;
                    }
                }
            }


            //foreach (var item in source)
            //{
            //    if (referenceIndex >= reference.Count
            //        || !match(item, reference[referenceIndex]))
            //    {
            //        removedItems.Add(item);
            //    }
            //    else
            //    {
            //        referenceIndex++;
            //    }
            //}

            foreach (var di in removedItems)
            {
                source.Remove(di);
            }
            return removedItems;
        }

        public static List<T1> FilterBy<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match)
        {

            //消えたアイテムの削除

            int referenceIndex = 0;

            var removedItems = new List<T1>();
            //var length = reference.Count;

            foreach (var item in source)
            {
                var existingIndex = reference.FindIndex(x => match(item, x), referenceIndex);

                if (existingIndex < 0)
                {
                    removedItems.Add(item);
                }
                else
                {
                    referenceIndex = existingIndex + 1;
                }


                //bool existance = false;
                //
                //for (int i = referenceIndex; i < length; i++)
                //{
                //    var checkItem = reference[i];
                //    if (match(item, checkItem))
                //    {
                //        referenceIndex = i + 1;
                //        existance = true;
                //        break;
                //    }
                //}
                //
                //if (!existance)
                //{
                //    removedItems.Add(item);
                //}
            }


            foreach (var di in removedItems)
            {
                source.Remove(di);
            }
            return removedItems;
        }


        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            return source.FindIndex(match, 0);
        }

        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> match, int startIndex)
        {
            int index = 0;
            foreach (var x in source)
            {
                if (index >= startIndex && match(x))
                {
                    return index;
                }
                index++;
            }
            return -1;
            //source.Where((x,c)=>match(x)&&c>=startIndex)
        }


        public static IEnumerable<double> Integral(this IEnumerable<double> source)
        {
            double sum = 0.0;

            foreach (var item in source)
            {
                sum += item;
                yield return sum;
            }
        }

        public static IEnumerable<int> Integral(this IEnumerable<int> source)
        {
            int sum = 0;

            foreach (var item in source)
            {
                sum += item;
                yield return sum;
            }
        }

        public static IEnumerable<long> Integral(this IEnumerable<long> source)
        {
            long sum = 0;

            foreach (var item in source)
            {
                sum += item;
                yield return sum;
            }
        }

        public static bool SequenceEqual<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, bool> match)
        {
            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            {
                while (e1.MoveNext())
                {
                    if (!(e2.MoveNext() && match(e1.Current, e2.Current))) return false;
                }
                if (e2.MoveNext()) return false;
            }
            return true;
        }

        /*
        public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second)
            where T : IEquatable<T>
        {
            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            {
                while (e1.MoveNext())
                {
                    if (!(e2.MoveNext() && e1.Current.Equals(e2.Current))) return false;
                }
                if (e2.MoveNext()) return false;
            }
            return true;
        }*/

        public static bool ContainsIndex<T>(this IList<T> list, int index)
        {
            if (list == null)
            {
                return false;
            }
            return (index >= 0 && index < list.Count);
        }
        public static bool ContainsIndex<T>(this T[] array, int index)
        {
            if (array == null)
            {
                return false;
            }
            return (index >= 0 && index < array.Length);
        }

        public static T FromIndexOrDefault<T>(this IList<T> list, int index)
        {
            if (list != null && list.ContainsIndex(index))
            {
                return list[index];
            }
            return default(T);
        }

        public static T FromIndexOrDefault<T>(this T[] array, int index)
        {
            if (array != null && array.ContainsIndex(index))
            {
                return array[index];
            }
            return default(T);
        }

        //public static string Join(this IEnumerable<string> list, string separator)
        //{
        //    return string.Join(separator, list);
        //}
        //public static string Join(this IEnumerable<string> list)
        //{
        //    return string.Join("", list);
        //}
    }
}
