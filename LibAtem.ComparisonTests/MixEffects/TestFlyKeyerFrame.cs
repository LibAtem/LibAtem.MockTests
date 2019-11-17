using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    [Collection("Client")]
    public class TestFlyKeyerFrame : MixEffectsTestBase
    {
        public TestFlyKeyerFrame(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private List<Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters>> GetFrames()
        {
            var res = new List<Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters>>();
            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyFlyParameters> key in GetKeyers<IBMDSwitcherKeyFlyParameters>())
            {
                key.Item3.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out IBMDSwitcherKeyFlyKeyFrameParameters frameA);
                key.Item3.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out IBMDSwitcherKeyFlyKeyFrameParameters frameB);

                res.Add(Tuple.Create(key.Item1, key.Item2, FlyKeyKeyFrameId.One, frameA));
                res.Add(Tuple.Create(key.Item1, key.Item2, FlyKeyKeyFrameId.Two, frameB));
            }

            return res;
        }

        private abstract class FlyKeyFrameTestDefinition<T> : TestDefinitionBase<MixEffectKeyFlyKeyframeSetCommand, T>
        {
            private readonly MixEffectBlockId _meId;
            private readonly UpstreamKeyId _keyId;
            private readonly FlyKeyKeyFrameId _frameId;
            protected readonly IBMDSwitcherKeyFlyKeyFrameParameters _sdk;

            public FlyKeyFrameTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper)
            {
                _meId = frame.Item1;
                _keyId = frame.Item2;
                _frameId = frame.Item3;
                _sdk = frame.Item4;
            }

            public override void SetupCommand(MixEffectKeyFlyKeyframeSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
                cmd.KeyFrame = _frameId;
            }

            public abstract T MangleBadValue(T v);

            public sealed override void UpdateExpectedState(ComparisonState state, bool goodValue, T v)
            {
                ComparisonMixEffectKeyerFlyFrameState obj = state.MixEffects[_meId].Keyers[_keyId].Fly.Frames[_frameId];
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyFlyKeyframeGetCommand() { MixEffectIndex = _meId, KeyerIndex = _keyId, KeyFrame = _frameId });
            }
        }

        private class FlyKeyFrameSizeXTestDefinition: FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameSizeXTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSizeX(10);

            public override string PropertyName => "XSize";
            public override double MangleBadValue(double v) => 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 0.01, 100, 999.99, 1000, 9999.99, 10000, 31999.99, 32760, 32767.99 };
            public override double[] BadValues => new double[] { -0.01, -1, -10, 32768 };
        }

        [Fact]
        public void TestSizeX()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameSizeXTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameSizeYTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameSizeYTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSizeY(10);

            public override string PropertyName => "YSize";
            public override double MangleBadValue(double v) => 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 0.01, 100, 999.99, 1000, 9999.99, 10000, 31999.99, 32760, 32767.99 };
            public override double[] BadValues => new double[] { -0.01, -1, -10, 32768 };
        }

        [Fact]
        public void TestSizeY()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameSizeYTestDefinition(helper, f).Run());
        }
        
        private class FlyKeyFramePositionXTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFramePositionXTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPositionX(10);

            public override string PropertyName => "XPosition";
            public override double MangleBadValue(double v) => v >= 32768 ? v - 2 * 32768 : v + 2 * 32768;

            public override double[] GoodValues => new double[] { -456, 0, 567, -32767.9, 32767.9 };
            public override double[] BadValues => new double[] { 32768, 33000, -32768 };
        }

        [Fact]
        public void TestPositionX()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFramePositionXTestDefinition(helper, f).Run());
        }

        private class FlyKeyFramePositionYTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFramePositionYTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPositionY(10);

            public override string PropertyName => "YPosition";
            public override double MangleBadValue(double v) => v >= 32768 ? v - 2 * 32768 : v + 2 * 32768;

            public override double[] GoodValues => new double[] { -456, 0, 567, -32767.9, 32767.9 };
            public override double[] BadValues => new double[] { 32768, 33000, -32768 };
        }

        [Fact]
        public void TestPositionY()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFramePositionYTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameRotationTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameRotationTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPositionY(10);

            public override string PropertyName => "Rotation";
            public override double MangleBadValue(double v) => v >= 32768 ? v - 2 * 32768 : v + 2 * 32768;

            public override double[] GoodValues => new double[] { -456, 0, 567, -32767.9, 32767.9 };
            public override double[] BadValues => new double[] { 32768, 33000, -32768 };
        }

        [Fact]
        public void TestRotation()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameRotationTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderInnerWidthTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameBorderInnerWidthTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderWidthIn(10);

            public override string PropertyName => "BorderInnerWidth";
            public override double MangleBadValue(double v) => v < 0 ? v + 655.36 * 2 : v - 655.35;

            public override double[] GoodValues => new double[] { 0, 15.9, 0.01, 16, 655.35 };
            public override double[] BadValues => new double[] { 660, -660, 700, -700 };
        }

        [Fact]
        public void TestBorderInnerWidth()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderInnerWidthTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderOuterWidthTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameBorderOuterWidthTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderWidthOut(10);

            public override string PropertyName => "BorderOuterWidth";
            public override double MangleBadValue(double v) => v < 0 ? v + 655.36 * 2 : v - 655.35;

            public override double[] GoodValues => new double[] { 0, 15.9, 0.01, 16, 655.35 };
            public override double[] BadValues => new double[] { 660, -660, 700, -700 };
        }

        [Fact]
        public void TestBorderOuterWidth()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderOuterWidthTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderOuterSoftnessTestDefinition : FlyKeyFrameTestDefinition<uint>
        {
            public FlyKeyFrameBorderOuterSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSoftnessOut(10);

            public override string PropertyName => "BorderOuterSoftness";
            public override uint MangleBadValue(uint v) => 254;

            public override uint[] GoodValues => new uint[] { 0, 15, 1, 99, 100, 101, 254 };
            public override uint[] BadValues => new uint[] { 255 };
        }

        [Fact]
        public void TestBorderOuterSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderOuterSoftnessTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderInnerSoftnessTestDefinition : FlyKeyFrameTestDefinition<uint>
        {
            public FlyKeyFrameBorderInnerSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSoftnessIn(10);

            public override string PropertyName => "BorderInnerSoftness";
            public override uint MangleBadValue(uint v) => 254;

            public override uint[] GoodValues => new uint[] { 0, 15, 1, 99, 100, 101, 254 };
            public override uint[] BadValues => new uint[] { 255 };
        }

        [Fact]
        public void TestBorderInnerSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderInnerSoftnessTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderBevelSoftnessTestDefinition : FlyKeyFrameTestDefinition<uint>
        {
            public FlyKeyFrameBorderBevelSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderBevelSoftness(10);

            public override string PropertyName => "BorderBevelSoftness";
            public override uint MangleBadValue(uint v) => 254;

            public override uint[] GoodValues => new uint[] { 0, 15, 1, 99, 100, 101, 254 };
            public override uint[] BadValues => new uint[] { 255 };
        }

        [Fact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderBevelSoftnessTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderBevelPositionTestDefinition : FlyKeyFrameTestDefinition<uint>
        {
            public FlyKeyFrameBorderBevelPositionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderBevelPosition(10);

            public override string PropertyName => "BorderBevelPosition";
            public override uint MangleBadValue(uint v) => 254;

            public override uint[] GoodValues => new uint[] { 0, 15, 1, 99, 100, 101, 254 };
            public override uint[] BadValues => new uint[] { 255 };
        }

        [Fact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderBevelPositionTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderOpacityTestDefinition : FlyKeyFrameTestDefinition<uint>
        {
            public FlyKeyFrameBorderOpacityTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderOpacity(10);

            public override string PropertyName => "BorderOpacity";
            public override uint MangleBadValue(uint v) => 254;

            public override uint[] GoodValues => new uint[] { 0, 15, 1, 99, 100, 101, 254 };
            public override uint[] BadValues => new uint[] { 255 };
        }

        /* TODO - sdk does not fire any changed event, so this test does not work
        [Fact]
        public void TestBorderOpacity()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderOpacityTestDefinition(helper, f).Run());
        }
        */

        private class FlyKeyFrameBorderHueTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameBorderHueTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderHue(10);

            public override string PropertyName => "BorderHue";
            public override double MangleBadValue(double v) => v > 0 ? v - 6553.6 : v + 6553.6;

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9, 6553.5 };
            public override double[] BadValues => new double[] { 6553.6, 6600, -0.1, -10, 6700 };
        }

        [Fact]
        public void TestBorderHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderHueTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderSaturationTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameBorderSaturationTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSaturation(10);

            public override string PropertyName => "BorderSaturation";
            public override double MangleBadValue(double v) => v > 0 ? v - 6553.6 : v + 6553.6;

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9, 6553.5 };
            public override double[] BadValues => new double[] { 6553.6, 6600, -0.1, -10, 6700 };
        }

        [Fact]
        public void TestBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderSaturationTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderLumaTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameBorderLumaTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLuma(10);

            public override string PropertyName => "BorderLuma";
            public override double MangleBadValue(double v) => v > 0 ? v - 6553.6 : v + 6553.6;

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9, 6553.5 };
            public override double[] BadValues => new double[] { 6553.6, 6600, -0.1, -10, 6700 };
        }

        [Fact]
        public void TestBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderLumaTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderLightSourceDirectionTestDefinition : FlyKeyFrameTestDefinition<double>
        {
            public FlyKeyFrameBorderLightSourceDirectionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLightSourceDirection(10);

            public override string PropertyName => "BorderLightSourceDirection";
            public override double MangleBadValue(double v) => v > 0 ? v - 360 : v + 73.6;

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9 };
            public override double[] BadValues => new double[] { 360, 380, -0.1, -10, 567.9 };
        }

        [Fact]
        public void TestBorderLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderLightSourceDirectionTestDefinition(helper, f).Run());
        }

        private class FlyKeyFrameBorderLightSourceAltitudeTestDefinition : FlyKeyFrameTestDefinition<uint>
        {
            public FlyKeyFrameBorderLightSourceAltitudeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters> frame) : base(helper, frame)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLightSourceAltitude(10);

            public override string PropertyName => "BorderLightSourceAltitude";
            public override uint MangleBadValue(uint v) => 254;

            public override uint[] GoodValues => new uint[] { 0, 9, 10, 15, 11, 99, 100, 101, 254 };
            public override uint[] BadValues => new uint[] { 255 };
        }

        [Fact]
        public void TestBorderLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetFrames().ForEach(f => new FlyKeyFrameBorderLightSourceAltitudeTestDefinition(helper, f).Run());
        }

        // TODO - mask

    }
}