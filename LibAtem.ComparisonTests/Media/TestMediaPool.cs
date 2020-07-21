using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Commands.Media;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.Net;
using LibAtem.Net.DataTransfer;
using LibAtem.Util.Media;
using NAudio.Wave;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Media
{
    [Collection("Client")]
    public class TestMediaPool
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestMediaPool(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        [Fact]
        public void TestClipUploadHD()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                helper.EnsureVideoMode(VideoMode.P1080i50);

                const uint clipIndex = 1;
                const uint frameCount = 52;
                string name = Guid.NewGuid().ToString();

                var timer = Stopwatch.StartNew();
                List<AtemFrame> frames = new List<AtemFrame>();
                for (int i = 0; i < frameCount; i++)
                {
                    byte col = (byte)(i * 255 / frameCount);
                    byte[] b = MediaPoolUtil.SolidColour(VideoModeResolution._1080, col, col, col, 255);
                    frames.Add(AtemFrame.FromRGBA("", b, ColourSpace.BT709));
                }
                _output.WriteLine("Elapsed frame gen: {0}", timer.ElapsedMilliseconds);


                timer.Restart();
                foreach (var fr in frames)
                    fr.GetRLEEncodedYCbCr();
                _output.WriteLine("Test RLE duration: {0}", timer.ElapsedMilliseconds);

                timer.Restart();
                AutoResetEvent evt = new AutoResetEvent(false);
                bool success = false;
                _client.Client.DataTransfer.QueueJob(new UploadMediaClipJob(clipIndex, name, frames, res =>
                {
                    success = res;
                    evt.Set();
                }));

                Assert.True(evt.WaitOne(TimeSpan.FromSeconds(frameCount)));
                Assert.True(success);
                _output.WriteLine("Elapsed upload: {0}", timer.ElapsedMilliseconds);

                /* TODO - need to track clips in AtemState first
                // Wait and ensure that we got a notify of the new still
                helper.Sleep(2000);
                var cmd = helper.FindWithMatching(new MediaPoolFileCommand { Type = MediaPoolFileType.Still, Index = stillIndex });
                Assert.NotNull(cmd);

                Assert.True(cmd.IsUsed);
                Assert.Equal(name.Substring(0, 20), cmd.Filename);

                Assert.Equal(frame.GetHash(), cmd.Hash);*/
            }
        }

        [Fact]
        public void TestAudioUpload()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                helper.EnsureVideoMode(VideoMode.P1080i50);

                const uint clipIndex = 1;
                string name = Guid.NewGuid().ToString();

                var timer = Stopwatch.StartNew();
                byte[] buffer;
                using (var reader = new WaveFileReader("TestFiles/wipe24bit.wav"))
                {
                    Assert.Equal(24, reader.WaveFormat.BitsPerSample);
                    buffer = new byte[reader.Length];
                    int read = reader.Read(buffer, 0, buffer.Length);
                    Assert.Equal(buffer.Length, read);
                    Assert.Equal(599040, buffer.Length);
                }
                _output.WriteLine("File read: {0}", timer.ElapsedMilliseconds);

                timer.Restart();
                byte[] buffer2 = new byte[buffer.Length];
                for (int i = 0; i < buffer.Length; i += 3)
                {
                    // 24bit samples, change endian
                    buffer2[i] = buffer[i + 2];
                    buffer2[i + 1] = buffer[i + 1];
                    buffer2[i + 2] = buffer[i];
                }
                _output.WriteLine("Swap byte orders: {0}", timer.ElapsedMilliseconds);

                timer.Restart();
                AutoResetEvent evt = new AutoResetEvent(false);
                bool success = false;
                _client.Client.DataTransfer.QueueJob(new UploadMediaAudioJob(clipIndex, name, buffer2, res =>
                {
                    success = res;
                    evt.Set();
                }));

                Assert.True(evt.WaitOne(TimeSpan.FromSeconds(30)));
                Assert.True(success);
                _output.WriteLine("Elapsed upload: {0}", timer.ElapsedMilliseconds);

                /* TODO - need to track clips in AtemState first
                // Wait and ensure that we got a notify of the new still
                helper.Sleep(2000);
                var cmd = helper.FindWithMatching(new MediaPoolFileCommand { Type = MediaPoolFileType.Still, Index = stillIndex });
                Assert.NotNull(cmd);

                Assert.True(cmd.IsUsed);
                Assert.Equal(name.Substring(0, 20), cmd.Filename);

                Assert.Equal(frame.GetHash(), cmd.Hash);*/
            }
        }

        #region Clear

        [Fact]
        public void TestClearStill() // Note: Having software control open causes this to fail
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                helper.EnsureVideoMode(VideoMode.P1080i50);

                const uint stillIndex = 1;
                MediaPoolUtil.EnsureStillExists(_client, VideoModeResolution._1080, stillIndex);
                helper.Sleep();

                Assert.True(helper.LibState.MediaPool.Stills[(int)stillIndex].IsUsed);
                helper.SendCommand(new MediaPoolClearStillCommand { Index = stillIndex });
                helper.Sleep();

                Assert.False(helper.LibState.MediaPool.Stills[(int)stillIndex].IsUsed);
                helper.AssertStatesMatch();

                const uint stillIndex2 = 8;
                MediaPoolUtil.EnsureStillExists(_client, VideoModeResolution._1080, stillIndex2);
                helper.Sleep();

                Assert.True(helper.LibState.MediaPool.Stills[(int)stillIndex2].IsUsed);
                helper.SendCommand(new MediaPoolClearStillCommand { Index = stillIndex2 });
                helper.Sleep();

                Assert.False(helper.LibState.MediaPool.Stills[(int)stillIndex2].IsUsed);
                helper.AssertStatesMatch();
            }
        }

        // TODO - finish
        [Fact]
        public void TestClearClip() // Note: Having software control open causes this to fail
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                /*
                helper.EnsureVideoMode(VideoMode.P1080i50);

                const uint stillIndex = 1;
                MediaPoolUtil.EnsureStillExists(_client, VideoModeResolution._1080, stillIndex);
                helper.Sleep();

                Assert.True(helper.LibState.MediaPool.Stills[stillIndex].IsUsed);
                helper.SendCommand(new MediaPoolClearStillCommand { Index = stillIndex });
                helper.Sleep();

                Assert.False(helper.LibState.MediaPool.Stills[stillIndex].IsUsed);
                helper.AssertStatesMatch();

                const uint stillIndex2 = 8;
                MediaPoolUtil.EnsureStillExists(_client, VideoModeResolution._1080, stillIndex2);
                helper.Sleep();

                Assert.True(helper.LibState.MediaPool.Stills[stillIndex2].IsUsed);
                helper.SendCommand(new MediaPoolClearStillCommand { Index = stillIndex2 });
                helper.Sleep();

                Assert.False(helper.LibState.MediaPool.Stills[stillIndex2].IsUsed);*/
                helper.AssertStatesMatch();
            }
        }

        #endregion Clear

    }
}