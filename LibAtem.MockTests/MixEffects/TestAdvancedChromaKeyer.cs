using System;
using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    public class TestAdvancedChromaKeyer : MixEffectsTestBase
    {
        public TestAdvancedChromaKeyer(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public void TestForegroundLevel()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("ForegroundLevel");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int) keyer.Item1].Keyers[(int) keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(0, 100, 10);
                        keyerBefore.Properties.ForegroundLevel = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetForegroundLevel(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBackgroundLevel()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("BackgroundLevel");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(0, 100, 10);
                        keyerBefore.Properties.BackgroundLevel = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBackgroundLevel(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestKeyEdge()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("KeyEdge");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(0, 100, 10);
                        keyerBefore.Properties.KeyEdge = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetKeyEdge(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSpillSuppression()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("SpillSuppression");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(0, 100, 10);
                        keyerBefore.Properties.SpillSuppression = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetSpillSuppress(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestFlareSuppression()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("FlareSuppression");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(0, 100, 10);
                        keyerBefore.Properties.FlareSuppression = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetFlareSuppress(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBrightness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("Brightness");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(-100, 100, 10);
                        keyerBefore.Properties.Brightness = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBrightness(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestContrast()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("Contrast");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(-100, 100, 10);
                        keyerBefore.Properties.Contrast = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetContrast(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSaturation()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("Saturation");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(0, 200, 10);
                        keyerBefore.Properties.Saturation = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetSaturation(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestRed()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("Red");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(-100, 100, 10);
                        keyerBefore.Properties.Red = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetRed(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGreen()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("Green");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(-100, 100, 10);
                        keyerBefore.Properties.Green = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetGreen(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBlue()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyAdvancedChromaPropertiesSetCommand, MixEffectKeyAdvancedChromaPropertiesGetCommand>("Blue");
            AtemMockServerWrapper.Each(Output, handler, DeviceTestCases.AdvancedChromaKeyer, helper =>
            {
                var keyers = GetKeyers<IBMDSwitcherKeyAdvancedChromaParameters>(helper);
                var useKeyers = SelectionOfGroup(keyers);

                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyAdvancedChromaParameters> keyer in useKeyers)
                {
                    tested = true;
                    AtemState stateBefore = helper.Helper.LibState;
                    MixEffectState.KeyerAdvancedChromaState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2].AdvancedChroma;
                    Assert.NotNull(keyerBefore);

                    IBMDSwitcherKeyAdvancedChromaParameters sdkKeyer = keyer.Item3;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(-100, 100, 10);
                        keyerBefore.Properties.Blue = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBlue(target / 100); });
                    }
                }
            });
            Assert.True(tested);
        }
    }
}