using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using ListExtensions = LibAtem.Util.ListExtensions;

namespace LibAtem.ComparisonTests.State.SDK
{
    public static class FairlightAudioInputStateBuilder
    {
        public static FairlightAudioState.InputState Build(IBMDSwitcherFairlightAudioInput props)
        {
            var state = new FairlightAudioState.InputState();

            // Input basics
            props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType portType);
            state.ExternalPortType = AtemEnumMaps.ExternalPortTypeMap.FindByValue(portType);
            props.GetConfiguration(out _BMDSwitcherFairlightAudioInputConfiguration configuration);
            state.ActiveConfiguration = AtemEnumMaps.FairlightInputConfigurationMap.FindByValue(configuration);
            props.GetSupportedConfigurations(out _BMDSwitcherFairlightAudioInputConfiguration supportedConfigurations);
            state.SupportedConfigurations = (FairlightInputConfiguration) supportedConfigurations;
            props.GetType(out _BMDSwitcherFairlightAudioInputType type);
            state.InputType = AtemEnumMaps.FairlightInputTypeMap.FindByValue(type);

            // Sources
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioSourceIterator>(props.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherFairlightAudioSource>(
                iterator.Next,
                (src, id) => { ListExtensions.AddIfNotNull(state.Sources, BuildSource(src)); });

            return state;
        }

        private static FairlightAudioState.InputSourceState BuildSource(IBMDSwitcherFairlightAudioSource props)
        {
            var state = new FairlightAudioState.InputSourceState();

            props.IsActive(out int active);
            if (active == 0)
                return null;
            
            props.GetId(out long id);
            state.SourceId = id;
            props.GetSupportedMixOptions(out _BMDSwitcherFairlightAudioMixOption supportedMixOptions);
            state.SupportedMixOptions = (FairlightAudioMixOption) supportedMixOptions;
            props.GetSourceType(out _BMDSwitcherFairlightAudioSourceType sourceType);
            //state.SourceType = AtemEnumMaps.FairlightAudioSourceTypeMap.FindByValue(sourceType);

            props.GetMaxDelayFrames(out ushort maxDelay);
            state.MaxFramesDelay = maxDelay;

            props.GetInputGain(out double inputGain);
            state.Gain = inputGain;
            props.GetPan(out double pan);
            state.Balance = pan;
            props.GetFaderGain(out double faderGain);
            state.FaderGain = faderGain;
            props.GetMixOption(out _BMDSwitcherFairlightAudioMixOption mixOption);
            state.MixOption = AtemEnumMaps.FairlightAudioMixOptionMap.FindByValue(mixOption);

            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(props.GetEffect);
            dynamics.GetMakeupGain(out double makeupGain);
            state.Dynamics.MakeUpGain = makeupGain;

            var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);
            ApplyCompressor(compressor, state.Dynamics.Compressor = new FairlightAudioState.CompressorState());
            var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
            ApplyLimiter(limiter, state.Dynamics.Limiter = new FairlightAudioState.LimiterState());
            var expander = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(dynamics.GetProcessor);
            ApplyExpander(expander, state.Dynamics.Expander = new FairlightAudioState.ExpanderState());

            var eq = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(props.GetEffect);
            eq.GetEnabled(out int eqEnabled);
            state.Equalizer.Enabled = eqEnabled != 0;
            eq.GetGain(out double eqGain);
            state.Equalizer.Gain = eqGain;

            return state;
        }

        private static void ApplyCompressor(IBMDSwitcherFairlightAudioCompressor compressor,
            FairlightAudioState.CompressorState state)
        {
            compressor.GetEnabled(out int enabled);
            state.CompressorEnabled = enabled != 0;
            compressor.GetThreshold(out double threshold);
            state.Threshold = threshold;
            compressor.GetRatio(out double ratio);
            state.Ratio = ratio;
            compressor.GetAttack(out double attack);
            state.Attack = attack;
            compressor.GetRelease(out double release);
            state.Release = release;
            compressor.GetHold(out double hold);
            state.Hold = hold;
        }

        private static void ApplyLimiter(IBMDSwitcherFairlightAudioLimiter limiter,
            FairlightAudioState.LimiterState state)
        {
            limiter.GetEnabled(out int enabled);
            state.LimiterEnabled = enabled != 0;
            limiter.GetThreshold(out double threshold);
            state.Threshold = threshold;
            limiter.GetAttack(out double attack);
            state.Attack = attack;
            limiter.GetRelease(out double release);
            state.Release = release;
            limiter.GetHold(out double hold);
            state.Hold = hold;
        }
        private static void ApplyExpander(IBMDSwitcherFairlightAudioExpander expander,
            FairlightAudioState.ExpanderState state)
        {
            expander.GetEnabled(out int enabled);
            state.ExpanderEnabled = enabled != 0;
            expander.GetGateMode(out int gateMode);
            state.GateEnabled = gateMode != 0;
            expander.GetThreshold(out double threshold);
            state.Threshold = threshold;
            expander.GetRatio(out double ratio);
            state.Ratio = ratio;
            expander.GetRange(out double range);
            state.Range = range;
            expander.GetAttack(out double attack);
            state.Attack = attack;
            expander.GetRelease(out double release);
            state.Release = release;
            expander.GetHold(out double hold);
            state.Hold = hold;
        }
    }

    public sealed class FairlightAudioInputCallback : SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioInput, _BMDSwitcherFairlightAudioInputEventType>, IBMDSwitcherFairlightAudioInputCallback
    {
        private readonly FairlightAudioState.InputState _state;

        private readonly List<IDisposable> _sources = new List<IDisposable>();

        public FairlightAudioInputCallback(FairlightAudioState.InputState state, IBMDSwitcherFairlightAudioInput props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();

            ResetSources();
        }

        public override void Dispose()
        {
            DisposeMany(_sources);
            base.Dispose();
        }

        private void FreeSource(IDisposable src)
        {
            src.Dispose();
            _sources.Remove(src);

            if (_sources.Count == 0)
            {
                ResetSources();
            }
        }

        public override void Notify(_BMDSwitcherFairlightAudioInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioInputEventType.bmdSwitcherFairlightAudioInputEventTypeCurrentExternalPortTypeChanged:
                    Props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType portType);
                    _state.ExternalPortType = AtemEnumMaps.ExternalPortTypeMap.FindByValue(portType);
                    break;
                case _BMDSwitcherFairlightAudioInputEventType.bmdSwitcherFairlightAudioInputEventTypeConfigurationChanged:
                    Props.GetConfiguration(out _BMDSwitcherFairlightAudioInputConfiguration configuration);
                    _state.ActiveConfiguration = AtemEnumMaps.FairlightInputConfigurationMap.FindByValue(configuration);
                    
                    // Need to clear the sources, as the number of them will have changed
                    ResetSources();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }

        private void ResetSources()
        {
            // Clean up existing
            _state.Sources.Clear();
            DisposeMany(_sources);
            _sources.Clear();

            // Start up new stuff
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioSourceIterator>(Props.CreateIterator);
            _state.Sources.AddRange(
                AtemSDKConverter.IterateList<IBMDSwitcherFairlightAudioSource, FairlightAudioState.InputSourceState>(
                    iterator.Next,
                    (src, id) =>
                    {
                        var srcState = new FairlightAudioState.InputSourceState();
                        src.GetId(out long sourceId);
                        _sources.Add(new FairlightAudioInputSourceCallback(srcState, src, AppendChange($"Sources.{sourceId}"), FreeSource));
                        return srcState;
                    }));
        }
    }

    public sealed class FairlightAudioInputSourceCallback : SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioSource, _BMDSwitcherFairlightAudioSourceEventType>, IBMDSwitcherFairlightAudioSourceCallback
    {
        private readonly FairlightAudioState.InputSourceState _state;
        private Action<IDisposable> _free;

        public FairlightAudioInputSourceCallback(FairlightAudioState.InputSourceState state, IBMDSwitcherFairlightAudioSource props, Action<string> onChange, Action<IDisposable> free) : base(props, onChange)
        {
            _state = state;
            _free = free;

            props.GetId(out long id);
            _state.SourceId = id;

            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherFairlightAudioSourceEventType eventType)
        {
            Props.IsActive(out int active);
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeIsActiveChanged:
                    if (active == 0)
                    {
                        _free?.Invoke(this);
                        _free = null;
                    }

                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeMaxDelayFramesChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeDelayFramesChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeInputGainChanged:
                    Props.GetInputGain(out double inputGain);
                    _state.Gain= inputGain;
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeStereoSimulationIntensityChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypePanChanged:
                    Props.GetPan(out double pan);
                    _state.Balance = pan;
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeFaderGainChanged:
                    Props.GetFaderGain(out double faderGain);
                    _state.FaderGain = faderGain;
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeMixOptionChanged:
                    Props.GetMixOption(out _BMDSwitcherFairlightAudioMixOption mixOption);
                    _state.MixOption = AtemEnumMaps.FairlightAudioMixOptionMap.FindByValue(mixOption);
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeIsMixedInChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            if (active != 0)
            {
                OnChange(null);
            }
        }

        public void OutputLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
        {
            //throw new NotImplementedException();
        }
    }
}