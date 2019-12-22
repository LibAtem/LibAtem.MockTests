using System;
using System.Collections.Generic;
using LibAtem.MockTests.DeviceMock;
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

        public AtemServerClientPool()
        {
            var commandData = new Dictionary<string, IReadOnlyList<byte[]>>();
            foreach (Tuple<ProtocolVersion, string> caseId in DeviceTestCases.All)
            {
                commandData[caseId.Item2] = WiresharkParser.BuildCommands(caseId.Item1, caseId.Item2);
            }
            Server = new AtemMockServer(commandData);

        }

        public void Dispose()
        {
            // TODO
            Server.Dispose();
        }
    }
}