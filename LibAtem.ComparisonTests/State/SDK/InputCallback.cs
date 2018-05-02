using System;
using BMDSwitcherAPI;
using LibAtem.Common;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class InputCallback : IBMDSwitcherInputCallback, INotify<_BMDSwitcherInputEventType>
    {
        private readonly ComparisonInputState _state;
        private readonly IBMDSwitcherInput _props;

        public InputCallback(ComparisonInputState state, IBMDSwitcherInput props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeShortNameChanged:
                    _props.GetShortName(out string name);
                    _state.ShortName = name;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeLongNameChanged:
                    _props.GetLongName(out string longName);
                    _state.LongName = longName;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeAreNamesDefaultChanged:
                    int isDefault = 0;
                    _props.AreNamesDefault(ref isDefault);
                    _state.AreNamesDefault = isDefault != 0;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeIsProgramTalliedChanged:
                    _props.IsProgramTallied(out int progTally);
                    _state.ProgramTally = progTally != 0;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeIsPreviewTalliedChanged:
                    _props.IsPreviewTallied(out int prevTally);
                    _state.PreviewTally = prevTally != 0;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeAvailableExternalPortTypesChanged:
                    _props.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);
                    _state.AvailableExternalPortTypes = AtemEnumMaps.ExternalPortTypeMap.FindFlagsByValue(types);
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeCurrentExternalPortTypeChanged:
                    _props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType value);
                    _state.CurrentExternalPortType = AtemEnumMaps.ExternalPortTypeMap.FindFlagsByValue(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}