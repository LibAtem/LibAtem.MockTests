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
        public void TestStateMock2ME() => RunTest(DeviceTestCases.Legacy2ME_8_0_1);

        [Fact]
        public void TestStateMockConstellation() => RunTest(DeviceTestCases.Constellation_8_0_2);

        [Fact]
        public void TestStateMockMini() => RunTest(DeviceTestCases.Mini_8_1);
        
        private void RunTest(Tuple<ProtocolVersion, string> caseId)
        {
            var commandData = WiresharkParser.BuildCommands(caseId.Item1, caseId.Item2);
            using var server = new AtemMockServer(commandData);
            using var helper = new AtemClientWrapper("127.0.0.1");
            helper.BindSdkState();

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
