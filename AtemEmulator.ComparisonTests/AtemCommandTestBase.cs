using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Net;
using LibAtem.Util;
using Xunit;

namespace AtemEmulator.ComparisonTests
{
    public sealed class AtemComparisonHelper : IDisposable
    {
        public const int CommandWaitTime = 50;

        private readonly AtemClient _client;
        private bool _clientConnected;


        private readonly AutoResetEvent _disposeEvent;
        private bool _isDisposing;

        private readonly IBMDSwitcherDiscovery _switcherDiscovery;
        public readonly IBMDSwitcher _sdkSwitcher;

        private readonly List<ICommand> _receivedCommands;

        private bool _handshakeFinished;
        private readonly AutoResetEvent _handshakeEvent;

        public AtemComparisonHelper()
        {
            const string address = "10.42.13.99";

            var connectionEvent = new AutoResetEvent(false);
            _disposeEvent = new AutoResetEvent(false);
            _handshakeEvent = new AutoResetEvent(false);

            _receivedCommands = new List<ICommand>();

            _client = new AtemClient(address);
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
            _client.OnReceive += (s, commands) =>
            {
                lock (_receivedCommands)
                {
                    if (_handshakeFinished || LogLibAtemHandshake)
                        _receivedCommands.AddRange(commands);

                    if (!_handshakeFinished && commands.Any(c => c is InitializationCompleteCommand))
                    {
                        _handshakeEvent.Set();
                        _handshakeFinished = true;
                    }
                }
            };

            Assert.True(connectionEvent.WaitOne(TimeSpan.FromSeconds(3)), "LibAtem: Connection attempt timed out");

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

        public bool LogLibAtemHandshake { get; set; } = false;

        public IBMDSwitcher SdkSwitcher => _sdkSwitcher;

        private bool WaitForHandshake(bool errorOnTimeout=true)
        {
            bool res = _handshakeEvent.WaitOne(TimeSpan.FromSeconds(20));

            if (errorOnTimeout)
                Assert.True(res);

            return res;
        }

        public void ClearReceivedCommands()
        {
            lock(_receivedCommands)
                _receivedCommands.Clear();
        }

        public List<T> GetReceivedCommands<T>() where T : ICommand
        {
            lock (_receivedCommands)
                return _receivedCommands.OfType<T>().ToList();
        }

        public T GetSingleReceivedCommands<T>() where T : ICommand
        {
            lock (_receivedCommands)
                return _receivedCommands.OfType<T>().Single();
        }

        public void SendCommand(params ICommand[] commands)
        {
            commands.ForEach(c => _client.SendCommand(c));
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

        public void Sleep(int sleep = -1)
        {
            Thread.Sleep(sleep == -1 ? CommandWaitTime : sleep);
        }
    }
}