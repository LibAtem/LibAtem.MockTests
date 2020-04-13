using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestSerialPort
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestSerialPort(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private List<Tuple<uint, IBMDSwitcherSerialPort>> GetSerialPorts(AtemMockServerWrapper helper)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSerialPortIterator>(helper.SdkClient.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<uint, IBMDSwitcherSerialPort>>();
            uint index = 0;
            for (iterator.Next(out IBMDSwitcherSerialPort r); r != null; iterator.Next(out r))
            {
                result.Add(Tuple.Create(index, r));
                index++;
            }

            return result;
        }

        [Fact]
        public void TestMode()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<SerialPortModeCommand, SerialPortModeCommand>("SerialMode", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SerialPort, helper =>
            {
                IBMDSwitcherSerialPort port = GetSerialPorts(helper).Single().Item2;

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    SerialMode mode = Randomiser.EnumValue<SerialMode>();

                    // TODO - when are these not supported?
                    port.DoesSupportFunction(AtemEnumMaps.SerialModeMap[mode], out int supported);
                    Assert.Equal(1, supported);

                    stateBefore.Settings.SerialMode = mode;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        port.SetFunction(AtemEnumMaps.SerialModeMap[mode]);
                    });
                }
            });
        }

    }
}