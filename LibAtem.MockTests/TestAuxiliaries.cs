using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestAuxiliaries
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAuxiliaries(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestSource()
        {
            AtemMockServerWrapper.Each(_output, _pool, SourceCommandHandler, DeviceTestCases.All, helper =>
            {
                Dictionary<VideoSource, IBMDSwitcherInputAux> allAuxes = helper.GetSdkInputsOfType<IBMDSwitcherInputAux>();
                VideoSource[] chosenIds = VideoSourceUtil.TakeSelection(allAuxes.Keys.ToArray());

                foreach (VideoSource auxSource in chosenIds)
                {
                    AuxiliaryId auxId = AtemEnumMaps.GetAuxId(auxSource);
                    IBMDSwitcherInputAux aux = allAuxes[auxSource];

                    // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                    // We track this another way
                    aux.GetInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                    Assert.Equal(availabilityMask, (_BMDSwitcherInputAvailability)((int)SourceAvailability.Auxiliary << 2));

                    AtemState stateBefore = helper.Helper.BuildLibState();

                    List<VideoSource> deviceSources = stateBefore.Settings.Inputs.Keys.ToList();

                    VideoSource[] validSources = deviceSources.Where(s =>
                        s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask) &&
                        s.IsAvailable(SourceAvailability.Auxiliary)).ToArray();
                    var sampleSources = VideoSourceUtil.TakeSelection(validSources);
                    
                    foreach (VideoSource src in sampleSources)
                    {
                        stateBefore.Auxiliaries[(int)auxId].Source = src;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            aux.SetInputSource((long)src);
                        });
                    }
                }
            });
        }

        private static IEnumerable<ICommand> SourceCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is AuxSourceSetCommand auxCmd)
            {
                Assert.Equal((uint) 1, auxCmd.Mask);

                var previous = previousCommands.Value.OfType<AuxSourceGetCommand>().Last(a => a.Id == auxCmd.Id);
                Assert.NotNull(previous);

                previous.Source = auxCmd.Source;
                yield return previous;
            }
        }

    }
}