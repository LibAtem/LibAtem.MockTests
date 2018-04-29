using System.Collections.Generic;
using LibAtem.Common;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.State
{
    public class TestComparisonStateComparer
    {
        private readonly ITestOutputHelper output;

        public TestComparisonStateComparer(ITestOutputHelper output)
        {
            this.output = output;
        }

        /*
        [Fact]
        public void TestValueTypeDifference()
        {
            var baseState = new ComparisonState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                    {MixEffectBlockId.Two, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                }
            };
            var update = new ComparisonState()
            {
                One = 3
            };
            var errors = ComparisonStateComparer.AreEqual(baseState, update);

            output.WriteLine(string.Join("\n", errors));
            Assert.Equal(1, errors.Count);
        }

        [Fact]
        public void TestDictionaryValueDifference()
        {
            var baseState = new ComparisonState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                    {MixEffectBlockId.Two, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                }
            };
            var update = new ComparisonState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Program = VideoSource.Input11}}
                }
            };
            var errors = ComparisonStateComparer.AreEqual(baseState, update);

            output.WriteLine(string.Join("\n", errors));
            Assert.Equal(1, errors.Count);
        }

        [Fact]
        public void TestNoDifference()
        {
            var baseState = new ComparisonState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                    {MixEffectBlockId.Two, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                }
            };
            var update = new ComparisonState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Program = VideoSource.Color1}}
                },
            };
            Assert.True(ComparisonStateComparer.AreEqual(output, baseState, update)));
        }*/
        
        [Fact]
        public void TestClone()
        {
            var baseState = new ComparisonState()
            {
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>()
                {
                    {MixEffectBlockId.One, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                    {MixEffectBlockId.Two, new ComparisonMixEffectState {Preview = VideoSource.Input10, Program = VideoSource.Color1}},
                }
            };
            var cloned = baseState.Clone();
            cloned.MixEffects[MixEffectBlockId.Two].Program = VideoSource.Input11;
            Assert.False(ComparisonStateComparer.AreEqual(output, baseState, cloned));
        }
    }
}