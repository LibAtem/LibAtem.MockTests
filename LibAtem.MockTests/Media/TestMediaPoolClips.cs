using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Commands.Media;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.Util;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<MediaPoolSettingsSetCommand, MediaPoolSettingsGetCommand>("MaxFrames", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                IBMDSwitcherMediaPool pool = GetMediaPool(helper);

                AtemState preState = helper.Helper.BuildLibState();
                int clipCount = preState.MediaPool.Clips.Count;
                uint totalMaxFrames = (uint) preState.MediaPool.Clips.Sum(c => c.MaxFrames);
                Assert.NotEqual(0u, totalMaxFrames);

                uint remainingFrames = totalMaxFrames;
                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    IntPtr ptr = Marshal.AllocHGlobal(sizeof(uint) * clipCount);
                    for (int o = 0; o < clipCount; o++)
                    {
                        uint v = 0;
                        if (o == clipCount - 1)
                        {
                            v = remainingFrames;
                        } else if (remainingFrames != 0)
                        {
                            v = Randomiser.RangeInt(remainingFrames);
                            remainingFrames -= v;
                        }

                        stateBefore.MediaPool.Clips[o].MaxFrames = v;
                        Marshal.WriteInt32(ptr, o * sizeof(uint), (int) v);
                    }

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

                        stateBefore.MediaPool.Clips[(int)index].IsUsed = cmd.IsUsed;
                        stateBefore.MediaPool.Clips[(int)index].Name = cmd.Name;

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }
        /*
        [Fact]
        public void TestSetInvalid()
        {
            AtemMockServerWrapper.Each(_output, _pool, ClearCommandHandler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                IBMDSwitcherClip stills = GetClip(helper);

                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.MediaPool.Clip.Count);
                    MediaPoolClipDescriptionCommand cmd = previousCommands.OfType<MediaPoolClipDescriptionCommand>().Single(c => c.Index == index && c.Bank == MediaPoolFileType.Still);
                    cmd.IsUsed = true;

                    // Set it to true first
                    stateBefore.MediaPool.Clip[(int)index].IsUsed = true;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    // Now set invalid
                    stateBefore.MediaPool.Clip[(int)index].IsUsed = false;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        stills.SetInvalid(index);
                    });
                }
            });
        }
        private static IEnumerable<ICommand> ClearCommandHandler(ImmutableList<ICommand> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolClearStillCommand clearCmd)
            {
                var previous = previousCommands.OfType<MediaPoolClipDescriptionCommand>().Last(a => a.Index == clearCmd.Index && a.Bank == MediaPoolFileType.Still);
                Assert.NotNull(previous);

                previous.IsUsed = false;
                yield return previous;
            }
        }
        */

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
        private static IEnumerable<ICommand> NameCommandHandler(ImmutableList<ICommand> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolSetClipCommand setNameCmd)
            {
                var previous = previousCommands.OfType<MediaPoolClipDescriptionCommand>().Last(a => a.Index == setNameCmd.Index);
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
            AtemMockServerWrapper.Each(_output, _pool, LockCommandHandler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                int clipCount = helper.Helper.BuildLibState().MediaPool.Clips.Count;
                for (int index = 0; index < clipCount; index++)
                {
                    IBMDSwitcherClip stills = GetClip(helper, (uint) index);

                    AtemState stateBefore = helper.Helper.BuildLibState();

                    var cb = new LockCallback();
                    helper.SendAndWaitForChange(stateBefore, () => { stills.Lock(cb); });
                    Assert.True(cb.Wait.WaitOne(2000));

                    helper.Helper.CheckStateChanges(stateBefore);

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(stateBefore, () => { stills.Unlock(cb); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);

                    helper.Helper.CheckStateChanges(stateBefore);
                }
            });
        }
        private static IEnumerable<ICommand> LockCommandHandler(ImmutableList<ICommand> previousCommands, ICommand cmd)
        {
            if (cmd is LockStateSetCommand lockCmd)
            {
                //Assert.True(lockCmd.Locked);

                if (lockCmd.Locked)
                {
                    yield return new LockObtainedCommand
                    {
                        Index = lockCmd.Index
                    };
                }

                yield return new LockStateChangedCommand
                {
                    Index = lockCmd.Index,
                    Locked = lockCmd.Locked
                };
            }
        }

    }
}