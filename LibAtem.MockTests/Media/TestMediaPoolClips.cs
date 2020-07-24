using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Commands.DeviceProfile;
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
    public class TestMediaPoolClips
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestMediaPoolClips(ITestOutputHelper output, AtemServerClientPool pool)
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

        private static IBMDSwitcherClip GetClip(AtemMockServerWrapper helper, uint index)
        {
            IBMDSwitcherMediaPool pool = GetMediaPool(helper);

            pool.GetClip(index, out IBMDSwitcherClip clip);
            Assert.NotNull(clip);

            return clip;
        }


        [Fact]
        public void TestClipLengths()
        {
            uint maxFrames = 0;
            Func< Lazy<ImmutableList<ICommand>>, ICommand, IEnumerable<ICommand>> handler2 = (previousCommands, cmd) =>
            {
                var cmd2 = (MediaPoolSettingsSetCommand) cmd;
                long allUsed = cmd2.MaxFrames.Sum(d => d);
                long remaining = Math.Max(maxFrames - allUsed, 0);
                return new List<ICommand>
                {
                    new MediaPoolSettingsGetCommand
                    {
                        MaxFrames = cmd2.MaxFrames,
                        UnassignedFrames = (uint) remaining
                    }
                };
            };
            AtemMockServerWrapper.Each(_output, _pool, handler2, DeviceTestCases.MediaPlayerClips, helper =>
            {
                IBMDSwitcherMediaPool pool = GetMediaPool(helper);

                AtemState preState = helper.Helper.BuildLibState();
                int clipCount = preState.MediaPool.Clips.Count;
                pool.GetFrameTotalForClips(out uint totalMaxFrames);
                // uint totalMaxFrames = (uint) preState.MediaPool.Clips.Sum(c => c.MaxFrames);
                Assert.NotEqual(0u, totalMaxFrames);
                maxFrames = totalMaxFrames;

                for (int i = 0; i < 5; i++)
                {
                    uint remainingFrames = totalMaxFrames;
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    IntPtr ptr = Marshal.AllocHGlobal(sizeof(uint) * clipCount);
                    for (int o = 0; o < clipCount; o++)
                    {
                        uint v = 0;
                        if (o == clipCount - 1 && clipCount <= 2)
                        {
                            v = remainingFrames;
                            remainingFrames = 0;
                        } else if (remainingFrames != 0)
                        {
                            v = Randomiser.RangeInt(remainingFrames / 2);
                            remainingFrames -= v;
                        }

                        stateBefore.MediaPool.Clips[o].MaxFrames = v;
                        stateBefore.MediaPool.Clips[o].Frames = Enumerable.Range(0, (int) v)
                            .Select(i => new MediaPoolState.FrameState()).ToList();
                        Marshal.WriteInt32(ptr, o * sizeof(uint), (int) v);
                    }

                    stateBefore.MediaPool.UnassignedFrames = remainingFrames;

                    helper.SendAndWaitForChange(stateBefore,
                        () =>
                        {
                            unsafe
                            {
                                uint* ptr2 = (uint*) ptr.ToPointer();
                                pool.SetClipMaxFrameCounts((uint) clipCount, ref *ptr2);
                            }
                        });
                }
            });
        }

        [Fact]
        public void TestIsUsed()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MediaPlayerClips, helper =>
            {
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                int clipCount = helper.Helper.BuildLibState().MediaPool.Clips.Count;
                for (int index = 0; index < clipCount; index++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    for (int i = 0; i < 5; i++)
                    {
                        MediaPoolClipDescriptionCommand cmd = previousCommands.OfType<MediaPoolClipDescriptionCommand>().Single(c => c.Index == index);
                        cmd.IsUsed = i % 2 == 0;
                        cmd.Name = cmd.IsUsed ? "abc" : "";

                        stateBefore.MediaPool.Clips[index].IsUsed = cmd.IsUsed;
                        stateBefore.MediaPool.Clips[index].Name = cmd.Name;

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }
        
        [Fact]
        public void TestSetInvalid()
        {
            AtemMockServerWrapper.Each(_output, _pool, ClearCommandHandler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                int clipCount = helper.Helper.BuildLibState().MediaPool.Clips.Count;
                for (int index = 0; index < clipCount; index++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    IBMDSwitcherClip clip = GetClip(helper, (uint) index);

                    for (int i = 0; i < 5; i++)
                    {
                        MediaPoolClipDescriptionCommand cmd = previousCommands.OfType<MediaPoolClipDescriptionCommand>().Single(c => c.Index == index);
                        cmd.IsUsed = true;

                        // Set it to true first
                        stateBefore.MediaPool.Clips[index].IsUsed = true;
                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                        // Now set invalid
                        stateBefore.MediaPool.Clips[index].IsUsed = false;
                        helper.SendAndWaitForChange(stateBefore, () => { clip.SetInvalid(); });
                    }
                }
            });
        }
        private static IEnumerable<ICommand> ClearCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolClearClipCommand clearCmd)
            {
                var previous = previousCommands.Value.OfType<MediaPoolClipDescriptionCommand>().Last(a => a.Index == clearCmd.Index);
                Assert.NotNull(previous);

                previous.IsUsed = false;
                yield return previous;
            }
        }

        [Fact]
        public void TestName()
        {
            AtemMockServerWrapper.Each(_output, _pool, NameCommandHandler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                int clipCount = helper.Helper.BuildLibState().MediaPool.Clips.Count;
                for (int index = 0; index < clipCount; index++)
                {
                    IBMDSwitcherClip clip = GetClip(helper, (uint) index);

                    for (int i = 0; i < 5; i++)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();

                        string name = (Guid.NewGuid().ToString() + Guid.NewGuid()).Substring(0, 44);

                        stateBefore.MediaPool.Clips[index].IsUsed = true;
                        stateBefore.MediaPool.Clips[index].Name = name;

                        helper.SendAndWaitForChange(stateBefore, () => { clip.SetName(name); });
                    }
                }
            });
        }
        private static IEnumerable<ICommand> NameCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolSetClipCommand setNameCmd)
            {
                var previous = previousCommands.Value.OfType<MediaPoolClipDescriptionCommand>().Last(a => a.Index == setNameCmd.Index);
                Assert.NotNull(previous);

                previous.Name = setNameCmd.Name;
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
            AtemMockServerWrapper.Each(_output, _pool, UploadJobWorker.LockCommandHandler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                helper.DisposeSdkClient = true;

                int clipCount = helper.Helper.BuildLibState().MediaPool.Clips.Count;
                for (int index = 0; index < clipCount; index++)
                {
                    IBMDSwitcherClip clip = GetClip(helper, (uint) index);

                    AtemState stateBefore = helper.Helper.BuildLibState();

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { clip.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    helper.Helper.CheckStateChanges(stateBefore);

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(stateBefore, () => { clip.Unlock(cb); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);

                    helper.Helper.CheckStateChanges(stateBefore);
                }
            });
        }

        class TransferCallback : IBMDSwitcherClipCallback
        {
            public ManualResetEvent Wait { get; }
            public _BMDSwitcherMediaPoolEventType Result { get; private set; }
            public IBMDSwitcherFrame Frame { get; private set; }
            public IBMDSwitcherAudio Audio { get; private set; }

            public TransferCallback()
            {
                Wait = new ManualResetEvent(false);
            }


            public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int frameIndex, IBMDSwitcherAudio audio,
                int clipIndex)
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
                        Audio = audio;
                        Wait.Set();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        [Fact]
        public void TestAudioUpload()
        {
            UploadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayerClips, helper =>
            {
                helper.DisposeSdkClient = true;

                var pidCmd = helper.Server.GetParsedDataDump().OfType<ProductIdentifierCommand>().Single();

                IBMDSwitcherMediaPool pool = GetMediaPool(helper);

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Clips.Count);
                    IBMDSwitcherClip clip = GetClip(helper, index);

                    uint sampleCount = 10000;

                    string name = Guid.NewGuid().ToString();
                    worker = new UploadJobWorker(sampleCount * 4, _output,
                        index + 1, 0, DataTransferUploadRequestCommand.TransferMode.Write2, false);

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { clip.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    byte[] bytes = MediaPoolUtil.RandomFrame(sampleCount);
                    pool.CreateAudio((uint) bytes.Length, out IBMDSwitcherAudio frame);
                    MediaPoolUtil.FillSdkAudio(frame, bytes);

                    var clipState = stateBefore.MediaPool.Clips[(int)index];
                    clipState.Audio.IsUsed = true;
                    clipState.Audio.Name = name;

                    var uploadCb = new TransferCallback();
                    clip.AddCallback(uploadCb);
                    clip.UploadAudio(name, frame);

                    helper.HandleUntil(uploadCb.Wait, 5000);
                    Assert.True(uploadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted,
                        uploadCb.Result);

                    // TODO - this needs a better rule that can be properly exposed via the lib
                    byte[] flippedBytes = pidCmd.Model >= ModelId.PS4K
                        ? MediaPoolUtil.FlipAudio(bytes)
                        : bytes;
                    Assert.Equal(BitConverter.ToString(flippedBytes), BitConverter.ToString(worker.Buffer));

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        clip.Unlock(cb);
                    });
                }
            });
        }

        [Fact]
        public void TestAbortingAudioUpload()
        {
            AbortedUploadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayerClips, helper =>
            {
                helper.DisposeSdkClient = true;

                IBMDSwitcherMediaPool pool = GetMediaPool(helper);

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Clips.Count);
                    IBMDSwitcherClip clip = GetClip(helper, index);

                    uint sampleCount = 10000;

                    string name = Guid.NewGuid().ToString();
                    worker = new AbortedUploadJobWorker(_output);

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { clip.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    byte[] bytes = MediaPoolUtil.RandomFrame(sampleCount);
                    pool.CreateAudio((uint)bytes.Length, out IBMDSwitcherAudio frame);
                    MediaPoolUtil.FillSdkAudio(frame, bytes);

                    var uploadCb = new TransferCallback();
                    clip.AddCallback(uploadCb);
                    clip.UploadAudio(name, frame);

                    // Short bit of work before the abort
                    helper.HandleUntil(uploadCb.Wait, 1000);
                    clip.CancelTransfer();

                    helper.HandleUntil(uploadCb.Wait, 1000);
                    Assert.True(uploadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCancelled,
                        uploadCb.Result);

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        clip.Unlock(cb);
                    });
                }
            });
        }

        [Fact]
        public void TestAudioDownload()
        {
            DownloadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayerClips, helper =>
            {
                helper.DisposeSdkClient = true;

                var pidCmd = helper.Server.GetParsedDataDump().OfType<ProductIdentifierCommand>().Single();

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Clips.Count);
                    IBMDSwitcherClip clip = GetClip(helper, index);

                    uint sampleCount = 10000;

                    {
                        var clipState = stateBefore.MediaPool.Clips[(int) index];
                        clipState.Audio.Name = "Some file";
                        clipState.Audio.IsUsed = true;
                        clipState.Audio.Hash = new byte[16];
                        helper.SendFromServerAndWaitForChange(stateBefore, new MediaPoolAudioDescriptionCommand
                        {
                            Name = "Some file",
                            Index = index + 1,
                            IsUsed = true
                        });
                    }
                    stateBefore = helper.Helper.BuildLibState();

                    var bytes = MediaPoolUtil.RandomFrame(sampleCount);
                    worker = new DownloadJobWorker(_output, index + 1, 0, bytes);

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { clip.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    var downloadCb = new TransferCallback();
                    clip.AddCallback(downloadCb);
                    clip.DownloadAudio();

                    helper.HandleUntil(downloadCb.Wait, 5000);
                    Assert.True(downloadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted,
                        downloadCb.Result);

                    Assert.Null(downloadCb.Frame);
                    Assert.NotNull(downloadCb.Audio);
                    Assert.Equal((int) (sampleCount * 4), downloadCb.Audio.GetSize());
                    byte[] sdkBytes = MediaPoolUtil.GetSdkAudioBytes(downloadCb.Audio);

                    // TODO - this needs a better rule that can be properly exposed via the lib
                    byte[] flippedBytes = pidCmd.Model >= ModelId.PS4K
                        ? MediaPoolUtil.FlipAudio(bytes)
                        : bytes;
                    Assert.Equal(BitConverter.ToString(flippedBytes), BitConverter.ToString(sdkBytes));

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        clip.Unlock(cb);
                    });
                }
            });
        }

        [Fact]
        public void TestSetAudioInvalid()
        {
            AtemMockServerWrapper.Each(_output, _pool, ClearAudioCommandHandler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                int clipCount = helper.Helper.BuildLibState().MediaPool.Clips.Count;
                for (int index = 0; index < clipCount; index++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    IBMDSwitcherClip clip = GetClip(helper, (uint)index);

                    for (int i = 0; i < 5; i++)
                    {
                        MediaPoolAudioDescriptionCommand cmd = previousCommands
                            .OfType<MediaPoolAudioDescriptionCommand>().Single(c => c.Index == index + 1);
                        cmd.IsUsed = true;

                        // Set it to true first
                        stateBefore.MediaPool.Clips[index].Audio.IsUsed = true;
                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                        // Now set invalid
                        stateBefore.MediaPool.Clips[index].Audio.IsUsed = false;
                        helper.SendAndWaitForChange(stateBefore, () => { clip.SetAudioInvalid(); });
                    }
                }
            });
        }
        private static IEnumerable<ICommand> ClearAudioCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolClearAudioCommand clearCmd)
            {
                var previous = previousCommands.Value.OfType<MediaPoolAudioDescriptionCommand>()
                    .Last(a => a.Index == clearCmd.Index + 1);
                Assert.NotNull(previous);

                previous.IsUsed = false;
                yield return previous;
            }
        }

        [Fact]
        public void TestClipFrameUpload()
        {
            UploadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayerClips, helper =>
            {
                helper.DisposeSdkClient = true;

                IBMDSwitcherMediaPool pool = GetMediaPool(helper);

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Tuple<uint, uint> resolution = stateBefore.Settings.VideoMode.GetResolution().GetSize();
                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Clips.Count);
                    uint frameIndex = Randomiser.RangeInt(stateBefore.MediaPool.Clips[(int) index].MaxFrames);
                    IBMDSwitcherClip clip = GetClip(helper, index);

                    worker = new UploadJobWorker(resolution.Item1 * resolution.Item2 * 4, _output,
                        index + 1, frameIndex, DataTransferUploadRequestCommand.TransferMode.Write);

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { clip.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    pool.CreateFrame(_BMDSwitcherPixelFormat.bmdSwitcherPixelFormat10BitYUVA, resolution.Item1,
                        resolution.Item2, out IBMDSwitcherFrame frame);
                    byte[] bytes = MediaPoolUtil.SolidColour(resolution.Item1 * resolution.Item2, 100, 0, 0, 255);
                    MediaPoolUtil.FillSdkFrame(frame, bytes);

                    var clipState = stateBefore.MediaPool.Clips[(int)index];
                    clipState.Frames[(int) frameIndex].IsUsed = true;
                    //clipState.Audio.Name = name;

                    var uploadCb = new TransferCallback();
                    clip.AddCallback(uploadCb);
                    clip.UploadFrame(frameIndex, frame);

                    helper.HandleUntil(uploadCb.Wait, 5000);
                    Assert.True(uploadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted,
                        uploadCb.Result);
                    Assert.Equal(BitConverter.ToString(bytes), BitConverter.ToString(worker.Buffer));

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        clip.Unlock(cb);
                    });
                }
            });
        }

        [Fact]
        public void TestClipSetValid()
        {
            AtemMockServerWrapper.Each(_output, _pool, SetValidCommandHandler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Clips.Count);
                    IBMDSwitcherClip clip = GetClip(helper, index);

                    string name = Guid.NewGuid().ToString();
                    uint frameCount = Randomiser.RangeInt(5) + 3;

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { clip.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    var clipState = stateBefore.MediaPool.Clips[(int)index];
                    clipState.IsUsed = true;
                    clipState.Name = name;
                    clipState.FrameCount = frameCount;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        clip.SetValid(name, frameCount);
                    });

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        clip.Unlock(cb);
                    });
                }
            });
        }
        private static IEnumerable<ICommand> SetValidCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            var lockRes = UploadJobWorker.LockCommandHandler(previousCommands, cmd).ToList();
            if (lockRes.Any())
            {
                foreach (ICommand cmd2 in lockRes)
                    yield return cmd2;
                yield break;
            }

            if (cmd is MediaPoolSetClipCommand clipCmd)
            {
                yield return new MediaPoolClipDescriptionCommand
                {
                    Index = clipCmd.Index,
                    IsUsed = true,
                    Name = clipCmd.Name,
                    FrameCount = clipCmd.Frames,
                };
            }
        }


        [Fact]
        public void TestClipFrameDownload()
        {
            DownloadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MediaPlayerClips, helper =>
            {
                helper.DisposeSdkClient = true;

                IBMDSwitcherMediaPool pool = GetMediaPool(helper);

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Tuple<uint, uint> resolution = stateBefore.Settings.VideoMode.GetResolution().GetSize();
                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Clips.Count);
                    uint frameIndex = Randomiser.RangeInt(stateBefore.MediaPool.Clips[(int)index].MaxFrames);
                    IBMDSwitcherClip clip = GetClip(helper, index);

                    {
                        var frameState = stateBefore.MediaPool.Clips[(int) index].Frames[(int) frameIndex];
                        frameState.IsUsed = true;
                        frameState.Hash = new byte[16];
                        helper.SendFromServerAndWaitForChange(stateBefore, new MediaPoolFrameDescriptionCommand
                        {
                            Bank = (MediaPoolFileType) index + 1,
                            Filename = "",
                            Index = frameIndex,
                            IsUsed = true
                        });
                    }
                    stateBefore = helper.Helper.BuildLibState();

                    byte[] bytes = new byte[resolution.Item1 * resolution.Item2 * 4];
                    worker = new DownloadJobWorker(_output, index + 1, frameIndex, FrameEncodingUtil.EncodeRLE(bytes));

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { clip.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));
                    
                    var downloadCb = new TransferCallback();
                    clip.AddCallback(downloadCb);
                    clip.DownloadFrame(frameIndex);

                    helper.HandleUntil(downloadCb.Wait, 5000);
                    Assert.True(downloadCb.Wait.WaitOne(500));

                    Assert.Equal(_BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted,
                        downloadCb.Result);
                    Assert.NotNull(downloadCb.Frame);
                    Assert.Null(downloadCb.Audio);

                    byte[] sdkBytes = MediaPoolUtil.GetSdkFrameBytes(downloadCb.Frame);
                    Assert.Equal(BitConverter.ToString(bytes), BitConverter.ToString(sdkBytes));

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        clip.Unlock(cb);
                    });
                }
            });
        }
    }
}