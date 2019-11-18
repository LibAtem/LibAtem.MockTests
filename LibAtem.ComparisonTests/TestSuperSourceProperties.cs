using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestSuperSourceProperties
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestSuperSourceProperties(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        private IBMDSwitcherInputSuperSource GetSuperSource(AtemComparisonHelper helper)
        {
            var ssrc = helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>().Select(s => s.Value).SingleOrDefault();
            Skip.If(ssrc == null, "Model does not support SuperSource");
            return ssrc;
        }

        [Fact]
        public void TestSuperSourceCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                var srcs = helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>();
                Assert.Equal(srcs.Count, (int) helper.Profile.SuperSource);
                Assert.True(srcs.Count == 0 || srcs.Count == 1); // Tests expect 0 or 1
            }
        }

        private abstract class SuperSourceTestDefinition<T> : TestDefinitionBase<SuperSourcePropertiesSetV8Command, T>
        {
            protected readonly SuperSourceId _ssrcId;
            protected readonly IBMDSwitcherInputSuperSource _sdk;

            public SuperSourceTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId != SuperSourceId.One)
            {
                _ssrcId = ssrcId;
                _sdk = ssrc;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                SetCommandProperty(state.SuperSources[(int)_ssrcId].Properties, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"SuperSources.{_ssrcId:D}.Properties";
            }
        }

        private class SuperSourceArtCutTestDefinition : SuperSourceTestDefinition<VideoSource>
        {
            public SuperSourceArtCutTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId, ssrc)
            {
                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                ssrc.GetCutInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                Assert.Equal(availabilityMask, _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt);
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputCut((long)VideoSource.ColorBars);

            public override string PropertyName => "ArtCutSource";

            private VideoSource[] ValidSources => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt | SourceAvailability.KeySource)).ToArray();
            public override VideoSource[] GoodValues => VideoSourceUtil.TakeSelection(ValidSources);
            public override VideoSource[] BadValues => VideoSourceUtil.TakeBadSelection(ValidSources);

            public override VideoSource MangleBadValue(VideoSource v) => v;
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue) state.SuperSources[(int)_ssrcId].Properties.ArtKeyInput = v;
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new string[0];
            }
        }

        [SkippableFact]
        public void TestArtCut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtCutTestDefinition(helper, SuperSourceId.One, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtFillTestDefinition : SuperSourceTestDefinition<VideoSource>
        {
            public SuperSourceArtFillTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId, ssrc)
            {
                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                ssrc.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt, availabilityMask);
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputFill((long)VideoSource.ColorBars);

            public override string PropertyName => "ArtFillSource";

            private VideoSource[] ValidSources => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt)).ToArray();
            public override VideoSource[] GoodValues => VideoSourceUtil.TakeSelection(ValidSources);
            public override VideoSource[] BadValues => VideoSourceUtil.TakeBadSelection(ValidSources);

            public override VideoSource MangleBadValue(VideoSource v) => v;
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.SuperSources[(int)_ssrcId].Properties.ArtFillInput = v;
                    if (VideoSourceLists.MediaPlayers.Contains(v))
                        state.SuperSources[(int)_ssrcId].Properties.ArtKeyInput = v + 1;
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new string[0];
            }
        }

        [SkippableFact]
        public void TestArtFill()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtFillTestDefinition(helper, SuperSourceId.One, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtOptionTestDefinition : SuperSourceTestDefinition<SuperSourceArtOption>
        {
            public SuperSourceArtOptionTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetArtOption(_BMDSwitcherSuperSourceArtOption.bmdSwitcherSuperSourceArtOptionForeground);

            public override string PropertyName => "ArtOption";
            public override SuperSourceArtOption MangleBadValue(SuperSourceArtOption v) => v;

            public override SuperSourceArtOption[] GoodValues => Enum.GetValues(typeof(SuperSourceArtOption)).OfType<SuperSourceArtOption>().ToArray();
        }

        [SkippableFact]
        public void TestArtOption()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtOptionTestDefinition(helper, SuperSourceId.One, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtPreMultipliedTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceArtPreMultipliedTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPreMultiplied(0);

            public override string PropertyName => "ArtPreMultiplied";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestArtPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtPreMultipliedTestDefinition(helper, SuperSourceId.One, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtClipTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceArtClipTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetClip(20);

            public override string PropertyName => "ArtClip";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtClipTestDefinition(helper, SuperSourceId.One, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtGainTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceArtGainTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "ArtGain";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtGainTestDefinition(helper, SuperSourceId.One, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtInvertKeyTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceArtInvertKeyTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "ArtInvertKey";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestArtInvertKey()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtInvertKeyTestDefinition(helper, SuperSourceId.One, GetSuperSource(helper)).Run();
        }
    }
}