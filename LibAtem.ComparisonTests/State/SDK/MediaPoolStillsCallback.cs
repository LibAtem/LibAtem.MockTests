using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MediaPoolStillsCallback : SdkCallbackBaseNotify<IBMDSwitcherStills, _BMDSwitcherMediaPoolEventType>, IBMDSwitcherStillsCallback
    {
        private readonly MediaPoolState _state;

        public MediaPoolStillsCallback(MediaPoolState state, IBMDSwitcherStills props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;

            Props.GetCount(out uint count);
            var stills = new List<MediaPoolState.StillState>();
            for (int i = 0; i < count; i++)
                stills.Add(new MediaPoolState.StillState());
            _state.Stills = stills;

            TriggerAllChanged(
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioValidChanged,
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioNameChanged,
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioHashChanged
            );
        }

        public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int index2)
        {
            uint index = (uint)index2;
            switch (eventType)
            {
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeValidChanged:
                    Props.IsValid(index, out int valid);
                    _state.Stills[(int)index].IsUsed = valid != 0;
                    OnChange($"{index:D}");
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeNameChanged:
                    Props.GetName(index, out string name);
                    _state.Stills[(int)index].Filename = name;
                    OnChange($"{index:D}");
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeHashChanged:
                    Props.GetHash(index, out BMDSwitcherHash hash);
                    _state.Stills[(int)index].Hash = hash.data;
                    OnChange($"{index:D}");
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

        public override void Notify(_BMDSwitcherMediaPoolEventType eventType)
        {
            Props.GetCount(out uint count);
            for (int i = 0; i < count; i++)
                Notify(eventType, null, i);
        }
    }
}