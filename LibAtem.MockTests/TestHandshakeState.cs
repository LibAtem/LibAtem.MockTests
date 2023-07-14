



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibAtem.Commands;
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
        public void TestStateMock2ME() => RunTest(DeviceTestCases.TwoME);

        [Fact]
        public void TestStateMock2ME4K() => RunTest(DeviceTestCases.TwoME4K);

        [Fact]
        public void TestStateMock4ME4K() => RunTest(DeviceTestCases.FourME4K);

        [Fact]
        public void TestStateMockConstellation() => RunTest(DeviceTestCases.Constellation);

        [Fact]
        public void TestStateMockConstellation2MEHD() => RunTest(DeviceTestCases.Constellation2MEHD);

        [Fact]
        public void TestStateMockMini() => RunTest(DeviceTestCases.Mini);
        
        [Fact]
        public void TestStateMockTVSHD() => RunTest(DeviceTestCases.TVSHD);

        [Fact]
        public void TestStateMockTVS() => RunTest(DeviceTestCases.TVS);

        [Fact]
        public void TestStateMockMiniExtremeIso() => RunTest(DeviceTestCases.MiniExtremeIso);

        [Fact]
        public void TestStateMockTVSHD8() => RunTest(DeviceTestCases.TVSHD8);

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

        private void RunTest(string caseId)
        {
            if (caseId == "") return;

            var commandData = DumpParser.BuildCommands(DeviceTestCases.Version, caseId);
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
            

            using var server = new AtemMockServer("127.0.0.1", commandData, DeviceTestCases.Version);
            var stateSettings = new AtemStateBuilderSettings();
            using var helper = new AtemSdkClientWrapper("127.0.0.1", stateSettings, 1);

            var libAtemState = AtemTestHelper.SanitiseStateIncompabalities(DeviceTestCases.Version,
                GetLibAtemState(stateSettings, "127.0.0.1"));
            var sdkState = helper.BuildState();

            List<string> before = AtemStateComparer.AreEqual(sdkState, libAtemState);
            if (before.Count != 0 && _output != null)
            {
                _output.WriteLine("state mismatch:");
                before.ForEach(_output.WriteLine);
            }
            Assert.Empty(before);
        }
    }
}
