using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MediaPoolStillsCallback : IBMDSwitcherStillsCallback, INotify<_BMDSwitcherMediaPoolEventType>
    {
        private readonly ComparisonMediaPoolState _state;
        private readonly IBMDSwitcherStills _props;

        public MediaPoolStillsCallback(ComparisonMediaPoolState state, IBMDSwitcherStills props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int index2)
        {
            uint index = (uint)index2;
            switch (eventType)
            {
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeValidChanged:
                    _props.IsValid(index, out int valid);
                    _state.Stills[index].IsUsed = valid != 0;
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeNameChanged:
                    _props.GetName(index, out string name);
                    _state.Stills[index].Name = name;
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeHashChanged:
                    _props.GetHash(index, out BMDSwitcherHash hash);
                    _state.Stills[index].Hash = hash.data;
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeLockBusy:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeLockIdle:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCancelled:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferFailed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        public void Init()
        {
            _props.GetCount(out uint count);
            for (uint i = 0; i < count; i++)
                _state.Stills[i] = new ComparisonMediaPoolStillState();
        }

        public void Notify(_BMDSwitcherMediaPoolEventType eventType)
        {
            _props.GetCount(out uint count);
            for (int i = 0; i < count; i++)
                Notify(eventType, null, i);
        }
    }
}