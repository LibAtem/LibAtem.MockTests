using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class InputCallback : IBMDSwitcherInputCallback, INotify<_BMDSwitcherInputEventType>
    {
        private readonly InputState _state;
        private readonly VideoSource _id;
        private readonly IBMDSwitcherInput _props;
        private readonly Action<CommandQueueKey> _onChange;

        public InputCallback(InputState state, VideoSource id, IBMDSwitcherInput props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeShortNameChanged:
                    _props.GetShortName(out string name);
                    _state.Properties.ShortName = name;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeLongNameChanged:
                    _props.GetLongName(out string longName);
                    _state.Properties.LongName = longName;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeAreNamesDefaultChanged:
                    int isDefault = 0;
                    _props.AreNamesDefault(ref isDefault);
                    //_state.AreNamesDefault = isDefault != 0;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeIsProgramTalliedChanged:
                    _props.IsProgramTallied(out int progTally);
                    _state.Tally.ProgramTally = progTally != 0;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeIsPreviewTalliedChanged:
                    _props.IsPreviewTallied(out int prevTally);
                    _state.Tally.PreviewTally = prevTally != 0;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeAvailableExternalPortTypesChanged:
                    _props.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);
                    _state.Properties.AvailableExternalPortTypes = (ExternalPortTypeFlags)types;
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeCurrentExternalPortTypeChanged:
                    _props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType value);
                    _state.Properties.CurrentExternalPortType = (ExternalPortTypeFlags)value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new InputPropertiesGetCommand() { Id = _id }));
        }
    }
}