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
    public class TestMixTransition : TestTransitionBase
    {
        public TestMixTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        [Fact]
        public void TestMixProps()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionMixParameters>(helper);
                Assert.NotNull(sdkProps);

                uint[] testValues = { 18, 28, 95 };

                ICommand Setter(uint v) => new TransitionMixSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Rate = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionMixGetCommand { Index = MixEffectBlockId.One })?.Rate;

                UIntValueComparer.Run(helper, Setter, sdkProps.GetRate, Getter, testValues);
            }
        }
    }
}