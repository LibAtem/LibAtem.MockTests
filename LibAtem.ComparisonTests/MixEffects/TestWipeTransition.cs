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
    public class TestWipeTransition : ComparisonTestBase
    {
        public static readonly IReadOnlyDictionary<Pattern, _BMDSwitcherPatternStyle> PatternMap;

        static TestWipeTransition()
        {
            PatternMap = new Dictionary<Pattern, _BMDSwitcherPatternStyle>
            {
                {Pattern.LeftToRightBar, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleLeftToRightBar},
                {Pattern.TopToBottomBar, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopToBottomBar},
                {Pattern.HorizontalBarnDoor, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleHorizontalBarnDoor},
                {Pattern.VerticalBarnDoor, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleVerticalBarnDoor},
                {Pattern.CornersInFourBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCornersInFourBox},
                {Pattern.RectangleIris, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleRectangleIris},
                {Pattern.DiamondIris, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleDiamondIris},
                {Pattern.CircleIris, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris},
                {Pattern.TopLeftBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopLeftBox},
                {Pattern.TopRightBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopRightBox},
                {Pattern.BottomRightBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleBottomRightBox},
                {Pattern.BottomLeftBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleBottomLeftBox},
                {Pattern.TopCentreBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopCentreBox},
                {Pattern.RightCentreBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleRightCentreBox},
                {Pattern.BottomCentreBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleBottomCentreBox},
                {Pattern.LeftCentreBox, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleLeftCentreBox},
                {Pattern.TopLeftDiagonal, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopLeftDiagonal},
                {Pattern.TopRightDiagonal, _BMDSwitcherPatternStyle.bmdSwitcherPatternStyleTopRightDiagonal},
            };
        }

        public TestWipeTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        [Fact]
        public void EnsurePattarnMapIsComplete()
        {
            EnumMap.EnsureIsComplete(PatternMap);
        }
        
        [Fact]
        public void TestWipeRate()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    uint[] testValues = {1, 18, 28, 95, 234, 244, 250};
                    uint[] badValues = {251, 255, 0};

                    ICommand Setter(uint v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.Rate,
                        Rate = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.Rate;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetRate, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetRate, Getter, badValues);
                }
            }
        }
        
        [Fact]
        public void TestWipePattern()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    Pattern[] testValues = Enum.GetValues(typeof(Pattern)).OfType<Pattern>().ToArray();

                    ICommand Setter(Pattern v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.Pattern,
                        Pattern = v,
                    };

                    Pattern? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.Pattern;

                    EnumValueComparer<Pattern, _BMDSwitcherPatternStyle>.Run(helper, PatternMap, Setter, me.Item2.GetPattern, Getter, testValues);
                }
            }
        }
        
        [Fact]
        public void TestWipeBorderSize()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.01};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.BorderWidth,
                        BorderWidth = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.BorderWidth;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetBorderSize, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetBorderSize, Getter, badValues, 100);
                }
            }
        }
        
        [Fact]
        public void TestWipeBorderInput()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    long[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(Client.Profile) && s.IsAvailable(me.Item1)).Select(s => (long)s).ToArray();
                    long[] badValues = VideoSourceLists.All.Select(s => (long)s).Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(long v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.BorderInput,
                        BorderInput = (VideoSource) v,
                    };

                    long? Getter() => (long?) helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.BorderInput;

                    ValueTypeComparer<long>.Run(helper, Setter, me.Item2.GetInputBorder, Getter, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, me.Item2.GetInputBorder, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestWipeSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    // Not all props support symmetry
                    me.Item2.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);

                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.01};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.Symmetry,
                        Symmetry = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.Symmetry;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetSymmetry, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetSymmetry, Getter, badValues, 100);
                }
            }
        }
        
        [Fact]
        public void TestWipeBorderSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.01};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.BorderSoftness,
                        BorderSoftness = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1 })?.BorderSoftness;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetSoftness, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetSoftness, Getter, badValues, 100);
                }
            }
        }
        
        [Fact]
        public void TestWipeHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    // Not all props support XPosition
                    me.Item2.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);

                    double[] testValues = { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
                    double[] badValues = { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.XPosition,
                        XPosition = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.XPosition;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetHorizontalOffset, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetHorizontalOffset, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestWipeVerticalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    // Not all props support YPosition
                    me.Item2.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);

                    double[] testValues = {0, 0.874, 0.147, 0.999, 1.00, 0.01};
                    double[] badValues = {1.001, 1.1, 1.01, -0.01, -1, -0.10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = MixEffectBlockId.One,
                        Mask = TransitionWipeSetCommand.MaskFlags.YPosition,
                        YPosition = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = MixEffectBlockId.One})?.YPosition;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetVerticalOffset, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetVerticalOffset, Getter, badValues);
                }
            }
        }
        
        [Fact]
        public void TestWipeReverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.ReverseDirection,
                        ReverseDirection = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.ReverseDirection;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetReverse, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestWipeFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.FlipFlop,
                        FlipFlop = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = me.Item1})?.FlipFlop;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetFlipFlop, Getter, testValues);
                }
            }
        }
    }
}