using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;
using AtemSDKConverter = LibAtem.ComparisonTests.AtemSDKConverter;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestSaveRecall
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestSaveRecall(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }
        
        [Fact]
        public void TestSaveStartupState()
        {
            var handler = CommandGenerator.MatchCommand(new StartupStateSaveCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SerialPort, helper =>
            {
                IBMDSwitcherSaveRecall saveRecall = helper.SdkClient.SdkSwitcher as IBMDSwitcherSaveRecall;
                Assert.NotNull(saveRecall);

                AtemState stateBefore = helper.Helper.BuildLibState();

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore, () => { saveRecall.Save(_BMDSwitcherSaveRecallType.bmdSwitcherSaveRecallTypeStartupState); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        [Fact]
        public void TestClearStartupState()
        {
            var handler = CommandGenerator.MatchCommand(new StartupStateClearCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SerialPort, helper =>
            {
                IBMDSwitcherSaveRecall saveRecall = helper.SdkClient.SdkSwitcher as IBMDSwitcherSaveRecall;
                Assert.NotNull(saveRecall);

                AtemState stateBefore = helper.Helper.BuildLibState();

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore, () => { saveRecall.Clear(_BMDSwitcherSaveRecallType.bmdSwitcherSaveRecallTypeStartupState); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

    }
}