using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using log4net;
using log4net.Config;
using Xunit;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.SdkStateBuilder;

namespace LibAtem.MockTests.Util
{
    public sealed class AtemSdkClientWrapper : IDisposable
    {
        private readonly IBMDSwitcherDiscovery _switcherDiscovery;
        private readonly IBMDSwitcher _sdkSwitcher;
        private readonly AtemSDKStateMonitor _sdkState;
        private readonly AtemStateBuilderSettings _updateSettings;

        public IBMDSwitcher SdkSwitcher => _sdkSwitcher;
        
        public delegate void StateChangeHandler(object sender);
        public event StateChangeHandler OnSdkStateChange;

        public AtemSdkClientWrapper(string address, AtemStateBuilderSettings updateSettings)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            if (!logRepository.Configured) // Default to all on the console
                BasicConfigurator.Configure(logRepository);

            _updateSettings = updateSettings;

            _switcherDiscovery = new CBMDSwitcherDiscovery();
            Assert.NotNull(_switcherDiscovery);

            _BMDSwitcherConnectToFailure failReason = 0;
            try
            {
                _switcherDiscovery.ConnectTo(address, out _sdkSwitcher, out failReason);
            }
            catch (COMException)
            {
                throw new Exception($"SDK Connection failure: {failReason}");
            }

            _sdkSwitcher.AddCallback(new SwitcherConnectionMonitor()); // TODO - make this monitor work better!

            _sdkState = new AtemSDKStateMonitor(_sdkSwitcher);
            _sdkState.OnStateChange += (s) => OnSdkStateChange?.Invoke(s);
        }

        public AtemState State => SdkStateBuilder.SdkStateBuilder.Build(SdkSwitcher, _updateSettings);
        
        public void Dispose()
        {
            _sdkState.Dispose();
            // TODO - reenable once LibAtem allows disconnection
            // Assert.True(_disposeEvent.WaitOne(TimeSpan.FromSeconds(1)), "LibAtem: Cleanup timed out");

            //Thread.Sleep(500);
        }

        public delegate void SwitcherEventHandler(object sender, object args);

        private class SwitcherConnectionMonitor : IBMDSwitcherCallback
        {
            // Events:
            public event SwitcherEventHandler SwitcherDisconnected;

            void IBMDSwitcherCallback.Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
            {
                if (eventType == _BMDSwitcherEventType.bmdSwitcherEventTypeDisconnected)
                {
                    SwitcherDisconnected?.Invoke(this, null);
                }
            }
        }
        
    }

}