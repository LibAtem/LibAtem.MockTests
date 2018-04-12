using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Settings
{
    [Collection("Client")]
    public class TestSerialPort
    {
        private static readonly IReadOnlyDictionary<SerialMode, _BMDSwitcherSerialPortFunction> FunctionMap;

        static TestSerialPort()
        {
            FunctionMap = new Dictionary<SerialMode, _BMDSwitcherSerialPortFunction>
            {
                {SerialMode.None, _BMDSwitcherSerialPortFunction.bmdSwitcherSerialPortFunctionNone},
                {SerialMode.Gvg100, _BMDSwitcherSerialPortFunction.bmdSwitcherSerialPortFunctionGvg100},
                {SerialMode.PtzVisca, _BMDSwitcherSerialPortFunction.bmdSwitcherSerialPortFunctionPtzVisca},
            };
        }

        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestSerialPort(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }
        
        [Fact]
        public void EnsureFunctionMapIsComplete()
        {
            EnumMap.EnsureIsComplete(FunctionMap);
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

                bool fail = false;

                // Ensure all modes are supported before continuing
                foreach (KeyValuePair<SerialMode, _BMDSwitcherSerialPortFunction> func in FunctionMap)
                {
                    port.DoesSupportFunction(func.Value, out int supported);
                    if (supported == 0)
                        fail = fail || WriteAndFail(String.Format("Unsupported serial mode: {0}. Unsure how to handle", func.Key));
                }
                Assert.False(fail);

                fail = CheckSerialPortMatches(helper, port);

                // Now try and set each one in turn
                foreach (KeyValuePair<SerialMode, _BMDSwitcherSerialPortFunction> func in FunctionMap)
                {
                    helper.SendCommand(new SerialPortModeCommand {SerialMode = func.Key});
                    helper.Sleep();

                    fail = fail || CheckSerialPortMatches(helper, port, func.Key);
                }

                Assert.False(fail);
            }
        }

        private bool CheckSerialPortMatches(AtemComparisonHelper helper, IBMDSwitcherSerialPort sdkProps, SerialMode? expected=null)
        {
            sdkProps.GetFunction(out _BMDSwitcherSerialPortFunction func);
            var cmd = helper.FindWithMatching(new SerialPortModeCommand());
            if (cmd == null || FunctionMap[cmd.SerialMode] != func)
                return WriteAndFail(string.Format("Mismatched serial mode. {0}, {1}", cmd?.SerialMode, func));

            if (expected != null && expected.Value != cmd.SerialMode)
                return WriteAndFail(string.Format("Mismatched serial mode. {0}, {1}", cmd.SerialMode, expected));

            return false;
        }

        private bool WriteAndFail(string s)
        {
            _output.WriteLine(s);
            return true;
        }
    }
}