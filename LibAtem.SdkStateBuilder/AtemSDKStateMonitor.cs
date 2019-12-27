using BMDSwitcherAPI;
using LibAtem.State.Builder;
using System;

namespace LibAtem.SdkStateBuilder
{
    public sealed class AtemSDKStateMonitor : IDisposable
    {
        private readonly SwitcherPropertiesCallback _root;
        private readonly IBMDSwitcher switcher;

        public delegate void StateChangeHandler(object sender);
        public event StateChangeHandler OnStateChange;

        public AtemSDKStateMonitor(IBMDSwitcher switcher)
        {
            this.switcher = switcher;
            //switcher.AllowStreamingToResume();

            _root = new SwitcherPropertiesCallback(() => OnStateChange?.Invoke(this));
            switcher.AddCallback(_root);
        }

        public void Dispose()
        {
            switcher.RemoveCallback(_root);
        }

        private sealed class SwitcherPropertiesCallback : IBMDSwitcherCallback
        {
            private readonly Action onChange;

            public SwitcherPropertiesCallback(Action onChange)
            {
                this.onChange = onChange;
            }

            public void Notify(_BMDSwitcherEventType eventType)
            {
                if (eventType == _BMDSwitcherEventType.bmdSwitcherEventTypeTimeCodeChanged)
                {
                    onChange();
                }
            }

            public void Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
            {
                Notify(eventType);
            }
        }
    }
}