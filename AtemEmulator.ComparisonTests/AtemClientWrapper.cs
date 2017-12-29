﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Net;
using Xunit;
using LibAtem.DeviceProfile;

namespace AtemEmulator.ComparisonTests
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

        public LibAtem.DeviceProfile.DeviceProfile Profile => _profile.Profile;

        public delegate void CommandKeyHandler(object sender, CommandQueueKey key);
        public event CommandKeyHandler OnCommandKey;

        public AtemClientWrapper()
        {
            const string address = "10.42.13.99";

            _lastReceivedLibAtem = new Dictionary<CommandQueueKey, ICommand>();

            _disposeEvent = new AutoResetEvent(false);
            _handshakeEvent = new AutoResetEvent(false);

            ConnectLibAtem(address);
            
            Thread.Sleep(1000);

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

            WaitForHandshake();
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
                lock (_lastReceivedLibAtem)
                {
                    foreach (ICommand cmd in commands)
                    {
                        CommandQueueKey key = new CommandQueueKey(cmd);
                        _lastReceivedLibAtem[key] = cmd;
                        OnCommandKey?.Invoke(this, key);
                    }

                    if (!_handshakeFinished && commands.Any(c => c is InitializationCompleteCommand))
                    {
                        _handshakeEvent.Set();
                        _handshakeFinished = true;
                    }
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

            Thread.Sleep(1000);
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

        internal T FindWithMatching<T>(T srcId) where T : ICommand
        {
            var id = new CommandQueueKey(srcId);

            lock (_lastReceivedLibAtem)
            {
                if (_lastReceivedLibAtem.TryGetValue(id, out var val))
                    return (T)val;

                return default(T);
            }
        }

        internal ICommand FindWithMatching<T>(CommandQueueKey id) where T : ICommand
        {
            lock (_lastReceivedLibAtem)
            {
                if (_lastReceivedLibAtem.TryGetValue(id, out var val))
                    return (T)val;

                return default(T);
            }
        }

        public List<T> FindAllOfType<T>() where T : ICommand
        {
            lock (_lastReceivedLibAtem)
            {
                return _lastReceivedLibAtem.Values.OfType<T>().ToList();
            }
        }
    }

}