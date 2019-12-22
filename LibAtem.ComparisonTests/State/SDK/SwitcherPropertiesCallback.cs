using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.State.Builder;
using Xunit;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SwitcherPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcher, _BMDSwitcherEventType>, IBMDSwitcherCallback
    {
        private readonly AtemState _state;

        public SwitcherPropertiesCallback(AtemState state, IBMDSwitcher props, Action<string> onChange, AtemStateBuilderSettings updateSettings) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();

            props.GetProductName(out string productName);
            state.Info.ProductName = productName;

            SetupInputs();
            SetupMixEffects();
            SetupSerialPorts();
            SetupMultiViews();
            SetupDownstreamKeyers();
            SetupMediaPool();
            SetupMediaPlayers(updateSettings);
            SetupMacros();
            SetupAudio();
            SetupHyperdecks();
        }

        public override void Notify(_BMDSwitcherEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherEventType.bmdSwitcherEventTypeVideoModeChanged:
                    Props.GetVideoMode(out _BMDSwitcherVideoMode videoMode);
                    _state.Settings.VideoMode = AtemEnumMaps.VideoModesMap.FindByValue(videoMode);
                    OnChange("Settings.VideoMode");
                    break;
                // TODO - the rest
                case _BMDSwitcherEventType.bmdSwitcherEventTypeMethodForDownConvertedSDChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeDownConvertedHDVideoModeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeMultiViewVideoModeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypePowerStatusChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeDisconnected:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventType3GSDIOutputLevelChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeTimeCodeChanged:
                    Props.GetTimeCode(out byte hours, out byte minutes, out byte seconds, out byte frames, out int dropFrame);
                    _state.Info.LastTimecode = new Timecode
                    {
                        Hour = hours,
                        Minute = minutes,
                        Second = seconds,
                        Frame = frames,
                        DropFrame = dropFrame != 0
                    };
                    OnChange("Info.LastTimecode");
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeTimeCodeLockedChanged:
                    Props.GetTimeCodeLocked(out int locked);
                    _state.Info.TimecodeLocked = locked != 0;
                    OnChange("Info.TimecodeLocked");
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeSuperSourceCascadeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeAutoVideoModeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeAutoVideoModeDetectedChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        public void Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
        {
            Notify(eventType);
        }

        private void SetupAudio()
        {
            if (Props is IBMDSwitcherAudioMixer mixer)
            {
                _state.Audio = new AudioState();
                Children.Add(new AudioMixerCallback(_state.Audio, mixer, AppendChange("Audio")));

                var talkback = Props as IBMDSwitcherTalkback;
                if (talkback != null)
                {
                    var cbt = new TalkbackCallback(_state.Audio.Talkback, talkback, AppendChange("Audio.Talkback"));
                    Children.Add(cbt);
                    cbt.NotifyAll(_state.Audio.Inputs.Keys);
                }

                // TODO others
            }
            else if (Props is IBMDSwitcherFairlightAudioMixer fairlightMixer)
            {
                _state.Fairlight = new FairlightAudioState();
                Children.Add(new FairlightAudioMixerCallback(_state.Fairlight, fairlightMixer, AppendChange("Fairlight")));
            }
        }

        private void SetupSerialPorts()
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSerialPortIterator>(Props.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherSerialPort>(iterator.Next, (port, id) =>
            {
                Assert.Equal((uint)0, id);

                Children.Add(new SerialPortPropertiesCallback(_state.Settings, port, AppendChange("Settings.SerialPort")));
            });
        }

        private void SetupHyperdecks()
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherHyperDeckIterator>(Props.CreateIterator);
            _state.Settings.Hyperdecks = AtemSDKConverter.IterateList<IBMDSwitcherHyperDeck, SettingsState.HyperdeckState>(iterator.Next, (deck, id) =>
            {
                var deckState = new SettingsState.HyperdeckState();
                Children.Add(new HyperDeckPropertiesCallback(deckState, deck, AppendChange($"Settings.Hyperdecks.{id:D}")));
                return deckState;
            });
        }

        private void SetupMultiViews()
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(Props.CreateIterator);
            _state.Settings.MultiViewers = AtemSDKConverter.IterateList<IBMDSwitcherMultiView, MultiViewerState>(iterator.Next,
                (mv, id) =>
                {
                    var mvState = new MultiViewerState();
                    Children.Add(new MultiViewPropertiesCallback(mvState, mv, AppendChange($"Settings.MultiViewers.{id:D}")));
                    return mvState;
                });
        }

        private void SetupMediaPlayers(AtemStateBuilderSettings updateSettings)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMediaPlayerIterator>(Props.CreateIterator);
            _state.MediaPlayers = AtemSDKConverter.IterateList<IBMDSwitcherMediaPlayer, MediaPlayerState>(iterator.Next,
                (media, id) =>
                {
                    var player = new MediaPlayerState();
                    if (_state.MediaPool.Clips.Count > 0)
                        player.ClipStatus = new MediaPlayerState.ClipStatusState();

                    Children.Add(new MediaPlayerCallback(player, updateSettings, media, AppendChange($"MediaPlayers.{id:D}")));
                    return player;
                });

        }

        private void SetupMediaPool()
        {
            var pool = Props as IBMDSwitcherMediaPool;

            // General
            // TODO

            // Stills
            pool.GetStills(out IBMDSwitcherStills stills);
            Children.Add(new MediaPoolStillsCallback(_state.MediaPool, stills, AppendChange("MediaPool.Stills")));

            // Clips
            pool.GetClipCount(out uint clipCount);
            _state.MediaPool.Clips = Enumerable.Repeat(0, (int)clipCount).Select(i => new MediaPoolState.ClipState()).ToList();
            for (uint i = 0; i < clipCount; i++)
            {
                pool.GetClip(i, out IBMDSwitcherClip clip);
                Children.Add(new MediaPoolClipCallback(_state.MediaPool.Clips[(int)i], clip, AppendChange($"MediaPool.Clips.{i:D}")));
            }
        }

        private void SetupMacros()
        {
            var pool = Props as IBMDSwitcherMacroPool;
            Children.Add(new MacroPoolCallback(_state.Macros, pool, AppendChange("Macros.Pool")));

            var ctrl = Props as IBMDSwitcherMacroControl;
            Children.Add(new MacroControlCallback(_state.Macros, ctrl, AppendChange("Macros")));
        }

        private void SetupDownstreamKeyers()
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherDownstreamKeyIterator>(Props.CreateIterator);
            _state.DownstreamKeyers = AtemSDKConverter.IterateList<IBMDSwitcherDownstreamKey, DownstreamKeyerState>(
                iterator.Next,
                (key, id) =>
                {
                    var dsk = new DownstreamKeyerState();
                    Children.Add(new DownstreamKeyerPropertiesCallback(dsk, key, AppendChange($"DownstreamKeyers.{id:D}")));
                    return dsk;
                });
        }

        private void SetupMixEffects()
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(Props.CreateIterator);
            _state.MixEffects = AtemSDKConverter.IterateList<IBMDSwitcherMixEffectBlock, MixEffectState>(iterator.Next, (me, id) =>
            {
                var meState = new MixEffectState();
                Children.Add(new MixEffectPropertiesCallback(meState, me, AppendChange($"MixEffects.{id:D}")));
                return meState;
            });
        }

        private void SetupInputs()
        {
            var auxes = new List<AuxState>();
            var cols = new List<ColorState>();
            var ssrcs = new List<SuperSourceState>();

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(Props.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherInput>(iterator.Next, (input, i) =>
            {
                input.GetInputId(out long id);
                var src = (VideoSource)id;

                var st = _state.Settings.Inputs[src] = new InputState();
                Children.Add(new InputCallback(st, input, AppendChange($"Settings.Inputs.{id:D}")));

                if (input is IBMDSwitcherInputAux aux)
                {
                    AuxiliaryId id2 = AtemEnumMaps.GetAuxId(src);
                    var st2 = new AuxState();
                    Children.Add(new AuxiliaryCallback(st2, aux, AppendChange($"Auxiliaries.{id2:D}")));
                    auxes.Add(st2);
                }

                if (input is IBMDSwitcherInputColor col)
                {
                    ColorGeneratorId id2 = AtemEnumMaps.GetSourceIdForGen(src);
                    var st2 = new ColorState();
                    Children.Add(new ColorCallback(st2, col, AppendChange($"ColorGenerators.{id2:D}")));
                    cols.Add(st2);
                }

                if (input is IBMDSwitcherInputSuperSource ssrc)
                {
                    // TODO - properly
                    SuperSourceId ssrcId = SuperSourceId.One;
                    var st2 = new SuperSourceState();
                    Children.Add(new SuperSourceCallback(st2, ssrc, AppendChange($"SuperSources.{ssrcId:D}")));
                    ssrcs.Add(st2);
                }
            });

            _state.Auxiliaries = auxes;
            _state.ColorGenerators = cols;
            _state.SuperSources = ssrcs;
        }
    }
}