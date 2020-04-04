using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Macro;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Macro
{
    [Collection("ServerClientPool")]
    public class TestMacroControl
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestMacroControl(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestRun()
        {
            var expectedCommand = RunThroughSerialize(new MacroActionCommand
            {
                Action = MacroActionCommand.MacroAction.Run
            });
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint index = Randomiser.RangeInt((uint) stateBefore.Macros.Pool.Count);
                    expectedCommand.Index = index;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        control.Run(index);
                    });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestContinue()
        {
            var expectedCommand = RunThroughSerialize(new MacroActionCommand
            {
                Action = MacroActionCommand.MacroAction.Continue
            });
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                uint timeBefore = helper.Server.CurrentTime;

                AtemState stateBefore = helper.Helper.BuildLibState();
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    control.ResumeRunning();
                });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        private static T RunThroughSerialize<T>(T cmd) where T : SerializableCommandBase
        {
            var builder = new ByteArrayBuilder(false);
            cmd.Serialize(builder);

            var parsedData = new ParsedByteArray(builder.ToByteArray(), false);
            T resCmd = (T) Activator.CreateInstance(typeof(T));
            resCmd.Deserialize(parsedData);
            return resCmd;
        }

        [Fact]
        public void TestStop()
        {
            var expectedCommand = RunThroughSerialize(new MacroActionCommand
            {
                Action = MacroActionCommand.MacroAction.Stop
            });
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                uint timeBefore = helper.Server.CurrentTime;

                AtemState stateBefore = helper.Helper.BuildLibState();
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    control.StopRunning();
                });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        [Fact]
        public void TestRecord()
        {
            var expectedCommand = RunThroughSerialize(new MacroRecordCommand());
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    string name = Guid.NewGuid().ToString().Substring(0, 20);
                    string description = Guid.NewGuid().ToString();

                    expectedCommand.Index = index;
                    expectedCommand.Name = name;
                    expectedCommand.Description = description;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(stateBefore, () => { control.Record(index, name, description); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestRecordUserWait()
        {
            var expectedCommand = RunThroughSerialize(new MacroActionCommand
            {
                Action = MacroActionCommand.MacroAction.InsertUserWait
            });
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                AtemState stateBefore = helper.Helper.BuildLibState();

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore, () => { control.RecordUserWait(); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        [Fact]
        public void TestRecordPause()
        {
            var expectedCommand = new MacroAddTimedPauseCommand();
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint frames = Randomiser.RangeInt(2500);

                    expectedCommand.Frames = frames;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(stateBefore, () => { control.RecordPause(frames); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestRecordStop()
        {
            var expectedCommand = RunThroughSerialize(new MacroActionCommand
            {
                Action = MacroActionCommand.MacroAction.StopRecord
            });
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                uint timeBefore = helper.Server.CurrentTime;

                AtemState stateBefore = helper.Helper.BuildLibState();
                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    control.StopRecording();
                });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        [Fact]
        public void TestLoop()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MacroRunStatusSetCommand, MacroRunStatusGetCommand>("Loop");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    bool newValue = i % 2 == 0;
                    stateBefore.Macros.RunStatus.Loop = newValue;

                    helper.SendAndWaitForChange(stateBefore, () => { control.SetLoop(newValue ? 1 : 0); });
                }
            });
        }

        [Fact]
        public void TestRunStatus()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    var status = Randomiser.EnumValue<MacroState.MacroRunStatus>();

                    stateBefore.Macros.RunStatus.RunIndex = index;
                    stateBefore.Macros.RunStatus.RunStatus = status;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        helper.Server.SendCommands(new MacroRunStatusGetCommand
                        {
                            Index = index,
                            IsWaiting = status == MacroState.MacroRunStatus.UserWait,
                            IsRunning = status == MacroState.MacroRunStatus.Running,
                            Loop = stateBefore.Macros.RunStatus.Loop,
                        });
                    });
                }
            });
        }

        [Fact]
        public void TestRecordStatus()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                var control = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroControl;
                Assert.NotNull(control);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    bool recording = Randomiser.RangeInt(1) == 1;

                    stateBefore.Macros.RecordStatus.RecordIndex = index;
                    stateBefore.Macros.RecordStatus.IsRecording = recording;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        helper.Server.SendCommands(new MacroRecordingStatusGetCommand()
                        {
                            Index = index,
                            IsRecording = recording,
                        });
                    });
                }
            });
        }
    }
}