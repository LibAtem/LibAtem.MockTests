using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LibAtem.State;

namespace LibAtem.MockTests.Util
{
    public static class Randomiser
    {
        private static Random _random;

        static Randomiser()
        {
            _random = new Random();
        }

        public static int RangeInt(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static uint RangeInt(uint max)
        {
            return (uint) _random.Next((int) max);
        }

        public static List<T> FlagComponents<T>(params T[] omit)
        {
            var rand = new Random();
            var vals = Enum.GetValues(typeof(T)).OfType<T>().Except(omit).ToList();
            if (vals.Count == 0) throw new ArgumentOutOfRangeException("No enum values");

            int count = rand.Next(1, vals.Count);
            return SelectionOfGroup(vals, count).ToList();
        }

        public static T EnumValue<T>(params T[] omit)
        {
            var rand = new Random();
            var vals = Enum.GetValues(typeof(T)).OfType<T>().Except(omit).ToArray();
            if (vals.Length == 0) throw new ArgumentOutOfRangeException("No enum values");
            var ind = rand.Next(0, vals.Length);
            return vals[ind];
        }
        
        public static double Range(double min = -100, double max = 6, double rounding = 100)
        {
            double scale = max - min;
            return RoundTo(_random.NextDouble() * scale + min, rounding);
        }

        public static double RoundTo(double value, double rounding)
        {
            return Math.Round(value * rounding) / rounding;
        }
        public static double FloorTo(double value, double rounding)
        {
            return Math.Floor(value * rounding) / rounding;
        }

        public static IEnumerable<T> SelectionOfGroup<T>(List<T> options, int randomCount = 3)
        {
            var rand = new Random();

            for (int i = 0; i < randomCount && options.Count > 0; i++)
            {
                int ind = rand.Next(0, options.Count);
                yield return options[ind];
                options.RemoveAt(ind);
            }
        }
        public static double[] ConvertDoubleArray(uint len, ref double vals)
        {
            return ConvertSdkArray<double>(len, ref vals);
        }

        public static T[] ConvertSdkArray<T>(uint len, ref T vals)
        {
            var res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = Unsafe.Add(ref vals, i);
            }
            return res;
        }

        public static IntPtr BuildSdkArray(int valueSize, int[] vals)
        {
            IntPtr ptr = Marshal.AllocHGlobal(valueSize * vals.Length);
            for (int o = 0; o < vals.Length; o++)
            {
                switch (valueSize)
                {
                    case 1:
                        Marshal.WriteByte(ptr, o * valueSize, (byte)((sbyte)vals[o]));
                        break;
                    case 2:
                        Marshal.WriteInt16(ptr, o * valueSize, (short)vals[o]);
                        break;
                    default:
                        Marshal.WriteInt32(ptr, o * valueSize, vals[o]);
                        break;
                }
            }
            return ptr;
        }

        public static IntPtr BuildSdkArray(long[] vals)
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(long) * vals.Length);
            for (int o = 0; o < vals.Length; o++)
            {
                Marshal.WriteInt64(ptr, o * sizeof(long), vals[o]);
            }
            return ptr;
        }

        public static IntPtr BuildSdkArray(double[] vals)
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(double) * vals.Length);
            for (int o = 0; o < vals.Length; o++)
            {
                byte[] v = BitConverter.GetBytes(vals[o]);
                int offset = o * sizeof(double);
                for (int p = 0; p < v.Length; p++)
                    Marshal.WriteByte(ptr, offset + p, v[p]);
            }
            return ptr;
        }

        public static string String(int length)
        {
            string r = Guid.NewGuid().ToString();
            while (r.Length < length) r += Guid.NewGuid();

            return r.Substring(0, length);
        }
    }

}