using System;
using BMDSwitcherAPI;
using LibAtem.Common;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class AuxiliaryCallback : IBMDSwitcherInputAuxCallback, INotify<_BMDSwitcherInputAuxEventType>
    {
        private readonly ComparisonAuxiliaryState _state;
        private readonly IBMDSwitcherInputAux _aux;

        public AuxiliaryCallback(ComparisonAuxiliaryState state, IBMDSwitcherInputAux aux)
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
                    _state.Source = (VideoSource)source;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}