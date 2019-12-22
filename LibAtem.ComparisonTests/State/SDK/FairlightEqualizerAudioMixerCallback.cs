using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightEqualizerAudioMixerCallback : IBMDSwitcherFairlightAudioEqualizerCallback, INotify<_BMDSwitcherFairlightAudioEqualizerEventType>
    {
        private readonly FairlightAudioState.EqualizerState _state;
        private readonly IBMDSwitcherFairlightAudioEqualizer _props;
        private readonly Action _onChange;

        public FairlightEqualizerAudioMixerCallback(FairlightAudioState.EqualizerState state, IBMDSwitcherFairlightAudioEqualizer props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherFairlightAudioEqualizerEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioEqualizerEventType.bmdSwitcherFairlightAudioEqualizerEventTypeEnabledChanged:
                    _props.GetEnabled(out int enabled);
                    _state.Enabled = enabled != 0;
                    break;
                case _BMDSwitcherFairlightAudioEqualizerEventType.bmdSwitcherFairlightAudioEqualizerEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }
    }

    public sealed class FairlightEqualizerBandAudioMixerCallback : IBMDSwitcherFairlightAudioEqualizerBandCallback, INotify<_BMDSwitcherFairlightAudioEqualizerBandEventType>
    {
        private readonly FairlightAudioState.EqualizerBandState _state;
        private readonly IBMDSwitcherFairlightAudioEqualizerBand _props;
        private readonly Action _onChange;

        public FairlightEqualizerBandAudioMixerCallback(FairlightAudioState.EqualizerBandState state, IBMDSwitcherFairlightAudioEqualizerBand props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherFairlightAudioEqualizerBandEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioEqualizerBandEventType.bmdSwitcherFairlightAudioEqualizerBandEventTypeEnabledChanged:
                    _props.GetEnabled(out int enabled);
                    _state.BandEnabled = enabled != 0;
                    break;
                case _BMDSwitcherFairlightAudioEqualizerBandEventType.bmdSwitcherFairlightAudioEqualizerBandEventTypeShapeChanged:
                    _props.GetShape(out _BMDSwitcherFairlightAudioEqualizerBandShape shape);
                    _state.Shape = AtemEnumMaps.FairlightEqualizerShape.FindByValue(shape);
                    break;
                case _BMDSwitcherFairlightAudioEqualizerBandEventType.bmdSwitcherFairlightAudioEqualizerBandEventTypeFrequencyRangeChanged:
                    _props.GetFrequencyRange(out _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange range);
                    _state.FrequencyRange = AtemEnumMaps.FairlightEqualizerBandRange.FindByValue(range);
                    break;
                case _BMDSwitcherFairlightAudioEqualizerBandEventType.bmdSwitcherFairlightAudioEqualizerBandEventTypeFrequencyChanged:
                    _props.GetFrequency(out uint freq);
                    _state.Frequency = freq;
                    break;
                case _BMDSwitcherFairlightAudioEqualizerBandEventType.bmdSwitcherFairlightAudioEqualizerBandEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherFairlightAudioEqualizerBandEventType.bmdSwitcherFairlightAudioEqualizerBandEventTypeQFactorChanged:
                    _props.GetQFactor(out double qfactor);
                    _state.QFactor = qfactor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }
    }
}