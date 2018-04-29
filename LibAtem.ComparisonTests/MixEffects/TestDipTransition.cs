using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestDipTransition : ComparisonTestBase
    {
        public TestDipTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        [Fact]
        public void TestDipRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDipParameters>())
                {
                    uint[] testValues = {1, 18, 28, 95, 234, 244, 250};
                    uint[] badValues = {251, 255, 0};
                    
                    ICommand Setter(uint v) => new TransitionDipSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDipSetCommand.MaskFlags.Rate,
                        Rate = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Dip.Rate = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Dip.Rate = v >= 250 ? 250 : (uint) 1;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDipInput()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDipParameters>())
                {
                    long[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(Client.Profile) && s.IsAvailable(me.Item1)).Select(s => (long)s).ToArray();
                    long[] badValues = VideoSourceLists.All.Select(s => (long)s).Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(long v) => new TransitionDipSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDipSetCommand.MaskFlags.Input,
                        Input = (VideoSource) v,
                    };

                    void UpdateExpectedState(ComparisonState state, long v) => state.MixEffects[me.Item1].Transition.Dip.Input = (VideoSource)v;

                    ValueTypeComparer<long>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, badValues);
                }
            }
        }
    }
}