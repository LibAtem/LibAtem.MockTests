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

    public sealed class AtemMockServerPoolItem : IDisposable
    {
        private readonly AtemStateBuilderSettings _builderSettings;
        private readonly string _bindIp;
        private readonly Queue<AtemSdkClientWrapper> _sdkClients;
        private readonly List<AtemSdkClientWrapper> _updatingClients;
        private int _nextSdkId;

        // public DeviceProfile.DeviceProfile DeviceProfile { get; }
        // public AtemState DefaultState { get; }
        public AtemMockServer Server { get; }
        public AtemClient LibAtemClient { get; }

        private bool _isDisposing;
        private bool _libAtemConnected;

        public AtemMockServerPoolItem(string caseId, AtemStateBuilderSettings builderSettings, string bindIp)
        {
            _builderSettings = builderSettings;
            _bindIp = bindIp;
            _sdkClients = new Queue<AtemSdkClientWrapper>();
            _updatingClients = new List<AtemSdkClientWrapper>();
            _nextSdkId = 1;

            List<byte[]> payloads = DumpParser.BuildCommands(DeviceTestCases.Version, caseId);
            // IReadOnlyList<ICommand> commands = DumpParser.ParseToCommands(DeviceTestCases.Version, payloads);

            // Build the device profile
            // var deviceProfileBuilder = new DeviceProfile.DeviceProfileHandler();
            // deviceProfileBuilder.HandleCommands(null, commands);
            // DeviceProfile = deviceProfileBuilder.Profile;

            // Build a default state
            // var state = new AtemState();
            // commands.ForEach(cmd => AtemStateBuilder.Update(state, cmd, builderSettings));
            // DefaultState = state;

            Server = new AtemMockServer(bindIp, payloads, DeviceTestCases.Version);

            var connectionEvent = new AutoResetEvent(false);
            LibAtemClient = new AtemClient(bindIp, false);
            LibAtemClient.OnDisconnect += o => { Assert.True(_isDisposing, "LibAtem: Disconnect before disposing"); };
            LibAtemClient.OnConnection += o =>
            {
                Assert.False(_libAtemConnected, "LibAtem: Got reconnect");
                _libAtemConnected = true;
                connectionEvent.Set();
            };
            LibAtemClient.Connect();

            //SdkClient = new AtemSdkClientWrapper(bindIp", StateSettings);

            Assert.True(connectionEvent.WaitOne(TimeSpan.FromSeconds(3)), "LibAtem: Connection attempt timed out");
        }

        public AtemSdkClientWrapper SelectSdkClient()
        {
            lock (_sdkClients)
            {
                if (_sdkClients.TryDequeue(out AtemSdkClientWrapper client))
                    return client;
            }

            return new AtemSdkClientWrapper(_bindIp, _builderSettings, _nextSdkId++);
        }

        public void ResetSdkClient(AtemSdkClientWrapper client, bool dispose)
        {
            if (!dispose)
            {
                lock (_updatingClients)
                    _updatingClients.Add(client);

                void TmpHandler(object o)
                {
                    client.OnSdkStateChange -= TmpHandler;
                    lock (_updatingClients)
                        _updatingClients.Remove(client);

                    lock (_sdkClients)
                        _sdkClients.Enqueue(client);

                }

                client.OnSdkStateChange += TmpHandler;

                Server.ResetClient(client.Id);
            }
            else
            {
                client.Dispose();
            }
        }

        public void Dispose()
        {
            _isDisposing = true;
            LibAtemClient.Dispose();
            //SdkClient.Dispose();
            Server.Dispose();

            lock (_updatingClients)
                _updatingClients.ForEach(client => client.Dispose());
            lock (_sdkClients)
                _sdkClients.ForEach(client => client.Dispose());
        }
    }

    public sealed class AtemServerClientPool : IDisposable
    {
        private readonly Dictionary<string, AtemMockServerPoolItem> _pool;

        public AtemStateBuilderSettings StateSettings { get; }

        public AtemServerClientPool()
        {
            _pool = new Dictionary<string, AtemMockServerPoolItem>();
            StateSettings = new AtemStateBuilderSettings();
        }

        public AtemMockServerPoolItem GetCase(string caseId)
        {
            if (!_pool.TryGetValue(caseId, out AtemMockServerPoolItem item))
            {
                string bindIp = $"127.0.1.{_pool.Count + 1}";
                item = _pool[caseId] = new AtemMockServerPoolItem(caseId, StateSettings, bindIp);
            }
            return item;
        }

        public void Dispose()
        {
            _pool.ForEach(item => item.Value.Dispose());
        }
    }
}