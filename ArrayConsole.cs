﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace SparkySimpArrayUtils
{
    /// <summary>
    /// Provides console extensions that can read/write collections.
    /// </summary>
    public static class ArrayConsole
    {
        /// <summary>
        /// Represents a stopwatch that measures execution time.
        /// </summary>
        public static Stopwatch ExecutionTimer { get; private set; } = new Stopwatch();
        /// <summary>
        /// Instantiates the execution timer.
        /// </summary>
        static ArrayConsole()
        {
            ExecutionTimer.Start();
        }
        #region Streams
        /// <summary>
        /// Represents standard input.
        /// </summary>
        public static TextReader In
        {
            get
            {
                return Console.In;
            }
            set
            {
                Console.SetIn(value);
            }
        }
        /// <summary>
        /// Represents standard output.
        /// </summary>
        public static TextWriter Out { get; private set; } = Console.Out;
        /// <summary>
        /// Represents standard error.
        /// </summary>
        public static TextWriter Error { get; private set; } = Console.Error;
        #endregion 
        #region Utility functions
        /// <summary>
        /// Prints an exception to the standard error.
        /// </summary>
        /// <param name="excp">The exception to dump.</param>
        /// <param name="toStdOut">Wheter to dump the error to standard out.</param>
        public static void DumpError(Exception excp, bool toStdOut = false, bool toDebugOrTrace = false)
        {
            if (toStdOut)
            {
                Out.WriteLine(excp.ToString());
            }
            if (toDebugOrTrace)
            {
#if DEBUG
                Debug.WriteLine(excp);
#elif TRACE
                Trace.WriteLine(excp);
#endif
            }
            Error.WriteLine(excp.ToString());
        }
        /// <summary>
        /// Read bytes from standad input.
        /// </summary>
        /// <param name="n">Number of bytes to allocate. </param>
        /// <returns>Array of read bytes.</returns>
        public static byte[] ReadBytes(int n)
        {
            byte[] bytes = new byte[n];
            char[] chars = new char[n / sizeof(char)];
            In.ReadBlock(chars, 0, n / sizeof(char));
            Buffer.BlockCopy(chars, 0, bytes, 0, n);

            return bytes;
        }

        /// <summary>
        /// Generates a set of unique random numbers.
        /// </summary>
        /// <param name="count">The amount of numbers to produce</param>
        /// <param name="max">Maximum possible number.</param>
        /// <param name="min">Minimum possible number.</param>
        /// <returns>The set of generated numbers.</returns>
        /// <exception cref="InvalidOperationException">Thrown when number count is not positive, or when the minimum value is lower than the number count. </exception>
        public static int[] GenerateUniqueRandomNumbers(int count, int max = Int32.MaxValue, int min = Int32.MinValue, int? prngSeed = null)
        {
            Random prng = new Random(prngSeed ?? DateTime.UtcNow.Ticks.GetHashCode());
            if (count <= 0) 
               throw new InvalidOperationException(message: "Amount of numbers to generate must be positive.");
            // I'm smelling some mistakes here.
            if (Math.Abs(max - min) < count)
               throw new InvalidOperationException(message: $"It is not possible to produce {count} unique numbers between {min} and {max}");
            int[] numbers = new int[count];
            int generatedNumber;
            for (int i = 0; i < numbers.Length; i++) // Special thanks to Emrah Porgalı...
            {
                if (Array.IndexOf(numbers, generatedNumber = prng.Next(min, max)) == -1)
                    numbers[i] = generatedNumber;
                else i--; // repeat this iteration.
            }
            return numbers;
        }
        #endregion
        #region Writers
        /// <summary>
        /// Writes the given collection to the standard output, appending a newline after each item.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="itemFormat">The format to use with each item.</param>
        /// <param name="ts">The items to write.</param>
        public static void WriteEnumerable<T>(string itemFormat, IEnumerable<T> ts)
        {
            foreach (var item in ts)
            {
                Out.WriteLine(itemFormat, item);
            }
        }
        /// <summary>
        /// Dumps an array to the standard output.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="ts">The array to dump.</param>
        public static void DumpArray<T>(IEnumerable<T> ts)
        {
            Out.Write("[");
            int i = 0;
            foreach (var item in ts)
            {
                Out.Write("{0}", item);
                if (i < (ts.Count() - 1))
                    Out.Write(", ");
                i++;
            }
            Out.WriteLine("]");
        }
        /// <summary>
        /// Dumps a key-value set to standard output.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dict">The dictionary to write.</param>
        public static void DumpDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            Out.WriteLine("{");
            int i = 0;
            foreach (var item in dict)
            {
                if (i < (dict.Count() - 1))
                    Out.WriteLine("\t\"{0}\":{1},", item.Key, item.Value is string str ? $"\"{str}\"" : item.Value.ToString());
                else
                    Out.WriteLine("\t\"{0}\":{1}", item.Key, item.Value is string str ? $"\"{str}\"" : item.Value.ToString());
                i++;
            }
            Out.WriteLine("}");
        }
        #endregion
        #region Reader implementations
        /// <summary>
        /// Reads an array of <see cref="int"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(int[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = int.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="byte"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(byte[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = byte.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="short"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(short[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = short.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="long"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(long[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = long.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="uint"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(uint[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = uint.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="ushort"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(ushort[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = ushort.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="ulong"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(ulong[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = ulong.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="sbyte"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(sbyte[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = sbyte.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="float"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(float[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = float.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="double"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(double[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = double.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        /// <summary>
        /// Reads an array of <see cref="decimal"/>s from standard input.
        /// </summary>
        /// <param name="array">The array to fill in.</param>
        /// <param name="start">The index to start filling in.</param>
        /// <param name="end">The index to end filling in.</param>
        /// <returns>Amount of numbers read.</returns>
        public static int Read(decimal[] array, int start, int end)
        {
            int itemsRead = 0;
            for (int i = start; i < end; i++)
            {
                try
                {
                    array[i] = decimal.Parse(In.ReadLine());
                    itemsRead++;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
            return itemsRead;
        }
        #endregion
    }
}