using System;
using System.Runtime.InteropServices;

namespace LibAtem.ComparisonTests.State.SDK
{
    public static class AtemSDKConverter
    {
        public delegate void CastGetter(ref Guid itId, out IntPtr ptr);

        public static T CastSdk<T>(CastGetter getter)
        {
            Guid itId = typeof(T).GUID;
            getter(ref itId, out IntPtr itPtr);
            return (T)Marshal.GetObjectForIUnknown(itPtr);
        }
    }
}
