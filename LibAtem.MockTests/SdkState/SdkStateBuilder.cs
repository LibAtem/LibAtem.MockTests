using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibAtem.MockTests.SdkState
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

            try
            {
                switcher.Get3GSDIOutputLevel(out _BMDSwitcher3GSDIOutputLevel outputLevel);
                state.Settings.SDI3GLevel = AtemEnumMaps.SDI3GOutputLevelMap.FindByValue(outputLevel);
            }
            catch (Exception e)
            {
                // This call fails on models which dont do 3g sdi
                state.Settings.SDI3GLevel = 0;
            }
            try
            {
                switcher.GetSuperSourceCascade(out int cascade);
                state.Settings.SuperSourceCascade = cascade != 0;
            }
            catch (Exception e)
            {
                // This call fails on models which dont have multiple ssrc
                state.Settings.SuperSourceCascade = false;
            }

            switcher.GetPowerStatus(out _BMDSwitcherPowerStatus powerStatus);
            state.Power = new[]
            {
                powerStatus.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply1),
                powerStatus.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply2),
            };

            bool supportsDownConvert = true;
            bool supportsMultiviewer = true;

            var modes = new List<VideoModeInfo>();
            var allModes = Enum.GetValues(typeof(_BMDSwitcherVideoMode)).OfType<_BMDSwitcherVideoMode>().ToArray();
            foreach (_BMDSwitcherVideoMode mode in allModes)
            {
                switcher.DoesSupportVideoMode(mode, out int supported);
                if (supported == 0) continue;

                switcher.DoesVideoModeChangeRequireReconfiguration(mode, out int requiresReconfig);

                var multiviewModes = new List<VideoMode>();
                if (supportsMultiviewer)
                {
                    try
                    {
                        foreach (_BMDSwitcherVideoMode mvMode in allModes)
                        {
                            switcher.DoesSupportMultiViewVideoMode(mode, mvMode, out int mvSupported);
                            if (mvSupported != 0)
                                multiviewModes.Add(AtemEnumMaps.VideoModesMap.FindByValue(mvMode));
                        }
                    }
                    catch (NotImplementedException e)
                    {
                        supportsMultiviewer = false;
                    }
                }

                var downConvertModes = new List<VideoMode>();
                if (supportsDownConvert)
                {
                    try
                    {
                        foreach (_BMDSwitcherVideoMode dcMode in allModes)
                        {
                            switcher.DoesSupportDownConvertedHDVideoMode(mode, dcMode, out int convertSupported);
                            if (convertSupported != 0)
                                downConvertModes.Add(AtemEnumMaps.VideoModesMap.FindByValue(dcMode));
                        }
                    }
                    catch (NotImplementedException e)
                    {
                        supportsDownConvert = false;
                    }
                }

                modes.Add(new VideoModeInfo
                {
                    Mode = AtemEnumMaps.VideoModesMap.FindByValue(mode),
                    RequiresReconfig = requiresReconfig != 0,
                    MultiviewModes = multiviewModes.ToArray(),
                    DownConvertModes = downConvertModes.ToArray(),
                });
            }
            state.Info.SupportedVideoModes = modes.OrderBy(s => s.Mode).ToList();

            try
            {
                switcher.GetMethodForDownConvertedSD(out _BMDSwitcherDownConversionMethod downConvertMethod);
                state.Settings.DownConvertMode = AtemEnumMaps.SDDownconvertModesMap.FindByValue(downConvertMethod);
            }
            catch (Exception e)
            {
                // Not supported
            }

            switcher.DoesSupportAutoVideoMode(out int autoModeSupported);
            state.Info.SupportsAutoVideoMode = autoModeSupported != 0;
            if (state.Info.SupportsAutoVideoMode)
            {
                switcher.GetAutoVideoMode(out int autoVideoMode);
                state.Settings.AutoVideoMode = autoVideoMode != 0;
            }

            SourceStateBuilder.Build(state, switcher);
            Hyperdecks(state, switcher);
            SerialPorts(state, switcher);
            Macros(state.Macros, switcher);
            MediaPoolStateBuilder.Build(state.MediaPool, switcher);
            //TalkbackStateBuilder.Build(state, switcher);
            MixMinusOutputs(state, switcher);
            StreamingStateBuilder.Build(state, switcher);

            state.DownstreamKeyers = DownstreamKeyerStateBuilder.Build(switcher);
            state.MediaPlayers = MediaPlayerStateBuilder.Build(switcher, updateSettings, state.MediaPool.Clips.Count > 0);
            state.MixEffects = MixEffectStateBuilder.Build(switcher);
            state.Settings.MultiViewers = MultiViewerStateBuilder.Build(switcher);

            if (switcher is IBMDSwitcherFairlightAudioMixer fairlight)
                state.Fairlight = FairlightAudioMixerStateBuilder.Build(fairlight);
            if (switcher is IBMDSwitcherAudioMixer audioMixer)
            {
                state.Audio = AudioStateBuilder.Build(audioMixer);
            }

            if (switcher is IBMDSwitcherCameraControl camera)
            {
                // CameraControlBuilder.Build(state, camera);
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
                pool.HasUnsupportedOps((uint)i, out int unsupported);
                pool.GetName((uint)i, out string name);
                pool.GetDescription((uint)i, out string description);
                return new MacroState.ItemState
                {
                    IsUsed = valid != 0,
                    HasUnsupportedOps = unsupported != 0,
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
                st.NetworkAddress = address == 0 ? null : IPUtil.IPToString(BitConverter.GetBytes(address).Reverse().ToArray());

                return st;
            });
        }

        private static void MixMinusOutputs(AtemState state, IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixMinusOutputIterator>(switcher.CreateIterator);
            state.Settings.MixMinusOutputs = AtemSDKConverter.IterateList<IBMDSwitcherMixMinusOutput, SettingsState.MixMinusOutputState>(iterator.Next,
                (props, id) =>
                {
                    props.GetAvailableAudioModes(out _BMDSwitcherMixMinusOutputAudioMode availableModes);
                    props.GetAudioMode(out _BMDSwitcherMixMinusOutputAudioMode mode);
                    props.HasMinusAudioInputId(out int hasInputId);
                    
                    long inputId = 0;
                    if (hasInputId != 0)
                    {
                        // TODO - is this good?
                        props.GetMinusAudioInputId(out inputId);
                    }

                    return new SettingsState.MixMinusOutputState
                    {
                        AudioInputId = (AudioSource) inputId,
                        SupportedModes = AtemEnumMaps.MixMinusModeMap.FindFlagsByValue(availableModes),
                        Mode = AtemEnumMaps.MixMinusModeMap.FindByValue(mode),
                    };
                });
        }
    }
}