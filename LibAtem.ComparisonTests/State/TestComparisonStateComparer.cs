using System.Collections.Generic;
using LibAtem.Common;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.State
{
    public class TestAtemStateComparer
    {
        private readonly ITestOutputHelper output;

        public TestAtemStateComparer(ITestOutputHelper output)
        {
            this.output = output;
        }

        /*
        [Fact]
        public void TestValueTypeDifference()
        {
            var baseState = new AtemState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                    {MixEffectBlockId.Two, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                }
            };
            var update = new AtemState()
            {
                One = 3
            };
            var errors = AtemStateComparer.AreEqual(baseState, update);

            output.WriteLine(string.Join("\n", errors));
            Assert.Equal(1, errors.Count);
        }

        [Fact]
        public void TestDictionaryValueDifference()
        {
            var baseState = new AtemState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                    {MixEffectBlockId.Two, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                }
            };
            var update = new AtemState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Program = VideoSource.Input11}}
                }
            };
            var errors = AtemStateComparer.AreEqual(baseState, update);

            output.WriteLine(string.Join("\n", errors));
            Assert.Equal(1, errors.Count);
        }

        [Fact]
        public void TestNoDifference()
        {
            var baseState = new AtemState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                    {MixEffectBlockId.Two, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                }
            };
            var update = new AtemState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Program = VideoSource.Color1}}
                },
            };
            Assert.True(AtemStateComparer.AreEqual(output, baseState, update)));
        }*/
        
        [Fact]
        public void TestClone()
        {
            var baseState = new AtemState()
            {
                MixEffects = new List<MixEffectState>()
                {
                    CreateMixEffectStateWithSources(VideoSource.Input10, VideoSource.Color1),
                    CreateMixEffectStateWithSources(VideoSource.Input10, VideoSource.Color1),
                }
            };
            var cloned = baseState.Clone();
            cloned.MixEffects[(int)MixEffectBlockId.Two].Sources.Program = VideoSource.Input11;
            Assert.False(AtemStateComparer.AreEqual(output, baseState, cloned));
        }

        private MixEffectState CreateMixEffectStateWithSources(VideoSource pvw, VideoSource pgm)
        {
            var state = new MixEffectState();
            state.Sources.Preview = pvw;
            state.Sources.Program = pgm;
            return state;
        }
    }
}