using System;
using BMDSwitcherAPI;
using LibAtem.State;
using LibAtem.State.Builder;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AtemSDKComparisonMonitor : IDisposable
    {
        private readonly SwitcherPropertiesCallback _root;

        public AtemState State { get; }

        public delegate void StateChangeHandler(object sender, string path);
        public event StateChangeHandler OnStateChange;

        public AtemSDKComparisonMonitor(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            State = new AtemState();
            //switcher.AllowStreamingToResume();

            _root = new SwitcherPropertiesCallback(State, switcher, path => OnStateChange?.Invoke(this, path), updateSettings);
        }

        public void Dispose()
        {
            _root.Dispose();
        }
    }
}