using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands.Media;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.Net.DataTransfer;
using LibAtem.Util.Media;
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
        public void TestDownloadStillAsYCbCr()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                const uint stillIndex = 1;

                helper.EnsureVideoMode(VideoMode.P1080i50);

                var sw = Stopwatch.StartNew();
                byte[] rawData = MediaPoolUtil.EnsureStillExists(_client, VideoModeResolution._1080, stillIndex);
                _output.WriteLine("Elapsed={0}", sw.ElapsedMilliseconds);

                sw.Restart();
                IBMDSwitcherFrame frame = DownloadStillSDK(stillIndex);
                _output.WriteLine("Elapsed={0}", sw.ElapsedMilliseconds);
                Assert.NotNull(frame);

                int bytes = frame.GetRowBytes() * frame.GetHeight();
                frame.GetBytes(out IntPtr buffer);
                byte[] sdkYuv = new byte[bytes];
                Marshal.Copy(buffer, sdkYuv, 0, sdkYuv.Length);

                sw.Restart();
                AtemFrame libYuv = DownloadStillLib(stillIndex, VideoModeResolution._1080);
                _output.WriteLine("Elapsed={0}", sw.ElapsedMilliseconds);

                // Ensure downloads match
                Assert.Equal(sdkYuv.Length, libYuv.GetYCbCrData().Length);
                Assert.Equal(sdkYuv, libYuv.GetYCbCrData());

                // Ensure matches upload
                Assert.Equal(sdkYuv.Length, rawData.Length);
                Assert.Equal(sdkYuv, rawData);

                // Ensure frame hash matches
                var pool = _client.SdkSwitcher as IBMDSwitcherMediaPool;
                Assert.NotNull(pool);

                pool.GetStills(out IBMDSwitcherStills stills);
                Assert.NotNull(stills);

                stills.GetHash(stillIndex, out BMDSwitcherHash hash);
                Assert.Equal(hash.data, libYuv.GetHash());
            }
        }

        private byte[] GetHash(byte[] yuvBytes)
        {
            using (MD5 md5Hash = MD5.Create())
                return md5Hash.ComputeHash(yuvBytes);
        }
        
        [Fact]
        public void TestMediaStillUploadSD()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                helper.EnsureVideoMode(VideoMode.P625i50PAL);

                byte[] b = new byte[720 * 576 * 4];
                new Random().NextBytes(b);

                const uint stillIndex = 2;
                string name = Guid.NewGuid().ToString();

                var frame = AtemFrame.FromYCbCr(name, b);

                AutoResetEvent evt = new AutoResetEvent(false);
                bool success = false;
                _client.Client.DataTransfer.QueueJob(new UploadMediaStillJob(stillIndex, frame, res =>
                {
                    success = res;
                    evt.Set();
                }));

                Assert.True(evt.WaitOne(TimeSpan.FromSeconds(30)));
                Assert.True(success);

                // Wait and ensure that we got a notify of the new still
                helper.Sleep(2000);
                var cmd = helper.FindWithMatching(new MediaPoolFileCommand { Type = MediaPoolFileType.Still, Index = stillIndex });
                Assert.NotNull(cmd);

                Assert.True(cmd.IsUsed);
                Assert.Equal(name.Substring(0, 20), cmd.Filename);

                Assert.Equal(GetHash(b), cmd.Hash);
            }
        }

        [Fact]
        public void TestMediaStillUploadHD()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                helper.EnsureVideoMode(VideoMode.P1080i50);

                byte[] b = new byte[1920 * 1080 * 4];
                new Random().NextBytes(b);

                const uint stillIndex = 2;
                string name = Guid.NewGuid().ToString();

                var frame = AtemFrame.FromYCbCr(name, b);

                AutoResetEvent evt = new AutoResetEvent(false);
                bool success = false;
                _client.Client.DataTransfer.QueueJob(new UploadMediaStillJob(stillIndex, frame, res =>
                {
                    success = res;
                    evt.Set();
                }));

                Assert.True(evt.WaitOne(TimeSpan.FromSeconds(30)));
                Assert.True(success);

                // Wait and ensure that we got a notify of the new still
                helper.Sleep(2000);
                var cmd = helper.FindWithMatching(new MediaPoolFileCommand { Type = MediaPoolFileType.Still, Index = stillIndex });
                Assert.NotNull(cmd);

                Assert.True(cmd.IsUsed);
                Assert.Equal(name.Substring(0, 20), cmd.Filename);

                Assert.Equal(GetHash(b), cmd.Hash);
            }
        }

        private IBMDSwitcherFrame DownloadStillSDK(uint index)
        {
            var pool = _client.SdkSwitcher as IBMDSwitcherMediaPool;
            Assert.NotNull(pool);

            pool.GetStills(out IBMDSwitcherStills stills);
            Assert.NotNull(stills);

            // Wait for lock
            var evt = new AutoResetEvent(false);
            var cb = new MediaPoolUtil.LockCallback(() => { evt.Set(); });
            stills.Lock(cb);
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(3)));
            _output.WriteLine("Locked");
            IBMDSwitcherFrame frame = null;
            stills.AddCallback(new MediaPoolUtil.TransferCompleteCallback(fr =>
            {
                stills.Unlock(cb);
                _output.WriteLine("Complete");
                frame = fr;
                evt.Set();
            }));

            _output.WriteLine("Downloading");
            evt.Reset();
            stills.Download(index);

            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(5)));
            Assert.NotNull(frame);
            return frame;
        }

        private AtemFrame DownloadStillLib(uint index, VideoModeResolution resolution)
        {
            var evt = new AutoResetEvent(false);
            AtemFrame res = null;
            _client.Client.DataTransfer.QueueJob(new DownloadMediaStillJob(index, resolution, b =>
            {
                res = b;
                evt.Set();
            }));

            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(10)));

            Assert.NotNull(res);

            return res;
        }

    }
}