using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using Xunit;

namespace LibAtem.ComparisonTests
{
    public static class AtemEnumMaps
    {
        public static readonly IReadOnlyDictionary<BorderBevel, _BMDSwitcherBorderBevelOption> BorderBevelMap;
        public static readonly IReadOnlyDictionary<DVEEffect, _BMDSwitcherDVETransitionStyle> DVEStyleMap;
        public static readonly IReadOnlyDictionary<MixEffectKeyType, _BMDSwitcherKeyType> MixEffectKeyTypeMap;
        public static readonly IReadOnlyDictionary<Pattern, _BMDSwitcherPatternStyle> PatternMap;
        public static readonly IReadOnlyDictionary<StingerSource, _BMDSwitcherStingerTransitionSource> StingerSourceMap;
        public static readonly IReadOnlyDictionary<TStyle, _BMDSwitcherTransitionStyle> TransitionStyleMap;

        static AtemEnumMaps()
        {
            BorderBevelMap = new Dictionary<BorderBevel, _BMDSwitcherBorderBevelOption>()
            {
                {BorderBevel.None, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionNone},
                {BorderBevel.InOut, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut},
                {BorderBevel.In, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionIn},
                {BorderBevel.Out, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionOut},
            };

            DVEStyleMap = new Dictionary<DVEEffect, _BMDSwitcherDVETransitionStyle>()
            {
                {DVEEffect.SwooshTopLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshTopLeft},
                {DVEEffect.SwooshTop, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshTop},
                {DVEEffect.SwooshTopRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshTopRight},
                {DVEEffect.SwooshLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshLeft},
                {DVEEffect.SwooshRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshRight},
                {DVEEffect.SwooshBottomLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshBottomLeft},
                {DVEEffect.SwooshBottom, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshBottom},
                {DVEEffect.SwooshBottomRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSwooshBottomRight},

                {DVEEffect.SpinCCWTopRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCCWTopRight},
                {DVEEffect.SpinCWTopLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCWTopLeft},
                {DVEEffect.SpinCCWBottomRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCCWBottomRight},
                {DVEEffect.SpinCWBottomLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCWBottomLeft},
                {DVEEffect.SpinCWTopRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCWTopRight},
                {DVEEffect.SpinCCWTopLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCCWTopLeft},
                {DVEEffect.SpinCWBottomRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCWBottomRight},
                {DVEEffect.SpinCCWBottomLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSpinCCWBottomLeft},

                {DVEEffect.SqueezeTopLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeTopLeft},
                {DVEEffect.SqueezeTop, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeTop},
                {DVEEffect.SqueezeTopRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeTopRight},
                {DVEEffect.SqueezeLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeLeft},
                {DVEEffect.SqueezeRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeRight},
                {DVEEffect.SqueezeBottomLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeBottomLeft},
                {DVEEffect.SqueezeBottom, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeBottom},
                {DVEEffect.SqueezeBottomRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleSqueezeBottomRight},

                {DVEEffect.PushTopLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushTopLeft},
                {DVEEffect.PushTop, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushTop},
                {DVEEffect.PushTopRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushTopRight},
                {DVEEffect.PushLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushLeft},
                {DVEEffect.PushRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushRight},
                {DVEEffect.PushBottomLeft, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushBottomLeft},
                {DVEEffect.PushBottom, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushBottom},
                {DVEEffect.PushBottomRight, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStylePushBottomRight},

                {DVEEffect.GraphicCWSpin, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleGraphicCWSpin},
                {DVEEffect.GraphicCCWSpin, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleGraphicCCWSpin},
                {DVEEffect.GraphicLogoWipe, _BMDSwitcherDVETransitionStyle.bmdSwitcherDVETransitionStyleGraphicLogoWipe},
            };

            MixEffectKeyTypeMap = new Dictionary<MixEffectKeyType, _BMDSwitcherKeyType>
            {
                {MixEffectKeyType.Luma, _BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma},
                {MixEffectKeyType.Chroma, _BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma},
                {MixEffectKeyType.Pattern, _BMDSwitcherKeyType.bmdSwitcherKeyTypePattern},
                {MixEffectKeyType.DVE, _BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE},
            };

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

            StingerSourceMap = new Dictionary<StingerSource, _BMDSwitcherStingerTransitionSource>
            {
                {StingerSource.None, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceNone},
                {StingerSource.MediaPlayer1, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer1},
                {StingerSource.MediaPlayer2, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer2},
                {StingerSource.MediaPlayer3, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer3},
                {StingerSource.MediaPlayer4, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer4},
            };

            TransitionStyleMap = new Dictionary<TStyle, _BMDSwitcherTransitionStyle>
            {
                {TStyle.Mix, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix},
                {TStyle.Dip, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDip},
                {TStyle.DVE, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDVE},
                {TStyle.Stinger, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleStinger},
                {TStyle.Wipe, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleWipe},
            };
        }

        public static Tk FindByValue<Tk, Tv>(this IReadOnlyDictionary<Tk, Tv> dict, Tv value)
        {
            return dict.First(v => Equals(v.Value, value)).Key;
        }

        public static AuxiliaryId GetAuxId(VideoSource id)
        {
            if (id >= VideoSource.Auxilary1 && id <= VideoSource.Auxilary6)
                return (AuxiliaryId)(id - VideoSource.Auxilary1);

            throw new Exception("Not an Aux");
        }

        public static ColorGeneratorId GetSourceIdForGen(VideoSource id)
        {
            switch (id)
            {
                case VideoSource.Color1:
                    return ColorGeneratorId.One;
                case VideoSource.Color2:
                    return ColorGeneratorId.Two;
                default:
                    throw new Exception("Not a ColorGen");
            }
        }
    }

    public class TestAtemEnumMaps
    {
        [Fact]
        public void EnsureBorderBevelMapIsComplete()
        {
            EnumMap.EnsureIsComplete(AtemEnumMaps.BorderBevelMap);
        }

        [Fact]
        public void EnsureDVEStyleMapIsComplete()
        {
            EnumMap.EnsureIsComplete(AtemEnumMaps.DVEStyleMap);
        }

        [Fact]
        public void EnsureMixEffectKeyTypeMapIsComplete()
        {
            EnumMap.EnsureIsComplete(AtemEnumMaps.MixEffectKeyTypeMap);
        }

        [Fact]
        public void EnsurePatternMapIsComplete()
        {
            EnumMap.EnsureIsComplete(AtemEnumMaps.PatternMap);
        }

        [Fact]
        public void EnsureStingerSourceMapIsComplete()
        {
            EnumMap.EnsureIsComplete(AtemEnumMaps.StingerSourceMap);
        }

        [Fact]
        public void EnsureTransitionStyleMapIsComplete()
        {
            EnumMap.EnsureIsComplete(AtemEnumMaps.TransitionStyleMap);
        }

        [Fact]
        public void EnsureTransitionLayerIsMapped()
        {
            EnumMap.EnsureIsMatching<TransitionLayer, _BMDSwitcherTransitionSelection>();
        }
    }
}
