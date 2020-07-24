using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.Util;
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

                        helper.SendFromServerAndWaitForChange(stateBefore, inputCmd);
                    }
                }
            });
        }
        */

        [Fact]
        public void TestAreNamesDefault()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    InputState.PropertiesState inputState = stateBefore.Settings.Inputs[id].Properties;

                    var cmd = helper.Server.GetParsedDataDump()
                        .OfType<InputPropertiesGetCommand>().Single(c => c.Id == id);

                    IBMDSwitcherInput input = GetInput(helper, id);

                    for (int i = 0; i < 5; i++)
                    {
                        inputState.AreNamesDefault = cmd.AreNamesDefault = !inputState.AreNamesDefault;

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }

        [Fact]
        public void TestResetNames()
        {
            var expectedCmd = new InputNameResetCommand();
            AtemMockServerWrapper.Each(_output, _pool, CommandGenerator.MatchCommand(expectedCmd), DeviceTestCases.All, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    expectedCmd.Id = id;

                    IBMDSwitcherInput input = GetInput(helper, id);

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        input.ResetNames();
                    });
                }
            });
        }

        [Fact]
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

        class LongNameHandler : UploadJobWorker
        {
            private readonly bool _hasMultiview;
            private readonly Func<Lazy<ImmutableList<ICommand>>, ICommand, IEnumerable<ICommand>> _propHandler;

            public ManualResetEvent Wait { get; } = new ManualResetEvent(false);

            public LongNameHandler(uint targetBytes, ITestOutputHelper output, VideoSource index, bool hasMultiview) 
                : base(targetBytes, output, 0xffff, (uint) index, DataTransferUploadRequestCommand.TransferMode.Clear2 | DataTransferUploadRequestCommand.TransferMode.Write)
            {
                _hasMultiview = hasMultiview;
                _propHandler = CommandGenerator.CreateAutoCommandHandler<InputPropertiesSetCommand, InputPropertiesGetCommand>("LongName");
            }

            public override IEnumerable<ICommand> HandleCommand(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
            {
                List<ICommand> propRes = _propHandler(previousCommands, cmd).ToList();
                if (propRes.Count > 0)
                    return propRes;

                return _hasMultiview ? base.HandleCommand(previousCommands, cmd) : new List<ICommand>();
            }

            protected override IEnumerable<ICommand> CompleteGetCommands()
            {
                Wait.Set();

                yield break;
            }
        }

        [Fact]
        public void TestLongName()
        {
            LongNameHandler worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MultiviewLabelSample, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    IBMDSwitcherInput input = GetInput(helper, id);

                    bool hasMultiviewers = stateBefore.Info.MultiViewers != null;

                    // TODO - this should really use a real byte count
                    worker = new LongNameHandler(0, _output, id, hasMultiviewers);

                    string newName = Guid.NewGuid().ToString().Substring(0, 20);
                    stateBefore.Settings.Inputs[id].Properties.LongName = newName;

                    // Send the change
                    input.SetLongName(newName);

                    if (hasMultiviewers)
                    {
                        // Process the upload
                        helper.HandleUntil(worker.Wait, 5000);
                        Assert.True(worker.Wait.WaitOne(500));
                    }

                    // TODO remove this, but we need to ensure we let the work queue drain sufficiently
                    helper.HandleUntil(null, 1000);

                    // The name property should be updated
                    helper.Helper.CheckStateChanges(stateBefore);
                }
            });
        }

        [Fact]
        public void TestAvailableExternalPortType()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                List<InputPropertiesGetCommand> previousCommands = helper.Server.GetParsedDataDump().OfType<InputPropertiesGetCommand>().ToList();

                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    var inputCmd = previousCommands.Single(c => c.Id == id);

                    for (int i = 0; i < 5; i++)
                    {
                        var portTypes = Randomiser.FlagComponents<VideoPortType>(VideoPortType.None);
                        stateBefore.Settings.Inputs[id].Properties.AvailableExternalPortTypes = portTypes;
                        inputCmd.AvailableExternalPorts = portTypes.CombineFlagComponents();

                        helper.SendFromServerAndWaitForChange(stateBefore, inputCmd);
                    }
                }
            });
        }

        [Fact]
        public void TestCurrentExternalPortType()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<InputPropertiesSetCommand, InputPropertiesGetCommand>("ExternalPortType");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                AtemState tmpState = helper.Helper.BuildLibState();
                List<VideoSource> inputIds = tmpState.Settings.Inputs.Keys.Where(i =>
                    tmpState.Settings.Inputs[i].Properties.AvailableExternalPortTypes.Count > 1).ToList();

                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    InputState inputState = stateBefore.Settings.Inputs[id];

                    IBMDSwitcherInput input = GetInput(helper, id);

                    List<VideoPortType> targets = Randomiser.SelectionOfGroup(inputState.Properties.AvailableExternalPortTypes.ToList())
                        .ToList();

                    tested = true;

                    foreach (VideoPortType value in targets)
                    {
                        _BMDSwitcherExternalPortType value2 = AtemEnumMaps.ExternalPortTypeMap[value];
                        inputState.Properties.CurrentExternalPortType = value;

                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            input.SetCurrentExternalPortType(value2);
                        });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestTally()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                List<VideoSource> inputIds = helper.Helper.BuildLibState().Settings.Inputs
                    .Where(i => i.Value.Properties.InternalPortType == InternalPortType.External).Select(i => i.Key)
                    .ToList();
                TallyBySourceCommand tallyCmd = helper.Server.GetParsedDataDump().OfType<TallyBySourceCommand>().Single();

                foreach (VideoSource id in Randomiser.SelectionOfGroup(inputIds))
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    InputState.TallyState tallyState = stateBefore.Settings.Inputs[id].Tally;

                    for (int i = 0; i < 5; i++)
                    {
                        bool isProgram = tallyState.ProgramTally = Randomiser.RangeInt(10) > 5;
                        bool isPreview = tallyState.PreviewTally = Randomiser.RangeInt(10) > 5;
                        tallyCmd.Tally[id] = Tuple.Create(isProgram, isPreview);

                        helper.SendFromServerAndWaitForChange(stateBefore, tallyCmd);
                    }
                }
            });
        }


    }
}