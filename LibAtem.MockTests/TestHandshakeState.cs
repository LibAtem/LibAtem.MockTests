



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibAtem.Commands;
using LibAtem.ComparisonTests.State;
using LibAtem.MockTests.DeviceMock;
using LibAtem.MockTests.Util;
using LibAtem.Net;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    public class TestHandshakeState
    {
        private readonly ITestOutputHelper _output;

        public TestHandshakeState(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestStateMock2ME() => RunTest(DeviceTestCases.Legacy2ME_8_0_1);

        [Fact]
        public void TestStateMockConstellation() => RunTest(DeviceTestCases.Constellation_8_0_2);

        [Fact]
        public void TestStateMockMini() => RunTest(DeviceTestCases.Mini_8_1);
        
        /*
        [Fact]
        public void TestStateReal()
        {
            var stateSettings = new AtemStateBuilderSettings();
            using var helper = new AtemSdkClientWrapper("10.42.13.95", stateSettings);

            var libAtemState = GetLibAtemState(stateSettings, "10.42.13.95");

            List<string> before = AtemStateComparer.AreEqual(helper.State, libAtemState);
            if (before.Count != 0 && _output != null)
            {
                _output.WriteLine("state mismatch:");
                before.ForEach(_output.WriteLine);
            }
            Assert.Empty(before);
        }
        */

        private AtemState GetLibAtemState(AtemStateBuilderSettings stateSettings, string address)
        {
            using var client = new AtemClient(address, false);
            var state = new AtemState();

            AutoResetEvent handshakeEvent = new AutoResetEvent(false);
            bool handshakeFinished = false;
            client.OnReceive += (o, cmds) =>
            {
                cmds.ForEach(cmd => AtemStateBuilder.Update(state, cmd, stateSettings));

                if (!handshakeFinished && cmds.Any(c => c is InitializationCompleteCommand))
                {
                    handshakeEvent.Set();
                    handshakeFinished = true;
                }
            };
            client.Connect();
            Assert.True(handshakeEvent.WaitOne(5000));

            return state;
        }

        private void RunTest(Tuple<ProtocolVersion, string> caseId)
        {
            var commandData = DumpParser.BuildCommands(caseId.Item1, caseId.Item2);
            /*
            var result = new List<string>();
            foreach (byte[] payload in commandData)
            {
                foreach (ParsedCommand rawCmd in ReceivedPacket.ParseCommands(payload))
                {
                    if (CommandParser.Parse(caseId.Item1, rawCmd) == null)
                    {
                        _output.WriteLine("{0} - {1}", rawCmd.Name, rawCmd.BodyLength);
                        result.Add(rawCmd.Name);
                    }
                }
            }
            Assert.Empty(result);
            // */
            

            using var server = new AtemMockServer(commandData);
            var stateSettings = new AtemStateBuilderSettings();
            using var helper = new AtemSdkClientWrapper("127.0.0.1", stateSettings);

            var libAtemState = GetLibAtemState(stateSettings, "127.0.0.1");

            List<string> before = AtemStateComparer.AreEqual(helper.BuildState(), libAtemState);
            if (before.Count != 0 && _output != null)
            {
                _output.WriteLine("state mismatch:");
                before.ForEach(_output.WriteLine);
            }
            Assert.Empty(before);
        }
    }
}
