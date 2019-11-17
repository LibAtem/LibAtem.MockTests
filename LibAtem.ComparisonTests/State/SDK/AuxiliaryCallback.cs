using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class AuxiliaryCallback : IBMDSwitcherInputAuxCallback, INotify<_BMDSwitcherInputAuxEventType>
    {
        private readonly ComparisonAuxiliaryState _state;
        private readonly AuxiliaryId _id;
        private readonly IBMDSwitcherInputAux _aux;
        private readonly Action<CommandQueueKey> _onChange;

        public AuxiliaryCallback(ComparisonAuxiliaryState state, AuxiliaryId id, IBMDSwitcherInputAux aux, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
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

            _onChange(new CommandQueueKey(new AuxSourceGetCommand() { Id = _id }));
        }
    }
}