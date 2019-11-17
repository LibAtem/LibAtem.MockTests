using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.Settings
{
    [Collection("Client")]
    public class TestSerialPort
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestSerialPort(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        private static List<IBMDSwitcherSerialPort> GetPorts(AtemComparisonHelper helper)
        {
            Guid itId = typeof(IBMDSwitcherSerialPortIterator).GUID;
            helper.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherSerialPortIterator iterator = (IBMDSwitcherSerialPortIterator) Marshal.GetObjectForIUnknown(itPtr);

            List<IBMDSwitcherSerialPort> result = new List<IBMDSwitcherSerialPort>();
            for (iterator.Next(out IBMDSwitcherSerialPort r); r != null; iterator.Next(out r))
                result.Add(r);

            return result;
        }

        [Fact]
        public void TestSerialPortCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                List<IBMDSwitcherSerialPort> ports = GetPorts(helper);
                Assert.Equal((int) helper.Profile.SerialPort, ports.Count);
                Assert.True(ports.Count <= 1); // Only 1 port is currently supported, so we are not prepared for there to be more
            }
        }

        private class SerialPortFunctionTestDefinition : TestDefinitionBase<SerialPortModeCommand, SerialMode>
        {
            private readonly IBMDSwitcherSerialPort _sdk;

            public SerialPortFunctionTestDefinition(AtemComparisonHelper helper, IBMDSwitcherSerialPort sdk) : base(helper)
            {
                _sdk = sdk;
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetFunction(AtemEnumMaps.SerialModeMap.Values.ToArray()[1]);

            public override string PropertyName => "SerialMode";

            public override SerialMode[] GoodValues => AtemEnumMaps.SerialModeMap.Keys.ToArray();

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, SerialMode v)
            {
                state.Settings.SerialMode = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, SerialMode v)
            {
                yield return new CommandQueueKey(new SerialPortModeCommand());
            }
        }


        [SkippableFact]
        public void TestSerialPortFunction()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherSerialPort port = GetPorts(helper).FirstOrDefault();
                Skip.If(port == null, "Model does not have a serial port");

                foreach (KeyValuePair<SerialMode, _BMDSwitcherSerialPortFunction> func in AtemEnumMaps.SerialModeMap)
                {
                    port.DoesSupportFunction(func.Value, out int supported);
                    Assert.NotEqual(0, supported);
                }

                new SerialPortFunctionTestDefinition(helper, port).Run();
            }
        }
    }
}