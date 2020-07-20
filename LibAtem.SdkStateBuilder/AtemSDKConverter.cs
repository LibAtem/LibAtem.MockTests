using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LibAtem.SdkStateBuilder
{
    public static class AtemSDKConverter
    {
        public delegate void CastGetter(ref Guid itId, out IntPtr ptr);
        public delegate void GetFunction<T>(out T val);

        public static T CastSdk<T>(CastGetter getter)
        {
            Guid itId = typeof(T).GUID;
            getter(ref itId, out IntPtr itPtr);
            return (T)Marshal.GetObjectForIUnknown(itPtr);
        }

        public static void Iterate<T>(GetFunction<T> next, Action<T, uint> fnc)
        {
            uint i = 0;
            for (next(out var val); val != null; next(out val))
            {
                fnc(val, i++);
            }
        }

        public static List<Tv> IterateList<T, Tv>(GetFunction<T> next, Func<T, uint, Tv> fnc)
        {
            var res = new List<Tv>();
            uint i = 0;
            for (next(out var val); val != null; next(out val))
            {
                res.Add(fnc(val, i++));
            }

            return res;
        }

        public static List<Tuple<Ts, Tv>> GetFlagsValues<Ts, Tv>(GetFunction<Ts> fcn, IReadOnlyDictionary<Tv, Ts> map) where Ts : Enum
        {
            fcn(out Ts supportedValues);
            List<Ts> components = supportedValues.FindFlagComponents();
            if (components.Count == 0)
                throw new Exception("No Flag Values");

            return components.Select(v => Tuple.Create(v, map.FindByValue(v))).ToList();
        }

    }
}
