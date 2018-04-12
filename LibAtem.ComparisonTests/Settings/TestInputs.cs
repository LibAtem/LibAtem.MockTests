using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using Xunit;

namespace LibAtem.ComparisonTests.Settings
{
    [Collection("Client")]
    public class TestInputs
    {
        private static readonly IReadOnlyDictionary<InternalPortType, _BMDSwitcherPortType> PortTypeMap;
        private static readonly IReadOnlyDictionary<ExternalPortType, _BMDSwitcherExternalPortType> ExternalPortTypeMap;

        static TestInputs()
        {
            PortTypeMap = new Dictionary<InternalPortType, _BMDSwitcherPortType>
            {
                {InternalPortType.Auxiliary, _BMDSwitcherPortType.bmdSwitcherPortTypeAuxOutput},
                {InternalPortType.Black, _BMDSwitcherPortType.bmdSwitcherPortTypeBlack},
                {InternalPortType.ColorBars, _BMDSwitcherPortType.bmdSwitcherPortTypeColorBars},
                {InternalPortType.ColorGenerator, _BMDSwitcherPortType.bmdSwitcherPortTypeColorGenerator},
                {InternalPortType.External, _BMDSwitcherPortType.bmdSwitcherPortTypeExternal},
                {InternalPortType.Mask, _BMDSwitcherPortType.bmdSwitcherPortTypeKeyCutOutput},
                {InternalPortType.MEOutput, _BMDSwitcherPortType.bmdSwitcherPortTypeMixEffectBlockOutput},
                {InternalPortType.MediaPlayerKey, _BMDSwitcherPortType.bmdSwitcherPortTypeMediaPlayerCut},
                {InternalPortType.MediaPlayerFill, _BMDSwitcherPortType.bmdSwitcherPortTypeMediaPlayerFill},
                {InternalPortType.SuperSource, _BMDSwitcherPortType.bmdSwitcherPortTypeSuperSource},
            };

            // Note: This map is expected to be incomplete, as bm include audio in it
            ExternalPortTypeMap = new Dictionary<ExternalPortType, _BMDSwitcherExternalPortType>
            {
                {ExternalPortType.Internal, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeInternal},
                {ExternalPortType.SDI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeSDI},
                {ExternalPortType.HDMI, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeHDMI},
                {ExternalPortType.Composite, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeComposite},
                {ExternalPortType.Component, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeComponent},
                {ExternalPortType.SVideo, _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeSVideo},
            };
        }

        private readonly AtemClientWrapper _client;

        public TestInputs(AtemClientWrapper client)
        {
            _client = client;
        }

        [Fact]
        public void EnsureInternalPortMapIsComplete()
        {
            EnumMap.EnsureIsComplete(PortTypeMap);
        }

        // TODO - test LibAtem setters

        [Fact]
        public void TestInputProperties()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                Dictionary<VideoSource, IBMDSwitcherInput> sdkInputs = helper.GetSdkInputsOfType<IBMDSwitcherInput>();
                List<InputPropertiesGetCommand> libAtemInputs = helper.FindAllOfType<InputPropertiesGetCommand>();

                Assert.Equal(sdkInputs.Count, libAtemInputs.Count);

                List<VideoSource> sdkIds = sdkInputs.Keys.ToList();
                List<VideoSource> libAtemIds = libAtemInputs.Select(i => i.Id).ToList();

                libAtemIds.Sort();
                sdkIds.Sort();
                Assert.Equal(sdkIds, libAtemIds);

                var failures = new List<string>();

                // TODO compare input properties
                // TODO - rewrite to use ValueComparers
                foreach (InputPropertiesGetCommand libAtemInput in libAtemInputs.OrderBy(i => i.Id))
                {
                    if (!sdkInputs.TryGetValue(libAtemInput.Id, out IBMDSwitcherInput sdkInput))
                    {
                        failures.Add(string.Format("Missing sdk input: {0}", libAtemInput.Id));
                        continue;
                    }

                    sdkInput.GetLongName(out string longName);
                    if (longName != libAtemInput.LongName)
                        failures.Add(string.Format("{0}: Long name mismatch: {1}, {2}", libAtemInput.Id, longName, libAtemInput.LongName));

                    sdkInput.GetShortName(out string shortName);
                    if (shortName != libAtemInput.ShortName)
                        failures.Add(string.Format("{0}: Short name mismatch: {1}, {2}", libAtemInput.Id, shortName, libAtemInput.ShortName));

                    sdkInput.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType sdkPortType);
                    if (sdkPortType != ExternalPortTypeMap[libAtemInput.ExternalPortType])
                        failures.Add(string.Format("{0}: ExternalPortType mismatch: {1}, {2}", libAtemInput.Id, sdkPortType, libAtemInput.ExternalPortType));

                    sdkInput.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);
                    _BMDSwitcherExternalPortType thisTypes = libAtemInput.ExternalPorts != null
                        ? (_BMDSwitcherExternalPortType) libAtemInput.ExternalPorts
                            .Select(p => (int) ExternalPortTypeMap[p]).Sum()
                        : _BMDSwitcherExternalPortType.bmdSwitcherExternalPortTypeInternal;
                    if (types != thisTypes)
                        failures.Add(string.Format("{0}: ExternalPortType mismatch: {1}, {2}", libAtemInput.Id, types, thisTypes));

                    sdkInput.GetPortType(out _BMDSwitcherPortType portType);
                    InternalPortType[] expectedLibAtem = PortTypeMap.Where(i => i.Value == portType).Select(i => i.Key).ToArray();
                    if (expectedLibAtem.Length == 0 || expectedLibAtem[0] != libAtemInput.InternalPortType)
                    {
                        var current = expectedLibAtem.Length == 0 ? "" : expectedLibAtem[0].ToString();
                        failures.Add(string.Format("{0}: Internal port type mismatch: {1}, {2}", libAtemInput.Id, current, libAtemInput.InternalPortType));
                    }

                    sdkInput.GetInputAvailability(out _BMDSwitcherInputAvailability availability);
                    int libAtemValue = ((int) libAtemInput.SourceAvailability << 2) + (int) libAtemInput.MeAvailability;
                    if (libAtemValue != (int) availability)
                    {
                        failures.Add(string.Format("{0}: Soure availability mismatch: {1}, {2}", libAtemInput.Id, (int) availability, libAtemValue));
                    }


                    //            public bool IsExternal { get; set; }

                    //                    IsProgramTallied Returns a flag indicating whether the input is currently
                    //                    IsPreviewTallied Returns a flag indicating whether the input is currently

                    //                    AreNamesDefault Determine if the long name and short name are both curren
                    //                    ResetNames Reset the long and short names for this switcher input to the
                    //                    SetCurrentExternalPortType
                    //                    SetShortName Set the short name describing the switcher input as a string
                    //                    SetLongName Set the long name describing the switcher input as a Unicode
                }

                Assert.Equal(new List<string>(), failures);
            }
        }
    }
}