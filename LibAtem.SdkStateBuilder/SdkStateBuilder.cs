﻿using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using System;
using System.Linq;

namespace LibAtem.SdkStateBuilder
{
    public static class SdkStateBuilder
    {
        public static AtemState Build(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            var state = new AtemState();

            switcher.GetProductName(out string productName);
            state.Info.ProductName = productName;
            switcher.GetVideoMode(out _BMDSwitcherVideoMode videoMode);
            state.Settings.VideoMode = AtemEnumMaps.VideoModesMap.FindByValue(videoMode);
            switcher.GetTimeCodeLocked(out int locked);
            state.Info.TimecodeLocked = locked != 0;
            switcher.GetTimeCode(out byte hours, out byte minutes, out byte seconds, out byte frames, out int dropFrame);
            state.Info.LastTimecode = new Timecode
            {
                Hour = hours,
                Minute = minutes,
                Second = seconds,
                Frame = frames,
                DropFrame = dropFrame != 0
            };

            SourceStateBuilder.Build(state, switcher);
            Hyperdecks(state, switcher);
            SerialPorts(state, switcher);
            Macros(state.Macros, switcher);
            MediaPoolStateBuilder.Build(state.MediaPool, switcher);

            state.DownstreamKeyers = DownstreamKeyerStateBuilder.Build(switcher);
            state.MediaPlayers = MediaPlayerStateBuilder.Build(switcher, updateSettings, state.MediaPool.Clips.Count > 0);
            state.MixEffects = MixEffectStateBuilder.Build(switcher);
            state.Settings.MultiViewers = MultiViewerStateBuilder.Build(switcher);

            if (switcher is IBMDSwitcherFairlightAudioMixer fairlight)
                state.Fairlight = FairlightAudioMixerStateBuilder.Build(fairlight);
            if (switcher is IBMDSwitcherAudioMixer audioMixer)
            {
                state.Audio = AudioStateBuilder.Build(audioMixer);
                if (switcher is IBMDSwitcherTalkback talkback)
                    TalkbackStateBuilder.Build(state.Audio.Talkback, talkback);
            }

            return state;
        }

        private static void SerialPorts(AtemState state, IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSerialPortIterator>(switcher.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherSerialPort>(iterator.Next, (port, id) =>
            {
                if (id != 0) throw new Exception("Too many serial ports");

                port.GetFunction(out _BMDSwitcherSerialPortFunction function);
                state.Settings.SerialMode = AtemEnumMaps.SerialModeMap.FindByValue(function);
            });
        }

        private static void Macros(MacroState state, IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMacroPool;
            pool.GetMaxCount(out uint count);
            state.Pool = Enumerable.Range(0, (int)count).Select(i =>
            {
                pool.IsValid((uint)i, out int valid);
                pool.GetName((uint)i, out string name);
                pool.GetDescription((uint)i, out string description);
                return new MacroState.ItemState
                {
                    IsUsed = valid != 0,
                    Name = name,
                    Description = description
                };
            }).ToList();

            var control = switcher as IBMDSwitcherMacroControl;
            control.GetRunStatus(out _BMDSwitcherMacroRunStatus status, out int loop, out uint index);

            switch (status)
            {
                case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusIdle:
                    state.RunStatus.RunStatus = MacroState.MacroRunStatus.Idle;
                    break;
                case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusRunning:
                    state.RunStatus.RunStatus = MacroState.MacroRunStatus.Running;
                    break;
                case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusWaitingForUser:
                    state.RunStatus.RunStatus = MacroState.MacroRunStatus.UserWait;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            state.RunStatus.Loop = loop != 0;
            state.RunStatus.RunIndex = index;
            control.GetRecordStatus(out _BMDSwitcherMacroRecordStatus recStatus, out uint recIndex);
            state.RecordStatus.IsRecording = recStatus == _BMDSwitcherMacroRecordStatus.bmdSwitcherMacroRecordStatusRecording;
            state.RecordStatus.RecordIndex = recIndex;
        }

        private static void Hyperdecks(AtemState state, IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherHyperDeckIterator>(switcher.CreateIterator);
            state.Settings.Hyperdecks = AtemSDKConverter.IterateList<IBMDSwitcherHyperDeck, SettingsState.HyperdeckState>(iterator.Next, (props, id) =>
            {
                var st = new SettingsState.HyperdeckState();

                props.GetSwitcherInput(out long inputId);
                st.Input = (VideoSource)inputId;
                props.GetAutoRollOnTake(out int autoRoll);
                st.AutoRoll = autoRoll != 0;
                props.GetAutoRollOnTakeFrameDelay(out ushort frameDelay);
                st.AutoRollFrameDelay = frameDelay;
                props.GetNetworkAddress(out uint address);
                st.NetworkAddress = address == 0 ? null : IPUtil.IPToString(BitConverter.GetBytes(address));

                return st;
            });
        }

    }
}