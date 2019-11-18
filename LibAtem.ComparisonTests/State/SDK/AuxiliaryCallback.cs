using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AuxiliaryCallback : IBMDSwitcherInputAuxCallback, INotify<_BMDSwitcherInputAuxEventType>
    {
        private readonly AuxState _state;
        private readonly IBMDSwitcherInputAux _aux;
        private readonly Action _onChange;

        public AuxiliaryCallback(AuxState state, IBMDSwitcherInputAux aux, Action onChange)
        {
            _state = state;
            _aux = aux;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherInputAuxEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputAuxEventType.bmdSwitcherInputAuxEventTypeInputSourceChanged:
                    _aux.GetInputSource(out long source);
                    _state.Source = (VideoSource)source;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }
    }
}