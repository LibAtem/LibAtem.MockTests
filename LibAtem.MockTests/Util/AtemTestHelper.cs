using System;
using System.Collections.Generic;
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

        public void CheckStateChanges(AtemState expected, Action<AtemState, AtemState> mutateStates = null)
        {
            AtemState sdkState = SdkState;
            AtemState libState = LibState;
            mutateStates?.Invoke(sdkState, libState);

            List<string> sdk = AtemStateComparer.AreEqual(expected, sdkState);
            List<string> lib = AtemStateComparer.AreEqual(expected, libState);

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