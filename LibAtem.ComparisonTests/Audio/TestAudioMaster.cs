using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.ComparisonTests.MixEffects;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Audio
{
    [Collection("Client")]
    public class TestAudioMaster : ComparisonTestBase
    {
        public TestAudioMaster(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }
        
        [Fact]
        public void TestAudioFollowVideo()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ICommand Setter(bool v) => new AudioMixerMasterSetCommand()
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.FollowFadeToBlack,
                    FollowFadeToBlack = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.Audio.ProgramOutFollowFadeToBlack = v;

                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, new[] { true, false });
            }
        }

        [Fact]
        public void TestBalance()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                double[] testValues = { -50, -49.99, -49, 10, 0, 35.84, 49.99, 50 };
                double[] badValues = { -50.01, -51, 50.01, 51 };

                ICommand Setter(double v) => new AudioMixerMasterSetCommand()
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.Balance,
                    Balance = v,
                };

                using (new ValueDefaults(helper, Setter(0))) // We need to do this, as it cannot be set via the stock client
                {
                    void UpdateExpectedState(ComparisonState state, double v) => state.Audio.ProgramOutBalance = v;
                    void UpdateBadState(ComparisonState state, double v) => state.Audio.ProgramOutBalance = v < -50 ? -50 : 50;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateBadState, badValues);
                }
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                double[] testValues = { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };

                ICommand Setter(double v) => new AudioMixerMasterSetCommand()
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.Gain,
                    Gain = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.Audio.ProgramOutGain = v;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
            }
        }
    }
}
