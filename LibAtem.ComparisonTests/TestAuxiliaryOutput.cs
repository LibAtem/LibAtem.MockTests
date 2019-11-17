using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
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
        
        private class AuxSourceTestDefinition : TestDefinitionBase<AuxSourceSetCommand, VideoSource>
        {
            private readonly IBMDSwitcherInputAux _sdk;
            private readonly AuxiliaryId _auxId;

            public AuxSourceTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputAux sdk, AuxiliaryId id) : base(helper)
            {
                _sdk = sdk;
                _auxId = id;
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputSource((long)VideoSource.ColorBars);

            public override void SetupCommand(AuxSourceSetCommand cmd)
            {
                cmd.Id = _auxId;
            }

            public override string PropertyName => "Source";

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.Auxiliary)).ToArray();
            
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.Auxiliaries[(int)_auxId].Source = (VideoSource)v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
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