using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Commands.Media;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.Util.Media;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Media
{
    [Collection("ServerClientPool")]
    public class TestMediaPoolStills
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestMediaPoolStills(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherMediaPool GetMediaPool(AtemMockServerWrapper helper)
        {
            var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMediaPool;
            Assert.NotNull(pool);

            return pool;
        }

        private static IBMDSwitcherStills GetStillsPool(AtemMockServerWrapper helper)
        {
            var pool = GetMediaPool(helper);
            pool.GetStills(out IBMDSwitcherStills stills);
            Assert.NotNull(stills);

            return stills;
        }

        [Fact]
        public void TestClearPool()
        {
            var expectedCmd = new MediaPoolClearAllCommand();
            AtemMockServerWrapper.Each(_output, _pool, CommandGenerator.MatchCommand(expectedCmd), DeviceTestCases.MediaPlayer, helper =>
            {
                IBMDSwitcherMediaPool pool = GetMediaPool(helper);

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        pool.Clear();
                    });
                }
            });
        }

        [Fact]
        public void TestIsUsed()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MediaPlayer, helper =>
            {
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Stills.Count);
                    MediaPoolFrameDescriptionCommand cmd = previousCommands.OfType<MediaPoolFrameDescriptionCommand>().Single(c => c.Index == index && c.Bank == MediaPoolFileType.Still);
                    cmd.IsUsed = i % 2 == 0;
                    cmd.Filename = cmd.IsUsed ? "abc" : "";

                    stateBefore.MediaPool.Stills[(int)index].IsUsed = cmd.IsUsed;
                    stateBefore.MediaPool.Stills[(int)index].Filename = cmd.Filename;

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestSetInvalid()
        {
            AtemMockServerWrapper.Each(_output, _pool, ClearCommandHandler, DeviceTestCases.MediaPlayer, helper =>
            {
                IBMDSwitcherStills stills = GetStillsPool(helper);

                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Stills.Count);
                    MediaPoolFrameDescriptionCommand cmd = previousCommands.OfType<MediaPoolFrameDescriptionCommand>().Single(c => c.Index == index && c.Bank == MediaPoolFileType.Still);
                    cmd.IsUsed = true;

                    // Set it to true first
                    stateBefore.MediaPool.Stills[(int)index].IsUsed = true;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    // Now set invalid
                    stateBefore.MediaPool.Stills[(int)index].IsUsed = false;
                    stateBefore.MediaPool.Stills[(int)index].Filename = "";
                    stateBefore.MediaPool.Stills[(int)index].Hash = new byte[16];
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        stills.SetInvalid(index);
                    });
                }
            });
        }
        private static IEnumerable<ICommand> ClearCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolClearStillCommand clearCmd)
            {
                yield return new MediaPoolFrameDescriptionCommand
                {
                    Index = clearCmd.Index,
                    IsUsed = false,
                    Filename = "",
                    Hash = new byte[16],
                };
            }
        }

        [Fact]
        public void TestHash()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MediaPlayer, helper =>
            {
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Stills.Count);
                    MediaPoolFrameDescriptionCommand cmd = previousCommands.OfType<MediaPoolFrameDescriptionCommand>().Single(c => c.Index == index && c.Bank == MediaPoolFileType.Still);
                    cmd.Hash = Guid.NewGuid().ToByteArray();

                    stateBefore.MediaPool.Stills[(int) index].Hash = cmd.Hash;

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestName()
        {
            AtemMockServerWrapper.Each(_output, _pool, NameCommandHandler, DeviceTestCases.MediaPlayer, helper =>
            {
                IBMDSwitcherStills stills = GetStillsPool(helper);

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Stills.Count);
                    string name = (Guid.NewGuid().ToString() + Guid.NewGuid()).Substring(0, 64);

                    stateBefore.MediaPool.Stills[(int) index].IsUsed = true;
                    stateBefore.MediaPool.Stills[(int) index].Filename = name;

                    helper.SendAndWaitForChange(stateBefore, () => { stills.SetName(index, name); });
                }
            });
        }
        private static IEnumerable<ICommand> NameCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolStillSetFilenameCommand setNameCmd)
            {
                var previous = previousCommands.Value.OfType<MediaPoolFrameDescriptionCommand>().Last(a => a.Index == setNameCmd.Index && a.Bank == MediaPoolFileType.Still);
                Assert.NotNull(previous);

                previous.Filename = setNameCmd.Filename;
                previous.IsUsed = true;
                yield return previous;
            }
        }

        class LockCallback : IBMDSwitcherLockCallback
        {
            public ManualResetEvent Wait { get; }

            public LockCallback()
            {
                Wait = new ManualResetEvent(false);
            }

            public void Obtained()
            {
                Assert.True(Wait.Set());
            }
        }

        [Fact]
        public void TestLockAndUnlock()
        {
            AtemMockServerWrapper.Each(_output, _pool, UploadJobWorker.LockCommandHandler, DeviceTestCases.MediaPlayer, helper =>
            {
                IBMDSwitcherStills stills = GetStillsPool(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                var cb = new LockCallback();
                helper.SendAndWaitForChange(stateBefore, () => { stills.Lock(cb); });
                Assert.True(cb.Wait.WaitOne(2000));

                helper.Helper.CheckStateChanges(stateBefore);

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    stills.Unlock(cb);
                });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);

                helper.Helper.CheckStateChanges(stateBefore);
            });
        }

        [Fact]
        public void TestStillCapture()
        {
            var handler = CommandGenerator.MatchCommand(new MediaPoolCaptureStillCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MediaPlayerStillCapture, helper =>
            {
                IBMDSwitcherStills stills = GetStillsPool(helper);

                IBMDSwitcherStillCapture stillCapture = stills as IBMDSwitcherStillCapture;
                Assert.NotNull(stillCapture);

                stillCapture.IsAvailable(out int available);
                Assert.Equal(1, available);

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(stateBefore, () => { stillCapture.CaptureStill(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        private void DoUpload(int iterations, int timeout, Func<uint, uint, byte[]> frameBytes)
        {
            UploadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayer, helper =>
            {
                helper.DisposeSdkClient = true;

                IBMDSwitcherMediaPool pool = GetMediaPool(helper);
                IBMDSwitcherStills stills = GetStillsPool(helper);

                for (int i = 0; i < iterations; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Tuple<uint, uint> resolution = stateBefore.Settings.VideoMode.GetResolution().GetSize();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Stills.Count);
                    string name = Guid.NewGuid().ToString();
                    worker = new UploadJobWorker(resolution.Item1 * resolution.Item2 * 4, _output,
                        (uint) MediaPoolFileType.Still, index, DataTransferUploadRequestCommand.TransferMode.Write);

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { stills.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    pool.CreateFrame(_BMDSwitcherPixelFormat.bmdSwitcherPixelFormat10BitYUVA, resolution.Item1,
                        resolution.Item2, out IBMDSwitcherFrame frame);
                    byte[] bytes = frameBytes(resolution.Item1, resolution.Item2);
                    if (bytes.Length > 0)
                        MediaPoolUtil.FillSdkFrame(frame, bytes);

                    var stillState = stateBefore.MediaPool.Stills[(int)index];
                    stillState.IsUsed = true;
                    stillState.Filename = name;

                    var uploadCb = new TransferCallback();
                    stills.AddCallback(uploadCb);
                    stills.Upload(index, name, frame);

                    helper.HandleUntil(uploadCb.Wait, timeout);
                    Assert.True(uploadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted,
                        uploadCb.Result);
                    Assert.Equal(BitConverter.ToString(bytes), BitConverter.ToString(worker.Buffer));

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        stills.Unlock(cb);
                    });
                }
            });
        }
        
        [Fact]
        public void TestBlankStillUpload()
        {
            DoUpload(3, 2000, (width, height) => MediaPoolUtil.SolidColour(width * height,
                (byte) Randomiser.Range(50, 200), (byte) Randomiser.Range(50, 200),
                (byte) Randomiser.Range(50, 200), (byte) Randomiser.Range(50, 200)));
        }

        [Fact]
        public void TestGeneratedStillUpload()
        {
            DoUpload(1, 45000, (width, height) => MediaPoolUtil.RandomFrame(width * height));
        }


        [Fact]
        public void TestAbortingStillUpload()
        {
            AbortedUploadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayerStillTransfer, helper =>
            {
                helper.DisposeSdkClient = true;

                IBMDSwitcherMediaPool pool = GetMediaPool(helper);
                IBMDSwitcherStills stills = GetStillsPool(helper);

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Tuple<uint, uint> resolution = stateBefore.Settings.VideoMode.GetResolution().GetSize();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Stills.Count);
                    string name = Guid.NewGuid().ToString();
                    worker = new AbortedUploadJobWorker(_output);

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { stills.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    pool.CreateFrame(_BMDSwitcherPixelFormat.bmdSwitcherPixelFormat10BitYUVA, resolution.Item1,
                        resolution.Item2, out IBMDSwitcherFrame frame);
                    MediaPoolUtil.FillSdkFrame(frame, MediaPoolUtil.RandomFrame(resolution.Item1 * resolution.Item2));

                    var uploadCb = new TransferCallback();
                    stills.AddCallback(uploadCb);
                    stills.Upload(index, name, frame);

                    // Short bit of work before the abort
                    helper.HandleUntil(uploadCb.Wait, 1000);
                    stills.CancelTransfer();

                    helper.HandleUntil(uploadCb.Wait, 1000);
                    Assert.True(uploadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCancelled,
                        uploadCb.Result);

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        stills.Unlock(cb);
                    });
                }
            });
        }

        [Fact]
        public void TestBlankStillDownload()
        {
            DoDownload(3, 3000, (width, height) =>
            {
                var bytes = MediaPoolUtil.SolidColour(width * height,
                    (byte) Randomiser.Range(50, 200), (byte) Randomiser.Range(50, 200),
                    (byte) Randomiser.Range(50, 200), (byte) Randomiser.Range(50, 200));
                return Tuple.Create(bytes, FrameEncodingUtil.EncodeRLE(bytes));
            });
        }

        [Fact]
        public void TestRandomStillDownload()
        {
            DoDownload(1, 180000, (width, height) =>
            {
                var bytes = MediaPoolUtil.RandomFrame(width * height);
                return Tuple.Create(bytes, FrameEncodingUtil.EncodeRLE(bytes));
            });
        }

        private void DoDownload(int iterations, int timeout, Func<uint, uint, Tuple<byte[], byte[]>> rawBytesGen)
        {
            DownloadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayerStillTransfer, helper =>
            {
                helper.DisposeSdkClient = true;

                IBMDSwitcherStills stills = GetStillsPool(helper);

                for (int i = 0; i < iterations; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Tuple<uint, uint> resolution = stateBefore.Settings.VideoMode.GetResolution().GetSize();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Stills.Count);

                    {
                        var stillState = stateBefore.MediaPool.Stills[(int) index];
                        stillState.Filename = "Some file";
                        stillState.IsUsed = true;
                        stillState.Hash = new byte[16];
                        helper.SendFromServerAndWaitForChange(stateBefore, new MediaPoolFrameDescriptionCommand
                        {
                            Bank = MediaPoolFileType.Still,
                            Filename = "Some file",
                            Index = index,
                            IsUsed = true
                        });
                    }
                    stateBefore = helper.Helper.BuildLibState();

                    Tuple<byte[], byte[]> rawBytes = rawBytesGen(resolution.Item1, resolution.Item2);
                    worker = new DownloadJobWorker(_output, (uint) MediaPoolFileType.Still, index,
                        rawBytes.Item2);

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { stills.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    var downloadCb = new TransferCallback();
                    stills.AddCallback(downloadCb);
                    stills.Download(index);

                    helper.HandleUntil(downloadCb.Wait, timeout);
                    Assert.True(downloadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted,
                        downloadCb.Result);

                    Assert.NotNull(downloadCb.Frame);
                    Assert.Equal(_BMDSwitcherPixelFormat.bmdSwitcherPixelFormat10BitYUVA,
                        downloadCb.Frame.GetPixelFormat());
                    Assert.Equal(resolution.Item1, (uint)downloadCb.Frame.GetWidth());
                    Assert.Equal(resolution.Item2, (uint)downloadCb.Frame.GetHeight());
                    byte[] sdkBytes = MediaPoolUtil.GetSdkFrameBytes(downloadCb.Frame);
                    Assert.Equal(BitConverter.ToString(rawBytes.Item1), BitConverter.ToString(sdkBytes));

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        stills.Unlock(cb);
                    });
                }
            });
        }

        class TransferCallback : IBMDSwitcherStillsCallback
        {
            public ManualResetEvent Wait { get; }
            public _BMDSwitcherMediaPoolEventType Result { get; private set; }
            public IBMDSwitcherFrame Frame { get; private set; }

            public TransferCallback()
            {
                Wait = new ManualResetEvent(false);
            }

            public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int index)
            {
                switch (eventType)
                {
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeValidChanged:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeNameChanged:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeHashChanged:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioValidChanged:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioNameChanged:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioHashChanged:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeLockBusy:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeLockIdle:
                        break;
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCancelled:
                    case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferFailed:
                        Result = eventType;
                        Frame = frame;
                        Wait.Set();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

    }
}