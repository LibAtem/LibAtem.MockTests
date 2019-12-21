using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio.Fairlight;
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
            AtemMockServerWrapper.Each(_output, GainCommandHandler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    stateBefore.Fairlight.ProgramOut.Gain = 2;
                    mixer.SetMasterOutFaderGain(2);
                });

                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    stateBefore.Fairlight.ProgramOut.Gain = -14;
                    mixer.SetMasterOutFaderGain(-14);
                });
            });
        }
        private static IEnumerable<ICommand> GainCommandHandler(ImmutableList<ICommand> previousCommands, ICommand cmd)
        {
            if (cmd is FairlightMixerMasterSetCommand masterCmd)
            {
                Assert.Equal(FairlightMixerMasterSetCommand.MaskFlags.Gain, masterCmd.Mask);

                var previous = previousCommands.OfType<FairlightMixerMasterGetCommand>().Last();
                Assert.NotNull(previous);

                previous.Gain = masterCmd.Gain;
                yield return previous;
            }
        }

        [Fact]
        public void TestFollowFadeToBlack()
        {
            AtemMockServerWrapper.Each(_output, FollowFadeToBlackCommandHandler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.LibState;

                if (stateBefore.Fairlight.ProgramOut.FollowFadeToBlack)
                {
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        stateBefore.Fairlight.ProgramOut.FollowFadeToBlack = false;
                        mixer.SetMasterOutFollowFadeToBlack(0);
                    });
                }

                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    stateBefore.Fairlight.ProgramOut.FollowFadeToBlack = true;
                    mixer.SetMasterOutFollowFadeToBlack(1);
                });

                helper.SendAndWaitForChange(stateBefore, () =>
                {
                    stateBefore.Fairlight.ProgramOut.FollowFadeToBlack = false;
                    mixer.SetMasterOutFollowFadeToBlack(0);
                });
            });
        }
        private static IEnumerable<ICommand> FollowFadeToBlackCommandHandler(ImmutableList<ICommand> previousCommands, ICommand cmd)
        {
            if (cmd is FairlightMixerMasterSetCommand masterCmd)
            {
                Assert.Equal(FairlightMixerMasterSetCommand.MaskFlags.FollowFadeToBlack, masterCmd.Mask);

                var previous = previousCommands.OfType<FairlightMixerMasterGetCommand>().Last();
                Assert.NotNull(previous);

                previous.FollowFadeToBlack = masterCmd.FollowFadeToBlack;
                yield return previous;
            }
        }




    }
}
