using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.ComparisonTests.State;
using LibAtem.Net;
using LibAtem.State;
using LibAtem.State.Builder;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Util
{
    public sealed class AtemTestHelper : IDisposable
    {
        private readonly AtemClient _libAtemClient;
        private AtemState _libAtemState;

        public const int CommandWaitTime = 80;

        public AtemSdkClientWrapper SdkClient { get; }

        public AtemStateBuilderSettings StateSettings { get; }

        public bool TestResult { get; set; } = true;

        public DeviceProfile.DeviceProfile Profile { get; }

        public delegate void CommandKeyHandler(object sender, string path);
        public event CommandKeyHandler OnLibAtemStateChange;

        public AtemTestHelper(AtemSdkClientWrapper client, ITestOutputHelper output, AtemClient libAtemClient, DeviceProfile.DeviceProfile profile, AtemState libAtemState, AtemStateBuilderSettings stateSettings)
        {
            _libAtemClient = libAtemClient;
            _libAtemState = libAtemState;
            SdkClient = client;
            Output = output;
            Profile = profile;
            StateSettings = stateSettings;

            _libAtemClient.OnReceive += LibAtemReceive;

            SyncStates();
            AssertStatesMatch();
        }

        public void SyncStates()
        {
           _libAtemState = SdkClient.State;
        }

        private void LibAtemReceive(object sender, IReadOnlyList<ICommand> commands)
        {
            foreach (ICommand cmd in commands)
            {
                // TODO - handle result?
                IUpdateResult result = AtemStateBuilder.Update(_libAtemState, cmd, StateSettings);
                foreach (string change in result.ChangedPaths)
                {
                    OnLibAtemStateChange?.Invoke(this, change);
                }
            }
        }

        public void Dispose()
        {
            _libAtemClient.OnReceive -= LibAtemReceive;
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
        
        public ITestOutputHelper Output { get; }

        public AtemState SdkState => SdkClient.State;
        public AtemState LibState => _libAtemState.Clone();

        public IBMDSwitcher SdkSwitcher => SdkClient.SdkSwitcher;

        
        public void SendAndWaitForChange(Action doSend, int timeout = -1)
        {
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

            OnLibAtemStateChange += HandlerLib;
            SdkClient.OnSdkStateChange += HandlerSdk;

            doSend();

            // Wait for the expected time. If no response, then go with last data
            libWait.WaitOne(timeout == -1 ? CommandWaitTime * 3 : timeout);
            // The Sdk doesn't send the same notifies if nothing changed, so once the lib has finished, wait a small time for sdk to finish up
            sdkWait.WaitOne(timeout == -1 ? CommandWaitTime * 2 : timeout);

            OnLibAtemStateChange -= HandlerLib;
            SdkClient.OnSdkStateChange -= HandlerSdk;

            if (pendingLib.Count > 0)
                Output.WriteLine("SendAndWaitForMatching: Pending Lib changes: " + string.Join(", ", pendingLib));

            if (pendingSdk.Count > 0)
                Output.WriteLine("SendAndWaitForMatching: Pending Sdk changes: " + string.Join(", ", pendingSdk));

            Output.WriteLine("");
        }

        public void CheckStateChanges(AtemState expected)
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