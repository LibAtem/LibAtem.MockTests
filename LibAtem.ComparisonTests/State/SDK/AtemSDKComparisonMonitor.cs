using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AtemSDKComparisonMonitor : IDisposable
    {
        public AtemState State { get; }

        private readonly List<Action> _cleanupCallbacks = new List<Action>();

        public AtemSDKComparisonMonitor(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            State = new AtemState();

            switcher.GetProductName(out string productName);
            State.Info.ProductName = productName;


            SetupInputs(switcher);
            SetupMixEffects(switcher);
            SetupSerialPorts(switcher);
            SetupMultiViews(switcher);
            SetupDownstreamKeyers(switcher);
            SetupMediaPool(switcher);
            SetupMediaPlayers(switcher, updateSettings);
            SetupMacroPool(switcher);
            SetupAudio(switcher);
            SetupHyperdecks(switcher);

            switcher.AllowStreamingToResume();

            var cb = new SwitcherPropertiesCallback(State, switcher, FireCommandKey);
            SetupCallback<SwitcherPropertiesCallback, _BMDSwitcherEventType>(cb, switcher.AddCallback, switcher.RemoveCallback);
        }

        public void Dispose()
        {
            _cleanupCallbacks.ForEach(cb => cb());
        }

        private void FireCommandKey(string path)
        {
            OnStateChange?.Invoke(this, path);
        }

        private Action<string> GetFireCommandKey(string root)
        {
            return SdkCallbackUtil.AppendChange(FireCommandKey, root);
        }

        public delegate void StateChangeHandler(object sender, string path);
        public event StateChangeHandler OnStateChange;
        
        private void TriggerAllChanged<T>(INotify<T> cb, params T[] skip)
        {
            Enum.GetValues(typeof(T)).OfType<T>().Where(v => !skip.Contains(v)).ForEach(cb.Notify);
        }

        private void SetupCallbackBasic<T>(T cb, Action<T> add, Action<T> remove) 
        {
            add(cb);
            _cleanupCallbacks.Add(() => remove(cb));
        }

        private void SetupCallback<T, Te>(T cb, Action<T> add, Action<T> remove, bool triggerAllChanged = true, params Te[] skip) where T : INotify<Te>
        {
            add(cb);
            _cleanupCallbacks.Add(() => remove(cb));

            if (triggerAllChanged)
            {
                TriggerAllChanged(cb, skip);
            }
        }

        private void SetupDisposable(IDisposable obj)
        {
            _cleanupCallbacks.Add(obj.Dispose);
        }

        private void SetupAudio(IBMDSwitcher switcher)
        {
            if (switcher is IBMDSwitcherAudioMixer mixer)
            {
                State.Audio = new AudioState();

                var cb = new AudioMixerCallback(State.Audio.ProgramOut, mixer,
                    () => FireCommandKey("Audio.ProgramOut"));
                SetupCallback<AudioMixerCallback, _BMDSwitcherAudioMixerEventType>(cb, mixer.AddCallback, mixer.RemoveCallback);

                var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioInputIterator>(mixer.CreateIterator);

                for (iterator.Next(out IBMDSwitcherAudioInput port); port != null; iterator.Next(out port))
                {
                    port.GetAudioInputId(out long inputId);
                    State.Audio.Inputs[inputId] = new AudioState.InputState();

                    var cbi = new AudioMixerInputCallback(State.Audio.Inputs[inputId], port,
                        str => FireCommandKey($"Audio.Inputs.{inputId:D}.{str}"));
                    SetupCallback<AudioMixerInputCallback, _BMDSwitcherAudioInputEventType>(cbi, port.AddCallback, port.RemoveCallback);
                }

                var monIt = AtemSDKConverter.CastSdk<IBMDSwitcherAudioMonitorOutputIterator>(mixer.CreateIterator);

                var mons = new List<AudioState.MonitorOutputState>();
                State.Audio.Monitors = mons;
                uint id2 = 0;
                for (monIt.Next(out IBMDSwitcherAudioMonitorOutput r); r != null; monIt.Next(out r))
                {
                    var mon = new AudioState.MonitorOutputState();
                    mons.Add(mon);
                    uint monId = id2++;

                    var cbi = new AudioMixerMonitorOutputCallback(mon, r,
                        () => FireCommandKey($"Audio.Monitors.{monId:D}"));
                    SetupCallback<AudioMixerMonitorOutputCallback, _BMDSwitcherAudioMonitorOutputEventType>(cbi, r.AddCallback, r.RemoveCallback);
                }

                var talkback = switcher as IBMDSwitcherTalkback;
                if (talkback != null)
                {
                    var cbt = new TalkbackCallback(State.Audio.Talkback, talkback,
                        () => FireCommandKey("Audio.Talkback"));
                    SetupCallbackBasic(cbt, talkback.AddCallback, talkback.RemoveCallback);
                    cbt.NotifyAll(State.Audio.Inputs.Keys);
                }

                // TODO others
            } else if (switcher is IBMDSwitcherFairlightAudioMixer fairlightMixer)
            {
                State.Fairlight = new FairlightAudioState();

                var cb = new FairlightAudioMixerCallback(State.Fairlight.ProgramOut, fairlightMixer, () => FireCommandKey("Fairlight.ProgramOut"));
                SetupCallback<FairlightAudioMixerCallback, _BMDSwitcherFairlightAudioMixerEventType>(cb, fairlightMixer.AddCallback, fairlightMixer.RemoveCallback);

                // Dynamics
                var pgmDynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(fairlightMixer.GetMasterOutEffect);
                SetupFairlightDynamics(State.Fairlight.ProgramOut.Dynamics, pgmDynamics, "Fairlight.ProgramOut.Dynamics", false);

                // Equalizer
                var pgmEqualizer = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(fairlightMixer.GetMasterOutEffect);
                SetupFairlightEqualizer(State.Fairlight.ProgramOut.Equalizer, pgmEqualizer, "Fairlight.ProgramOut.Equalizer");

                // Inputs
                var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioInputIterator>(fairlightMixer.CreateIterator);
                for (iterator.Next(out IBMDSwitcherFairlightAudioInput input); input != null; iterator.Next(out input))
                {
                    input.GetId(out long id);
                    input.GetType(out _BMDSwitcherFairlightAudioInputType type);
                    input.GetSupportedConfigurations(out _BMDSwitcherFairlightAudioInputConfiguration configs);

                    var inputPath = $"Fairlight.Input.{id}";
                    var inputState = State.Fairlight.Inputs[id] = new FairlightAudioState.InputState
                    {
                        InputType = AtemEnumMaps.FairlightAudioInputType.FindByValue(type),
                        SupportedConfigurations = (FairlightInputConfiguration)configs
                    };

                    var cb2 = new FairlightAudioInputCallback(inputState, input, str => FireCommandKey($"{inputPath}.{str}"));
                    SetupCallback<FairlightAudioInputCallback, _BMDSwitcherFairlightAudioInputEventType>(cb2, input.AddCallback, input.RemoveCallback);

                    // Sources
                    var sourceIterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioSourceIterator>(input.CreateIterator);
                    int srcId = 0;
                    for (sourceIterator.Next(out IBMDSwitcherFairlightAudioSource src); src != null; sourceIterator.Next(out src))
                    {
                        var srcState = new FairlightAudioState.InputSourceState();
                        inputState.Sources.Add(srcState);

                        var srcId2 = srcId;
                        var cb3 = new FairlightAudioInputSourceCallback(srcState, src, () => FireCommandKey($"{inputPath}.Sources.{srcId2}"));
                        SetupCallback<FairlightAudioInputSourceCallback, _BMDSwitcherFairlightAudioSourceEventType>(cb3, src.AddCallback, src.RemoveCallback);
                        
                        // TODO - Effects?

                        srcId++;
                    }
                }
            }
        }

        private void SetupFairlightDynamics(FairlightAudioState.DynamicsState state, IBMDSwitcherFairlightAudioDynamicsProcessor proc, string path, bool hasExpander)
        {
            var dynCb = new FairlightDynamicsAudioMixerCallback(state, proc, () => FireCommandKey(path));
            SetupCallback<FairlightDynamicsAudioMixerCallback, _BMDSwitcherFairlightAudioDynamicsProcessorEventType>(dynCb, proc.AddCallback, proc.RemoveCallback);

            // Limiter
            var limiterProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(proc.GetProcessor);

            state.Limiter = new FairlightAudioState.LimiterState();
            var limiterCb = new FairlightLimiterDynamicsAudioMixerCallback(state.Limiter, limiterProps, str => FireCommandKey($"{path}.Limiter.{str}"));
            SetupCallback<FairlightLimiterDynamicsAudioMixerCallback, _BMDSwitcherFairlightAudioLimiterEventType>(limiterCb, limiterProps.AddCallback, limiterProps.RemoveCallback);

            // Compressor
            var compressorProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(proc.GetProcessor);
            
            state.Compressor = new FairlightAudioState.CompressorState();
            var compressorCb = new FairlightCompressorDynamicsAudioMixerCallback(state.Compressor, compressorProps, str => FireCommandKey($"{path}.Compressor.{str}"));
            SetupCallback<FairlightCompressorDynamicsAudioMixerCallback, _BMDSwitcherFairlightAudioCompressorEventType>(compressorCb, compressorProps.AddCallback, compressorProps.RemoveCallback);

            if (hasExpander)
            {
                // Expander
                var expanderProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(proc.GetProcessor);

            }
        }

        private void SetupFairlightEqualizer(FairlightAudioState.EqualizerState state, IBMDSwitcherFairlightAudioEqualizer eq, string path)
        {
            var eqCb = new FairlightEqualizerAudioMixerCallback(state, eq, () => FireCommandKey(path));
            SetupCallback<FairlightEqualizerAudioMixerCallback, _BMDSwitcherFairlightAudioEqualizerEventType>(eqCb, eq.AddCallback, eq.RemoveCallback);

            /*
            // Bands
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizerBandIterator>(eq.CreateIterator);

            var bands = new List<FairlightAudioState.EqualizerBandState>();

            int id = 0;
            for (iterator.Next(out IBMDSwitcherFairlightAudioEqualizerBand band); band != null; iterator.Next(out band))
            {
                var bandState = new FairlightAudioState.EqualizerBandState();

                var id2 = id;
                var cb = new FairlightEqualizerBandAudioMixerCallback(bandState, band, () => FireCommandKey($"{path}.Bands.{id2:D}"));
                SetupCallback(cb, band.AddCallback, band.RemoveCallback);

                id++;
            }

            state.Bands = bands;
            */
        }

        private void SetupSerialPorts(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSerialPortIterator>(switcher.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherSerialPort>(iterator.Next, (port, id) =>
            {
                Assert.Equal((uint) 0, id);

                SetupDisposable(new SerialPortPropertiesCallback(State.Settings, port, GetFireCommandKey("Settings.SerialPort")));
            });
        }

        private void SetupHyperdecks(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherHyperDeckIterator>(switcher.CreateIterator);
            State.Settings.Hyperdecks = AtemSDKConverter.IterateList<IBMDSwitcherHyperDeck, SettingsState.HyperdeckState>(iterator.Next, (deck, id) =>
            {
                var deckState = new SettingsState.HyperdeckState();
                SetupDisposable(new HyperDeckPropertiesCallback(deckState, deck, GetFireCommandKey($"Settings.Hyperdecks.{id:D}")));
                return deckState;
            });
        }

        private void SetupMultiViews(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(switcher.CreateIterator);
            State.Settings.MultiViewers = AtemSDKConverter.IterateList<IBMDSwitcherMultiView, MultiViewerState>(iterator.Next,
                (mv, id) =>
                {
                    var mvState = new MultiViewerState();
                    SetupDisposable(new MultiViewPropertiesCallback(mvState, mv, GetFireCommandKey($"Settings.MultiViewers.{id:D}")));
                    return mvState;
                });
        }

        private void SetupMediaPlayers(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMediaPlayerIterator>(switcher.CreateIterator);
            State.MediaPlayers = AtemSDKConverter.IterateList<IBMDSwitcherMediaPlayer, MediaPlayerState>(iterator.Next,
                (media, id) =>
                {
                    var player = new MediaPlayerState();
                    if (State.MediaPool.Clips.Count > 0)
                        player.ClipStatus = new MediaPlayerState.ClipStatusState();

                    SetupDisposable(new MediaPlayerCallback(player, updateSettings, media, GetFireCommandKey($"MediaPlayers.{id:D}")));
                    return player;
                });

        }
        
        private void SetupMediaPool(IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMediaPool;

            // General
            // TODO

            // Stills
            pool.GetStills(out IBMDSwitcherStills stills);

            var skipStills = new[]
            {
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioValidChanged,
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioNameChanged,
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioHashChanged
            };
            var cbs = new MediaPoolStillsCallback(State.MediaPool, stills, str => FireCommandKey($"MediaPool.Stills.{str}"));
            SetupCallback(cbs, stills.AddCallback, stills.RemoveCallback, true, skipStills);

            // Clips
            pool.GetClipCount(out uint clipCount);
            State.MediaPool.Clips = Enumerable.Repeat(0, (int)clipCount).Select(i => new MediaPoolState.ClipState()).ToList();
            for (uint i = 0; i < clipCount; i++)
            {
                pool.GetClip(i, out IBMDSwitcherClip clip);
                uint clipId = i;

                var cbc = new MediaPoolClipCallback(State.MediaPool.Clips[(int)i], clip, () => FireCommandKey($"MediaPool.Clips.{clipId:D}"));
                SetupCallback<MediaPoolClipCallback, _BMDSwitcherMediaPoolEventType>(cbc, clip.AddCallback, clip.RemoveCallback);
            }
        }

        private void SetupMacroPool(IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMacroPool;
            
            var cbs = new MacroPoolCallback(State.Macros, pool, str => FireCommandKey($"Macros.Pool.{str}"));
            SetupCallbackBasic(cbs, pool.AddCallback, pool.RemoveCallback);

            pool.GetMaxCount(out uint count);
            State.Macros.Pool = Enumerable.Repeat(0, (int)count).Select(i => new MacroState.ItemState()).ToList();
            for (uint i = 0; i < count; i++)
            {
                Enum.GetValues(typeof(_BMDSwitcherMacroPoolEventType)).OfType<_BMDSwitcherMacroPoolEventType>().ForEach(e => cbs.Notify(e, i, null));
            }

            var ctrl = switcher as IBMDSwitcherMacroControl;

            var cbs2 = new MacroControlCallback(State.Macros, ctrl, str => FireCommandKey($"Macros.{str}"));
            SetupCallback<MacroControlCallback, _BMDSwitcherMacroControlEventType>(cbs2, ctrl.AddCallback, ctrl.RemoveCallback);
        }

        private void SetupDownstreamKeyers(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherDownstreamKeyIterator>(switcher.CreateIterator);
            State.DownstreamKeyers = AtemSDKConverter.IterateList<IBMDSwitcherDownstreamKey, DownstreamKeyerState>(
                iterator.Next,
                (key, id) =>
                {
                    var dsk = new DownstreamKeyerState();
                    SetupDisposable(new DownstreamKeyerPropertiesCallback(dsk, key, GetFireCommandKey($"DownstreamKeyers.{id:D}")));
                    return dsk;
                });
        }

        private void SetupMixEffects(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(switcher.CreateIterator);
            State.MixEffects = AtemSDKConverter.IterateList<IBMDSwitcherMixEffectBlock, MixEffectState>(iterator.Next, (me, id) =>
            {
                var meState = new MixEffectState();
                SetupDisposable(new MixEffectPropertiesCallback(meState, me, GetFireCommandKey($"MixEffects.{id:D}")));
                return meState;
            });
        }

        private void SetupInputs(IBMDSwitcher switcher)
        {
            var auxes = new List<AuxState>();
            var cols = new List<ColorState>();
            var ssrcs = new List<SuperSourceState>();

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(switcher.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherInput>(iterator.Next, (input, i) =>
            {
                input.GetInputId(out long id);
                var src = (VideoSource)id;

                var st = State.Settings.Inputs[src] = new InputState();
                var cb = new InputCallback(st, input, str => FireCommandKey($"Settings.Inputs.{id:D}.{str}"));
                SetupCallback<InputCallback, _BMDSwitcherInputEventType>(cb, input.AddCallback, input.RemoveCallback);

                if (input is IBMDSwitcherInputAux aux)
                {
                    AuxiliaryId id2 = AtemEnumMaps.GetAuxId(src);
                    var st2 = new AuxState();
                    SetupDisposable(new AuxiliaryCallback(st2, aux, GetFireCommandKey($"Auxiliaries.{id2:D}")));
                    auxes.Add(st2);
                }

                if (input is IBMDSwitcherInputColor col)
                {
                    ColorGeneratorId id2 = AtemEnumMaps.GetSourceIdForGen(src);
                    var st2 = new ColorState();
                    SetupDisposable(new ColorCallback(st2, col, GetFireCommandKey($"ColorGenerators.{id2:D}")));
                    cols.Add(st2);
                }

                if (input is IBMDSwitcherInputSuperSource ssrc)
                {
                    // TODO - properly
                    SuperSourceId ssrcId = SuperSourceId.One;
                    var st2 = new SuperSourceState();
                    SetupDisposable(new SuperSourceCallback(st2, ssrc, GetFireCommandKey($"SuperSources.{ssrcId:D}")));
                    ssrcs.Add(st2);
                }
            });

            State.Auxiliaries = auxes;
            State.ColorGenerators = cols;
            State.SuperSources = ssrcs;
        }

    }
}