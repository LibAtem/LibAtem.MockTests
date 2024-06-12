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
    public class TestAudioRoutingSource
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAudioRoutingSource(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

#if !ATEM_v8_1

        private static Dictionary<uint, IBMDSwitcherAudioRoutingSource> GetRoutableSources(AtemMockServerWrapper helper)
        {
            var res = new Dictionary<uint, IBMDSwitcherAudioRoutingSource>();

            var sourceIterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioRoutingSourceIterator>(helper.SdkClient.SdkSwitcher.CreateIterator);
            var sourceList = AtemSDKConverter.ToList<IBMDSwitcherAudioRoutingSource>(sourceIterator.Next);

            foreach (var source in sourceList)
            {
                source.GetId(out uint id);
                res[id] = source;
            }

            return res;
        }

        [Fact]
        public void TestName()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioRoutingSourceSetCommand, AudioRoutingSourceGetCommand>("Name");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.AudioRouting, helper =>
            {
                Dictionary<uint, IBMDSwitcherAudioRoutingSource> allSources = GetRoutableSources(helper);
                List<uint> chosenIds = Randomiser.SelectionOfGroup(allSources.Keys.ToList()).ToList();

                foreach (var sourceId in chosenIds)
                {
                    IBMDSwitcherAudioRoutingSource source = allSources[sourceId];
                    Assert.NotNull(source);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    Assert.NotNull(stateBefore.AudioRouting);

                    for (int i = 0; i < 5; i++)
                    {
                        string name = Randomiser.String(64);

                        stateBefore.AudioRouting.Sources[sourceId].Name = name;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            source.SetName(name);
                        });
                    }
                }
            });
        }

#endif

    }
}