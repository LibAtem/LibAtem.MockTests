using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibAtem.ComparisonTests.State.SDK
{
    public static class AtemSDKConverter
    {
        public delegate void CastGetter(ref Guid itId, out IntPtr ptr);
        public delegate void IteratorNext<T>(out T val);

        public static T CastSdk<T>(CastGetter getter)
        {
            Guid itId = typeof(T).GUID;
            getter(ref itId, out IntPtr itPtr);
            return (T)Marshal.GetObjectForIUnknown(itPtr);
        }

        public static void Iterate<T>(IteratorNext<T> next, Action<T, int> fnc)
        {
            int i = 0;
            for (next(out var val); val != null; next(out val))
            {
                fnc(val, i++);
            }
        }

        public static List<Tv> IterateList<T, Tv>(IteratorNext<T> next, Func<T, int, Tv> fnc)
        {
            var res = new List<Tv>();
            int i = 0;
            for (next(out var val); val != null; next(out val))
            {
                res.Add(fnc(val, i++));
            }

            return res;
        }

    }
}
