using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State
{
    public sealed class AtemSDKComparisonMonitor
    {
        public ComparisonState State { get; }

        private readonly List<Action> _cleanupCallbacks = new List<Action>();

        public AtemSDKComparisonMonitor(IBMDSwitcher switcher)
        {
            State = new ComparisonState
            {
                Auxiliaries = new Dictionary<AuxiliaryId, ComparisonAuxiliaryState>(),
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>(),
                Colors = new Dictionary<ColorGeneratorId, ComparisonColorState>(),
            };
            
            SetupInputs(switcher);
            SetupMixEffects(switcher);

        }

        ~AtemSDKComparisonMonitor()
        {
            _cleanupCallbacks.ForEach(cb => cb());
        }

        private interface INotify<in T>
        {
            void Notify(T eventType);
        }
        private void TriggerAllChanged<T>(INotify<T> cb, params T[] skip)
        {
            Enum.GetValues(typeof(T)).OfType<T>().Where(v => !skip.Contains(v)).ForEach(cb.Notify);
        }

        private void SetupMixEffects(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator) Marshal.GetObjectForIUnknown(itPtr);

            var id = MixEffectBlockId.One;
            for (iterator.Next(out IBMDSwitcherMixEffectBlock me); me != null; iterator.Next(out me))
            {
                me.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out long pgm);
                me.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out long pvw);

                State.MixEffects[id] = new ComparisonMixEffectState()
                {
                    Preview = (VideoSource) pvw,
                    Program = (VideoSource) pgm,
                };
                
                SetupMixEffectKeyer(me, id);

                SetupMixEffectTransition(me, id);

                id++;
            }
        }

        private void SetupMixEffectTransition(IBMDSwitcherMixEffectBlock me, MixEffectBlockId id)
        {
            var dip = me as IBMDSwitcherTransitionDipParameters;
            if (dip != null)
                SetupMixEffectTransitionDip(dip, id);
        }

        private sealed class MixEffectTransitionDipCallback : IBMDSwitcherTransitionDipParametersCallback, INotify<_BMDSwitcherTransitionDipParametersEventType>
        {
            private readonly ComparisonMixEffectTransitionDipState _state;
            private readonly IBMDSwitcherTransitionDipParameters _props;

            public MixEffectTransitionDipCallback(ComparisonMixEffectTransitionDipState state, IBMDSwitcherTransitionDipParameters props)
            {
                _state = state;
                _props = props;
            }

            public void Notify(_BMDSwitcherTransitionDipParametersEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherTransitionDipParametersEventType.bmdSwitcherTransitionDipParametersEventTypeRateChanged:
                        _props.GetRate(out uint rate);
                        _state.Rate = rate;
                        break;
                    case _BMDSwitcherTransitionDipParametersEventType.bmdSwitcherTransitionDipParametersEventTypeInputDipChanged:
                        _props.GetInputDip(out long input);
                        _state.Input = (VideoSource) input;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }
        private void SetupMixEffectTransitionDip(IBMDSwitcherTransitionDipParameters dip, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionDipState st = State.MixEffects[id].Transition.Dip;

            var cb = new MixEffectTransitionDipCallback(st, dip);
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectKeyer(IBMDSwitcherMixEffectBlock me, MixEffectBlockId id)
        {
            Guid itId = typeof(IBMDSwitcherKeyIterator).GUID;
            me.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherKeyIterator iterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(itPtr);

            var keyId = UpstreamKeyId.One;
            for (iterator.Next(out IBMDSwitcherKey keyer); keyer != null; iterator.Next(out keyer))
            {
                State.MixEffects[id].Keyers[keyId] = new ComparisonMixEffectKeyerState();

                var chroma = keyer as IBMDSwitcherKeyChromaParameters;
                if (chroma != null)
                    SetupMixEffectChromaKeyer(chroma, id, keyId);
                var dve = keyer as IBMDSwitcherKeyDVEParameters;
                if (dve != null)
                    SetupMixEffectDVEKeyer(dve, id, keyId);
                var fly = keyer as IBMDSwitcherKeyFlyParameters;
                if (fly != null)
                    SetupMixEffectFlyKeyer(fly, id, keyId);

                keyId++;
            }
        }

        private sealed class MixEffectKeyerChromaCallback : IBMDSwitcherKeyChromaParametersCallback, INotify<_BMDSwitcherKeyChromaParametersEventType>
        {
            private readonly ComparisonMixEffectKeyerChromaState _state;
            private readonly IBMDSwitcherKeyChromaParameters _props;

            public MixEffectKeyerChromaCallback(ComparisonMixEffectKeyerChromaState state, IBMDSwitcherKeyChromaParameters props)
            {
                _state = state;
                _props = props;
            }

            public void Notify(_BMDSwitcherKeyChromaParametersEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeHueChanged:
                        _props.GetHue(out double hue);
                        _state.Hue = hue;
                        break;
                    case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeGainChanged:
                        _props.GetGain(out double gain);
                        _state.Gain = gain * 100;
                        break;
                    case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeYSuppressChanged:
                        _props.GetYSuppress(out double ySuppress);
                        _state.YSuppress = ySuppress * 100;
                        break;
                    case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeLiftChanged:
                        _props.GetLift(out double lift);
                        _state.Lift = lift * 100;
                        break;
                    case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeNarrowChanged:
                        _props.GetNarrow(out int narrow);
                        _state.Narrow = narrow != 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        private void SetupMixEffectChromaKeyer(IBMDSwitcherKeyChromaParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerChromaState chroma = State.MixEffects[meId].Keyers[keyId].Chroma;
            
            var cb = new MixEffectKeyerChromaCallback(chroma, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private sealed class MixEffectKeyerDVECallback : IBMDSwitcherKeyDVEParametersCallback, INotify<_BMDSwitcherKeyDVEParametersEventType>
        {
            private readonly ComparisonMixEffectKeyerDVEState _state;
            private readonly IBMDSwitcherKeyDVEParameters _props;

            public MixEffectKeyerDVECallback(ComparisonMixEffectKeyerDVEState state, IBMDSwitcherKeyDVEParameters props)
            {
                _state = state;
                _props = props;
            }

            public void Notify(_BMDSwitcherKeyDVEParametersEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeShadowChanged:
                        _props.GetShadow(out int shadow);
                        _state.BorderShadow = shadow != 0;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeLightSourceDirectionChanged:
                        _props.GetLightSourceDirection(out double deg);
                        _state.LightSourceDirection = deg;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeLightSourceAltitudeChanged:
                        _props.GetLightSourceAltitude(out double alt);
                        _state.LightSourceAltitude = (uint) (alt * 100);
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderEnabledChanged:
                        _props.GetBorderEnabled(out int on);
                        _state.BorderEnabled = on != 0;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelChanged:
                        _props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevel);
                        _state.BorderBevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevel);
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderWidthInChanged:
                        _props.GetBorderWidthIn(out double widthIn);
                        _state.InnerWidth = widthIn;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderWidthOutChanged:
                        _props.GetBorderWidthOut(out double widthOut);
                        _state.OuterWidth = widthOut;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessInChanged:
                        _props.GetBorderSoftnessIn(out double softIn);
                        _state.InnerSoftness = (uint) (softIn * 100);
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessOutChanged:
                        _props.GetBorderSoftnessOut(out double softOut);
                        _state.OuterSoftness = (uint) (softOut * 100);
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelSoftnessChanged:
                        _props.GetBorderBevelSoftness(out double bevelSoft);
                        _state.BevelSoftness = (uint) (bevelSoft * 100);
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelPositionChanged:
                        _props.GetBorderBevelPosition(out double bevelPosition);
                        _state.BevelPosition = (uint) (bevelPosition * 100);
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderOpacityChanged:
                        _props.GetBorderOpacity(out double opacity);
                        _state.BorderOpacity = (uint) (opacity * 100);
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderHueChanged:
                        _props.GetBorderHue(out double hue);
                        _state.BorderHue = hue;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSaturationChanged:
                        _props.GetBorderSaturation(out double sat);
                        _state.BorderSaturation = sat * 100;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderLumaChanged:
                        _props.GetBorderLuma(out double luma);
                        _state.BorderLuma = luma * 100;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskedChanged:
                        _props.GetMasked(out int enabled);
                        _state.MaskEnabled = enabled != 0;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskTopChanged:
                        _props.GetMaskTop(out double top);
                        _state.MaskTop = top;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskBottomChanged:
                        _props.GetMaskBottom(out double bottom);
                        _state.MaskBottom = bottom;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskLeftChanged:
                        _props.GetMaskLeft(out double left);
                        _state.MaskLeft = left;
                        break;
                    case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskRightChanged:
                        _props.GetMaskRight(out double right);
                        _state.MaskRight = right;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        private void SetupMixEffectDVEKeyer(IBMDSwitcherKeyDVEParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerDVEState dve = State.MixEffects[meId].Keyers[keyId].DVE;

            var cb = new MixEffectKeyerDVECallback(dve, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private sealed class MixEffectKeyerFlyCallback : IBMDSwitcherKeyFlyParametersCallback, INotify<_BMDSwitcherKeyFlyParametersEventType>
        {
            private readonly ComparisonMixEffectKeyerFlyState _state;
            private readonly IBMDSwitcherKeyFlyParameters _props;

            public MixEffectKeyerFlyCallback(ComparisonMixEffectKeyerFlyState state, IBMDSwitcherKeyFlyParameters props)
            {
                _state = state;
                _props = props;
            }

            public void Notify(_BMDSwitcherKeyFlyParametersEventType eventType)
            {
                Notify(eventType, _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull);
            }

            public void Notify(_BMDSwitcherKeyFlyParametersEventType eventType, _BMDSwitcherFlyKeyFrame keyFrame)
            {
                switch (eventType)
                {
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged:
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged:
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeRateChanged:
                        _props.GetRate(out uint rate);
                        _state.Rate = rate;
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeXChanged:
                        _props.GetSizeX(out double sizeX);
                        _state.SizeX = sizeX;
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeYChanged:
                        _props.GetSizeY(out double sizeY);
                        _state.SizeY = sizeY;
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionXChanged:
                        _props.GetPositionX(out double positionX);
                        _state.PositionX = positionX;
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionYChanged:
                        _props.GetPositionY(out double positionY);
                        _state.PositionY = positionY;
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeRotationChanged:
                        _props.GetRotation(out double rotation);
                        _state.Rotation = rotation;
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged:
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged:
                        break;
                    case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        private void SetupMixEffectFlyKeyer(IBMDSwitcherKeyFlyParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerFlyState fly = State.MixEffects[meId].Keyers[keyId].Fly;

            var cb = new MixEffectKeyerFlyCallback(fly, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            var ignore = new[]
            {
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged
            };
            TriggerAllChanged(cb, ignore);
        }

        private void SetupInputs(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherInputIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherInputIterator iterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(itPtr);

            for (iterator.Next(out IBMDSwitcherInput input); input != null; iterator.Next(out input))
            {
                input.GetInputId(out long id);
                var src = (VideoSource) id;

                // TODO - normal input stuff

                var aux = input as IBMDSwitcherInputAux;
                if (aux != null)
                    SetupAuxiliary(src, aux);

                var col = input as IBMDSwitcherInputColor;
                if (col != null)
                    SetupColor(src, col);
            }
        }

        #region Auxiliary

        private sealed class AuxCallback : IBMDSwitcherInputAuxCallback, INotify<_BMDSwitcherInputAuxEventType>
        {
            private readonly ComparisonAuxiliaryState _state;
            private readonly IBMDSwitcherInputAux _aux;

            public AuxCallback(ComparisonAuxiliaryState state, IBMDSwitcherInputAux aux)
            {
                _state = state;
                _aux = aux;
            }

            public void Notify(_BMDSwitcherInputAuxEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherInputAuxEventType.bmdSwitcherInputAuxEventTypeInputSourceChanged:
                        _aux.GetInputSource(out long source);
                        _state.Source = (VideoSource) source;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        private void SetupAuxiliary(VideoSource id, IBMDSwitcherInputAux aux)
        {
            var c = new ComparisonAuxiliaryState();
            State.Auxiliaries[AtemEnumMaps.GetAuxId(id)] = c;
            var cb = new AuxCallback(c, aux);
            aux.AddCallback(cb);
            _cleanupCallbacks.Add(() => aux.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        #endregion Auxiliary

        #region Color

        private sealed class ColorCallback : IBMDSwitcherInputColorCallback, INotify<_BMDSwitcherInputColorEventType>
        {
            private readonly ComparisonColorState _state;
            private readonly IBMDSwitcherInputColor _color;

            public ColorCallback(ComparisonColorState state, IBMDSwitcherInputColor color)
            {
                _state = state;
                _color = color;
            }

            public void Notify(_BMDSwitcherInputColorEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeHueChanged:
                        _color.GetHue(out double hue);
                        _state.Hue = hue;
                        break;
                    case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeSaturationChanged:
                        _color.GetSaturation(out double saturation);
                        _state.Saturation = saturation * 100;
                        break;
                    case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeLumaChanged:
                        _color.GetLuma(out double luma);
                        _state.Luma = luma * 100;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        private void SetupColor(VideoSource id, IBMDSwitcherInputColor col)
        {
            var c = new ComparisonColorState();
            State.Colors[AtemEnumMaps.GetSourceIdForGen(id)] = c;
            var cb = new ColorCallback(c, col);
            col.AddCallback(cb);
            _cleanupCallbacks.Add(() => col.RemoveCallback(cb));
            
            TriggerAllChanged(cb);
        }

        #endregion Color
    }
}