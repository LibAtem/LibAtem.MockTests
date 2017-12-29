using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestDipTransition : TestTransitionBase
    {
        public TestDipTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        [Fact]
        public void TestDipRate()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDipParameters>(helper);
                Assert.NotNull(sdkProps);


                uint[] testValues = { 18, 28, 95 };

                ICommand Setter(uint v) => new TransitionDipSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDipSetCommand.MaskFlags.Rate,
                    Rate = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionDipGetCommand { Index = MixEffectBlockId.One })?.Rate;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetRate, Getter, testValues);
            }
        }

        [Fact]
        public void TestDipInput()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDipParameters>(helper);
                Assert.NotNull(sdkProps);
                
                long[] testValues =
                {
                    (long) VideoSource.Color1,
                    (long) VideoSource.MediaPlayer1,
                    (long) VideoSource.Input3
                };

                ICommand Setter(long v) => new TransitionDipSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDipSetCommand.MaskFlags.Input,
                    Input = (VideoSource)v,
                };

                long? Getter() => (long?)helper.FindWithMatching(new TransitionDipGetCommand { Index = MixEffectBlockId.One })?.Input;

                ValueTypeComparer<long>.Run(helper, Setter, sdkProps.GetInputDip, Getter, testValues);
            }
        }
    }
}