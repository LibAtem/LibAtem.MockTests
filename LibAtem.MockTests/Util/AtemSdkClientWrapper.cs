using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using log4net;
using log4net.Config;
using LibAtem.ComparisonTests.State.SDK;
using Xunit;
using LibAtem.State;
using LibAtem.State.Builder;

namespace LibAtem.MockTests.Util
{
    public sealed class AtemSdkClientWrapper : IDisposable
    {
        private readonly IBMDSwitcherDiscovery _switcherDiscovery;
        private readonly IBMDSwitcher _sdkSwitcher;
        private readonly AtemSDKComparisonMonitor _sdkState;
        private readonly AtemStateBuilderSettings _updateSettings;

        public IBMDSwitcher SdkSwitcher => _sdkSwitcher;
        
        public delegate void StateChangeHandler(object sender, string path);
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

            _sdkState = new AtemSDKComparisonMonitor(_sdkSwitcher, _updateSettings);
            _sdkState.OnStateChange += (s, e) => OnSdkStateChange?.Invoke(s, e);
        }

        public AtemState State
        {
            get
            {
                var state = _sdkState.State.Clone();

                /**
                 * It is very hard to tell when the sdk has changed the sources in the iterator.
                 * As a workaround, we instead do a fresh state build everytime we need a state.
                 * TODO - verify this is still true now the SourceDelete command has been found.
                 */
                if (state.Fairlight != null)
                {
                    var mixer = SdkSwitcher as IBMDSwitcherFairlightAudioMixer;
                    var iterator =
                        AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioInputIterator>(mixer.CreateIterator);

                    state.Fairlight.Inputs.Clear();
                    AtemSDKConverter
                        .Iterate<IBMDSwitcherFairlightAudioInput>(
                            iterator.Next,
                            (inp, i) =>
                            {
                                inp.GetId(out long id);
                                state.Fairlight.Inputs[id] = FairlightAudioInputStateBuilder.Build(inp);
                            });
                }

                return state;
            }
        }
        
        public void Dispose()
        {
            _sdkState.Dispose();
            // TODO - reenable once LibAtem allows disconnection
            // Assert.True(_disposeEvent.WaitOne(TimeSpan.FromSeconds(1)), "LibAtem: Cleanup timed out");

            Thread.Sleep(500);
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