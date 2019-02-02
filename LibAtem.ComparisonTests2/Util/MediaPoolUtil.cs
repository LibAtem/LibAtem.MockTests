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

namespace LibAtem.ComparisonTests2.Util
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

        public sealed class SolidClipUploadHelper : IDisposable
        {
            private readonly AtemComparisonHelper _helper;

            public SolidClipUploadHelper(AtemComparisonHelper helper, uint clipIndex, string name, uint frameCount, byte r = 128, byte g = 0, byte b = 0, byte a = 128)
            {
                _helper = helper;

                VideoModeResolution res = helper.SdkState.Settings.VideoMode.GetResolution();
                byte[] frame = SolidColour(res, r, g, b, a);
                byte[][] frames = Enumerable.Repeat(frame, (int)frameCount).ToArray();

                UploadClipSdk(helper.Client, clipIndex, name, res, frames);
                // TODO - check if existing is valid with hash
            }

            public void Dispose()
            {
                // Nothing to do
            }
        }

        public sealed class AudioClipUploadHelper : IDisposable
        {
            private readonly AtemComparisonHelper _helper;

            public AudioClipUploadHelper(AtemComparisonHelper helper, uint clipIndex, string filename, int expectedLength)
            {
                _helper = helper;

                UploadAudioSdk(helper.Client, clipIndex, filename, filename, expectedLength);
                // TODO - check if existing is valid with hash
            }

            public void Dispose()
            {
                // Nothing to do
            }
        }

        public sealed class MediaPlayingHelper : IDisposable
        {
            private readonly AtemComparisonHelper _helper;

            public MediaPlayingHelper(AtemComparisonHelper helper, MediaPlayerId playerId, uint clipIndex)
            {
                _helper = helper;

                // Ensure there is a clip loaded
                // TODO - reenable
                //var frame = SolidColour(Common.VideoModeResolution._1080, 255, 0, 0, 128);
                //UploadClipSdk(helper.Client, 0, "Single", new[] { frame });
                //UploadAudioSdk(helper.Client, 0, "Tone", "tone24bit", 5292);

                VideoSource source = VideoSourceLists.MediaPlayers[(int)playerId];
                AudioSource? source2 = source.GetAudioSource();
                
                if (source2.HasValue)
                {
                    helper.SendCommand(new AudioMixerInputSetCommand
                    {
                        Index = source2.Value,
                        Mask = AudioMixerInputSetCommand.MaskFlags.MixOption | AudioMixerInputSetCommand.MaskFlags.Balance | AudioMixerInputSetCommand.MaskFlags.Gain,
                        MixOption = AudioMixOption.AudioFollowVideo,
                        Balance = 0,
                        Gain = 0,
                    });
                }

                helper.SendCommand(new MediaPlayerSourceSetCommand
                {
                    Index = playerId,
                    Mask = MediaPlayerSourceSetCommand.MaskFlags.SourceType | MediaPlayerSourceSetCommand.MaskFlags.ClipIndex,
                    SourceType = MediaPlayerSource.Clip,
                    ClipIndex = clipIndex,
                },
                new MediaPlayerClipStatusSetCommand
                {
                    Index = playerId,
                    Mask = MediaPlayerClipStatusSetCommand.MaskFlags.Loop | MediaPlayerClipStatusSetCommand.MaskFlags.Playing,
                    Loop = true,
                    Playing = true
                }); // TODO - pfl/solo mp1 or ensure solo is off
            }

            public void Dispose()
            {
                _helper.SendCommand(new MediaPlayerClipStatusSetCommand
                {
                    Index = MediaPlayerId.One,
                    Mask = MediaPlayerClipStatusSetCommand.MaskFlags.Playing,
                    Playing = false
                });
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

        public static void UploadAudioSdk(AtemClientWrapper client, uint index, string name, string filename, int expectedLength)
        {
            byte[] buffer;
            using (var reader = new WaveFileReader("TestFiles/" + filename + ".wav"))
            {
                Assert.Equal(24, reader.WaveFormat.BitsPerSample);
                buffer = new byte[reader.Length];
                int read = reader.Read(buffer, 0, buffer.Length);
                Assert.Equal(buffer.Length, read);
                Assert.Equal(expectedLength, buffer.Length);
            }

            var pool = (IBMDSwitcherMediaPool)client.SdkSwitcher;
            pool.CreateAudio((uint)buffer.Length, out IBMDSwitcherAudio audio);
            Assert.NotNull(audio);

            audio.GetBytes(out IntPtr target);
            Marshal.Copy(buffer, 0, target, buffer.Length);

            pool.GetClip(index, out IBMDSwitcherClip clip);
            Assert.NotNull(clip);
            
            // Wait for lock
            var evt = new AutoResetEvent(false);
            var cb = new LockCallback(() => { evt.Set(); });
            clip.Lock(cb);
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(3)));
            clip.AddCallback(new TransferClipCompleteCallback((fr, au) =>
            {
                Assert.NotNull(au);
                clip.Unlock(cb);
                evt.Set();
            }));

            evt.Reset();
            clip.UploadAudio(name, audio);

            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(5)));
        }


        public static void UploadClipSdk(AtemClientWrapper client, uint index, string name, VideoModeResolution res, byte[][] frames, _BMDSwitcherPixelFormat mode = _BMDSwitcherPixelFormat.bmdSwitcherPixelFormat10BitYUVA)
        {
            var pool = client.SdkSwitcher as IBMDSwitcherMediaPool;
            Assert.NotNull(pool);
            
            pool.GetClip(index, out IBMDSwitcherClip clip);
            Assert.NotNull(clip);

            clip.GetMaxFrameCount(out uint maxFrames);
            if (frames.Length > maxFrames)
                throw new Exception("Too many frames to upload");

            // Wait for lock
            var evt = new AutoResetEvent(false);
            var cb = new LockCallback(() => { evt.Set(); });
            clip.Lock(cb);
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(2)));

            clip.AddCallback(new TransferClipCompleteCallback((fr, au) =>
            {
                if (fr != null) evt.Set();
            }));

            clip.SetInvalid();

            Tuple<uint, uint> dims = res.GetSize();

            uint i = 0;
            foreach (byte[] data in frames)
            {
                pool.CreateFrame(mode, dims.Item1, dims.Item2, out IBMDSwitcherFrame frame);
                Assert.NotNull(frame);

                frame.GetBytes(out IntPtr buffer);
                Marshal.Copy(data, 0, buffer, (int)(dims.Item1 * dims.Item2 * 4));

                evt.Reset();
                clip.UploadFrame(i, frame);
                Assert.True(evt.WaitOne(TimeSpan.FromSeconds(5)));

                i++;
            }

            clip.SetValid(name, (uint)frames.Length);
            clip.Unlock(cb);
        }
    }

}