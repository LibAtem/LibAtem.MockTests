using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Settings
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

        private static IBMDSwitcherSerialPort GetPort(AtemComparisonHelper helper)
        {
            return GetPorts(helper).FirstOrDefault();
        }

        [Fact]
        public void TestSerialPortCount()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                List<IBMDSwitcherSerialPort> ports = GetPorts(helper);
                Assert.Equal((int) helper.Profile.SerialPort, ports.Count);
                Assert.True(ports.Count <= 1); // Only 1 port is currently supported, so we are not prepared for there to be more
            }
        }

        [Fact]
        public void TestSerialPortFunction()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                IBMDSwitcherSerialPort port = GetPort(helper);
                if (port == null)
                {
                    _output.WriteLine("No serial ports on device. Skipping tests");
                    return;
                }

                foreach (KeyValuePair<SerialMode, _BMDSwitcherSerialPortFunction> func in AtemEnumMaps.SerialModeMap)
                {
                    port.DoesSupportFunction(func.Value, out int supported);
                    Assert.NotEqual(0, supported);
                }

                ICommand Setter(SerialMode v) => new SerialPortModeCommand { SerialMode = v };

                void UpdateExpectedState(ComparisonState state, SerialMode v) => state.Settings.SerialMode = v;

                SerialMode[] newVals = AtemEnumMaps.SerialModeMap.Keys.ToArray();

                ValueTypeComparer<SerialMode>.Run(helper, Setter, UpdateExpectedState, newVals);
            }
        }
    }
}