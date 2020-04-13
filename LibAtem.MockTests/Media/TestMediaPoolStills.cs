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

        private static IBMDSwitcherStills GetStillsPool(AtemMockServerWrapper helper)
        {
            var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMediaPool;
            Assert.NotNull(pool);

            pool.GetStills(out IBMDSwitcherStills stills);
            Assert.NotNull(stills);

            return stills;
        }

        [Fact]
        public void TestIsUsed()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MediaPlayer, helper =>
            {
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 10; i++)
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
                var previous = previousCommands.OfType<MediaPoolFrameDescriptionCommand>().Last(a => a.Index == clearCmd.Index && a.Bank == MediaPoolFileType.Still);
                Assert.NotNull(previous);

                previous.IsUsed = false;
                yield return previous;
            }
        }

        [Fact]
        public void TestHash()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MediaPlayer, helper =>
            {
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 10; i++)
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

                for (int i = 0; i < 10; i++)
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
        private static IEnumerable<ICommand> NameCommandHandler(ImmutableList<ICommand> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPoolStillSetFilenameCommand setNameCmd)
            {
                var previous = previousCommands.OfType<MediaPoolFrameDescriptionCommand>().Last(a => a.Index == setNameCmd.Index && a.Bank == MediaPoolFileType.Still);
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
            AtemMockServerWrapper.Each(_output, _pool, LockCommandHandler, DeviceTestCases.MediaPlayer, helper =>
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
    }
}