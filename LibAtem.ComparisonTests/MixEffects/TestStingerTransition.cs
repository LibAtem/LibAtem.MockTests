using System;
using System.Collections.Generic;
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
    public class TestStingerTransition : ComparisonTestBase
    {
        private static readonly IReadOnlyDictionary<StingerSource, _BMDSwitcherStingerTransitionSource> SourceMap;

        static TestStingerTransition()
        {
            SourceMap = new Dictionary<StingerSource, _BMDSwitcherStingerTransitionSource>
            {
                {StingerSource.None, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceNone},
                {StingerSource.MediaPlayer1, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer1},
                {StingerSource.MediaPlayer2, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer2},
                {StingerSource.MediaPlayer3, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer3},
                {StingerSource.MediaPlayer4, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer4},
            };
        }

        public TestStingerTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private void ResetProps(IBMDSwitcherTransitionStingerParameters props)
        {
            props.SetTriggerPoint(1);
            props.SetPreroll(1);
            props.SetMixRate(1);
            props.SetClipDuration(40);
            props.SetTriggerPoint(10);
            props.SetPreroll(5);
            props.SetMixRate(15);

        }

        [Fact]
        public void EnsureSourceMapIsComplete()
        {
            EnumMap.EnsureIsComplete(SourceMap);
        }

        [Fact]
        public void TestStingerSource()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    StingerSource[] testValues = Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => s.IsAvailable(helper.Profile)).ToArray();
                    StingerSource[] badValues = Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(StingerSource v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Source,
                        Source = v,
                    };

                    StingerSource? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.Source;

                    EnumValueComparer<StingerSource, _BMDSwitcherStingerTransitionSource>.Run(helper, SourceMap, Setter, me.Item2.GetSource, Getter, testValues);
                    EnumValueComparer<StingerSource, _BMDSwitcherStingerTransitionSource>.Fail(helper, SourceMap, Setter, me.Item2.GetSource, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestStingerPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.PreMultipliedKey,
                        PreMultipliedKey = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.PreMultipliedKey;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetPreMultiplied, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestStingerClip()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Clip,
                        Clip = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.Clip;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetClip, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetClip, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestStingerGain()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.01};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Gain,
                        Gain = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.Gain;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetGain, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetGain, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestStingerInverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Invert,
                        Invert = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.Invert;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetInverse, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestStingerPreRoll()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    uint[] testValues = {0, 1, 18, 28, 90};
                    uint[] badValues = {999, 251};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Preroll,
                        Preroll = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.Preroll;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetPreroll, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetPreroll, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestStingerClipDuration()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    // These props all have various relations, that need better testing
                    ResetProps(me.Item2);
                    me.Item2.SetTriggerPoint(17);
                    me.Item2.SetMixRate(13);
                    me.Item2.SetPreroll(5);
                    helper.Sleep();

                    uint[] testValues = {35, 48, 95, 199, 30};
                    uint[] badValues = {1, 29, 5};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.ClipDuration,
                        ClipDuration = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.ClipDuration;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetClipDuration, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetClipDuration, Getter, badValues);

                    me.Item2.SetTriggerPoint(4);
                    me.Item2.SetMixRate(6);

                    uint[] testValues2 = {11, 30, 10};
                    uint[] badValues2 = {9, 1};

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetClipDuration, Getter, testValues2);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetClipDuration, Getter, badValues2);
                }
            }
        }

        [Fact]
        public void TestStingerTriggerPoint()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    // These props all have various relations, that need better testing
                    ResetProps(me.Item2);
                    me.Item2.SetMixRate(15);

                    uint[] testValues = {1, 18, 28, 39};
                    uint[] badValues = {40, 41, 50};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.TriggerPoint,
                        TriggerPoint = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.TriggerPoint;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetTriggerPoint, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetTriggerPoint, Getter, badValues);

                    me.Item2.SetTriggerPoint(1);
                    me.Item2.SetClipDuration(25);

                    uint[] testValues2 = {11, 24, 10};
                    uint[] badValues2 = {25, 26, 30};

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetTriggerPoint, Getter, testValues2);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetTriggerPoint, Getter, badValues2);
                }
            }
        }

        [Fact]
        public void TestStingerMixRate()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    // These props all have various relations, that need better testing
                    ResetProps(me.Item2);

                    uint[] testValues = {1, 18, 28, 30};
                    uint[] badValues = {31, 32, 40};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.MixRate,
                        MixRate = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand {Index = me.Item1})?.MixRate;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetMixRate, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetMixRate, Getter, badValues);

                    me.Item2.SetMixRate(5);
                    me.Item2.SetClipDuration(20);

                    uint[] testValues2 = {9, 1, 10};
                    uint[] badValues2 = {11, 12, 20};

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetMixRate, Getter, testValues2);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetMixRate, Getter, badValues2);
                }
            }
        }
    }
}
