using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AudioMixerCallback : SdkCallbackBaseNotify<IBMDSwitcherAudioMixer, _BMDSwitcherAudioMixerEventType>, IBMDSwitcherAudioMixerCallback
    {
        private readonly AudioState.ProgramOutState _state;

        public AudioMixerCallback(AudioState state, IBMDSwitcherAudioMixer props, Action<string> onChange) : base(props, onChange)
        {
            _state = state.ProgramOut;
            TriggerAllChanged();

            var inputIt = AtemSDKConverter.CastSdk<IBMDSwitcherAudioInputIterator>(props.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherAudioInput>(inputIt.Next, (port, i) =>
            {
                port.GetAudioInputId(out long inputId);
                var st = state.Inputs[inputId] = new AudioState.InputState();
                Children.Add(new AudioMixerInputCallback(st, port, AppendChange($"Inputs.{inputId:D}")));
            });

            var monIt = AtemSDKConverter.CastSdk<IBMDSwitcherAudioMonitorOutputIterator>(props.CreateIterator);
            state.Monitors =
                AtemSDKConverter.IterateList<IBMDSwitcherAudioMonitorOutput, AudioState.MonitorOutputState>(
                    monIt.Next,
                    (mon, id) =>
                    {
                        var monState = new AudioState.MonitorOutputState();
                        Children.Add(new AudioMixerMonitorOutputCallback(monState, mon, AppendChange($"Monitors.{id:D}")));
                        return monState;
                    });
        }

        public override void Notify(_BMDSwitcherAudioMixerEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutGainChanged:
                    Props.GetProgramOutGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutBalanceChanged:
                    Props.GetProgramOutBalance(out double balance);
                    _state.Balance = balance * 50;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutFollowFadeToBlackChanged:
                    Props.GetProgramOutFollowFadeToBlack(out int follow);
                    _state.FollowFadeToBlack = follow != 0;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeAudioFollowVideoCrossfadeTransitionChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange("ProgramOut");
        }

        public void ProgramOutLevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            _state.LevelLeft = left;
            _state.LevelRight = right;
            _state.PeakLeft = peakLeft;
            _state.PeakRight = peakRight;
        }
    }
}