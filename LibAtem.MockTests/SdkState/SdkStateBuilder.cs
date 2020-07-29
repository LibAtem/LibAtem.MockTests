using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

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

            switcher.GetAreOutputsConfigurable(out int configurable);
            state.Info.OnlyConfigurableOutputs = configurable != 0;
            state.Info.HasCameraControl = switcher is IBMDSwitcherCameraControl;
            state.Info.AdvancedChromaKeyers = MixEffectStateBuilder.SupportsAdvancedChromaKeyers(switcher);

            try
            {
                switcher.Get3GSDIOutputLevel(out _BMDSwitcher3GSDIOutputLevel outputLevel);
                state.Settings.SDI3GLevel = AtemEnumMaps.SDI3GOutputLevelMap.FindByValue(outputLevel);
            }
            catch (Exception)
            {
                // This call fails on models which dont do 3g sdi
                state.Settings.SDI3GLevel = 0;
            }
            try
            {
                switcher.GetSuperSourceCascade(out int cascade);
                state.Settings.SuperSourceCascade = cascade != 0;
            }
            catch (Exception)
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
                    catch (NotImplementedException)
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
                    catch (NotImplementedException)
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

            if (supportsMultiviewer)
            {
                foreach (VideoModeInfo mode in state.Info.SupportedVideoModes)
                {
                    switcher.GetMultiViewVideoMode(AtemEnumMaps.VideoModesMap[mode.Mode],
                        out _BMDSwitcherVideoMode mvMode);
                    state.Settings.MultiviewVideoModes[mode.Mode] = AtemEnumMaps.VideoModesMap.FindByValue(mvMode);
                }
            }
            if (supportsDownConvert)
            {
                foreach (VideoModeInfo mode in state.Info.SupportedVideoModes)
                {
                    switcher.GetDownConvertedHDVideoMode(AtemEnumMaps.VideoModesMap[mode.Mode],
                        out _BMDSwitcherVideoMode dcMode);
                    state.Settings.DownConvertVideoModes[mode.Mode] = AtemEnumMaps.VideoModesMap.FindByValue(dcMode);
                }
            }

            try
            {
                switcher.GetMethodForDownConvertedSD(out _BMDSwitcherDownConversionMethod downConvertMethod);
                state.Settings.DownConvertMode = AtemEnumMaps.SDDownconvertModesMap.FindByValue(downConvertMethod);
            }
            catch (Exception)
            {
                // Not supported
            }

            switcher.DoesSupportAutoVideoMode(out int autoModeSupported);
            state.Info.SupportsAutoVideoMode = autoModeSupported != 0;
            if (state.Info.SupportsAutoVideoMode)
            {
                switcher.GetAutoVideoMode(out int autoVideoMode);
                state.Settings.AutoVideoMode = autoVideoMode != 0;
                switcher.GetAutoVideoModeDetected(out int detected);
                state.Settings.DetectedVideoMode = detected != 0;
            }

            DveInfo(state, switcher);

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
            MultiViewerStateBuilder.Build(switcher, state);

            if (switcher is IBMDSwitcherFairlightAudioMixer fairlight)
                state.Fairlight = FairlightAudioMixerStateBuilder.Build(fairlight);
            if (switcher is IBMDSwitcherAudioMixer audioMixer)
            {
                state.Audio = AudioStateBuilder.Build(audioMixer);
            }

            if (switcher is IBMDSwitcherCameraControl camera)
            {
                CameraControlBuilder.Build(state, camera, updateSettings);
            }

            return state;
        }

        private static void DveInfo(AtemState state, IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(switcher.CreateIterator);
            var me = AtemSDKConverter.ToList<IBMDSwitcherMixEffectBlock>(iterator.Next).FirstOrDefault();
            if (me == null) return;

            var keyers = AtemSDKConverter.CastSdk<IBMDSwitcherKeyIterator>(me.CreateIterator);
            var keyer = AtemSDKConverter.ToList<IBMDSwitcherKey>(keyers.Next).FirstOrDefault();
            if (keyer == null) return;

            var flyKey = keyer as IBMDSwitcherKeyFlyParameters;
            var dveTrans = me as IBMDSwitcherTransitionDVEParameters;

            if (flyKey == null || dveTrans == null) return;
            
            flyKey.GetCanRotate(out int canRotate);
            flyKey.GetCanScaleUp(out int canScaleUp);

            var dveStyles = new List<DVEEffect>();
            foreach (DVEEffect style in Enum.GetValues(typeof(DVEEffect)).OfType<DVEEffect>())
            {
                _BMDSwitcherDVETransitionStyle style2 = AtemEnumMaps.DVEStyleMap[style];
                dveTrans.DoesSupportStyle(style2, out int supported);
                if (supported != 0)
                    dveStyles.Add(style);
            }

            dveTrans.GetNumSupportedStyles(out uint styleCount);
            if (dveStyles.Count != styleCount)
                throw new Exception("Mismatch in number of supported DVE transition styles");

            state.Info.DVE = new InfoState.DveInfoState
            {
                CanScaleUp = canScaleUp != 0,
                CanRotate = canRotate != 0,
                SupportedTransitions = dveStyles,
            };
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
            state.Hyperdecks = AtemSDKConverter.IterateList<IBMDSwitcherHyperDeck, HyperdeckState>(iterator.Next, (props, id) =>
            {
                var st = new HyperdeckState();

                props.GetSwitcherInput(out long inputId);
                st.Settings.Input = (VideoSource)inputId;
                props.GetAutoRollOnTake(out int autoRoll);
                st.Settings.AutoRoll = autoRoll != 0;
                props.GetAutoRollOnTakeFrameDelay(out ushort frameDelay);
                st.Settings.AutoRollFrameDelay = frameDelay;
                props.GetNetworkAddress(out uint address);
                st.Settings.NetworkAddress = IPUtil.IPToString(BitConverter.GetBytes(address).Reverse().ToArray());

                props.GetLoopedPlayback(out int loop);
                st.Player.Loop = loop != 0;
                props.GetSingleClipPlayback(out int single);
                st.Player.SingleClip = single != 0;

                props.GetPlayerState(out _BMDSwitcherHyperDeckPlayerState playState);
                st.Player.State = playState == _BMDSwitcherHyperDeckPlayerState.bmdSwitcherHyperDeckStateUnknown
                    ? HyperDeckPlayerState.Idle
                    : AtemEnumMaps.HyperDeckPlayerStateMap.FindByValue(playState);
                props.GetShuttleSpeed(out int speed);
                st.Player.PlaybackSpeed = speed;

                props.GetCurrentClipTime(out ushort clipHours, out byte clipMinutes, out byte clipSeconds, out byte clipFrames);
                st.Player.ClipTime = new HyperDeckTime
                    {Hour = clipHours, Minute = clipMinutes, Second = clipSeconds, Frame = clipFrames};
                props.GetCurrentTimelineTime(out ushort tlHours, out byte tlMinutes, out byte tlSeconds, out byte tlFrames);
                st.Player.TimelineTime = new HyperDeckTime
                    {Hour = tlHours, Minute = tlMinutes, Second = tlSeconds, Frame = tlFrames};

                props.GetCurrentClip(out long clipId);
                st.Storage.CurrentClipId = (int)clipId;

                props.GetFrameRate(out uint frameRate, out uint timeScale);
                st.Storage.FrameRate = frameRate;
                st.Storage.TimeScale = timeScale;
                props.IsInterlacedVideo(out int isInterlaced);
                st.Storage.IsInterlaced = isInterlaced != 0;
                props.IsDropFrameTimeCode(out int isDropFrame);
                st.Storage.IsDropFrameTimecode = isDropFrame != 0;

                props.GetEstimatedRecordTimeRemaining(out ushort recordHours, out byte recordMinutes,
                    out byte recordSeconds, out byte recordFrames);
                st.Storage.RemainingRecordTime = new HyperDeckTime
                    {Hour = recordHours, Minute = recordMinutes, Second = recordSeconds, Frame = recordFrames};

                props.GetConnectionStatus(out _BMDSwitcherHyperDeckConnectionStatus status);
                st.Settings.Status = AtemEnumMaps.HyperDeckConnectionStatusMap.FindByValue(status);

                props.IsRemoteAccessEnabled(out int remoteEnabled);
                st.Settings.IsRemoteEnabled = remoteEnabled != 0;

                props.GetStorageMediaCount(out uint storageCount);
                st.Settings.StorageMediaCount = storageCount;
                props.GetActiveStorageMedia(out int activeMedia);
                st.Storage.ActiveStorageMedia = activeMedia;
                if (activeMedia >= 0)
                {
                    props.GetStorageMediaState((uint) activeMedia, out _BMDSwitcherHyperDeckStorageMediaState storageState);
                    st.Storage.ActiveStorageStatus = AtemEnumMaps.HyperDeckStorageStatusMap.FindByValue(storageState);
                }

                var clipIterator = AtemSDKConverter.CastSdk<IBMDSwitcherHyperDeckClipIterator>(props.CreateIterator);
                st.Clips = AtemSDKConverter.IterateList<IBMDSwitcherHyperDeckClip, HyperdeckState.ClipState>(clipIterator.Next, (clip, i) =>
                {
                    clip.GetId(out long clipId);
                    clip.GetDuration(out ushort hours, out byte minutes, out byte seconds, out byte frames);
                    clip.GetName(out string name);
                    clip.GetTimelineStart(out ushort startHours, out byte startMinutes, out byte startSeconds,
                        out byte startFrames);
                    clip.GetTimelineEnd(out ushort endHours, out byte endMinutes, out byte endSeconds,
                        out byte endFrames);

                    clip.IsInfoAvailable(out int infoAvailable);
                    clip.IsValid(out int valid);

                    Assert.Equal(1, valid);
                    // Assert.Equal(1, infoAvailable);

                    return new HyperdeckState.ClipState
                    {
                        Name = name,
                        Duration = infoAvailable != 0
                            ? new HyperDeckTime {Hour = hours, Minute = minutes, Second = seconds, Frame = frames}
                            : null,
                        TimelineStart = infoAvailable != 0
                            ? new HyperDeckTime
                                {Hour = startHours, Minute = startMinutes, Second = startSeconds, Frame = startFrames}
                            : null,
                        TimelineEnd = infoAvailable != 0
                            ? new HyperDeckTime
                                {Hour = endHours, Minute = endMinutes, Second = endSeconds, Frame = endFrames}
                            : null,
                    };
                });

                props.GetClipCount(out uint count);
                Assert.Equal((int) count, st.Clips.Count);
                
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
                        props.GetMinusAudioInputId(out inputId);

                    return new SettingsState.MixMinusOutputState
                    {
                        HasAudioInputId = hasInputId != 0,
                        AudioInputId = (AudioSource) inputId,
                        SupportedModes = AtemEnumMaps.MixMinusModeMap.FindFlagsByValue(availableModes),
                        Mode = AtemEnumMaps.MixMinusModeMap.FindByValue(mode),
                    };
                });
        }
    }
}