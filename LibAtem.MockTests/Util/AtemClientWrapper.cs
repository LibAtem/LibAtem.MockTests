using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using log4net;
using log4net.Config;
using LibAtem.Commands;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.DeviceProfile;
using LibAtem.Net;
using Xunit;
using LibAtem.State;
using LibAtem.State.Builder;

namespace LibAtem.MockTests.Util
{
    [CollectionDefinition("Client")]
    public class ClientCollection : ICollectionFixture<AtemClientWrapper>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public sealed class AtemClientWrapper : IDisposable
    {
        private readonly Dictionary<CommandQueueKey, ICommand> _lastReceivedLibAtem;
        private readonly AutoResetEvent _disposeEvent;
        private readonly AutoResetEvent _handshakeEvent;
        private readonly IBMDSwitcherDiscovery _switcherDiscovery;
        private readonly IBMDSwitcher _sdkSwitcher;

        private AtemClient _client;
        private DeviceProfileHandler _profile;

        private bool _clientConnected;
        private bool _isDisposing;
        private bool _handshakeFinished;

        public IBMDSwitcher SdkSwitcher => _sdkSwitcher;
        public AtemClient Client => _client;
        public AtemStateBuilderSettings StateSettings => _updateSettings;

        public LibAtem.DeviceProfile.DeviceProfile Profile => _profile.Profile;

        private readonly AtemStateBuilderSettings _updateSettings;

        private readonly AtemSDKComparisonMonitor _sdkState;
        private AtemState _libState;

        public delegate void CommandKeyHandler(object sender, string path);
        public event CommandKeyHandler OnStateChange;

        public delegate void StateChangeHandler(object sender, string path);
        public event StateChangeHandler OnSdkStateChange;

        private readonly List<ICommand> _libAtemReceived;

        public ImmutableList<ICommand> LibAtemReceived
        {
            get
            {
                lock (_libAtemReceived)
                {
                    return _libAtemReceived.ToImmutableList();
                }
            }
        }

        public AtemClientWrapper(string address = "10.42.13.95")
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            if (!logRepository.Configured) // Default to all on the console
                BasicConfigurator.Configure(logRepository);

            _lastReceivedLibAtem = new Dictionary<CommandQueueKey, ICommand>();

            _disposeEvent = new AutoResetEvent(false);
            _handshakeEvent = new AutoResetEvent(false);

            _updateSettings = new AtemStateBuilderSettings();

            _libAtemReceived = new List<ICommand>();

            _libState = new AtemState();

            ConnectLibAtem(address);

            // Thread.Sleep(1000);

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

            WaitForHandshake();
        }

        public AtemState SdkState => _sdkState.State.Clone();
        public AtemState LibState => _libState.Clone();

        public void SyncStates()
        {
            _libState = _sdkState.State.Clone();
        }

        private void ConnectLibAtem(string address)
        {
            var connectionEvent = new AutoResetEvent(false);

            _client = new AtemClient(address, false);
            _profile = new DeviceProfileHandler();
            _client.OnConnection += s =>
            {
                if (_clientConnected)
                    Assert.True(false, "LibAtem: Got unexpected reconnect");

                _clientConnected = true;
                connectionEvent.Set();
            };
            _client.OnDisconnect += s =>
            {
                if (!_isDisposing)
                    Assert.True(false, "LibAtem: Got early termination");

                _clientConnected = false;
                _disposeEvent.Set();
            };
            _client.OnReceive += _profile.HandleCommands;
            _client.OnReceive += (s, commands) =>
            {
                foreach (ICommand cmd in commands)
                {
                    // TODO - handle result?
                    IUpdateResult result = AtemStateBuilder.Update(_libState, cmd, _updateSettings);
                    foreach (string change in result.ChangedPaths)
                    {
                        OnStateChange?.Invoke(this, change);
                    }
                }
                lock (_lastReceivedLibAtem)
                {
                    foreach (ICommand cmd in commands)
                    {
                        CommandQueueKey key = new CommandQueueKey(cmd);
                        _lastReceivedLibAtem[key] = cmd;
                    }

                    if (!_handshakeFinished && commands.Any(c => c is InitializationCompleteCommand))
                    {
                        _handshakeEvent.Set();
                        _handshakeFinished = true;
                    }
                }

                lock (_libAtemReceived)
                {
                    _libAtemReceived.AddRange(commands);
                }
            };
            _client.Connect();

            Assert.True(connectionEvent.WaitOne(TimeSpan.FromSeconds(3)), "LibAtem: Connection attempt timed out");
        }

        private bool WaitForHandshake(bool errorOnTimeout = true)
        {
            bool res = _handshakeEvent.WaitOne(TimeSpan.FromSeconds(20));

            if (errorOnTimeout)
                Assert.True(res);

            return res;
        }

        public void Dispose()
        {
            _isDisposing = true;
            _client.Dispose();
            // TODO - reenable once LibAtem allows disconnection
            // Assert.True(_disposeEvent.WaitOne(TimeSpan.FromSeconds(1)), "LibAtem: Cleanup timed out");

            Thread.Sleep(100);
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