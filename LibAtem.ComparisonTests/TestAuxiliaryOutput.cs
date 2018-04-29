using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestAuxiliaryOutput
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestAuxiliaryOutput(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }
        
        [Fact]
        public void TestAuxCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Dictionary<VideoSource, IBMDSwitcherInputAux> sdkAux = helper.GetSdkInputsOfType<IBMDSwitcherInputAux>();
                Assert.Equal((int)helper.Profile.Auxiliaries, sdkAux.Count);

                Assert.True(sdkAux.Keys.All(k => k.GetPortType() == InternalPortType.Auxiliary));
            }
        }

        [Fact]
        public void TestAuxSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputAux> a in helper.GetSdkInputsOfType<IBMDSwitcherInputAux>())
                {
                    AuxiliaryId auxId = AtemEnumMaps.GetAuxId(a.Key);
                    IBMDSwitcherInputAux aux = a.Value;

                    // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                    // We track this another way
                    aux.GetInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                    Assert.Equal(availabilityMask, (_BMDSwitcherInputAvailability) ((int) SourceAvailability.Auxiliary << 2));

                    long[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.Auxiliary)).Select(s => (long)s).ToArray();
                    long[] badValues = VideoSourceLists.All.Select(s => (long)s).Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(long v) => new AuxSourceSetCommand
                    {
                        Id = auxId,
                        Source = (VideoSource)v,
                    };

                    void UpdateExpectedState(ComparisonState state, long v) => state.Auxiliaries[auxId].Source = (VideoSource) v;

                    ValueTypeComparer<long>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, badValues);   
                }
            }
        }
    }
}