using System;
using System.Collections.Generic;
using LibAtem.MockTests.DeviceMock;
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
        public AtemMockServer Server { get; }
        private Dictionary<string, AtemClientWrapper> _clients;

        public AtemServerClientPool()
        {
            var commandData = new Dictionary<string, IReadOnlyList<byte[]>>();
            foreach (Tuple<ProtocolVersion, string> caseId in DeviceTestCases.All)
            {
                commandData[caseId.Item2] = WiresharkParser.BuildCommands(caseId.Item1, caseId.Item2);
            }
            Server = new AtemMockServer(commandData);
            _clients = new Dictionary<string, AtemClientWrapper>();
        }

        public AtemClientWrapper GetOrCreateClients(string caseId)
        {
            if (_clients.TryGetValue(caseId, out AtemClientWrapper client))
            {
                //Server.ResendDataDumps();
                return client;
            }

            return _clients[caseId] = new AtemClientWrapper("127.0.0.1");
        }

        public void Dispose()
        {
            _clients.Values.ForEach(cl => cl.Dispose());
            Server.Dispose();
        }
    }
}