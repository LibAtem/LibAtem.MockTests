using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using Xunit;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class TalkbackCallback : SdkCallbackBase<IBMDSwitcherTalkback>, IBMDSwitcherTalkbackCallback
    {
        private readonly AudioState.TalkbackState _state;

        public TalkbackCallback(AudioState.TalkbackState state, IBMDSwitcherTalkback props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
        }

        public void Notify(_BMDSwitcherTalkbackEventType eventType, long audioInputId)
        {
            switch (eventType)
            {
                case _BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeMuteSDIChanged:
                    Props.GetMuteSDI(out int muteSDI);
                    _state.MuteSDI = muteSDI != 0;
                    break;
                case _BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeInputMuteSDIChanged:
                    Props.InputCanMuteSDI(audioInputId, out int supports);
                    if (supports == 0) // If hardware doesnt support it, it exceptions
                    {
                        _state.Inputs[audioInputId] = false;
                        break;
                    }

                    Assert.True(false, "Not tested");
                    Props.GetInputMuteSDI(audioInputId, out int muteSDIin);
                    _state.Inputs[audioInputId] = muteSDIin != 0;
                    break;
                case _BMDSwitcherTalkbackEventType.bmdSwitcherTalkbackEventTypeCurrentInputSupportsMuteSDIChanged:
                    Props.CurrentInputSupportsMuteSDI(audioInputId, out int supportsMuteSDI);
                    // TODO - this will be fired when changed port type
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
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