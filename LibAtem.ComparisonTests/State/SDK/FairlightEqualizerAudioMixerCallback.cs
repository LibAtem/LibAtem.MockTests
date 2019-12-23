using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightEqualizerAudioMixerCallback : SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioEqualizer, _BMDSwitcherFairlightAudioEqualizerEventType>, IBMDSwitcherFairlightAudioEqualizerCallback
    {
        private readonly FairlightAudioState.EqualizerState _state;

        public FairlightEqualizerAudioMixerCallback(FairlightAudioState.EqualizerState state, IBMDSwitcherFairlightAudioEqualizer props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();

            /*
            // Bands
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizerBandIterator>(eq.CreateIterator);

            var bands = new List<FairlightAudioState.EqualizerBandState>();

            int id = 0;
            for (iterator.Next(out IBMDSwitcherFairlightAudioEqualizerBand band); band != null; iterator.Next(out band))
            {
                var bandState = new FairlightAudioState.EqualizerBandState();

                var id2 = id;
                var cb = new FairlightEqualizerBandAudioMixerCallback(bandState, band, () => FireCommandKey($"{path}.Bands.{id2:D}"));
                SetupCallback(cb, band.AddCallback, band.RemoveCallback);

                id++;
            }

            state.Bands = bands;
            */
        }

        public override void Notify(_BMDSwitcherFairlightAudioEqualizerEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioEqualizerEventType.bmdSwitcherFairlightAudioEqualizerEventTypeEnabledChanged:
                    Props.GetEnabled(out int enabled);
                    _state.Enabled = enabled != 0;
                    break;
                case _BMDSwitcherFairlightAudioEqualizerEventType.bmdSwitcherFairlightAudioEqualizerEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Gain = gain;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
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
                    _state.Shape = AtemEnumMaps.FairlightEqualizerBandShapeMap.FindByValue(shape);
                    break;
                case _BMDSwitcherFairlightAudioEqualizerBandEventType.bmdSwitcherFairlightAudioEqualizerBandEventTypeFrequencyRangeChanged:
                    _props.GetFrequencyRange(out _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange range);
                    _state.FrequencyRange = AtemEnumMaps.FairlightEqualizerFrequencyRangeMap.FindByValue(range);
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