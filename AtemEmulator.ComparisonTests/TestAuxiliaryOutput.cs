using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.XmlState;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests
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
            using (var helper = new AtemComparisonHelper(_client))
            {
                Dictionary<VideoSource, IBMDSwitcherInputAux> sdkAux = helper.GetSdkInputsOfType<IBMDSwitcherInputAux>();
                Assert.Equal((int)helper.Profile.Auxiliaries, sdkAux.Count);

                Assert.True(sdkAux.Keys.All(k => k.GetPortType() == InternalPortType.Auxiliary));
            }
        }

        [Fact]
        public void TestAuxProperties()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                Dictionary<VideoSource, IBMDSwitcherInputAux> sdkAux = helper.GetSdkInputsOfType<IBMDSwitcherInputAux>();

                var failures = new List<string>();

                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputAux> a in sdkAux)
                {
                    AuxiliaryId auxId = GetAuxId(a.Key);
                    IBMDSwitcherInputAux aux = a.Value;

                    // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                    // We track this another way
                    aux.GetInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                    if (availabilityMask != (_BMDSwitcherInputAvailability) ((int)SourceAvailability.Auxiliary << 2))
                        failures.Add("Incorrect SourceAvailability value");

                    failures.AddRange(CheckAuxProps(helper, aux, auxId));
                    helper.ClearReceivedCommands();

                    // Now try changing value and ensure an update is received

                    helper.SendCommand(new AuxSourceSetCommand
                    {
                        Id = auxId,
                        Source = VideoSource.Color1
                    });
                    helper.Sleep();
                    failures.AddRange(CheckAuxProps(helper, aux, auxId, VideoSource.Color1));
                    if (helper.CountAndClearReceivedCommands<AuxSourceGetCommand>() == 0)
                        failures.Add("No response when setting aux input");

                    helper.SendCommand(new AuxSourceSetCommand
                    {
                        Id = auxId,
                        Source = VideoSource.Input2
                    });
                    helper.Sleep();
                    failures.AddRange(CheckAuxProps(helper, aux, auxId, VideoSource.Input2));
                    if (helper.CountAndClearReceivedCommands<AuxSourceGetCommand>() == 0)
                        failures.Add("No response when setting aux input");
                }

                failures.ForEach(f => _output.WriteLine(f));
                Assert.Equal(new List<string>(), failures);
            }
        }

        private static AuxiliaryId GetAuxId(VideoSource id)
        {
            if (id >= VideoSource.Auxilary1 && id <= VideoSource.Auxilary6)
                return (AuxiliaryId)(id - VideoSource.Auxilary1);

            throw new Exception("Not an Aux");
        }

        private static IEnumerable<string> CheckAuxProps(AtemComparisonHelper helper, IBMDSwitcherInputAux sdkProps, AuxiliaryId id, VideoSource? expected=null)
        {
            var auxCmd = helper.FindWithMatching(new AuxSourceGetCommand { Id = id });
            if (auxCmd == null)
            {
                yield return string.Format("{0}: Aux missing state props", id);
                yield break;
            }

            sdkProps.GetInputSource(out long src);
            if ((VideoSource)src != auxCmd.Source)
                yield return string.Format("{0}: Aux source mismatch: {1}, {2}", id, (VideoSource)src, auxCmd.Source);

            if (expected != null && expected.Value != auxCmd.Source)
                yield return string.Format("{0}: Aux source mismatch: {1}, {2}", id, expected, auxCmd.Source);
        }
    }
}