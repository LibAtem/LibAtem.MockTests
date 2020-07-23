using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio;
using LibAtem.Commands.Media;
using LibAtem.Commands.MixEffects;
using LibAtem.Common;
using NAudio.Wave;
using Xunit;

namespace LibAtem.ComparisonTests.Util
{
    public static class MediaPoolUtil
    {
        #region Helper Classes

        public sealed class TransferCompleteCallback : IBMDSwitcherStillsCallback
        {
            private readonly Action<IBMDSwitcherFrame> _action;

            public TransferCompleteCallback(Action<IBMDSwitcherFrame> action)
            {
                _action = action;
            }

            public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int index)
            {
                switch (eventType)
                {
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted:
                        _action(frame);
                        break;
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCancelled:
                        _action(null);
                        break;
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferFailed:
                        _action(null);
                        break;
                }
            }
        }

        public sealed class TransferClipCompleteCallback : IBMDSwitcherClipCallback
        {
            private readonly Action<IBMDSwitcherFrame, IBMDSwitcherAudio> _action;

            public TransferClipCompleteCallback(Action<IBMDSwitcherFrame, IBMDSwitcherAudio> action)
            {
                _action = action;
            }

            public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int frameIndex, IBMDSwitcherAudio audio, int clipIndex)
            {
                switch (eventType)
                {
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeValidChanged:
                        _action(null, null);
                        break;
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted:
                        _action(frame, audio);
                        break;
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCancelled:
                        _action(null, null);
                        break;
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferFailed:
                        _action(null, null);
                        break;
                }
            }
        }

        public sealed class LockCallback : IBMDSwitcherLockCallback
        {
            private readonly Action action;

            public LockCallback(Action action)
            {
                this.action = action;
            }

            public void Obtained()
            {
                action();
            }
        }


        #endregion Helper Classes

        public static byte[] EnsureStillExists(AtemClientWrapper client, VideoModeResolution size, uint index)
        {
            byte[] data = RandomFrame(size);
            UploadStillSdk(client, index, Guid.NewGuid().ToString(), data, _BMDSwitcherPixelFormat.bmdSwitcherPixelFormat10BitYUVA);
            return data;
        }

        public static byte[] RandomFrame(VideoModeResolution size)
        {
            var r = new Random();
            byte[] b = new byte[size.GetByteCount()];
            r.NextBytes(b);
            return b;
        }

        public static byte[] SolidColour(VideoModeResolution size, byte r, byte g, byte b, byte a)
        {
            byte[] res = new byte[size.GetByteCount()];
            for (int i = 0; i < res.Length; i += 4)
            {
                res[i] = r;
                res[i + 1] = g;
                res[i + 2] = b;
                res[i + 3] = a;
            }
            return res;
        }

        public static void UploadStillSdk(AtemClientWrapper client, uint index, string name, byte[] data, _BMDSwitcherPixelFormat mode = _BMDSwitcherPixelFormat.bmdSwitcherPixelFormat10BitYUVA)
        {
            var pool = client.SdkSwitcher as IBMDSwitcherMediaPool;
            Assert.NotNull(pool);

            pool.GetStills(out IBMDSwitcherStills stills);
            Assert.NotNull(stills);

            pool.CreateFrame(mode, 1920, 1080, out IBMDSwitcherFrame frame);
            Assert.NotNull(frame);

            frame.GetBytes(out IntPtr buffer);
            Marshal.Copy(data, 0, buffer, 1920 * 1080 * 4);

            // Wait for lock
            var evt = new AutoResetEvent(false);
            var cb = new LockCallback(() => { evt.Set(); });
            stills.Lock(cb);
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(3)));
            stills.AddCallback(new TransferCompleteCallback(fr =>
            {
                stills.Unlock(cb);
                evt.Set();
            }));

            evt.Reset();
            stills.Upload(index, name, frame);

            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(5)));
        }

    }

}