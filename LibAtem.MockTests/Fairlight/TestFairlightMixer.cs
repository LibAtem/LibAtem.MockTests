using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.MockTests.Util;
using LibAtem.State;
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

                    helper.SendAndWaitForChange(null, () => { mixer.SetAllLevelNotificationsEnabled(i % 2); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        /*
        [Fact]
        public void TestTestProgramOutPeaks()
        {
            var expected = new FairlightMixerSendLevelsCommand();
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(null, () => { mixer.ResetMasterOutPeakLevels(); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }*/
    }
}
