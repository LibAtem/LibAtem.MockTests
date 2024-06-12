using BMDSwitcherAPI;
using LibAtem.Commands.AudioRouting;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.AudioRouting
{
    [Collection("ServerClientPool")]
    public class TestAudioRoutingOutput
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAudioRoutingOutput(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

#if !ATEM_v8_1

        private static Dictionary<uint, IBMDSwitcherAudioRoutingOutput> GetRoutableOutputs(AtemMockServerWrapper helper)
        {
            var res = new Dictionary<uint, IBMDSwitcherAudioRoutingOutput>();

            var outputIterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioRoutingOutputIterator>(helper.SdkClient.SdkSwitcher.CreateIterator);
            var outputsList = AtemSDKConverter.ToList<IBMDSwitcherAudioRoutingOutput>(outputIterator.Next);

            foreach (var output in outputsList)
            {
                output.GetId(out uint id);
                res[id] = output;
            }

            return res;
        }

        [Fact]
        public void TestSourceId()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioRoutingOutputSetCommand, AudioRoutingOutputGetCommand>("SourceId");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.AudioRouting, helper =>
            {
                Dictionary<uint, IBMDSwitcherAudioRoutingOutput> allOutputs = GetRoutableOutputs(helper);
                List<uint> chosenIds = Randomiser.SelectionOfGroup(allOutputs.Keys.ToList()).ToList();

                foreach (var outputId in chosenIds)
                {
                    IBMDSwitcherAudioRoutingOutput output = allOutputs[outputId];
                    Assert.NotNull(output);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Assert.NotNull(stateBefore.AudioRouting);

                    for (int i = 0; i < 5; i++)
                    {
                        uint sourceId = Randomiser.RangeInt(65535);

                        stateBefore.AudioRouting.Outputs[outputId].SourceId = sourceId;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            output.SetSource(sourceId);
                        });
                    }
                }
            });
        }

        [Fact]
        public void TestName()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioRoutingOutputSetCommand, AudioRoutingOutputGetCommand>("Name");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.AudioRouting, helper =>
            {
                Dictionary<uint, IBMDSwitcherAudioRoutingOutput> allOutputs = GetRoutableOutputs(helper);
                List<uint> chosenIds = Randomiser.SelectionOfGroup(allOutputs.Keys.ToList()).ToList();

                foreach (var outputId in chosenIds)
                {
                    IBMDSwitcherAudioRoutingOutput output = allOutputs[outputId];
                    Assert.NotNull(output);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Assert.NotNull(stateBefore.AudioRouting);

                    for (int i = 0; i < 5; i++)
                    {
                        string name = Randomiser.String(64);

                        stateBefore.AudioRouting.Outputs[outputId].Name = name;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            output.SetName(name);
                        });
                    }
                }
            });
        }
    }

#endif

}