using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;
using LibAtem.Commands.Settings;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestDisplayClock
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestDisplayClock(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("Enabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    bool setEnabled = i % 2 == 0;

                    stateBefore.DisplayClock.Properties.Enabled = setEnabled;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetEnabled(setEnabled ? 1 : 0);
                    });
                }

            });
        }

        [Fact]
        public void TestOpacity()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("Opacity");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint opacity = Randomiser.RangeInt(100);

                    stateBefore.DisplayClock.Properties.Opacity = opacity;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetOpacity((ushort)opacity);
                    });
                }

            });
        }

        [Fact]
        public void TestSize()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("Size");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint size = Randomiser.RangeInt(100);

                    stateBefore.DisplayClock.Properties.Size = size;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetSize((ushort)size);
                    });
                }

            });
        }

        [Fact]
        public void TestPositionX()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("PositionX");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double pos = Randomiser.Range(-16, 16);

                    stateBefore.DisplayClock.Properties.PositionX = pos;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetPositionX(pos);
                    });
                }

            });
        }

        [Fact]
        public void TestPositionY()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("PositionY");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double pos = Randomiser.Range(-9, 9);

                    stateBefore.DisplayClock.Properties.PositionY = pos;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetPositionY(pos);
                    });
                }

            });
        }

        [Fact]
        public void TestAutoHide()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("AutoHide");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    bool setAutoHide = i % 2 == 0;

                    stateBefore.DisplayClock.Properties.AutoHide = setAutoHide;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetAutoHide(setAutoHide ? 1 : 0);
                    });
                }

            });
        }

        [Fact]
        public void TestClockMode()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("ClockMode");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    DisplayClockClockMode mode = Randomiser.EnumValue<DisplayClockClockMode>();

                    stateBefore.DisplayClock.Properties.ClockMode = mode;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetClockMode(AtemEnumMaps.DisplayClockModeMap[mode]);
                    });
                }

            });
        }

        [Fact]
        public void TestStartFrom()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayClockPropertiesSetCommand, DisplayClockPropertiesGetCommand>("StartFrom");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint hours = (uint)Randomiser.RangeInt(1, 20);
                    uint minutes = (uint)Randomiser.RangeInt(1, 59);
                    uint seconds = (uint)Randomiser.RangeInt(1, 59);
                    uint frames = (uint)Randomiser.RangeInt(1, 20);
                    var time = new HyperDeckTime
                    { Hour = hours, Minute = minutes, Second = seconds, Frame = frames };

                    stateBefore.DisplayClock.Properties.StartFrom = time;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetStartFrom((byte)hours, (byte)minutes, (byte)seconds, (byte)frames);
                    });
                }

            });
        }

        [Fact]
        public void TestClockState()
        {
            AtemMockServerWrapper.Each(_output, _pool, ClockStateCommandHandler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                Assert.Equal(DisplayClockClockState.Reset, stateBefore.DisplayClock.Properties.ClockState);

                stateBefore.DisplayClock.Properties.ClockState = DisplayClockClockState.Running;
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    displayClock.Start();
                });

                stateBefore.DisplayClock.Properties.ClockState = DisplayClockClockState.Stopped;
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    displayClock.Stop();
                });

                stateBefore.DisplayClock.Properties.ClockState = DisplayClockClockState.Reset;
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    displayClock.Reset();
                });
            });
        }
        private static IEnumerable<ICommand> ClockStateCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is DisplayClockStateSetCommand stateCmd)
            {
                var previous = previousCommands.Value.OfType<DisplayClockPropertiesGetCommand>().Last(); // TODO Id?
                Assert.NotNull(previous);

                previous.ClockState = stateCmd.State;

                yield return previous;
            }
        }

        [Fact]
        public void TestStartFromFrames()
        {
            AtemMockServerWrapper.Each(_output, _pool, StartFromFramesCommandHandler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();
               
                uint frameRate = (uint)Math.Round(stateBefore.Settings.VideoMode.GetRate());

                for (int i = 0; i < 5; i++)
                {
                    uint hours = (uint)Randomiser.RangeInt(1, 20);
                    uint minutes = (uint)Randomiser.RangeInt(1, 59);
                    uint seconds = (uint)Randomiser.RangeInt(1, 59);
                    uint frames = (uint)Randomiser.RangeInt(1, 20);
                    var time = new HyperDeckTime
                    { Hour = hours, Minute = minutes, Second = seconds, Frame = frames };
                    uint totalFrames = (hours * 3600 + minutes * 60 + seconds) * frameRate + frames;

                    stateBefore.DisplayClock.Properties.StartFrom = time;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetStartFromFrames(totalFrames);
                    });
                }

            });
        }
        private static IEnumerable<ICommand> StartFromFramesCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is DisplayClockPropertiesSetCommand propsCmd)
            {
                var modeCmd = previousCommands.Value.OfType<VideoModeGetCommand>().Last();
                Assert.NotNull(modeCmd);

                var previous = previousCommands.Value.OfType<DisplayClockPropertiesGetCommand>().Last(); // TODO Id?
                Assert.NotNull(previous);

                uint frameRate = (uint)Math.Round(modeCmd.VideoMode.GetRate());
                previous.StartFrom = new HyperDeckTime()
                {
                    Hour = propsCmd.StartFromFrames / (3600 * frameRate),
                    Minute = (propsCmd.StartFromFrames / (60 * frameRate)) % 60,
                    Second = (propsCmd.StartFromFrames / frameRate) % 60,
                    Frame = propsCmd.StartFromFrames % frameRate,
                };

                yield return previous;
            }
        }

        [Fact]
        public void TestRequestTime()
        {
            var handler = CommandGenerator.MatchCommand(new DisplayClockRequestTimeCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                helper.SendAndWaitForChange(null, () =>
                {
                    displayClock.RequestTime();
                });
            });
        }

        [Fact]
        public void TestUpdateTime()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.DisplayClock, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayClock(helper);
                Assert.NotNull(displayClock);

                AtemState expectedState = helper.Helper.BuildLibState();

                var testCmd = new DisplayClockCurrentTimeCommand()
                {
                    Time = new HyperDeckTime
                    {
                        Hour = (uint)Randomiser.RangeInt(1, 20),
                        Minute = (uint)Randomiser.RangeInt(1, 59),
                        Second = (uint)Randomiser.RangeInt(1, 59),
                        Frame = (uint)Randomiser.RangeInt(1, 20),
                    },
                };

                expectedState.DisplayClock.CurrentTime = testCmd.Time;

                helper.SendAndWaitForChange(expectedState, () =>
                {
                    helper.Server.SendCommands(testCmd);
                });
            });
        }

        private static IBMDSwitcherDisplayClock GetDisplayClock(AtemMockServerWrapper helper)
        {
            Dictionary<VideoSource, IBMDSwitcherInputAux> allAuxes = helper.GetSdkInputsOfType<IBMDSwitcherInputAux>();
            foreach (KeyValuePair<VideoSource, IBMDSwitcherInputAux> aux in allAuxes)
            {
                if (aux.Value is IBMDSwitcherDisplayClock dc)
                    return dc;
            }

            return null;
        }
    }
}