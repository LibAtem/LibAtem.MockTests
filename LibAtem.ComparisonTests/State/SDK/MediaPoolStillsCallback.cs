using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MediaPoolStillsCallback : IBMDSwitcherStillsCallback, INotify<_BMDSwitcherMediaPoolEventType>
    {
        private readonly MediaPoolState _state;
        private readonly IBMDSwitcherStills _props;
        private readonly Action<string> _onChange;

        public MediaPoolStillsCallback(MediaPoolState state, IBMDSwitcherStills props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int index2)
        {
            uint index = (uint)index2;
            switch (eventType)
            {
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeValidChanged:
                    _props.IsValid(index, out int valid);
                    _state.Stills[(int)index].IsUsed = valid != 0;
                    _onChange($"{index:D}");
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeNameChanged:
                    _props.GetName(index, out string name);
                    _state.Stills[(int)index].Filename = name;
                    _onChange($"{index:D}");
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeHashChanged:
                    _props.GetHash(index, out BMDSwitcherHash hash);
                    _state.Stills[(int)index].Hash = hash.data;
                    _onChange($"{index:D}");
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

            var stills = new List<MediaPoolState.StillState>();
            for (int i = 0; i < count; i++)
                stills.Add(new MediaPoolState.StillState());
            _state.Stills = stills;
        }

        public void Notify(_BMDSwitcherMediaPoolEventType eventType)
        {
            _props.GetCount(out uint count);
            for (int i = 0; i < count; i++)
                Notify(eventType, null, i);
        }
    }
}