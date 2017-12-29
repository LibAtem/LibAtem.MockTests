using System;
using System.Collections.Generic;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestWipeTransition : ComparisonTestBase
    {
        private static readonly IReadOnlyDictionary<Pattern, _BMDSwitcherPatternStyle> PatternMap;

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
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                uint[] testValues = {18, 28, 95};

                ICommand Setter(uint v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.Rate,
                    Rate = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.Rate;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetRate, Getter, testValues);
            }
        }
        
        [Fact]
        public void TestWipePattern()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                Pattern[] testValues = {Pattern.BottomLeftBox, Pattern.CircleIris, Pattern.LeftToRightBar};

                ICommand Setter(Pattern v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.Pattern,
                    Pattern = v,
                };

                Pattern? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.Pattern;

                EnumValueComparer<Pattern, _BMDSwitcherPatternStyle>.Run(helper, PatternMap, Setter, sdkProps.GetPattern, Getter, testValues);
            }
        }
        
        [Fact]
        public void TestWipeBorderSize()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                double[] testValues = {87.4, 14.7};

                ICommand Setter(double v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.BorderWidth,
                    BorderWidth = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.BorderWidth;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetBorderSize, Getter, testValues, 100);
            }
        }
        
        [Fact]
        public void TestWipeBorderInput()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                long[] testValues =
                {
                    (long) VideoSource.Color1,
                    (long) VideoSource.MediaPlayer1,
                    (long) VideoSource.Input3
                };

                ICommand Setter(long v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.BorderInput,
                    BorderInput = (VideoSource)v,
                };

                long? Getter() => (long?) helper.FindWithMatching(new TransitionWipeGetCommand {Index = MixEffectBlockId.One})?.BorderInput;

                ValueTypeComparer<long>.Run(helper, Setter, sdkProps.GetInputBorder, Getter, testValues);
            }
        }

        [Fact]
        public void TestWipeSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                // Not all props support symmetry
                sdkProps.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);


                double[] testValues = { 87.4, 14.7 };

                ICommand Setter(double v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.Symmetry,
                    Symmetry = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.Symmetry;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetSymmetry, Getter, testValues, 100);
            }
        }
        
        [Fact]
        public void TestWipeBorderSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);


                double[] testValues = { 87.4, 14.7 };

                ICommand Setter(double v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.BorderSoftness,
                    BorderSoftness = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.BorderSoftness;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetSoftness, Getter, testValues, 100);
            }
        }
        
        [Fact]
        public void TestWipeHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                // Not all props support XPosition
                sdkProps.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);


                double[] testValues = { 0.76, 0.24, 0.97};

                ICommand Setter(double v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.XPosition,
                    XPosition = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.XPosition;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetHorizontalOffset, Getter, testValues);
            }
        }

        [Fact]
        public void TestWipeVerticalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                // Not all props support YPosition
                sdkProps.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                
                double[] testValues = { 0.24, 0.37, 0.69 };

                ICommand Setter(double v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.YPosition,
                    YPosition = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.YPosition;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetVerticalOffset, Getter, testValues);
            }
        }
        
        [Fact]
        public void TestWipeReverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.ReverseDirection,
                    ReverseDirection = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand { Index = MixEffectBlockId.One })?.ReverseDirection;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetReverse, Getter, testValues);
            }
        }

        [Fact]
        public void TestWipeFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionWipeParameters>();
                Assert.NotNull(sdkProps);

                bool[] testValues = {true, false};

                ICommand Setter(bool v) => new TransitionWipeSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionWipeSetCommand.MaskFlags.FlipFlop,
                    FlipFlop = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionWipeGetCommand {Index = MixEffectBlockId.One})?.FlipFlop;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetFlipFlop, Getter, testValues);
            }
        }
    }
}