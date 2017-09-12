using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using Xunit;

namespace AtemEmulator.ComparisonTests.Settings
{
    public class TestInputs : AtemCommandTestBase
    {
        protected override bool LogLibAtemHandshake => true;

        private static readonly IReadOnlyDictionary<InternalPortType, _BMDSwitcherPortType> portTypeMap;

        static TestInputs()
        {
            portTypeMap = new Dictionary<InternalPortType, _BMDSwitcherPortType>
            {
                {InternalPortType.Auxilary, _BMDSwitcherPortType.bmdSwitcherPortTypeAuxOutput},
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
        }

        [Fact]
        public void TestInputProperties()
        {
            Guid itId = typeof(IBMDSwitcherInputIterator).GUID;
            _sdkSwitcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherInputIterator iterator = (IBMDSwitcherInputIterator) Marshal.GetObjectForIUnknown(itPtr);

            List<IBMDSwitcherInput> sdkInputs = new List<IBMDSwitcherInput>();
            for (iterator.Next(out IBMDSwitcherInput input); input != null; iterator.Next(out input))
                sdkInputs.Add(input);

            List<InputPropertiesGetCommand> libAtemInputs = GetReceivedCommands<InputPropertiesGetCommand>();

            Assert.Equal(sdkInputs.Count, libAtemInputs.Count);
            
            List<long> sdkIds = sdkInputs.Select(i =>
            {
                i.GetInputId(out var id);
                return id;
            }).ToList();
            List<long> libAtemIds = libAtemInputs.Select(i => (long) i.Id).ToList();
            Assert.Equal(sdkIds, libAtemIds);

            var failures = new List<string>();

            // TODO compare input properties
            foreach (InputPropertiesGetCommand libAtemInput in libAtemInputs.OrderBy(i => i.Id))
            {
                IBMDSwitcherInput sdkInput = sdkInputs.FirstOrDefault(i =>
                {
                    i.GetInputId(out var id);
                    return id == (long) libAtemInput.Id;
                });
                if (sdkInput == null)
                {
                    failures.Add(string.Format("Missing sdk input: {0}", libAtemInput.Id));
                    continue;
                }

                sdkInput.GetLongName(out string longName);
                if (longName != libAtemInput.LongName)
                {
                    failures.Add(string.Format("{0}: Long name mismatch: {1}, {2}", libAtemInput.Id, longName, libAtemInput.LongName));
                    continue;
                }

                sdkInput.GetShortName(out string shortName);
                if (shortName != libAtemInput.ShortName)
                {
                    failures.Add(string.Format("{0}: Short name mismatch: {1}, {2}", libAtemInput.Id, shortName, libAtemInput.ShortName));
                    continue;
                }

                // TODO these need matching up in some way
                //                sdkInput.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);
                //sdkInput.GetCurrentExternalPortType();

                sdkInput.GetPortType(out _BMDSwitcherPortType portType);
                InternalPortType[] expectedLibAtem = portTypeMap.Where(i => i.Value == portType).Select(i => i.Key).ToArray();
                if (expectedLibAtem.Length == 0 || expectedLibAtem[0] != libAtemInput.InternalPortType)
                {
                    var current = expectedLibAtem.Length == 0 ? "" : expectedLibAtem[0].ToString();
                    failures.Add(string.Format("{0}: Internal port type mismatch: {1}, {2}", libAtemInput.Id, current, libAtemInput.InternalPortType));
                    continue;
                }

                sdkInput.GetInputAvailability(out _BMDSwitcherInputAvailability availability);
                int libAtemValue = ((int) libAtemInput.SourceAvailability << 2) + (int) libAtemInput.MeAvailability;
                if (libAtemValue != (int)availability)
                {
                    failures.Add(string.Format("{0}: Soure availability mismatch: {1}, {2}", libAtemInput.Id, (int)availability, libAtemValue));
                    continue;
                }

                
                //            public bool IsExternal { get; set; }
                //            public ExternalPortType ExternalPortType { get; set; }

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