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
    public interface INotify<in T>
    {
        void Notify(T eventType);
    }

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

            int id = 0;
            for (iterator.Next(out IBMDSwitcherSerialPort port); port != null; iterator.Next(out port))
            {
                Assert.Equal(0, id);
                
                var cb = new SerialPortPropertiesCallback(State.Settings, port, () => FireCommandKey("Settings.SerialPort"));
                SetupCallback<SerialPortPropertiesCallback, _BMDSwitcherSerialPortEventType>(cb, port.AddCallback, port.RemoveCallback);

                id++;
            }
        }

        private void SetupHyperdecks(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherHyperDeckIterator>(switcher.CreateIterator);

            var hyperdecks = new List<SettingsState.HyperdeckState>();
            uint id = 0;
            for (iterator.Next(out IBMDSwitcherHyperDeck deck); deck != null; iterator.Next(out deck))
            {
                var deck2 = deck;
                var deckState = new SettingsState.HyperdeckState();
                hyperdecks.Add(deckState);
                uint deckId = id++;

                var cb = new HyperDeckPropertiesCallback(deckState, deck2, str => FireCommandKey($"Settings.Hyperdecks.{deckId:D}.{str}"));
                SetupCallback<HyperDeckPropertiesCallback, _BMDSwitcherHyperDeckEventType>(cb, deck.AddCallback, deck.RemoveCallback);
            }
            State.Settings.Hyperdecks = hyperdecks;
        }

        private void SetupMultiViews(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(switcher.CreateIterator);

            var mvs = new List<MultiViewerState>();
            uint id = 0;
            for (iterator.Next(out IBMDSwitcherMultiView mv); mv != null; iterator.Next(out mv))
            {
                var mvState = new MultiViewerState();
                mvs.Add(mvState);
                uint mvId = id++;

                mv.GetWindowCount(out uint count);
                mvState.Windows = Enumerable.Repeat(0, (int)count).Select(i => new MultiViewerState.WindowState()).ToList();

                mv.SupportsProgramPreviewSwap(out int canSwap);
                mvState.SupportsProgramPreviewSwapped = canSwap != 0;

                var cb = new MultiViewPropertiesCallback(mvState, mv, str => FireCommandKey($"Settings.MultiViewers.{mvId:D}.{str}"));
                SetupCallback<MultiViewPropertiesCallback, _BMDSwitcherMultiViewEventType>(cb, mv.AddCallback, mv.RemoveCallback);
            }
            State.Settings.MultiViewers = mvs;
        }

        private void SetupMediaPlayers(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMediaPlayerIterator>(switcher.CreateIterator);

            var players = new List<MediaPlayerState>();
            MediaPlayerId id = 0;
            for (iterator.Next(out IBMDSwitcherMediaPlayer media); media != null; iterator.Next(out media))
            {
                var player = new MediaPlayerState();
                players.Add(player);
                MediaPlayerId playerId = id++;

                if (State.MediaPool.Clips.Count > 0)
                    player.ClipStatus = new MediaPlayerState.ClipStatusState();

                var cb = new MediaPlayerCallback(player, updateSettings, media,
                    str => FireCommandKey($"MediaPlayers.{playerId:D}.{str}"));
                SetupCallbackBasic(cb, media.AddCallback, media.RemoveCallback);
                cb.Notify();
            }
            State.MediaPlayers = players;
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

            var dsks = new List<DownstreamKeyerState>();
            DownstreamKeyId id = 0;
            for (iterator.Next(out IBMDSwitcherDownstreamKey key); key != null; iterator.Next(out key))
            {
                var dsk = new DownstreamKeyerState();
                dsks.Add(dsk);
                DownstreamKeyId dskId = id++;

                var cb = new DownstreamKeyerPropertiesCallback(dsk, key, str => FireCommandKey($"DownstreamKeyers.{dskId:D}.{str}"));
                SetupCallback<DownstreamKeyerPropertiesCallback, _BMDSwitcherDownstreamKeyEventType>(cb, key.AddCallback, key.RemoveCallback);
            }
            State.DownstreamKeyers = dsks;
        }

        private void SetupMixEffects(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(switcher.CreateIterator);

            var mes = new List<MixEffectState>();
            State.MixEffects = mes;

            var id = MixEffectBlockId.One;
            for (iterator.Next(out IBMDSwitcherMixEffectBlock me); me != null; iterator.Next(out me))
            {
                var meState = new MixEffectState();
                mes.Add(meState);
                var meId = id++;

                var cb = new MixEffectPropertiesCallback(meState, me, str => FireCommandKey($"MixEffects.{meId:D}.{str}"));
                SetupCallback<MixEffectPropertiesCallback, _BMDSwitcherMixEffectBlockEventType>(cb, me.AddCallback, me.RemoveCallback);

                SetupMixEffectKeyer(me, meId);

                SetupMixEffectTransition(me, meId);
            }
        }

        private void SetupMixEffectTransition(IBMDSwitcherMixEffectBlock me, MixEffectBlockId id)
        {
            MixEffectState.TransitionState st = State.MixEffects[(int)id].Transition;

            if (me is IBMDSwitcherTransitionParameters trans)
            {
                var cb = new MixEffectTransitionPropertiesCallback(st, trans, str => FireCommandKey($"MixEffects.{id:D}.Transition.{str}"));
                SetupCallback<MixEffectTransitionPropertiesCallback, _BMDSwitcherTransitionParametersEventType>(cb, trans.AddCallback, trans.RemoveCallback);
            }

            if (me is IBMDSwitcherTransitionMixParameters mix)
            {
                st.Mix = new MixEffectState.TransitionMixState();
                var cb = new MixEffectTransitionMixCallback(st.Mix, mix, () => FireCommandKey($"MixEffects.{id:D}.Transition.Mix"));
                SetupCallback<MixEffectTransitionMixCallback, _BMDSwitcherTransitionMixParametersEventType>(cb, mix.AddCallback, mix.RemoveCallback);
            }

            if (me is IBMDSwitcherTransitionDipParameters dip)
            {
                st.Dip = new MixEffectState.TransitionDipState();
                var cb = new MixEffectTransitionDipCallback(st.Dip, dip, () => FireCommandKey($"MixEffects.{id:D}.Transition.Dip"));
                SetupCallback<MixEffectTransitionDipCallback, _BMDSwitcherTransitionDipParametersEventType>(cb, dip.AddCallback, dip.RemoveCallback);
            }

            if (me is IBMDSwitcherTransitionWipeParameters wipe)
            {
                st.Wipe = new MixEffectState.TransitionWipeState();
                var cb = new MixEffectTransitionWipeCallback(st.Wipe, wipe, () => FireCommandKey($"MixEffects.{id:D}.Transition.Wipe"));
                SetupCallback<MixEffectTransitionWipeCallback, _BMDSwitcherTransitionWipeParametersEventType>(cb, wipe.AddCallback, wipe.RemoveCallback);
            }

            if (me is IBMDSwitcherTransitionStingerParameters stinger)
            {
                st.Stinger = new MixEffectState.TransitionStingerState();
                var cb = new MixEffectTransitionStingerCallback(st.Stinger, stinger, () => FireCommandKey($"MixEffects.{id:D}.Transition.Stinger"));
                SetupCallback<MixEffectTransitionStingerCallback, _BMDSwitcherTransitionStingerParametersEventType>(cb, stinger.AddCallback, stinger.RemoveCallback);
            }

            if (me is IBMDSwitcherTransitionDVEParameters dve)
            {
                st.DVE = new MixEffectState.TransitionDVEState();
                var cb = new MixEffectTransitionDVECallback(st.DVE, dve, () => FireCommandKey($"MixEffects.{id:D}.Transition.DVE"));
                SetupCallback<MixEffectTransitionDVECallback, _BMDSwitcherTransitionDVEParametersEventType>(cb, dve.AddCallback, dve.RemoveCallback);
            }
        }

        private void SetupMixEffectKeyer(IBMDSwitcherMixEffectBlock me, MixEffectBlockId meId)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherKeyIterator>(me.CreateIterator);

            var keyId = UpstreamKeyId.One;
            var keyers = new List<MixEffectState.KeyerState>();
            for (iterator.Next(out IBMDSwitcherKey keyer); keyer != null; iterator.Next(out keyer))
            {
                var keyerState = new MixEffectState.KeyerState();
                keyers.Add(keyerState);

                var cb2 = new MixEffectKeyerCallback(keyerState, keyer, str => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.{str}"));
                SetupCallback(cb2, keyer.AddCallback, keyer.RemoveCallback, true, _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeCanBeDVEKeyChanged);

                if (keyer is IBMDSwitcherKeyLumaParameters luma)
                {
                    keyerState.Luma = new MixEffectState.KeyerLumaState();
                    var cb = new MixEffectKeyerLumaCallback(keyerState.Luma, luma, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.Luma"));
                    SetupCallback<MixEffectKeyerLumaCallback, _BMDSwitcherKeyLumaParametersEventType>(cb, luma.AddCallback, luma.RemoveCallback);
                }

                if (keyer is IBMDSwitcherKeyChromaParameters chroma)
                {
                    keyerState.Chroma = new MixEffectState.KeyerChromaState();
                    var cb = new MixEffectKeyerChromaCallback(keyerState.Chroma, chroma, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.Chroma"));
                    SetupCallback<MixEffectKeyerChromaCallback, _BMDSwitcherKeyChromaParametersEventType>(cb, chroma.AddCallback, chroma.RemoveCallback);
                }

                if (keyer is IBMDSwitcherKeyAdvancedChromaParameters advancedChroma)
                {
                    keyerState.AdvancedChroma = new MixEffectState.KeyerAdvancedChromaState();
                    var cb = new MixEffectKeyerAdvancedChromaCallback(keyerState.AdvancedChroma, advancedChroma, str => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.AdvancedChroma.{str}"));
                    SetupCallback<MixEffectKeyerAdvancedChromaCallback, _BMDSwitcherKeyAdvancedChromaParametersEventType>(cb, advancedChroma.AddCallback, advancedChroma.RemoveCallback);
                }

                if (keyer is IBMDSwitcherKeyPatternParameters pattern)
                {
                    keyerState.Pattern = new MixEffectState.KeyerPatternState();
                    var cb = new MixEffectKeyerPatternCallback(keyerState.Pattern, pattern, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.Pattern"));
                    SetupCallback<MixEffectKeyerPatternCallback, _BMDSwitcherKeyPatternParametersEventType>(cb, pattern.AddCallback, pattern.RemoveCallback);
                }

                if (keyer is IBMDSwitcherKeyDVEParameters dve)
                {
                    keyerState.DVE = new MixEffectState.KeyerDVEState();
                    var cb = new MixEffectKeyerDVECallback(keyerState.DVE, dve, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.DVE"));
                    SetupCallback<MixEffectKeyerDVECallback, _BMDSwitcherKeyDVEParametersEventType>(cb, dve.AddCallback, dve.RemoveCallback);
                }

                if (keyer is IBMDSwitcherKeyFlyParameters fly)
                    SetupMixEffectFlyKeyer(fly, keyerState, meId, keyId);
                
                keyId++;
            }
            State.MixEffects[(int)meId].Keyers = keyers;
        }

        private void SetupMixEffectFlyKeyer(IBMDSwitcherKeyFlyParameters props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            var ignore = new[]
            {
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged
            };
            var cb = new MixEffectKeyerFlyCallback(state.DVE, props, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.DVE"));
            SetupCallback(cb, props.AddCallback, props.RemoveCallback, true, ignore);

            props.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeA);
            props.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeB);

            state.FlyFrames = new[]
            {
                new MixEffectState.KeyerFlyFrameState(),
                new MixEffectState.KeyerFlyFrameState()
            };

            var cb2 = new MixEffectKeyerFlyKeyFrameCallback(state.FlyFrames[0], keyframeA, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.FlyFrames.0"));
            SetupCallback<MixEffectKeyerFlyKeyFrameCallback, _BMDSwitcherKeyFlyKeyFrameParametersEventType>(cb2, keyframeA.AddCallback, keyframeA.RemoveCallback);

            var cb3 = new MixEffectKeyerFlyKeyFrameCallback(state.FlyFrames[1], keyframeB, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.FlyFrames.1"));
            SetupCallback<MixEffectKeyerFlyKeyFrameCallback, _BMDSwitcherKeyFlyKeyFrameParametersEventType>(cb3, keyframeB.AddCallback, keyframeB.RemoveCallback);
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