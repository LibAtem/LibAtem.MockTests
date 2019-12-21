using System;
using System.Collections.Generic;
using System.Text;
using LibAtem.ComparisonTests.State;
using LibAtem.MockTests.DeviceMock;
using LibAtem.MockTests.Util;
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
        public void TestStateMock2ME() => RunTest("8.0.1-2me");

        [Fact]
        public void TestStateMockConstellation() => RunTest("8.0.2-constellation");

        [Fact]
        public void TestStateMockMini() => RunTest("8.1-mini");
        
        private void RunTest(string filename)
        {
            var commandData = WiresharkParser.BuildCommands(ProtocolVersion.V8_0_1, $"TestFiles/Handshake/{filename}.pcapng");
            using var server = new AtemMockServer(commandData);
            using var helper = new AtemClientWrapper("127.0.0.1");

            List<string> before = AtemStateComparer.AreEqual(helper.SdkState, helper.LibState);
            if (before.Count != 0 && _output != null)
            {
                _output.WriteLine("state mismatch:");
                before.ForEach(_output.WriteLine);
            }
            Assert.Empty(before);
        }
    }
}
