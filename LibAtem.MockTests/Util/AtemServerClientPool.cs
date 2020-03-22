using System;
using System.Collections.Generic;
using System.Threading;
using LibAtem.Commands;
using LibAtem.MockTests.DeviceMock;
using LibAtem.Net;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;

namespace LibAtem.MockTests.Util
{
    [CollectionDefinition("ServerClientPool")]
    public class ClientCollection : ICollectionFixture<AtemServerClientPool>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public sealed class AtemServerClientPool : IDisposable
    {
        private readonly Dictionary<string, DeviceProfile.DeviceProfile> _deviceProfiles;
        private readonly Dictionary<string, AtemState> _defaultStates;

        private bool _isDisposing;
        private bool _libAtemConnected;

        public AtemMockServer Server { get; }
        public AtemClient LibAtemClient { get; }
        public AtemSdkClientWrapper SdkClient { get; }
        public AtemStateBuilderSettings StateSettings { get; }

        public AtemServerClientPool()
        {
            _deviceProfiles = new Dictionary<string, DeviceProfile.DeviceProfile>();
            _defaultStates = new Dictionary<string, AtemState>();
            StateSettings = new AtemStateBuilderSettings();

            var commandData = new Dictionary<string, IReadOnlyList<byte[]>>();
            foreach (Tuple<ProtocolVersion, string> caseId in DeviceTestCases.All)
            {
                var payloads = DumpParser.BuildCommands(caseId.Item1, caseId.Item2);
                commandData[caseId.Item2] = payloads;

                IReadOnlyList<ICommand> commands = DumpParser.ParseToCommands(caseId.Item1, payloads);

                // Build the device profile
                var deviceProfileBuilder = new DeviceProfile.DeviceProfileHandler();
                deviceProfileBuilder.HandleCommands(null, commands);
                _deviceProfiles[caseId.Item2] = deviceProfileBuilder.Profile;

                // Build a default state
                var state = new AtemState();
                commands.ForEach(cmd => AtemStateBuilder.Update(state, cmd, StateSettings));
                _defaultStates[caseId.Item2] = state;
            }
            Server = new AtemMockServer(commandData);
            // We need the server to have some data for the LibAtem connection.
            Server.CurrentCase = DeviceTestCases.All[0].Item2;
            Server.CurrentVersion = DeviceTestCases.All[0].Item1;

            var connectionEvent = new AutoResetEvent(false);
            LibAtemClient = new AtemClient("127.0.0.1", false);
            LibAtemClient.OnDisconnect += o => { Assert.True(_isDisposing, "LibAtem: Disconnect before disposing"); };
            LibAtemClient.OnConnection += o =>
            {
                Assert.False(_libAtemConnected, "LibAtem: Got reconnect");
                _libAtemConnected = true;
                connectionEvent.Set();
            };
            LibAtemClient.Connect();

            SdkClient = new AtemSdkClientWrapper("127.0.0.1", StateSettings);

            Assert.True(connectionEvent.WaitOne(TimeSpan.FromSeconds(3)), "LibAtem: Connection attempt timed out");
        }

        public DeviceProfile.DeviceProfile GetDeviceProfile(string caseId) => _deviceProfiles[caseId];
        public AtemState GetDefaultState(string caseId) => _defaultStates[caseId].Clone();
        
        public void Dispose()
        {
            _isDisposing = true;
            LibAtemClient.Dispose();
            SdkClient.Dispose();
            Server.Dispose();
        }
    }
}