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
        public static readonly IReadOnlyDictionary<VideoMode, _BMDSwitcherVideoMode> VideoModesMap;
        public static readonly IReadOnlyDictionary<DownConvertMode, _BMDSwitcherDownConversionMethod> SDDownconvertModesMap;
        public static readonly IReadOnlyDictionary<SerialMode, _BMDSwitcherSerialPortFunction> SerialModeMap;
        public static readonly IReadOnlyDictionary<MultiViewLayout, _BMDSwitcherMultiViewLayout> MultiViewLayoutMap;
        public static readonly IReadOnlyDictionary<InternalPortType, _BMDSwitcherPortType> InternalPortTypeMap;
        public static readonly IReadOnlyDictionary<ExternalPortType, _BMDSwitcherExternalPortType> ExternalPortTypeMap;
        public static readonly IReadOnlyDictionary<SuperSourceArtOption, _BMDSwitcherSuperSourceArtOption> SuperSourceArtOptionMap;
        public static readonly IReadOnlyDictionary<MediaPlayerSource, _BMDSwitcherMediaPlayerSourceType> MediaPlayerSourceMap;
        public static readonly IReadOnlyDictionary<AudioMixOption, _BMDSwitcherAudioMixOption> AudioMixOptionMap;

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

            VideoModesMap = new Dictionary<VideoMode, _BMDSwitcherVideoMode>
            {
                {VideoMode.N525i5994NTSC, _BMDSwitcherVideoMode.bmdSwitcherVideoMode525i5994NTSC},
                {VideoMode.P625i50PAL, _BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL},
                {VideoMode.N525i5994169, _BMDSwitcherVideoMode.bmdSwitcherVideoMode525i5994Anamorphic},
                {VideoMode.P625i50169, _BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50Anamorphic},
                {VideoMode.P720p50, _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50},
                {VideoMode.N720p5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p5994},
                {VideoMode.P1080i50, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i50},
                {VideoMode.N1080i5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i5994},
                {VideoMode.N1080p2398, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2398},
                {VideoMode.N1080p24, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p24},
                {VideoMode.P1080p25, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p25},
                {VideoMode.N1080p2997, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2997},
                {VideoMode.P1080p50, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p50},
                {VideoMode.N1080p5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p5994},
                {VideoMode.N4KHDp2398, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2398},
                {VideoMode.N4KHDp24, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp24},
                {VideoMode.P4KHDp25, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp25},
                {VideoMode.N4KHDp2997, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2997},
                {VideoMode.P4KHDp5000, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp50},
                {VideoMode.N4KHDp5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp5994},
            };

            SDDownconvertModesMap = new Dictionary<DownConvertMode, _BMDSwitcherDownConversionMethod>()
            {
                {DownConvertMode.CentreCut, _BMDSwitcherDownConversionMethod.bmdSwitcherDownConversionMethodCentreCut},
                {DownConvertMode.Letterbox, _BMDSwitcherDownConversionMethod.bmdSwitcherDownConversionMethodLetterbox},
                {DownConvertMode.Anamorphic, _BMDSwitcherDownConversionMethod.bmdSwitcherDownConversionMethodAnamorphic},
            };

            SerialModeMap = new Dictionary<SerialMode, _BMDSwitcherSerialPortFunction>
            {
                {SerialMode.None, _BMDSwitcherSerialPortFunction.bmdSwitcherSerialPortFunctionNone},
                {SerialMode.Gvg100, _BMDSwitcherSerialPortFunction.bmdSwitcherSerialPortFunctionGvg100},
                {SerialMode.PtzVisca, _BMDSwitcherSerialPortFunction.bmdSwitcherSerialPortFunctionPtzVisca},
            };

            MultiViewLayoutMap = new Dictionary<MultiViewLayout, _BMDSwitcherMultiViewLayout>
            {
                {MultiViewLayout.ProgramBottom, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramBottom},
                {MultiViewLayout.ProgramLeft, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramLeft},
                {MultiViewLayout.ProgramRight, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramRight},
                {MultiViewLayout.ProgramTop, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramTop},
            };

            InternalPortTypeMap = new Dictionary<InternalPortType, _BMDSwitcherPortType>
            {
                {InternalPortType.Auxiliary, _BMDSwitcherPortType.bmdSwitcherPortTypeAuxOutput},
                {InternalPortType.Black, _BMDSwitcherPortType.bmdSwitcherPortTypeBlack},
                {InternalPortType.ColorBars, _BMDSwitcherPortType.bmdSwitcherPortTypeColorBars},
                {InternalPortType.ColorGenerator, _BMDSwitcherPortType.bmdSwitcherPortTypeColorGenerator},
                {InternalPortType.External, _BMDSwitcherPortType.bmdSwitcherPortTypeExternal},
                {InternalPortType.Mask, _BMDSwitcherPortType.bmdSwitcherPortTypeKeyCutOutput},
                {InternalPortType.MEOutput, _BMDSwitcherPortType.bmdSwitcherPortTypeMixEffectBlockOutput},
                {InternalPortType.MediaPlayerKey, _BMDSwitcherPortType.bmdSwitcherPortTypeMediaPlayerCut},
                {InternalPortType.MediaPlayerFill, _BMDSwitcherPortType.bmdSwitcherPortTypeMediaPlayerFill},
                {InternalPortType.SuperSource, _BMDSwitcherPortType.bmdSwitcherPortTypeSuperSource},
            };

            ExternalPortTypeMap = new Dictionary<ExternalPortType, _BMDSwitcherExternalPortType>
            {
                {ExternalPortType.Internal, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeInternal},
                {ExternalPortType.SDI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeSDI},
                {ExternalPortType.HDMI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeHDMI},
                {ExternalPortType.Composite, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeComposite},
                {ExternalPortType.Component, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeComponent},
                {ExternalPortType.SVideo, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeSVideo},

                {ExternalPortType.XLR, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeXLR},
                {ExternalPortType.AESEBU, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeAESEBU},
                {ExternalPortType.RCA, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeRCA},
                {ExternalPortType.TSJack, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeTSJack},
            };

            SuperSourceArtOptionMap = new Dictionary<SuperSourceArtOption, _BMDSwitcherSuperSourceArtOption>
            {
                {SuperSourceArtOption.Background, _BMDSwitcherSuperSourceArtOption.bmdSwitcherSuperSourceArtOptionBackground},
                {SuperSourceArtOption.Foreground, _BMDSwitcherSuperSourceArtOption.bmdSwitcherSuperSourceArtOptionForeground},
            };

            MediaPlayerSourceMap = new Dictionary<MediaPlayerSource, _BMDSwitcherMediaPlayerSourceType>
            {
                {MediaPlayerSource.Clip, _BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeClip},
                {MediaPlayerSource.Still, _BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill},
            };

            AudioMixOptionMap = new Dictionary<AudioMixOption, _BMDSwitcherAudioMixOption>
            {
                {AudioMixOption.Off, _BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOff},
                {AudioMixOption.On, _BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionOn},
                {AudioMixOption.AudioFollowVideo, _BMDSwitcherAudioMixOption.bmdSwitcherAudioMixOptionAudioFollowVideo},
            };
        }

        public static Tk FindByValue<Tk, Tv>(this IReadOnlyDictionary<Tk, Tv> dict, Tv value)
        {
            return dict.First(v => Equals(v.Value, value)).Key;
        }

        public static List<T> FindFlagComponents<T>(this T value)
        {
            dynamic val2 = value;
            return Enum.GetValues(typeof(T)).OfType<T>().Where(v => val2.HasFlag(v)).ToList();
        }

        public static Tk FindFlagsByValue<Tk, Tv>(this IReadOnlyDictionary<Tk, Tv> dict, Tv value)
        {
            int res = value.FindFlagComponents().Select(v => Convert.ToInt32(dict.FindByValue(v))).Sum();
            return (Tk) Enum.ToObject(typeof(Tk), res);
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

        public static double GetDefaultPatternSymmetry(this Pattern pattern)
        {
            switch (pattern)
            {
                case Pattern.HorizontalBarnDoor:
                case Pattern.VerticalBarnDoor:
                case Pattern.TopCentreBox:
                case Pattern.RightCentreBox:
                case Pattern.BottomCentreBox:
                case Pattern.LeftCentreBox:
                    return 100;
                case Pattern.LeftToRightBar:
                case Pattern.TopToBottomBar:
                case Pattern.CornersInFourBox:
                case Pattern.RectangleIris:
                case Pattern.DiamondIris:
                case Pattern.TopLeftBox:
                case Pattern.TopRightBox:
                case Pattern.BottomRightBox:
                case Pattern.BottomLeftBox:
                case Pattern.TopLeftDiagonal:
                case Pattern.TopRightDiagonal:
                    return 50;
                case Pattern.CircleIris:
                    return 65.5;
                default:
                    return 50;
            }
        }
    }
}
