using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    public class TestFairlightProgramOut
    {
        private readonly ITestOutputHelper _output;

        public TestFairlightProgramOut(ITestOutputHelper output)
        {
            _output = output;
        }


        private static IBMDSwitcherFairlightAudioMixer GetFairlightMixer(AtemMockServerWrapper helper)
        {
            var mixer = helper.Helper.SdkSwitcher as IBMDSwitcherFairlightAudioMixer;
            Assert.NotNull(mixer);
            return mixer;
        }

        public static IBMDSwitcherFairlightAudioDynamicsProcessor GetDynamics(AtemMockServerWrapper helper)
        {
            var mixer = GetFairlightMixer(helper);
            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(mixer.GetMasterOutEffect);
            Assert.NotNull(dynamics);
            return dynamics;
        }

        /**
         * Notes:
         * The flow is to always send commands via the sdk.
         * We then verify we interpret the commands correctly by deserializing in the server.
         * And the response we send proves that we understand the response structure.
         *
         * TODO - perhaps we should drop working with LibAtem in the client side of these tests?
         * The only benefit to keeping it is to verify that the state building is correct (which tbh is a good idea)
         */

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterSetCommand, FairlightMixerMasterGetCommand>("Gain");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    stateBefore.Fairlight.ProgramOut.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        mixer.SetMasterOutFaderGain(target);
                    });
                }
            });
        }

        [Fact]
        public void TestFollowFadeToBlack()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterSetCommand, FairlightMixerMasterGetCommand>("FollowFadeToBlack");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    stateBefore.Fairlight.ProgramOut.FollowFadeToBlack = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        mixer.SetMasterOutFollowFadeToBlack(i % 2);
                    });
                }
            });
        }

        [Fact]
        public void TestMakeUp()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterSetCommand, FairlightMixerMasterGetCommand>("MakeUpGain");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = GetDynamics(helper);

                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range(0, 20);
                    stateBefore.Fairlight.ProgramOut.Dynamics.MakeUpGain = target;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        dynamics.SetMakeupGain(target);
                    });
                }
            });
        }


    }
}
