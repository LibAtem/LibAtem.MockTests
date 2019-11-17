using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Common;
using Xunit;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class TalkbackCallback : IBMDSwitcherTalkbackCallback
    {
        private readonly ComparisonTalkbackState _state;
        private readonly IBMDSwitcherTalkback _props;

        public TalkbackCallback(ComparisonTalkbackState state, IBMDSwitcherTalkback props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherTalkbackEventType eventType, long audioInputId)
        {
            switch (eventType)
            {
                case _BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeMuteSDIChanged:
                    _props.GetMuteSDI(out int muteSDI);
                    _state.MuteSDI = muteSDI != 0;
                    break;
                case _BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeInputMuteSDIChanged:
                    _props.InputCanMuteSDI(audioInputId, out int supports);
                    if (supports == 0) // If hardware doesnt support it, it exceptions
                    {
                        _state.Inputs[audioInputId] = false;
                        break;
                    }

                    Assert.True(false, "Not tested");
                    _props.GetInputMuteSDI(audioInputId, out int muteSDIin);
                    _state.Inputs[audioInputId] = muteSDIin != 0;
                    break;
                case _BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeCurrentInputSupportsMuteSDIChanged:
                    _props.CurrentInputSupportsMuteSDI(audioInputId, out int supportsMuteSDI);
                    // TODO - this will be fired when changed port type
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        public void NotifyAll(IEnumerable<long> ids)
        {
            Notify(_BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeMuteSDIChanged, -1);

            foreach (long i in ids)
            {
                var id = (AudioSource)i;
                VideoSource? vSrc = id.GetVideoSource();
                if (!vSrc.HasValue || vSrc.Value.GetPortType() != InternalPortType.External)
                    continue;
                
                Notify(_BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeInputMuteSDIChanged, i);
                Notify(_BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeCurrentInputSupportsMuteSDIChanged, i);
            }
        }
    }
}