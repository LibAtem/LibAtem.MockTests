using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DeviceProfile;
using LibAtem.Commands.Settings;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestSwitcher
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestSwitcher(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestPowerStatus()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();

                int maxState = 1 << (2 - 1);

                for (int newValue = 0; newValue < maxState; newValue++)
                {
                    stateBefore.Power = new[] {(newValue & 1) != 0, (newValue & 2) != 0};
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        helper.Server.SendCommands(new PowerStatusCommand()
                        {
                            Pin1 = (newValue & 1) != 0,
                            Pin2 = (newValue & 2) != 0,
                        });
                    });
                }
            });
        }

        [Fact]
        public void TestRequestTimecode()
        {
            var handler = CommandGenerator.MatchCommand(new TimeCodeRequestCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SerialPort, helper =>
            {
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                AtemState stateBefore = helper.Helper.BuildLibState();

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore, () => { switcher.RequestTimeCode(); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        // GetTimecode is tested as part of the framework

        [Fact]
        public void TestSetTimecode()
        {
            var expectedCmd = new TimeCodeCommand();
            var handler = CommandGenerator.MatchCommand(expectedCmd, "IsDropFrame");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SerialPort, helper =>
            {
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                AtemState stateBefore = helper.Helper.BuildLibState();

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore,
                    () =>
                    {
                        switcher.SetTimeCode((byte) expectedCmd.Hour, (byte) expectedCmd.Minute,
                            (byte) expectedCmd.Second, (byte) expectedCmd.Frame);
                    });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        [Fact]
        public void TestTimecodeLocked()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.SerialPort, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    bool newValue = i % 2 == 0;
                    stateBefore.Info.TimecodeLocked = newValue;

                    helper.SendAndWaitForChange(stateBefore,
                        () => { helper.Server.SendCommands(new TimecodeLockedCommand {Locked = newValue}); });

                }
            });
        }

        [Fact]
        public void TestSuperSourceCascade()
        {
            var expectedCmd = new SuperSourceCascadeCommand();
            var handler = CommandGenerator.EchoCommand(expectedCmd);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSourceCascade, helper =>
            {
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    bool newValue = i % 2 == 0;
                    stateBefore.Settings.SuperSourceCascade = newValue;
                    expectedCmd.Cascade = newValue;

                    helper.SendAndWaitForChange(stateBefore,
                        () => { switcher.SetSuperSourceCascade(newValue ? 1 : 0); });

                }
            });
        }

        [Fact]
        public void TestSetSDI3GLevel()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<SDI3GLevelOutputSetCommand, SDI3GLevelOutputGetCommand>("SDI3GOutputLevel", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SDI3G, helper =>
            {
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                AtemState stateBefore = helper.Helper.BuildLibState();

                var values = Enum.GetValues(typeof(SDI3GOutputLevel)).OfType<SDI3GOutputLevel>().ToArray();
                for (int i = 0; i < 5; i++)
                {
                    SDI3GOutputLevel newValue = values[i % values.Length];
                    stateBefore.Settings.SDI3GLevel = newValue;

                    helper.SendAndWaitForChange(stateBefore,
                        () => { switcher.Set3GSDIOutputLevel(AtemEnumMaps.SDI3GOutputLevelMap[newValue]); });

                }
            });
        }

    }
}