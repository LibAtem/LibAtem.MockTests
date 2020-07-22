using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestInputs
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestInputs(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        public static IBMDSwitcherInput GetInput(AtemMockServerWrapper helper, VideoSource targetId)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(helper.SdkClient.SdkSwitcher.CreateIterator);

            iterator.GetById((long) targetId, out IBMDSwitcherInput input);
            Assert.NotNull(input);
            return input;
        }

        /*
         TODO - sdk doesnt detect changes to this
        [Fact]
        public void TestPortType()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    var inputCmd = previousCommands.OfType<InputPropertiesGetCommand>().Single(c => c.Id == id);

                    for (int i = 0; i < 5; i++)
                    {
                        InternalPortType portType = Randomiser.EnumValue<InternalPortType>();
                        stateBefore.Settings.Inputs[id].Properties.InternalPortType = portType;
                        inputCmd.InternalPortType = portType;

                        helper.SendAndWaitForChange(stateBefore, () => { helper.Server.SendCommands(inputCmd); });
                    }
                }
            });
        }
        */

        [Fact] // TODO fix
        public void TestShortName()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<InputPropertiesSetCommand, InputPropertiesGetCommand>("ShortName");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    IBMDSwitcherInput input = GetInput(helper, id);

                    for (int i = 0; i < 5; i++)
                    {
                        string newName = Guid.NewGuid().ToString().Substring(0, 4);
                        stateBefore.Settings.Inputs[id].Properties.ShortName = newName;

                        helper.SendAndWaitForChange(stateBefore, () => { input.SetShortName(newName); });
                    }
                }
            });
        }

        [Fact] // TODO fix
        public void TestLongName()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<InputPropertiesSetCommand, InputPropertiesGetCommand>("LongName");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    IBMDSwitcherInput input = GetInput(helper, id);

                    for (int i = 0; i < 5; i++)
                    {
                        string newName = Guid.NewGuid().ToString().Substring(0, 20);
                        stateBefore.Settings.Inputs[id].Properties.LongName = newName;

                        helper.SendAndWaitForChange(stateBefore, () => { input.SetLongName(newName); });
                    }
                }
            });
        }

    }
}