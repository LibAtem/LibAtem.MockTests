using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
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
            using (var helper = new AtemComparisonHelper(Client))
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

                    uint? Getter() => helper.FindWithMatching(new TransitionDipGetCommand {Index = me.Item1})?.Rate;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetRate, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetRate, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDipInput()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    long? Getter() => (long?) helper.FindWithMatching(new TransitionDipGetCommand {Index = me.Item1})?.Input;

                    ValueTypeComparer<long>.Run(helper, Setter, me.Item2.GetInputDip, Getter, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, me.Item2.GetInputDip, Getter, badValues);
                }
            }
        }
    }
}