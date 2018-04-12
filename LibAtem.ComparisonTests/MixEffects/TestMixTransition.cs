using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestMixTransition : ComparisonTestBase
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
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionMixParameters>())
                {
                    uint[] testValues = {1, 18, 28, 95, 234, 244, 250};
                    uint[] badValues = {251, 255, 0};
                    
                    ICommand Setter(uint v) => new TransitionMixSetCommand
                    {
                        Index = me.Item1,
                        Rate = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionMixGetCommand {Index = me.Item1})?.Rate;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetRate, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetRate, Getter, badValues);
                }
            }
        }
    }
}