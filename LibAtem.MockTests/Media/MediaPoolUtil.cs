using System;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Common;

namespace LibAtem.MockTests.Media
{
    internal static class MediaPoolUtil
    {
        public static byte[] RandomFrame(uint pixels)
        {
            var r = new Random();
            byte[] b = new byte[pixels * 4];
            r.NextBytes(b);
            return b;
        }

        public static byte[] SolidColour(uint pixels, byte r, byte g, byte b, byte a)
        {
            byte[] res = new byte[pixels * 4];
            for (int i = 0; i < res.Length; i += 4)
            {
                res[i] = r;
                res[i + 1] = g;
                res[i + 2] = b;
                res[i + 3] = a;
            }
            return res;
        }

        public static void FillSdkFrame(IBMDSwitcherFrame frame, byte[] bytes)
        {
            frame.GetBytes(out IntPtr buffer);
            Marshal.Copy(bytes, 0, buffer, bytes.Length);
        }

        public static void FillSdkAudio(IBMDSwitcherAudio frame, byte[] bytes)
        {
            frame.GetBytes(out IntPtr buffer);
            Marshal.Copy(bytes, 0, buffer, bytes.Length);
        }

        public static byte[] GetSdkFrameBytes(IBMDSwitcherFrame frame)
        {
            byte[] res = new byte[frame.GetWidth() * frame.GetHeight() * 4];
            frame.GetBytes(out IntPtr buffer);
            Marshal.Copy(buffer, res, 0, res.Length);
            return res;
        }

        public static byte[] GetSdkAudioBytes(IBMDSwitcherAudio frame)
        {
            byte[] res = new byte[frame.GetSize()];
            frame.GetBytes(out IntPtr buffer);
            Marshal.Copy(buffer, res, 0, res.Length);
            return res;
        }
        public static byte[] FlipAudio(byte[] data)
        {
            var res = new byte[data.Length];
            for (int i = 0; i < data.Length; i += 4)
            {
                res[i] = data[i + 3];
                res[i + 1] = data[i + 2];
                res[i + 2] = data[i + 1];
                res[i + 3] = data[i];
            }
            return res;
        }
    }
}