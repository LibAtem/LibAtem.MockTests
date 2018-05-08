using System;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Common;
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

        public class LockCallback : IBMDSwitcherLockCallback
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