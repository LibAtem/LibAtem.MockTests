using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibAtem.MockTests.SdkState
{
    public static class MixEffectStateBuilder
    {
        public static bool SupportsAdvancedChromaKeyers(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(switcher.CreateIterator);
            var me = AtemSDKConverter.ToList<IBMDSwitcherMixEffectBlock>(iterator.Next).FirstOrDefault();
            if (me == null) return false;

            var iterator2 = AtemSDKConverter.CastSdk<IBMDSwitcherKeyIterator>(me.CreateIterator);
            var key = AtemSDKConverter.ToList<IBMDSwitcherKey>(iterator2.Next).FirstOrDefault();
            if (key == null) return false;

            key.DoesSupportAdvancedChroma(out int supported);
            return supported != 0;
        }

        public static IReadOnlyList<MixEffectState> Build(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(switcher.CreateIterator);
            return AtemSDKConverter.IterateList<IBMDSwitcherMixEffectBlock, MixEffectState>(iterator.Next, (me, id) => BuildOne(me));
        }

        private static MixEffectState BuildOne(IBMDSwitcherMixEffectBlock props)
        {
            var state = new MixEffectState();

            props.GetProgramInput(out long program);
            state.Sources.Program = (VideoSource)program;
            props.GetPreviewInput(out long preview);
            state.Sources.Preview = (VideoSource)preview;
            props.GetFadeToBlackFramesRemaining(out uint frames);
            state.FadeToBlack.Status.RemainingFrames = frames;
            props.GetFadeToBlackRate(out uint rate);
            state.FadeToBlack.Properties.Rate = rate;
            props.GetFadeToBlackFullyBlack(out int isFullyBlack);
            state.FadeToBlack.Status.IsFullyBlack = isFullyBlack != 0;
            props.GetFadeToBlackInTransition(out int inTransition);
            state.FadeToBlack.Status.InTransition = inTransition != 0;

            if (props is IBMDSwitcherTransitionParameters trans)
            {
                BuildTransition(state.Transition, trans);

                props.GetPreviewTransition(out int previewTrans);
                state.Transition.Properties.Preview = previewTrans != 0;
                props.GetPreviewLive(out int previewLive);
                state.Transition.Properties.IsInPreview = previewLive != 0;

                props.GetTransitionPosition(out double position);
                state.Transition.Position.HandlePosition = position;
                props.GetTransitionFramesRemaining(out uint framesRemaining);
                state.Transition.Position.RemainingFrames = framesRemaining;
                props.GetInTransition(out int inTransition2);
                state.Transition.Position.InTransition = inTransition2 != 0;
            }

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherKeyIterator>(props.CreateIterator);
            state.Keyers = AtemSDKConverter.IterateList<IBMDSwitcherKey, MixEffectState.KeyerState>(iterator.Next,
                (keyer, id) => BuildKeyer(keyer));

            return state;
        }

        private static void BuildTransition(MixEffectState.TransitionState state, IBMDSwitcherTransitionParameters props)
        {
            props.GetTransitionStyle(out _BMDSwitcherTransitionStyle style);
            state.Properties.Style = AtemEnumMaps.TransitionStyleMap.FindByValue(style);
            props.GetNextTransitionStyle(out _BMDSwitcherTransitionStyle nextStyle);
            state.Properties.NextStyle = AtemEnumMaps.TransitionStyleMap.FindByValue(nextStyle);
            props.GetTransitionSelection(out _BMDSwitcherTransitionSelection selection);
            state.Properties.Selection = (TransitionLayer)selection;
            props.GetNextTransitionSelection(out _BMDSwitcherTransitionSelection nextSelection);
            state.Properties.NextSelection = (TransitionLayer)nextSelection;

            if (props is IBMDSwitcherTransitionMixParameters mix)
                state.Mix = BuildTransitionMix(mix);

            if (props is IBMDSwitcherTransitionDipParameters dip)
                state.Dip = BuildTransitionDip(dip);

            if (props is IBMDSwitcherTransitionWipeParameters wipe)
                state.Wipe = BuildTransitionWipe(wipe);

            if (props is IBMDSwitcherTransitionStingerParameters stinger)
                state.Stinger = BuildTransitionStinger(stinger);

            if (props is IBMDSwitcherTransitionDVEParameters dve)
                state.DVE = BuildTransitionDVE(dve);
        }

        private static MixEffectState.TransitionMixState BuildTransitionMix(IBMDSwitcherTransitionMixParameters props)
        {
            var state = new MixEffectState.TransitionMixState();

            props.GetRate(out uint rate);
            state.Rate = rate;

            return state;
        }

        private static MixEffectState.TransitionDipState BuildTransitionDip(IBMDSwitcherTransitionDipParameters props)
        {
            var state = new MixEffectState.TransitionDipState();

            props.GetRate(out uint rate);
            state.Rate = rate;
            props.GetInputDip(out long input);
            state.Input = (VideoSource)input;

            return state;
        }

        private static MixEffectState.TransitionWipeState BuildTransitionWipe(IBMDSwitcherTransitionWipeParameters props)
        {
            var state = new MixEffectState.TransitionWipeState();

            props.GetRate(out uint rate);
            state.Rate = rate;
            props.GetPattern(out _BMDSwitcherPatternStyle pattern);
            state.Pattern = AtemEnumMaps.PatternMap.FindByValue(pattern);
            props.GetBorderSize(out double size);
            state.BorderWidth = size * 100;
            props.GetInputBorder(out long input);
            state.BorderInput = (VideoSource)input;
            props.GetSymmetry(out double symmetry);
            state.Symmetry = symmetry * 100;
            props.GetSoftness(out double soft);
            state.BorderSoftness = soft * 100;
            props.GetHorizontalOffset(out double xPos);
            state.XPosition = xPos;
            props.GetVerticalOffset(out double yPos);
            state.YPosition = yPos;
            props.GetReverse(out int reverse);
            state.ReverseDirection = reverse != 0;
            props.GetFlipFlop(out int flipflop);
            state.FlipFlop = flipflop != 0;

            return state;
        }

        private static MixEffectState.TransitionStingerState BuildTransitionStinger(IBMDSwitcherTransitionStingerParameters props)
        {
            var state = new MixEffectState.TransitionStingerState();
            
            props.GetSource(out _BMDSwitcherStingerTransitionSource src);
            state.Source = AtemEnumMaps.StingerSourceMap.FindByValue(src);
            props.GetPreMultiplied(out int preMultiplied);
            state.PreMultipliedKey = preMultiplied != 0;
            props.GetClip(out double clip);
            state.Clip = clip * 100;
            props.GetGain(out double gain);
            state.Gain = gain * 100;
            props.GetInverse(out int inverse);
            state.Invert = inverse != 0;
            props.GetPreroll(out uint preroll);
            state.Preroll = preroll;
            props.GetClipDuration(out uint duration);
            state.ClipDuration = duration;
            props.GetTriggerPoint(out uint trigger);
            state.TriggerPoint = trigger;
            props.GetMixRate(out uint mixrate);
            state.MixRate = mixrate;

            return state;
        }

        private static MixEffectState.TransitionDVEState BuildTransitionDVE(IBMDSwitcherTransitionDVEParameters props)
        {
            var state = new MixEffectState.TransitionDVEState();

            props.GetRate(out uint rate);
            state.Rate = rate;
            props.GetLogoRate(out uint logoRate);
            state.LogoRate = logoRate;
            props.GetReverse(out int reverse);
            state.Reverse = reverse != 0;
            props.GetFlipFlop(out int flipflop);
            state.FlipFlop = flipflop != 0;
            props.GetStyle(out _BMDSwitcherDVETransitionStyle style);
            state.Style = AtemEnumMaps.DVEStyleMap.FindByValue(style);
            props.GetInputFill(out long input);
            state.FillSource = (VideoSource)input;
            props.GetInputCut(out long inputCut);
            state.KeySource = (VideoSource)inputCut;
            props.GetEnableKey(out int enable);
            state.EnableKey = enable != 0;
            props.GetPreMultiplied(out int preMultiplied);
            state.PreMultiplied = preMultiplied != 0;
            props.GetClip(out double clip);
            state.Clip = clip * 100;
            props.GetGain(out double gain);
            state.Gain = gain * 100;
            props.GetInverse(out int inverse);
            state.InvertKey = inverse != 0;

            return state;
        }

        private static MixEffectState.KeyerState BuildKeyer(IBMDSwitcherKey props)
        {
            var state = new MixEffectState.KeyerState();

            
            props.GetType(out _BMDSwitcherKeyType type);
            state.Properties.KeyType = AtemEnumMaps.MixEffectKeyTypeMap.FindByValue(type);
            props.GetInputCut(out long inputCut);
            state.Properties.CutSource = (VideoSource)inputCut;
            props.GetInputFill(out long input);
            state.Properties.FillSource = (VideoSource)input;
            props.GetOnAir(out int onAir);
            state.OnAir = onAir != 0;
            props.GetMasked(out int masked);
            state.Properties.MaskEnabled = masked != 0;
            props.GetMaskTop(out double top);
            state.Properties.MaskTop = top;
            props.GetMaskBottom(out double bottom);
            state.Properties.MaskBottom = bottom;
            props.GetMaskLeft(out double left);
            state.Properties.MaskLeft = left;
            props.GetMaskRight(out double right);
            state.Properties.MaskRight = right;
            
            if (props is IBMDSwitcherKeyLumaParameters luma)
                state.Luma = BuildKeyerLuma(luma);

            if (props is IBMDSwitcherKeyChromaParameters chroma)
                state.Chroma = BuildKeyerChroma(chroma);

            if (props is IBMDSwitcherKeyAdvancedChromaParameters advancedChroma)
                state.AdvancedChroma = BuildKeyerAdvancedChroma(advancedChroma);

            if (props is IBMDSwitcherKeyPatternParameters pattern)
                state.Pattern = BuildKeyerPattern(pattern);

            if (props is IBMDSwitcherKeyDVEParameters dve)
                state.DVE = BuildKeyerDVE(dve);

            if (props is IBMDSwitcherKeyFlyParameters fly && state.DVE != null)
            {
                BuildKeyerFly(state.DVE, fly);

                fly.GetFly(out int isFlyKey);
                state.Properties.FlyEnabled = isFlyKey != 0;
                fly.GetCanFly(out int canFly);
                state.Properties.CanFlyKey = canFly != 0;

                state.FlyProperties = new MixEffectState.KeyerFlyProperties();

                fly.IsKeyFrameStored(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out int aStored);
                state.FlyProperties.IsASet = aStored != 0;
                fly.IsKeyFrameStored(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out int bStored);
                state.FlyProperties.IsBSet = bStored != 0;

                // This is a pretty meaningless value, as it is really the LastRunKeyFrame
                // fly.IsAtKeyFrames(out _BMDSwitcherFlyKeyFrame isAtFrame); 
                // state.FlyProperties.IsAtKeyFrame = (uint) isAtFrame;

                fly.IsRunning(out int isRunning, out _BMDSwitcherFlyKeyFrame destination);
                state.FlyProperties.RunningToInfinite = 0;
                if (isRunning == 0)
                {
                    state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.None;
                }
                else
                {
                    switch (destination)
                    {
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.Full;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityCentreOfKey:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.CentreOfKey;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityTopLeft:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.TopLeft;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityTop:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.TopCentre;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityTopRight:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.TopRight;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityLeft:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.MiddleLeft;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityCentre:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.MiddleCentre;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityRight:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.MiddleRight;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityBottomLeft:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.BottomLeft;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityBottom:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.BottomCentre;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityBottomRight:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.RunToInfinite;
                            state.FlyProperties.RunningToInfinite = FlyKeyLocation.BottomRight;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.A;
                            break;
                        case _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB:
                            state.FlyProperties.RunningToKeyFrame = FlyKeyKeyFrameType.B;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                fly.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeA);
                fly.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeB);

                state.FlyFrames = new List<MixEffectState.KeyerFlyFrameState>
                {
                    BuildKeyerFlyFrame(keyframeA),
                    BuildKeyerFlyFrame(keyframeB)
                };
            }

            return state;
        }

        private static MixEffectState.KeyerLumaState BuildKeyerLuma(IBMDSwitcherKeyLumaParameters props)
        {
            var state = new MixEffectState.KeyerLumaState();

            props.GetPreMultiplied(out int preMultiplied);
            state.PreMultiplied = preMultiplied != 0;
            props.GetClip(out double clip);
            state.Clip = clip * 100;
            props.GetGain(out double gain);
            state.Gain = gain * 100;
            props.GetInverse(out int inverse);
            state.Invert = inverse != 0;

            return state;
        }

        private static MixEffectState.KeyerChromaState BuildKeyerChroma(IBMDSwitcherKeyChromaParameters props)
        {
            var state = new MixEffectState.KeyerChromaState();

            props.GetHue(out double hue);
            state.Hue = hue;
            props.GetGain(out double gain);
            state.Gain = gain * 100;
            props.GetYSuppress(out double ySuppress);
            state.YSuppress = ySuppress * 100;
            props.GetLift(out double lift);
            state.Lift = lift * 100;
            props.GetNarrow(out int narrow);
            state.Narrow = narrow != 0;

            return state;
        }

        private static MixEffectState.KeyerAdvancedChromaState BuildKeyerAdvancedChroma(IBMDSwitcherKeyAdvancedChromaParameters props)
        {
            var state = new MixEffectState.KeyerAdvancedChromaState();

            props.GetForegroundLevel(out double foreground);
            state.Properties.ForegroundLevel = foreground * 100;
            props.GetBackgroundLevel(out double background);
            state.Properties.BackgroundLevel = background * 100;
            props.GetKeyEdge(out double keyEdge);
            state.Properties.KeyEdge = keyEdge * 100;
            props.GetSpillSuppress(out double spillSuppress);
            state.Properties.SpillSuppression = spillSuppress * 100;
            props.GetFlareSuppress(out double flareSuppress);
            state.Properties.FlareSuppression = flareSuppress * 100;
            props.GetBrightness(out double brightness);
            state.Properties.Brightness = brightness * 100;
            props.GetContrast(out double contrast);
            state.Properties.Contrast = contrast * 100;
            props.GetSaturation(out double saturation);
            state.Properties.Saturation = saturation * 100;
            props.GetRed(out double red);
            state.Properties.Red = red * 100;
            props.GetGreen(out double green);
            state.Properties.Green = green * 100;
            props.GetBlue(out double blue);
            state.Properties.Blue = blue * 100;

            props.GetSamplingModeEnabled(out int sampleEnabled);
            state.Sample.EnableCursor = sampleEnabled != 0;
            props.GetPreviewEnabled(out int previewEnabled);
            state.Sample.Preview = previewEnabled != 0;
            props.GetCursorXPosition(out double xPos);
            state.Sample.CursorX = xPos;
            props.GetCursorYPosition(out double yPos);
            state.Sample.CursorY = yPos;
            props.GetCursorSize(out double size);
            state.Sample.CursorSize = size * 100;
            props.GetSampledColor(out double y, out double cb, out double cr);
            state.Sample.SampledY = y;
            state.Sample.SampledCb = cb;
            state.Sample.SampledCr = cr;

            return state;
        }

        private static MixEffectState.KeyerPatternState BuildKeyerPattern(IBMDSwitcherKeyPatternParameters props)
        {
            var state = new MixEffectState.KeyerPatternState();

            props.GetPattern(out _BMDSwitcherPatternStyle pattern);
            state.Pattern = AtemEnumMaps.PatternMap.FindByValue(pattern);
            props.GetSize(out double size);
            state.Size = size * 100;
            props.GetSymmetry(out double symmetry);
            state.Symmetry = symmetry * 100;
            props.GetSoftness(out double softness);
            state.Softness = softness * 100;
            props.GetHorizontalOffset(out double xPos);
            state.XPosition = xPos;
            props.GetVerticalOffset(out double yPos);
            state.YPosition = yPos;
            props.GetInverse(out int inverse);
            state.Inverse = inverse != 0;

            return state;
        }

        private static MixEffectState.KeyerDVEState BuildKeyerDVE(IBMDSwitcherKeyDVEParameters props)
        {
            var state = new MixEffectState.KeyerDVEState();

            props.GetShadow(out int shadow);
            state.BorderShadowEnabled = shadow != 0;
            props.GetLightSourceDirection(out double deg);
            state.LightSourceDirection = deg;
            props.GetLightSourceAltitude(out double alt);
            state.LightSourceAltitude = (uint)Math.Round(alt * 100);
            props.GetBorderEnabled(out int on);
            state.BorderEnabled = on != 0;
            props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevel);
            state.BorderBevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevel);
            props.GetBorderWidthIn(out double widthIn);
            state.BorderInnerWidth = widthIn;
            props.GetBorderWidthOut(out double widthOut);
            state.BorderOuterWidth = widthOut;
            props.GetBorderSoftnessIn(out double softIn);
            state.BorderInnerSoftness = (uint)Math.Round(softIn * 100);
            props.GetBorderSoftnessOut(out double softOut);
            state.BorderOuterSoftness = (uint)Math.Round(softOut * 100);
            props.GetBorderBevelSoftness(out double bevelSoft);
            state.BorderBevelSoftness = (uint)Math.Round(bevelSoft * 100);
            props.GetBorderBevelPosition(out double bevelPosition);
            state.BorderBevelPosition = (uint)Math.Round(bevelPosition * 100);
            props.GetBorderOpacity(out double opacity);
            state.BorderOpacity = (uint)Math.Round(opacity * 100);
            props.GetBorderHue(out double hue);
            state.BorderHue = hue;
            props.GetBorderSaturation(out double sat);
            state.BorderSaturation = sat * 100;
            props.GetBorderLuma(out double luma);
            state.BorderLuma = luma * 100;
            props.GetMasked(out int enabled);
            state.MaskEnabled = enabled != 0;
            props.GetMaskTop(out double top);
            state.MaskTop = top;
            props.GetMaskBottom(out double bottom);
            state.MaskBottom = bottom;
            props.GetMaskLeft(out double left);
            state.MaskLeft = left;
            props.GetMaskRight(out double right);
            state.MaskRight = right;

            return state;
        }

        private static void BuildKeyerFly(MixEffectState.KeyerDVEState state, IBMDSwitcherKeyFlyParameters props)
        {
            props.GetCanScaleUp(out int canScaleUp);
            // state.CanScaleUp = canScaleUp != 0;
            props.GetCanRotate(out int canRotate);
            // state.CanRotate = canRotate != 0;

            props.GetRate(out uint rate);
            state.Rate = rate;
            props.GetSizeX(out double sizeX);
            state.SizeX = sizeX;
            props.GetSizeY(out double sizeY);
            state.SizeY = sizeY;
            props.GetPositionX(out double positionX);
            state.PositionX = positionX;
            props.GetPositionY(out double positionY);
            state.PositionY = positionY;
            props.GetRotation(out double rotation);
            state.Rotation = rotation;
        }

        private static MixEffectState.KeyerFlyFrameState BuildKeyerFlyFrame(IBMDSwitcherKeyFlyKeyFrameParameters props)
        {
            var state = new MixEffectState.KeyerFlyFrameState();

            props.GetBorderOpacity(out double opacity);
            state.BorderOpacity = (uint)Math.Round(opacity * 100);

            // TODO MaskEnabled?
            props.GetMaskTop(out double maskTop);
            state.MaskTop = maskTop;
            props.GetMaskBottom(out double maskBottom);
            state.MaskBottom = maskBottom;
            props.GetMaskLeft(out double maskLeft);
            state.MaskLeft = maskLeft;
            props.GetMaskRight(out double maskRight);
            state.MaskRight = maskRight;

            props.GetSizeX(out double sizeX);
            state.SizeX = sizeX;
            props.GetSizeY(out double sizeY);
            state.SizeY = sizeY;
            props.GetPositionX(out double positionX);
            state.PositionX = positionX;
            props.GetPositionY(out double positionY);
            state.PositionY = positionY;
            props.GetRotation(out double rotation);
            state.Rotation = rotation;
            props.GetBorderWidthOut(out double widthOut);
            state.OuterWidth = widthOut;
            props.GetBorderWidthIn(out double widthIn);
            state.InnerWidth = widthIn;
            props.GetBorderSoftnessOut(out double borderSoftnessOut);
            state.OuterSoftness = (uint)Math.Round(borderSoftnessOut * 100);
            props.GetBorderSoftnessIn(out double borderSoftnessIn);
            state.InnerSoftness = (uint)Math.Round(borderSoftnessIn * 100);
            props.GetBorderBevelSoftness(out double borderBevelSoftness);
            state.BevelSoftness = (uint)Math.Round(borderBevelSoftness * 100);
            props.GetBorderBevelPosition(out double borderBevelPosition);
            state.BevelPosition = (uint)Math.Round(borderBevelPosition * 100);
            props.GetBorderHue(out double hue);
            state.BorderHue = hue;
            props.GetBorderSaturation(out double sat);
            state.BorderSaturation = sat * 100;
            props.GetBorderLuma(out double luma);
            state.BorderLuma = luma * 100;
            props.GetBorderLightSourceDirection(out double deg);
            state.LightSourceDirection = deg;
            props.GetBorderLightSourceAltitude(out double alt);
            state.LightSourceAltitude = (uint)Math.Round(alt * 100);

            return state;
        }
    }
}