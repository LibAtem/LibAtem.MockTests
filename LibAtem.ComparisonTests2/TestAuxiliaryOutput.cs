using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2
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

        private class AuxSourceTestDefinition : TestDefinitionBase<long>
        {
            private readonly IBMDSwitcherInputAux _sdk;
            private readonly AuxiliaryId _auxId;

            public AuxSourceTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputAux sdk, AuxiliaryId id) : base(helper)
            {
                _sdk = sdk;
                _auxId = id;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputSource((long)VideoSource.ColorBars);
            }

            public override long[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.Auxiliary)).Select(s => (long)s).ToArray();
            }
            public override long[] BadValues()
            {
                var goodValues = GoodValues();
                return VideoSourceLists.All.Select(s => (long)s).Where(s => !goodValues.Contains(s)).ToArray();
            }

            public override ICommand GenerateCommand(long v)
            {
                return new AuxSourceSetCommand
                {
                    Id = _auxId,
                    Source = (VideoSource)v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, long v)
            {
                if (goodValue)
                {
                    state.Auxiliaries[_auxId].Source = (VideoSource)v;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, long v)
            {
                yield return new CommandQueueKey(new AuxSourceGetCommand() { Id = _auxId });
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
                    Assert.Equal(availabilityMask, (_BMDSwitcherInputAvailability)((int)SourceAvailability.Auxiliary << 2));

                    new AuxSourceTestDefinition(helper, aux, auxId).Run();
                }
            }
        }

    }
}