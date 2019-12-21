using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Util
{
    public sealed class AtemTestHelper : IDisposable
    {
        public const int CommandWaitTime = 80;

        private readonly AtemClientWrapper _client;
        public AtemClientWrapper Client => _client;
        public AtemStateBuilderSettings StateSettings => _client.StateSettings;

        private readonly List<ICommand> _receivedCommands;

        private AutoResetEvent responseWait;

        public bool TestResult { get; set; } = true;

        public AtemTestHelper(AtemClientWrapper client, ITestOutputHelper output)
        {
            _client = client;
            Output = output;
            _receivedCommands = new List<ICommand>();

            _client.Client.OnReceive += OnReceive;

            _client.SyncStates();
            AssertStatesMatch();
        }

        public void Dispose()
        {
            _client.Client.OnReceive -= OnReceive;

            Assert.True(TestResult);
        }

        public void AssertStatesMatch()
        {
            List<string> before = AtemStateComparer.AreEqual(SdkState, LibState);
            if (before.Count != 0 && Output != null)
            {
                Output.WriteLine("state mismatch:");
                before.ForEach(Output.WriteLine);
            }
            Assert.Empty(before);
        }

        private void OnReceive(object sender, IReadOnlyList<ICommand> commands)
        {
            lock (_receivedCommands)
            {
                _receivedCommands.AddRange(commands);
            }
        }

        public ITestOutputHelper Output { get; }

        public AtemState SdkState => _client.SdkState;
        public AtemState LibState => _client.LibState;

        public IBMDSwitcher SdkSwitcher => _client.SdkSwitcher;

        public LibAtem.DeviceProfile.DeviceProfile Profile => _client.Profile;
        
        public void SendAndWaitForChange(Action doSend, int timeout = -1)
        {
            if (responseWait != null)
                throw new Exception("a SendAndWaitForMatching is already running");

            var expected = new[] {"Info.LastTimecode"};

            var libWait = new ManualResetEvent(false);
            var sdkWait = new ManualResetEvent(false);

            var pendingLib = expected.ToList();
            var pendingSdk = expected.ToList();

            void HandlerLib(object sender, string queueKey)
            {
                Output.WriteLine("SendAndWaitForMatching: Got Lib change: " + queueKey);

                lock (pendingLib)
                {
                    pendingLib.Remove(queueKey);
                    if (pendingLib.Count == 0)
                        libWait.Set();
                }
            }
            void HandlerSdk(object sender, string queueKey)
            {
                Output.WriteLine("SendAndWaitForMatching: Got Sdk change: " + queueKey);

                lock (pendingSdk)
                {
                    pendingSdk.Remove(queueKey);
                    if (pendingSdk.Count == 0)
                        sdkWait.Set();
                }
            }

            _client.OnStateChange += HandlerLib;
            _client.OnSdkStateChange += HandlerSdk;

            doSend();

            // Wait for the expected time. If no response, then go with last data
            libWait.WaitOne(timeout == -1 ? CommandWaitTime * 3 : timeout);
            // The Sdk doesn't send the same notifies if nothing changed, so once the lib has finished, wait a small time for sdk to finish up
            sdkWait.WaitOne(timeout == -1 ? CommandWaitTime * 2 : timeout);

            _client.OnStateChange -= HandlerLib;
            _client.OnSdkStateChange -= HandlerSdk;

            if (pendingLib.Count > 0)
                Output.WriteLine("SendAndWaitForMatching: Pending Lib changes: " + string.Join(", ", pendingLib));

            if (pendingSdk.Count > 0)
                Output.WriteLine("SendAndWaitForMatching: Pending Sdk changes: " + string.Join(", ", pendingSdk));

            Output.WriteLine("");
        }

        public void AssertStateChanged(AtemState expected)
        {
            List<string> sdk = AtemStateComparer.AreEqual(expected, SdkState);
            List<string> lib = AtemStateComparer.AreEqual(expected, LibState);

            if (sdk.Count > 0 || lib.Count > 0)
            {
                if (sdk.Count > 0)
                {
                    Output.WriteLine("SDK wrong");
                    sdk.ForEach(Output.WriteLine);
                }

                if (lib.Count > 0)
                {
                    Output.WriteLine("Lib wrong");
                    lib.ForEach(Output.WriteLine);
                }

                TestResult = false;
            }
        }
    }

}