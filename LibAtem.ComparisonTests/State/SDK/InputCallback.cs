using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class InputCallback : SdkCallbackBaseNotify<IBMDSwitcherInput, _BMDSwitcherInputEventType>, IBMDSwitcherInputCallback
    {
        private readonly InputState _state;

        public InputCallback(InputState state, IBMDSwitcherInput props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeShortNameChanged:
                    Props.GetShortName(out string name);
                    _state.Properties.ShortName = name;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeLongNameChanged:
                    Props.GetLongName(out string longName);
                    _state.Properties.LongName = longName;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeAreNamesDefaultChanged:
                    int isDefault = 0;
                    Props.AreNamesDefault(ref isDefault);
                    //_state.AreNamesDefault = isDefault != 0;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeIsProgramTalliedChanged:
                    Props.IsProgramTallied(out int progTally);
                    _state.Tally.ProgramTally = progTally != 0;
                    OnChange("Tally");
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeIsPreviewTalliedChanged:
                    Props.IsPreviewTallied(out int prevTally);
                    _state.Tally.PreviewTally = prevTally != 0;
                    OnChange("Tally");
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeAvailableExternalPortTypesChanged:
                    Props.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);
                    _state.Properties.AvailableExternalPortTypes = (ExternalPortTypeFlags)types;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeCurrentExternalPortTypeChanged:
                    Props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType value);
                    _state.Properties.CurrentExternalPortType = (ExternalPortTypeFlags)value;
                    OnChange("Properties");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}