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
using LibAtem.Commands.Media;
using LibAtem.Commands.Audio;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestDisplayCounter
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestDisplayCounter(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("Enabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    bool setEnabled = i % 2 == 0;

                    stateBefore.DisplayCounter.Properties.Enabled = setEnabled;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("Opacity");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint opacity = Randomiser.RangeInt(100);

                    stateBefore.DisplayCounter.Properties.Opacity = opacity;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("Size");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint size = Randomiser.RangeInt(100);

                    stateBefore.DisplayCounter.Properties.Size = size;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("PositionX");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double pos = Randomiser.Range(-16, 16);

                    stateBefore.DisplayCounter.Properties.PositionX = pos;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("PositionY");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double pos = Randomiser.Range(-9, 9);

                    stateBefore.DisplayCounter.Properties.PositionY = pos;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("AutoHide");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    bool setAutoHide = i % 2 == 0;

                    stateBefore.DisplayCounter.Properties.AutoHide = setAutoHide;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("ClockMode");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    DisplayCounterClockMode mode = Randomiser.EnumValue<DisplayCounterClockMode>();

                    stateBefore.DisplayCounter.Properties.ClockMode = mode;
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
            var handler = CommandGenerator.CreateAutoCommandHandler<DisplayCounterPropertiesSetCommand, DisplayCounterPropertiesGetCommand>("StartFrom");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
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

                    stateBefore.DisplayCounter.Properties.StartFrom = time;
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
            AtemMockServerWrapper.Each(_output, _pool, ClockStateCommandHandler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState stateBefore = helper.Helper.BuildLibState();

                Assert.Equal(DisplayCounterClockState.Reset, stateBefore.DisplayCounter.Properties.ClockState);

                stateBefore.DisplayCounter.Properties.ClockState = DisplayCounterClockState.Running;
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    displayClock.Start();
                });

                stateBefore.DisplayCounter.Properties.ClockState = DisplayCounterClockState.Stopped;
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    displayClock.Stop();
                });

                stateBefore.DisplayCounter.Properties.ClockState = DisplayCounterClockState.Reset;
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    displayClock.Reset();
                });
            });
        }
        private static IEnumerable<ICommand> ClockStateCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is DisplayCounterStateSetCommand stateCmd)
            {
                var previous = previousCommands.Value.OfType<DisplayCounterPropertiesGetCommand>().Last(); // TODO Id?
                Assert.NotNull(previous);

                previous.ClockState = stateCmd.State;

                yield return previous;
            }
        }

        [Fact]
        public void TestStartFromFrames()
        {
            AtemMockServerWrapper.Each(_output, _pool, StartFromFramesCommandHandler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
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

                    stateBefore.DisplayCounter.Properties.StartFrom = time;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        displayClock.SetStartFromFrames(totalFrames);
                    });
                }

            });
        }
        private static IEnumerable<ICommand> StartFromFramesCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is DisplayCounterPropertiesSetCommand propsCmd)
            {
                var modeCmd = previousCommands.Value.OfType<VideoModeGetCommand>().Last();
                Assert.NotNull(modeCmd);

                var previous = previousCommands.Value.OfType<DisplayCounterPropertiesGetCommand>().Last(); // TODO Id?
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
            var handler = CommandGenerator.MatchCommand(new DisplayCounterRequestTimeCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
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
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.DisplayCounter, helper =>
            {
                IBMDSwitcherDisplayClock displayClock = GetDisplayCounter(helper);
                Assert.NotNull(displayClock);

                AtemState expectedState = helper.Helper.BuildLibState();

                var testCmd = new DisplayCounterCurrentTimeCommand()
                {
                    Time = new HyperDeckTime
                    {
                        Hour = (uint)Randomiser.RangeInt(1, 20),
                        Minute = (uint)Randomiser.RangeInt(1, 59),
                        Second = (uint)Randomiser.RangeInt(1, 59),
                        Frame = (uint)Randomiser.RangeInt(1, 20),
                    },
                };

                expectedState.DisplayCounter.CurrentTime = testCmd.Time;

                helper.SendAndWaitForChange(expectedState, () =>
                {
                    helper.Server.SendCommands(testCmd);
                });
            });
        }

        private static IBMDSwitcherDisplayClock GetDisplayCounter(AtemMockServerWrapper helper)
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