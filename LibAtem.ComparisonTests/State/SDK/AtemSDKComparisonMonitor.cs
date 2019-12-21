using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

    public sealed class AtemSDKComparisonMonitor
    {
        public AtemState State { get; }

        private readonly List<Action> _cleanupCallbacks = new List<Action>();

        public AtemSDKComparisonMonitor(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            State = new AtemState();

            switcher.GetProductName(out string productName);
            State.Info.ProductName = productName;
            switcher.GetTimeCodeLocked(out int timecodeLocked);
            State.Info.TimecodeLocked = timecodeLocked != 0;


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
            switcher.AddCallback(cb);
            _cleanupCallbacks.Add(() => switcher.RemoveCallback(cb));
            TriggerAllChanged(cb);

        }

        // TODO - this should probably be replaced being being disposable
        ~AtemSDKComparisonMonitor()
        {
            _cleanupCallbacks.ForEach(cb => cb());
        }

        private void FireCommandKey(string path)
        {
            OnStateChange?.Invoke(this, path);
        }

        public delegate void StateChangeHandler(object sender, string path);
        public event StateChangeHandler OnStateChange;
        
        private void TriggerAllChanged<T>(INotify<T> cb, params T[] skip)
        {
            Enum.GetValues(typeof(T)).OfType<T>().Where(v => !skip.Contains(v)).ForEach(cb.Notify);
        }

        private void SetupAudio(IBMDSwitcher switcher)
        {
            if (switcher is IBMDSwitcherAudioMixer mixer)
            {
                State.Audio = new AudioState();

                var cb = new AudioMixerCallback(State.Audio.ProgramOut, mixer,
                    () => FireCommandKey("Audio.ProgramOut"));
                mixer.AddCallback(cb);
                _cleanupCallbacks.Add(() => mixer.RemoveCallback(cb));
                TriggerAllChanged(cb);

                var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioInputIterator>(mixer.CreateIterator);

                for (iterator.Next(out IBMDSwitcherAudioInput port); port != null; iterator.Next(out port))
                {
                    port.GetAudioInputId(out long inputId);
                    State.Audio.Inputs[inputId] = new AudioState.InputState();

                    var cbi = new AudioMixerInputCallback(State.Audio.Inputs[inputId], port,
                        str => FireCommandKey($"Audio.Inputs.{inputId:D}.{str}"));
                    port.AddCallback(cbi);
                    _cleanupCallbacks.Add(() => port.RemoveCallback(cbi));
                    TriggerAllChanged(cbi);
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
                    r.AddCallback(cbi);
                    _cleanupCallbacks.Add(() => r.RemoveCallback(cbi));
                    TriggerAllChanged(cbi);
                }

                var talkback = switcher as IBMDSwitcherTalkback;
                if (talkback != null)
                {
                    var cbt = new TalkbackCallback(State.Audio.Talkback, talkback,
                        () => FireCommandKey("Audio.Talkback"));
                    talkback.AddCallback(cbt);
                    _cleanupCallbacks.Add(() => talkback.RemoveCallback(cbt));
                    cbt.NotifyAll(State.Audio.Inputs.Keys);
                }

                // TODO others
            } else if (switcher is IBMDSwitcherFairlightAudioMixer fairlightMixer)
            {
                State.Fairlight = new FairlightAudioState();

                var cb = new FairlightAudioMixerCallback(State.Fairlight.ProgramOut, fairlightMixer,
                    () => FireCommandKey("Fairlight.ProgramOut"));
                fairlightMixer.AddCallback(cb);
                _cleanupCallbacks.Add(() => fairlightMixer.RemoveCallback(cb));
                TriggerAllChanged(cb);

                // Dynamics
                var pgmDynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(fairlightMixer.GetMasterOutEffect);
                SetupFairlightDynamics(State.Fairlight.ProgramOut.Dynamics, pgmDynamics, "Fairlight.ProgramOut.Dynamics", false);

            }
        }

        private void SetupFairlightDynamics(FairlightAudioState.DynamicsState state, IBMDSwitcherFairlightAudioDynamicsProcessor proc, string path, bool hasExpander)
        {
            var dynCb = new FairlightDynamicsAudioMixerCallback(state, proc, str => FireCommandKey($"{path}.{str}"));
            proc.AddCallback(dynCb);
            _cleanupCallbacks.Add(() => proc.RemoveCallback(dynCb));
            TriggerAllChanged(dynCb);

            // Limiter
            var limiterProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(proc.GetProcessor);

            state.Limiter = new FairlightAudioState.LimiterState();
            var limiterCb = new FairlightLimiterDynamicsAudioMixerCallback(state.Limiter, limiterProps, str => FireCommandKey($"{path}.Limiter.{str}"));
            limiterProps.AddCallback(limiterCb);
            _cleanupCallbacks.Add(() => limiterProps.RemoveCallback(limiterCb));
            TriggerAllChanged(limiterCb);

            // Compressor
            var compressorProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(proc.GetProcessor);
            
            state.Compressor = new FairlightAudioState.CompressorState();
            var compressorCb = new FairlightCompressorDynamicsAudioMixerCallback(state.Compressor, compressorProps, str => FireCommandKey($"{path}.Compressor.{str}"));
            compressorProps.AddCallback(compressorCb);
            _cleanupCallbacks.Add(() => compressorProps.RemoveCallback(compressorCb));
            TriggerAllChanged(compressorCb);

            if (hasExpander)
            {
                // Expander
                var expanderProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(proc.GetProcessor);

            }
        }

        private void SetupSerialPorts(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSerialPortIterator>(switcher.CreateIterator);

            int id = 0;
            for (iterator.Next(out IBMDSwitcherSerialPort port); port != null; iterator.Next(out port))
            {
                Assert.Equal(0, id);
                
                var cb = new SerialPortPropertiesCallback(State.Settings, port, () => FireCommandKey("Settings.SerialPort"));
                port.AddCallback(cb);
                _cleanupCallbacks.Add(() => port.RemoveCallback(cb));
                TriggerAllChanged(cb);

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
                deck2.AddCallback(cb);
                _cleanupCallbacks.Add(() => deck2.RemoveCallback(cb));
                TriggerAllChanged(cb);
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
                mv.AddCallback(cb);
                _cleanupCallbacks.Add(() => mv.RemoveCallback(cb));
                TriggerAllChanged(cb);
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
                media.AddCallback(cb);
                _cleanupCallbacks.Add(() => media.RemoveCallback(cb));
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

            var cbs = new MediaPoolStillsCallback(State.MediaPool, stills, str => FireCommandKey($"MediaPool.Stills.{str}"));
            stills.AddCallback(cbs);
            _cleanupCallbacks.Add(() => stills.RemoveCallback(cbs));

            cbs.Init();
            var skipStills = new[]
            {
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioValidChanged,
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioNameChanged,
                _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioHashChanged
            };
            TriggerAllChanged(cbs, skipStills);

            // Clips
            pool.GetClipCount(out uint clipCount);
            State.MediaPool.Clips = Enumerable.Repeat(0, (int)clipCount).Select(i => new MediaPoolState.ClipState()).ToList();
            for (uint i = 0; i < clipCount; i++)
            {
                pool.GetClip(i, out IBMDSwitcherClip clip);
                uint clipId = i;

                var cbc = new MediaPoolClipCallback(State.MediaPool.Clips[(int)i], clip, () => FireCommandKey($"MediaPool.Clips.{clipId:D}"));
                clip.AddCallback(cbc);
                _cleanupCallbacks.Add(() => clip.RemoveCallback(cbc));

                cbc.Init();
                TriggerAllChanged(cbc);
            }
        }

        private void SetupMacroPool(IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMacroPool;
            
            var cbs = new MacroPoolCallback(State.Macros, pool, str => FireCommandKey($"Macros.Pool.{str}"));
            pool.AddCallback(cbs);
            _cleanupCallbacks.Add(() => pool.RemoveCallback(cbs));

            pool.GetMaxCount(out uint count);
            State.Macros.Pool = Enumerable.Repeat(0, (int)count).Select(i => new MacroState.ItemState()).ToList();
            for (uint i = 0; i < count; i++)
            {
                Enum.GetValues(typeof(_BMDSwitcherMacroPoolEventType)).OfType<_BMDSwitcherMacroPoolEventType>().ForEach(e => cbs.Notify(e, i, null));
            }

            var ctrl = switcher as IBMDSwitcherMacroControl;

            var cbs2 = new MacroControlCallback(State.Macros, ctrl, str => FireCommandKey($"Macros.{str}"));
            ctrl.AddCallback(cbs2);
            _cleanupCallbacks.Add(() => ctrl.RemoveCallback(cbs2));

            TriggerAllChanged(cbs2);
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
                key.AddCallback(cb);
                _cleanupCallbacks.Add(() => key.RemoveCallback(cb));
                TriggerAllChanged(cb);
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
                me.AddCallback(cb);
                _cleanupCallbacks.Add(() => me.RemoveCallback(cb));
                TriggerAllChanged(cb);

                SetupMixEffectKeyer(me, meId);

                SetupMixEffectTransition(me, meId);
            }
        }

        private void SetupMixEffectTransition(IBMDSwitcherMixEffectBlock me, MixEffectBlockId id)
        {
            if (me is IBMDSwitcherTransitionParameters trans)
                SetupMixEffectTransitionProperties(trans, id);
            if (me is IBMDSwitcherTransitionMixParameters mix)
                SetupMixEffectTransitionMix(mix, id);
            if (me is IBMDSwitcherTransitionDipParameters dip)
                SetupMixEffectTransitionDip(dip, id);
            if (me is IBMDSwitcherTransitionWipeParameters wipe)
                SetupMixEffectTransitionWipe(wipe, id);
            if (me is IBMDSwitcherTransitionStingerParameters stinger)
                SetupMixEffectTransitionStinger(stinger, id);
            if (me is IBMDSwitcherTransitionDVEParameters dve)
                SetupMixEffectTransitionDVE(dve, id);
        }

        private void SetupMixEffectTransitionProperties(IBMDSwitcherTransitionParameters trans, MixEffectBlockId id)
        {
            MixEffectState.TransitionState st = State.MixEffects[(int)id].Transition;

            var cb = new MixEffectTransitionPropertiesCallback(st, trans, str => FireCommandKey($"MixEffects.{id:D}.Transition.{str}"));
            trans.AddCallback(cb);
            _cleanupCallbacks.Add(() => trans.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionMix(IBMDSwitcherTransitionMixParameters dip, MixEffectBlockId id)
        {
            MixEffectState.TransitionMixState st = State.MixEffects[(int)id].Transition.Mix = new MixEffectState.TransitionMixState();

            var cb = new MixEffectTransitionMixCallback(st, dip, () => FireCommandKey($"MixEffects.{id:D}.Transition.Mix"));
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionDip(IBMDSwitcherTransitionDipParameters dip, MixEffectBlockId id)
        {
            MixEffectState.TransitionDipState st = State.MixEffects[(int)id].Transition.Dip = new MixEffectState.TransitionDipState();

            var cb = new MixEffectTransitionDipCallback(st, dip, () => FireCommandKey($"MixEffects.{id:D}.Transition.Dip"));
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionWipe(IBMDSwitcherTransitionWipeParameters wipe, MixEffectBlockId id)
        {
            MixEffectState.TransitionWipeState st = State.MixEffects[(int)id].Transition.Wipe = new MixEffectState.TransitionWipeState();

            var cb = new MixEffectTransitionWipeCallback(st, wipe, () => FireCommandKey($"MixEffects.{id:D}.Transition.Wipe"));
            wipe.AddCallback(cb);
            _cleanupCallbacks.Add(() => wipe.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionStinger(IBMDSwitcherTransitionStingerParameters stinger, MixEffectBlockId id)
        {
            MixEffectState.TransitionStingerState st = State.MixEffects[(int)id].Transition.Stinger = new MixEffectState.TransitionStingerState();

            var cb = new MixEffectTransitionStingerCallback(st, stinger, () => FireCommandKey($"MixEffects.{id:D}.Transition.Stinger"));
            stinger.AddCallback(cb);
            _cleanupCallbacks.Add(() => stinger.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionDVE(IBMDSwitcherTransitionDVEParameters dve, MixEffectBlockId id)
        {
            MixEffectState.TransitionDVEState st = State.MixEffects[(int)id].Transition.DVE = new MixEffectState.TransitionDVEState();

            var cb = new MixEffectTransitionDVECallback(st, dve, () => FireCommandKey($"MixEffects.{id:D}.Transition.DVE"));
            dve.AddCallback(cb);
            _cleanupCallbacks.Add(() => dve.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectKeyer(IBMDSwitcherMixEffectBlock me, MixEffectBlockId id)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherKeyIterator>(me.CreateIterator);

            var keyId = UpstreamKeyId.One;
            var keyers = new List<MixEffectState.KeyerState>();
            for (iterator.Next(out IBMDSwitcherKey keyer); keyer != null; iterator.Next(out keyer))
            {
                var keyerState = new MixEffectState.KeyerState();
                keyers.Add(keyerState);

                SetupMixEffectKeyerProps(keyer, keyerState, id, keyId);

                if (keyer is IBMDSwitcherKeyLumaParameters luma)
                    SetupMixEffectLumaKeyer(luma, keyerState, id, keyId);
                if (keyer is IBMDSwitcherKeyChromaParameters chroma)
                    SetupMixEffectChromaKeyer(chroma, keyerState, id, keyId);
                if (keyer is IBMDSwitcherKeyAdvancedChromaParameters advancedChroma)
                    SetupMixEffectAdvancedChromaKeyer(advancedChroma, keyerState, id, keyId);
                if (keyer is IBMDSwitcherKeyPatternParameters pattern)
                    SetupMixEffectPatternKeyer(pattern, keyerState, id, keyId);
                if (keyer is IBMDSwitcherKeyDVEParameters dve)
                    SetupMixEffectDVEKeyer(dve, keyerState, id, keyId);
                if (keyer is IBMDSwitcherKeyFlyParameters fly)
                    SetupMixEffectFlyKeyer(fly, keyerState, id, keyId);
                
                keyId++;
            }
            State.MixEffects[(int)id].Keyers = keyers;
        }

        private void SetupMixEffectKeyerProps(IBMDSwitcherKey props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            var cb = new MixEffectKeyerCallback(state, props, str => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.{str}"));
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb, _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeCanBeDVEKeyChanged);
        }

        private void SetupMixEffectLumaKeyer(IBMDSwitcherKeyLumaParameters props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            state.Luma = new MixEffectState.KeyerLumaState();

            var cb = new MixEffectKeyerLumaCallback(state.Luma, props, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.Luma"));
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectChromaKeyer(IBMDSwitcherKeyChromaParameters props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            state.Chroma = new MixEffectState.KeyerChromaState();

            var cb = new MixEffectKeyerChromaCallback(state.Chroma, props, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.Chroma"));
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectAdvancedChromaKeyer(IBMDSwitcherKeyAdvancedChromaParameters props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            state.AdvancedChroma = new MixEffectState.KeyerAdvancedChromaState();

            var cb = new MixEffectKeyerAdvancedChromaCallback(state.AdvancedChroma, props, str => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.AdvancedChroma.{str}"));
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectPatternKeyer(IBMDSwitcherKeyPatternParameters props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            state.Pattern = new MixEffectState.KeyerPatternState();

            var cb = new MixEffectKeyerPatternCallback(state.Pattern, props, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.Pattern"));
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectDVEKeyer(IBMDSwitcherKeyDVEParameters props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            state.DVE = new MixEffectState.KeyerDVEState();

            var cb = new MixEffectKeyerDVECallback(state.DVE, props, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.DVE"));
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectFlyKeyer(IBMDSwitcherKeyFlyParameters props, MixEffectState.KeyerState state, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            var cb = new MixEffectKeyerFlyCallback(state.DVE, props, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.DVE"));
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            var ignore = new[]
            {
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged
            };
            TriggerAllChanged(cb, ignore);

            props.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeA);
            props.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeB);

            state.FlyFrames = new[]
            {
                new MixEffectState.KeyerFlyFrameState(),
                new MixEffectState.KeyerFlyFrameState()
            };

            var cb2 = new MixEffectKeyerFlyKeyFrameCallback(state.FlyFrames[0], keyframeA, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.FlyFrames.0"));
            keyframeA.AddCallback(cb2);
            _cleanupCallbacks.Add(() => keyframeA.RemoveCallback(cb2));
            TriggerAllChanged(cb2);

            var cb3 = new MixEffectKeyerFlyKeyFrameCallback(state.FlyFrames[1], keyframeB, () => FireCommandKey($"MixEffects.{meId:D}.Keyers.{keyId:D}.FlyFrames.1"));
            keyframeB.AddCallback(cb3);
            _cleanupCallbacks.Add(() => keyframeB.RemoveCallback(cb3));
            TriggerAllChanged(cb3);
        }

        private void SetupInputs(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(switcher.CreateIterator);

            var auxes = new List<AuxState>();
            var cols = new List<ColorState>();
            var ssrcs = new List<SuperSourceState>();
            for (iterator.Next(out IBMDSwitcherInput input); input != null; iterator.Next(out input))
            {
                input.GetInputId(out long id);
                var src = (VideoSource) id;

                SetInputProperties(src, input);

                if (input is IBMDSwitcherInputAux aux)
                    auxes.Add(SetupAuxiliary(src, aux));
                if (input is IBMDSwitcherInputColor col)
                    cols.Add(SetupColor(src, col));
                if (input is IBMDSwitcherInputSuperSource ssrc)
                    ssrcs.Add(SetupSuperSource(ssrc));
            }
            State.Auxiliaries = auxes;
            State.ColorGenerators = cols;
            State.SuperSources = ssrcs;
        }

        private void SetInputProperties(VideoSource id, IBMDSwitcherInput inp)
        {
            var c = new InputState();
            State.Settings.Inputs[id] = c;
            var cb = new InputCallback(c, inp, str => FireCommandKey($"Settings.Inputs.{id:D}.{str}"));
            inp.AddCallback(cb);
            _cleanupCallbacks.Add(() => inp.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private AuxState SetupAuxiliary(VideoSource id, IBMDSwitcherInputAux aux)
        {
            AuxiliaryId id2 = AtemEnumMaps.GetAuxId(id);
            var c = new AuxState();
            var cb = new AuxiliaryCallback(c, aux, () => FireCommandKey($"Auxiliaries.{id2:D}"));
            aux.AddCallback(cb);
            _cleanupCallbacks.Add(() => aux.RemoveCallback(cb));

            TriggerAllChanged(cb);
            return c;
        }

        private ColorState SetupColor(VideoSource id, IBMDSwitcherInputColor col)
        {
            ColorGeneratorId id2 = AtemEnumMaps.GetSourceIdForGen(id);
            var c = new ColorState();
            var cb = new ColorCallback(c, col, () => FireCommandKey($"ColorGenerators.{id2:D}"));
            col.AddCallback(cb);
            _cleanupCallbacks.Add(() => col.RemoveCallback(cb));
            
            TriggerAllChanged(cb);
            return c;
        }

        private SuperSourceState SetupSuperSource(IBMDSwitcherInputSuperSource ssrc)
        {
            // TODO - properly
            SuperSourceId ssrcId = SuperSourceId.One;

            var c = new SuperSourceState();
            var cb = new SuperSourceCallback(c.Properties, ssrc, () => FireCommandKey($"SuperSources.{ssrcId:D}.Properties"));
            ssrc.AddCallback(cb);
            _cleanupCallbacks.Add(() => ssrc.RemoveCallback(cb));
            TriggerAllChanged(cb);

            var ssrc2 = ssrc as IBMDSwitcherSuperSourceBorder;
            var cb3 = new SuperSourceBorderCallback(c.Border, ssrc2, () => FireCommandKey($"SuperSources.{ssrcId:D}.Border"));
            ssrc2.AddCallback(cb3);
            _cleanupCallbacks.Add(() => ssrc2.RemoveCallback(cb3));
            TriggerAllChanged(cb3);

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSuperSourceBoxIterator>(ssrc.CreateIterator);

            var boxes = new List<SuperSourceState.BoxState>();
            SuperSourceBoxId id = 0;
            for (iterator.Next(out IBMDSwitcherSuperSourceBox box); box != null; iterator.Next(out box))
            {
                var boxState = new SuperSourceState.BoxState();
                boxes.Add(boxState);
                var boxId = id++;

                var cb2 = new SuperSourceBoxCallback(boxState, box, () => FireCommandKey($"SuperSources.{ssrcId:D}.Boxes.{boxId:D}"));
                box.AddCallback(cb2);
                _cleanupCallbacks.Add(() => box.RemoveCallback(cb2));

                TriggerAllChanged(cb2);
            }

            c.Boxes = boxes;
            return c;
        }

    }
}