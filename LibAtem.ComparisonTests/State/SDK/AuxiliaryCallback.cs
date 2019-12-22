using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AuxiliaryCallback : SdkCallbackBaseNotify<IBMDSwitcherInputAux, _BMDSwitcherInputAuxEventType>, IBMDSwitcherInputAuxCallback
    {
        private readonly AuxState _state;

        public AuxiliaryCallback(AuxState state, IBMDSwitcherInputAux aux, Action<string> onChange) : base(aux, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherInputAuxEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputAuxEventType.bmdSwitcherInputAuxEventTypeInputSourceChanged:
                    Props.GetInputSource(out long source);
                    _state.Source = (VideoSource)source;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }
}