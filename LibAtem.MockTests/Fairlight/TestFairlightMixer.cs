using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightMixer
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightMixer(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        public static IBMDSwitcherFairlightAudioMixer GetFairlightMixer(AtemMockServerWrapper helper)
        {
            var mixer = helper.Helper.SdkSwitcher as IBMDSwitcherFairlightAudioMixer;
            Assert.NotNull(mixer);
            return mixer;
        }

        [Fact]
        public void TestSendLevelsCommand()
        {
            var expected = new FairlightMixerSendLevelsCommand();
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    expected.SendLevels = i % 2 == 1;

                    helper.SendAndWaitForChange(stateBefore, () => { mixer.SetAllLevelNotificationsEnabled(i % 2); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestResetProgramOutPeaks()
        {
            var expected = new FairlightMixerResetPeakLevelsCommand { Master = true };
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore, () => { mixer.ResetMasterOutPeakLevels(); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        [Fact]
        public void TestResetAllPeaks()
        {
            var expected = new FairlightMixerResetPeakLevelsCommand { All = true };
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(stateBefore, () => { mixer.ResetAllPeakLevels(); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }

        [Fact]
        public void TestTally()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    var cmd = new FairlightMixerTallyCommand
                    {
                        Tally = new Dictionary<Tuple<AudioSource, long>, bool>()
                    };

                    Assert.NotEmpty(stateBefore.Fairlight.Tally);

                    // the sdk is a bit picky about ids, so best to go with what it expects
                    foreach (KeyValuePair<Tuple<AudioSource, long>, bool> k in stateBefore.Fairlight.Tally)
                    {
                        bool isMixedIn = Randomiser.Range(0, 1) > 0.7;
                        cmd.Tally[k.Key] = isMixedIn;
                    }

                    stateBefore.Fairlight.Tally = cmd.Tally;
                    helper.SendAndWaitForChange(stateBefore, () => { helper.Server.SendCommands(cmd); });
                }
            });
        }
    }
}
