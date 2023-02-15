﻿using BMDSwitcherAPI;
using LibAtem.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using LibAtem.Util;

namespace LibAtem.MockTests.SdkState
{
    public static class AtemEnumMaps
    {
        public static readonly IReadOnlyDictionary<BorderBevel, _BMDSwitcherBorderBevelOption> BorderBevelMap;
        public static readonly IReadOnlyDictionary<DVEEffect, _BMDSwitcherDVETransitionStyle> DVEStyleMap;
        public static readonly IReadOnlyDictionary<MixEffectKeyType, _BMDSwitcherKeyType> MixEffectKeyTypeMap;
        public static readonly IReadOnlyDictionary<Pattern, _BMDSwitcherPatternStyle> PatternMap;
        public static readonly IReadOnlyDictionary<StingerSource, _BMDSwitcherStingerTransitionSource> StingerSourceMap;
        public static readonly IReadOnlyDictionary<TransitionStyle, _BMDSwitcherTransitionStyle> TransitionStyleMap;
        public static readonly IReadOnlyDictionary<VideoMode, _BMDSwitcherVideoMode> VideoModesMap;
        public static readonly IReadOnlyDictionary<DownConvertMode, _BMDSwitcherDownConversionMethod> SDDownconvertModesMap;
        public static readonly IReadOnlyDictionary<SerialMode, _BMDSwitcherSerialPortFunction> SerialModeMap;
        public static readonly IReadOnlyDictionary<MultiViewLayoutV8, _BMDSwitcherMultiViewLayout> MultiViewLayoutMap;
        public static readonly IReadOnlyDictionary<InternalPortType, _BMDSwitcherPortType> InternalPortTypeMap;
        public static readonly IReadOnlyDictionary<VideoPortType, _BMDSwitcherExternalPortType> VideoPortTypeMap;
        public static readonly IReadOnlyDictionary<AudioPortType, _BMDSwitcherExternalPortType> AudioPortTypeMap;
        public static readonly IReadOnlyDictionary<SuperSourceArtOption, _BMDSwitcherSuperSourceArtOption> SuperSourceArtOptionMap;
        public static readonly IReadOnlyDictionary<MediaPlayerSource, _BMDSwitcherMediaPlayerSourceType> MediaPlayerSourceMap;
        public static readonly IReadOnlyDictionary<AudioMixOption, _BMDSwitcherAudioMixOption> AudioMixOptionMap;
        public static readonly IReadOnlyDictionary<AudioSourceType, _BMDSwitcherAudioInputType> AudioSourceTypeMap;
        public static readonly IReadOnlyDictionary<FairlightEqualizerBandShape, _BMDSwitcherFairlightAudioEqualizerBandShape> FairlightEqualizerBandShapeMap;
        public static readonly IReadOnlyDictionary<FairlightEqualizerFrequencyRange, _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange> FairlightEqualizerFrequencyRangeMap;
        public static readonly IReadOnlyDictionary<FairlightInputType, _BMDSwitcherFairlightAudioInputType> FairlightInputTypeMap;
        public static readonly IReadOnlyDictionary<FairlightInputConfiguration, _BMDSwitcherFairlightAudioInputConfiguration> FairlightInputConfigurationMap;
        public static readonly IReadOnlyDictionary<FairlightAudioMixOption, _BMDSwitcherFairlightAudioMixOption> FairlightAudioMixOptionMap;
        public static readonly IReadOnlyDictionary<FairlightAudioSourceType, _BMDSwitcherFairlightAudioSourceType> FairlightAudioSourceTypeMap;
        public static readonly IReadOnlyDictionary<FairlightAnalogInputLevel, _BMDSwitcherFairlightAudioAnalogInputLevel> FairlightAnalogInputLevelMap;
        public static readonly IReadOnlyDictionary<SDI3GOutputLevel, _BMDSwitcher3GSDIOutputLevel> SDI3GOutputLevelMap;
        public static readonly IReadOnlyDictionary<TalkbackChannel, _BMDSwitcherTalkbackId> TalkbackChannelMap;
        public static readonly IReadOnlyDictionary<MixMinusMode, _BMDSwitcherMixMinusOutputAudioMode> MixMinusModeMap;
        public static readonly IReadOnlyDictionary<HyperDeckPlayerState, _BMDSwitcherHyperDeckPlayerState> HyperDeckPlayerStateMap;
        public static readonly IReadOnlyDictionary<HyperDeckConnectionStatus, _BMDSwitcherHyperDeckConnectionStatus> HyperDeckConnectionStatusMap;
        public static readonly IReadOnlyDictionary<HyperDeckStorageStatus, _BMDSwitcherHyperDeckStorageMediaState> HyperDeckStorageStatusMap;

#if !ATEM_v8_1
        public static readonly IReadOnlyDictionary<TimeCodeMode, _BMDSwitcherTimeCodeMode> TimeCodeModeMap;

        public static readonly IReadOnlyDictionary<StreamingStatus, _BMDSwitcherStreamRTMPState> StreamingStatusMap;
        public static readonly IReadOnlyDictionary<StreamingError, _BMDSwitcherStreamRTMPError> StreamingErrorMap;
        public static readonly IReadOnlyDictionary<RecordingDiskStatus, _BMDSwitcherRecordDiskStatus> RecordingDiskStatusMap;
        public static readonly IReadOnlyDictionary<RecordingStatus, _BMDSwitcherRecordAVState> RecordingStateMap;
        public static readonly IReadOnlyDictionary<RecordingError, _BMDSwitcherRecordAVError> RecordingErrorMap;
        public static readonly IReadOnlyDictionary<DisplayCounterClockMode, _BMDSwitcherDisplayClockMode> DisplayClockModeMap;
        public static readonly IReadOnlyDictionary<DisplayCounterClockState, _BMDSwitcherDisplayClockState> DisplayClockStateMap;
#endif

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

            TransitionStyleMap = new Dictionary<TransitionStyle, _BMDSwitcherTransitionStyle>
            {
                {TransitionStyle.Mix, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix},
                {TransitionStyle.Dip, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDip},
                {TransitionStyle.DVE, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDVE},
                {TransitionStyle.Stinger, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleStinger},
                {TransitionStyle.Wipe, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleWipe},
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
                {VideoMode.N8KHDp2398, _BMDSwitcherVideoMode.bmdSwitcherVideoMode8KHDp2398},
                {VideoMode.N8KHDp24, _BMDSwitcherVideoMode.bmdSwitcherVideoMode8KHDp24},
                {VideoMode.P8KHDp25, _BMDSwitcherVideoMode.bmdSwitcherVideoMode8KHDp25},
                {VideoMode.N8KHDp2997, _BMDSwitcherVideoMode.bmdSwitcherVideoMode8KHDp2997},
                {VideoMode.P8KHDp50, _BMDSwitcherVideoMode.bmdSwitcherVideoMode8KHDp50},
                {VideoMode.N8KHDp5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode8KHDp5994},
                {VideoMode.N1080p30, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p30},
                {VideoMode.N1080p60, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p60},
                {VideoMode.N720p60, _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p60},
                {VideoMode.N1080i60, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i60}
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

            MultiViewLayoutMap = new Dictionary<MultiViewLayoutV8, _BMDSwitcherMultiViewLayout>
            {
                {MultiViewLayoutV8.ProgramBottom, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramBottom},
                {MultiViewLayoutV8.ProgramLeft, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramLeft},
                {MultiViewLayoutV8.ProgramRight, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramRight},
                {MultiViewLayoutV8.ProgramTop, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramTop},

                {MultiViewLayoutV8.TopLeftSmall, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutTopLeftSmall},
                {MultiViewLayoutV8.TopRightSmall, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutTopRightSmall},
                {MultiViewLayoutV8.BottomLeftSmall, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutBottomLeftSmall},
                {MultiViewLayoutV8.BottomRightSmall, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutBottomRightSmall},
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
#if !ATEM_v8_1
                {InternalPortType.MultiViewer, _BMDSwitcherPortType.bmdSwitcherPortTypeMultiview},
                {InternalPortType.ExternalDirect, _BMDSwitcherPortType.bmdSwitcherPortTypeExternalDirect},
#endif
            };

            VideoPortTypeMap = new Dictionary<VideoPortType, _BMDSwitcherExternalPortType>
            {
                // TODO - limited test
                {VideoPortType.Internal, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeInternal},
                {VideoPortType.SDI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeSDI},
                {VideoPortType.HDMI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeHDMI},
                {VideoPortType.Composite, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeComposite},
                {VideoPortType.Component, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeComponent},
                {VideoPortType.SVideo, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeSVideo},
            };
            AudioPortTypeMap = new Dictionary<AudioPortType, _BMDSwitcherExternalPortType>
            {
                // TODO - limited test
                {AudioPortType.SDI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeSDI},
                {AudioPortType.HDMI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeHDMI},
                {AudioPortType.XLR, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeXLR},
                {AudioPortType.AESEBU, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeAESEBU},
                {AudioPortType.RCA, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeRCA},
                {AudioPortType.Internal, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeInternal},
                {AudioPortType.TSJack, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeTSJack},
                {AudioPortType.MADI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeMADI},
                {AudioPortType.TRSJack, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeTRS},
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

            AudioSourceTypeMap = new Dictionary<AudioSourceType, _BMDSwitcherAudioInputType>
            {
                {AudioSourceType.ExternalAudio, _BMDSwitcherAudioInputType.bmdSwitcherAudioInputTypeAudioIn},
                {AudioSourceType.ExternalVideo, _BMDSwitcherAudioInputType.bmdSwitcherAudioInputTypeEmbeddedWithVideo},
                {AudioSourceType.MediaPlayer, _BMDSwitcherAudioInputType.bmdSwitcherAudioInputTypeMediaPlayer},
            };

            FairlightEqualizerBandShapeMap = new Dictionary<FairlightEqualizerBandShape, _BMDSwitcherFairlightAudioEqualizerBandShape>
            {
                {FairlightEqualizerBandShape.LowShelf, _BMDSwitcherFairlightAudioEqualizerBandShape.bmdSwitcherFairlightAudioEqualizerBandShapeLowShelf},
                {FairlightEqualizerBandShape.LowPass, _BMDSwitcherFairlightAudioEqualizerBandShape.bmdSwitcherFairlightAudioEqualizerBandShapeLowPass},
                {FairlightEqualizerBandShape.BandPass, _BMDSwitcherFairlightAudioEqualizerBandShape.bmdSwitcherFairlightAudioEqualizerBandShapeBandPass},
                {FairlightEqualizerBandShape.Notch, _BMDSwitcherFairlightAudioEqualizerBandShape.bmdSwitcherFairlightAudioEqualizerBandShapeNotch},
                {FairlightEqualizerBandShape.HighPass, _BMDSwitcherFairlightAudioEqualizerBandShape.bmdSwitcherFairlightAudioEqualizerBandShapeHighPass},
                {FairlightEqualizerBandShape.HighShelf, _BMDSwitcherFairlightAudioEqualizerBandShape.bmdSwitcherFairlightAudioEqualizerBandShapeHighShelf},
            };
            FairlightEqualizerFrequencyRangeMap = new Dictionary<FairlightEqualizerFrequencyRange, _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange>
            {
                {FairlightEqualizerFrequencyRange.Low, _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange.bmdSwitcherFairlightAudioEqualizerBandFrequencyRangeLow},
                {FairlightEqualizerFrequencyRange.MidLow, _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange.bmdSwitcherFairlightAudioEqualizerBandFrequencyRangeMidLow},
                {FairlightEqualizerFrequencyRange.MidHigh, _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange.bmdSwitcherFairlightAudioEqualizerBandFrequencyRangeMidHigh},
                {FairlightEqualizerFrequencyRange.High, _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange.bmdSwitcherFairlightAudioEqualizerBandFrequencyRangeHigh},
            };
            FairlightInputTypeMap = new Dictionary<FairlightInputType, _BMDSwitcherFairlightAudioInputType>
            {
                {FairlightInputType.AudioIn, _BMDSwitcherFairlightAudioInputType.bmdSwitcherFairlightAudioInputTypeAudioIn},
                {FairlightInputType.EmbeddedWithVideo, _BMDSwitcherFairlightAudioInputType.bmdSwitcherFairlightAudioInputTypeEmbeddedWithVideo},
                {FairlightInputType.MADI, _BMDSwitcherFairlightAudioInputType.bmdSwitcherFairlightAudioInputTypeMADI},
                {FairlightInputType.MediaPlayer, _BMDSwitcherFairlightAudioInputType.bmdSwitcherFairlightAudioInputTypeMediaPlayer},
            };
            FairlightInputConfigurationMap = new Dictionary<FairlightInputConfiguration, _BMDSwitcherFairlightAudioInputConfiguration>
            {
                {FairlightInputConfiguration.Mono, _BMDSwitcherFairlightAudioInputConfiguration.bmdSwitcherFairlightAudioInputConfigurationMono},
                {FairlightInputConfiguration.Stereo, _BMDSwitcherFairlightAudioInputConfiguration.bmdSwitcherFairlightAudioInputConfigurationStereo},
                {FairlightInputConfiguration.DualMono, _BMDSwitcherFairlightAudioInputConfiguration.bmdSwitcherFairlightAudioInputConfigurationDualMono},
            };
            FairlightAudioMixOptionMap = new Dictionary<FairlightAudioMixOption, _BMDSwitcherFairlightAudioMixOption>{
                {FairlightAudioMixOption.Off, _BMDSwitcherFairlightAudioMixOption.bmdSwitcherFairlightAudioMixOptionOff},
                {FairlightAudioMixOption.On, _BMDSwitcherFairlightAudioMixOption.bmdSwitcherFairlightAudioMixOptionOn},
                {FairlightAudioMixOption.AudioFollowVideo, _BMDSwitcherFairlightAudioMixOption.bmdSwitcherFairlightAudioMixOptionAudioFollowVideo},
            };
            FairlightAudioSourceTypeMap = new Dictionary<FairlightAudioSourceType, _BMDSwitcherFairlightAudioSourceType>
            {
                {FairlightAudioSourceType.Mono, _BMDSwitcherFairlightAudioSourceType.bmdSwitcherFairlightAudioSourceTypeMono},
                {FairlightAudioSourceType.Stereo, _BMDSwitcherFairlightAudioSourceType.bmdSwitcherFairlightAudioSourceTypeStereo},
            };
            FairlightAnalogInputLevelMap = new Dictionary<FairlightAnalogInputLevel, _BMDSwitcherFairlightAudioAnalogInputLevel>
            {
                {FairlightAnalogInputLevel.Microphone, _BMDSwitcherFairlightAudioAnalogInputLevel.bmdSwitcherFairlightAudioAnalogInputLevelMicrophone},
                {FairlightAnalogInputLevel.ConsumerLine, _BMDSwitcherFairlightAudioAnalogInputLevel.bmdSwitcherFairlightAudioAnalogInputLevelConsumerLine},
#if !ATEM_v8_1
                {FairlightAnalogInputLevel.ProLine, _BMDSwitcherFairlightAudioAnalogInputLevel.bmdSwitcherFairlightAudioAnalogInputLevelProLine },
#endif
            };

            SDI3GOutputLevelMap = new Dictionary<SDI3GOutputLevel, _BMDSwitcher3GSDIOutputLevel>
            {
                {SDI3GOutputLevel.LevelA, _BMDSwitcher3GSDIOutputLevel.bmdSwitcher3GSDIOutputLevelA},
                {SDI3GOutputLevel.LevelB, _BMDSwitcher3GSDIOutputLevel.bmdSwitcher3GSDIOutputLevelB},
            };

            TalkbackChannelMap = new Dictionary<TalkbackChannel, _BMDSwitcherTalkbackId>
            {
                {TalkbackChannel.Production, _BMDSwitcherTalkbackId.bmdSwitcherTalkbackIdProduction},
                {TalkbackChannel.Engineering, _BMDSwitcherTalkbackId.bmdSwitcherTalkbackIdEngineering},
            };

            MixMinusModeMap = new Dictionary<MixMinusMode, _BMDSwitcherMixMinusOutputAudioMode>
            {
                {MixMinusMode.ProgramOut, _BMDSwitcherMixMinusOutputAudioMode.bmdSwitcherMixMinusOutputAudioModeProgramOut},
                {MixMinusMode.MixMinus, _BMDSwitcherMixMinusOutputAudioMode.bmdSwitcherMixMinusOutputAudioModeMixMinus}
            };

            HyperDeckPlayerStateMap = new Dictionary<HyperDeckPlayerState, _BMDSwitcherHyperDeckPlayerState>
            {
                {HyperDeckPlayerState.Idle, _BMDSwitcherHyperDeckPlayerState.bmdSwitcherHyperDeckStateIdle},
                {HyperDeckPlayerState.Playing, _BMDSwitcherHyperDeckPlayerState.bmdSwitcherHyperDeckStatePlay},
                {HyperDeckPlayerState.Recording, _BMDSwitcherHyperDeckPlayerState.bmdSwitcherHyperDeckStateRecord},
                {HyperDeckPlayerState.Shuttle, _BMDSwitcherHyperDeckPlayerState.bmdSwitcherHyperDeckStateShuttle},
                // {HyperDeckPlayerState.UUUU, _BMDSwitcherHyperDeckPlayerState.bmdSwitcherHyperDeckStateUnknown},
            };
            HyperDeckConnectionStatusMap = new Dictionary<HyperDeckConnectionStatus, _BMDSwitcherHyperDeckConnectionStatus>
            {
                {HyperDeckConnectionStatus.Connected, _BMDSwitcherHyperDeckConnectionStatus.bmdSwitcherHyperDeckConnectionStatusConnected},
                {HyperDeckConnectionStatus.Connecting, _BMDSwitcherHyperDeckConnectionStatus.bmdSwitcherHyperDeckConnectionStatusConnecting},
                {HyperDeckConnectionStatus.NotConnected, _BMDSwitcherHyperDeckConnectionStatus.bmdSwitcherHyperDeckConnectionStatusNotConnected},
                {HyperDeckConnectionStatus.Incompatible, _BMDSwitcherHyperDeckConnectionStatus.bmdSwitcherHyperDeckConnectionStatusIncompatible},
            };
            HyperDeckStorageStatusMap = new Dictionary<HyperDeckStorageStatus, _BMDSwitcherHyperDeckStorageMediaState>
            {
                {HyperDeckStorageStatus.Ready, _BMDSwitcherHyperDeckStorageMediaState.bmdSwitcherHyperDeckStorageMediaStateReady},
                {HyperDeckStorageStatus.Unavailable, _BMDSwitcherHyperDeckStorageMediaState.bmdSwitcherHyperDeckStorageMediaStateUnavailable},
            };

#if !ATEM_v8_1
            TimeCodeModeMap = new Dictionary<TimeCodeMode, _BMDSwitcherTimeCodeMode>
            {
                {TimeCodeMode.FreeRun, _BMDSwitcherTimeCodeMode.bmdSwitcherTimeCodeModeFreeRun},
                {TimeCodeMode.TimeOfDay, _BMDSwitcherTimeCodeMode.bmdSwitcherTimeCodeModeTimeOfDay},
            };

            StreamingStatusMap = new Dictionary<StreamingStatus, _BMDSwitcherStreamRTMPState>
            {
                {StreamingStatus.Idle, _BMDSwitcherStreamRTMPState.bmdSwitcherStreamRTMPStateIdle},
                {StreamingStatus.Connecting, _BMDSwitcherStreamRTMPState.bmdSwitcherStreamRTMPStateConnecting},
                {StreamingStatus.Streaming, _BMDSwitcherStreamRTMPState.bmdSwitcherStreamRTMPStateStreaming},
                {StreamingStatus.Stopping, _BMDSwitcherStreamRTMPState.bmdSwitcherStreamRTMPStateStopping},
            };
            StreamingErrorMap = new Dictionary<StreamingError, _BMDSwitcherStreamRTMPError>
            {
                {StreamingError.None, _BMDSwitcherStreamRTMPError.bmdSwitcherStreamRTMPErrorNone},
                {StreamingError.InvalidState, _BMDSwitcherStreamRTMPError.bmdSwitcherStreamRTMPErrorInvalidState},
                {StreamingError.Unknown, _BMDSwitcherStreamRTMPError.bmdSwitcherStreamRTMPErrorUnknown},
            };
            RecordingStateMap = new Dictionary<RecordingStatus, _BMDSwitcherRecordAVState>
            {
                {RecordingStatus.Idle, _BMDSwitcherRecordAVState.bmdSwitcherRecordAVStateIdle},
                {RecordingStatus.Recording, _BMDSwitcherRecordAVState.bmdSwitcherRecordAVStateRecording},
                {RecordingStatus.Stopping, _BMDSwitcherRecordAVState.bmdSwitcherRecordAVStateStopping},
            };
            RecordingErrorMap = new Dictionary<RecordingError, _BMDSwitcherRecordAVError>
            {
                {RecordingError.None, _BMDSwitcherRecordAVError.bmdSwitcherRecordAVErrorNone},
                {RecordingError.NoMedia, _BMDSwitcherRecordAVError.bmdSwitcherRecordAVErrorNoMedia},
                {RecordingError.MediaFull, _BMDSwitcherRecordAVError.bmdSwitcherRecordAVErrorMediaFull},
                {RecordingError.MediaError, _BMDSwitcherRecordAVError.bmdSwitcherRecordAVErrorMediaError},
                {RecordingError.MediaUnformatted, _BMDSwitcherRecordAVError.bmdSwitcherRecordAVErrorMediaUnformatted},
                {RecordingError.DroppingFrames, _BMDSwitcherRecordAVError.bmdSwitcherRecordAVErrorDroppingFrames},
                {RecordingError.Unknown, _BMDSwitcherRecordAVError.bmdSwitcherRecordAVErrorUnknown},
            };
            RecordingDiskStatusMap = new Dictionary<RecordingDiskStatus, _BMDSwitcherRecordDiskStatus>
            {
                {RecordingDiskStatus.Idle, _BMDSwitcherRecordDiskStatus.bmdSwitcherRecordDiskIdle},
                {RecordingDiskStatus.Unformatted, _BMDSwitcherRecordDiskStatus.bmdSwitcherRecordDiskUnformatted},
                {RecordingDiskStatus.Active, _BMDSwitcherRecordDiskStatus.bmdSwitcherRecordDiskActive},
                {RecordingDiskStatus.Recording, _BMDSwitcherRecordDiskStatus.bmdSwitcherRecordDiskRecording},
            };
            DisplayClockModeMap = new Dictionary<DisplayCounterClockMode, _BMDSwitcherDisplayClockMode>
            {
                {DisplayCounterClockMode.Countdown, _BMDSwitcherDisplayClockMode.bmdSwitcherDisplayClockModeCountdown},
                {DisplayCounterClockMode.Countup, _BMDSwitcherDisplayClockMode.bmdSwitcherDisplayClockModeCountup},
                {DisplayCounterClockMode.TimeOfDay, _BMDSwitcherDisplayClockMode.bmdSwitcherDisplayClockModeTimeOfDay},
            };
            DisplayClockStateMap = new Dictionary<DisplayCounterClockState, _BMDSwitcherDisplayClockState>
            {
                {DisplayCounterClockState.Reset, _BMDSwitcherDisplayClockState.bmdSwitcherDisplayClockStateReset},
                {DisplayCounterClockState.Stopped, _BMDSwitcherDisplayClockState.bmdSwitcherDisplayClockStateStopped},
                {DisplayCounterClockState.Running, _BMDSwitcherDisplayClockState.bmdSwitcherDisplayClockStateRunning},
            };
#endif
        }

        public static Tk FindByValue<Tk, Tv>(this IReadOnlyDictionary<Tk, Tv> dict, Tv value)
        {
            return dict.First(v => Equals(v.Value, value)).Key;
        }

        public static List<T> FindFlagComponents<T>(this T value) where T : Enum
        {
            return EnumExtensions.FindFlagComponents(value);
        }

        public static List<Tk> FindFlagsComponentsByValue<Tk, Tv>(this IReadOnlyDictionary<Tk, Tv> dict, Tv value) where Tv : Enum
        {
            return value.FindFlagComponents().Select(dict.FindByValue).ToList();
        }

        public static Tk FindFlagsByValue<Tk, Tv>(this IReadOnlyDictionary<Tk, Tv> dict, Tv value) where Tv : Enum
        {
            int res = value.FindFlagComponents().Select(v => Convert.ToInt32(dict.FindByValue(v))).Sum();
            return (Tk)Enum.ToObject(typeof(Tk), res);
        }

        public static uint GetAuxId(VideoSource id)
        {
            if (id >= VideoSource.Auxilary1 && id <= VideoSource.Auxilary24)
                return (uint)(id - VideoSource.Auxilary1);

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

        public static Tuple<SourceAvailability, MeAvailability> TranslateSourceAvailability(_BMDSwitcherInputAvailability sdkInput)
        {
            var source = SourceAvailability.None;
            var me = MeAvailability.None;

            foreach (_BMDSwitcherInputAvailability val in sdkInput.FindFlagComponents())
            {
                switch (val) {
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0:
                        me |= MeAvailability.Me1;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock1:
                        me |= MeAvailability.Me2;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock2:
                        me |= MeAvailability.Me3;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock3:
                        me |= MeAvailability.Me4;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityAuxOutputs:
                        source |= SourceAvailability.Auxiliary;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMultiView:
                        source |= SourceAvailability.Multiviewer;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt:
                        source |= SourceAvailability.SuperSourceArt;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceBox:
                        source |= SourceAvailability.SuperSourceBox;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut:
                        source |= SourceAvailability.KeySource;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityAux1Output:
                        source |= SourceAvailability.Aux1Output;
                        break;
                    case _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityAux2Output:
                        source |= SourceAvailability.Aux2Output;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            return Tuple.Create(source, me);
        }
    }
}
