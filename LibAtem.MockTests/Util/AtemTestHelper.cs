using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.MockTests.DeviceMock;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Util
{
    public sealed class AtemTestHelper : IDisposable
    {
        private readonly AtemMockServer _mockServer;
        private readonly object _libAtemStateLock = new object();

        // private readonly AtemClient _libAtemClient;
        private AtemState _libAtemState;

        public AtemSdkClientWrapper SdkClient { get; }

        public AtemStateBuilderSettings StateSettings { get; }

        public bool TestResult { get; set; } = true;

        // public DeviceProfile.DeviceProfile Profile { get; }

        public delegate void CommandKeyHandler(object sender, string path);
        public event CommandKeyHandler OnLibAtemStateChange;

        public delegate void CommandHandler(object sender, ICommand command);
        public event CommandHandler OnLibAtemCommand;

        public AtemTestHelper(AtemSdkClientWrapper client, ITestOutputHelper output, AtemMockServer mockServer, AtemStateBuilderSettings stateSettings)
        {
            _mockServer = mockServer;
            SdkClient = client;
            Output = output;
            StateSettings = stateSettings;

            SyncStates();
            _mockServer.LibReceive += LibAtemReceive;

            AssertStatesMatch();
        }

        public void SyncStates()
        {
            lock (_libAtemStateLock)
            {
                _libAtemState = SdkClient.BuildState();
            }
        }

        private void LibAtemReceive(object sender, IReadOnlyList<byte[]> commands)
        {
            lock (_libAtemStateLock)
            {
                foreach (byte[] cmdBytes in commands)
                {
                    // It should be safe to assume exactly one per 
                    ParsedCommand.ReadNextCommand(cmdBytes, 0, out ParsedCommandSpec? cmd);
                    if (cmd == null)
                        throw new Exception("Failed to parse command");

                    ICommand cmd2 = CommandParser.Parse(_mockServer.CurrentVersion, cmd.Value);
                    if (cmd2 == null)
                        throw new Exception("Failed to parse command2");

                    OnLibAtemCommand?.Invoke(this, cmd2);

                    // TODO - handle result?
                    IUpdateResult result = AtemStateBuilder.Update(_libAtemState, cmd2, StateSettings);
                    foreach (string change in result.ChangedPaths)
                    {
                        OnLibAtemStateChange?.Invoke(this, change);
                    }
                }
            }
        }

        public void Dispose()
        {
            _mockServer.LibReceive -= LibAtemReceive;
            Assert.True(TestResult);
        }

        public void AssertStatesMatch()
        {
            List<string> before = AtemStateComparer.AreEqual(BuildSdkState(), BuildLibState());
            if (before.Count != 0 && Output != null)
            {
                Output.WriteLine("state mismatch:");
                before.ForEach(Output.WriteLine);
            }
            Assert.Empty(before);
        }
        
        public ITestOutputHelper Output { get; }

        public AtemState BuildSdkState() => SdkClient.BuildState();

        public AtemState BuildLibState()
        {
            AtemState libState;
            lock (_libAtemStateLock)
            {
                libState = _libAtemState.Clone();
            }

            return SanitiseStateIncompabalities(_mockServer.CurrentVersion, libState);
        }

        public static AtemState SanitiseStateIncompabalities(ProtocolVersion version, AtemState state)
        {
            if (version < ProtocolVersion.V8_1_1)
            {
                // Before 8.1.2, the sdk was broken when trying to access the equalizer bands, so we need to discard this data to match
                state.Fairlight?.Inputs.ForEach(input =>
                {
                    input.Value.Sources.ForEach(source =>
                    {
                        source.Equalizer.Bands = new List<FairlightAudioState.EqualizerBandState>();
                    });
                });
            }

            return state;
        }

        public IBMDSwitcher SdkSwitcher => SdkClient.SdkSwitcher;

        public void CheckStateChanges(AtemState expected, Action<AtemState, AtemState> mutateStates = null)
        {
            AtemState sdkState = BuildSdkState();
            AtemState libState = BuildLibState();
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