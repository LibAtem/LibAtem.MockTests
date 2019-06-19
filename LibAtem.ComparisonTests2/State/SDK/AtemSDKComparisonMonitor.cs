using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.Util;
using Xunit;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public interface INotify<in T>
    {
        void Notify(T eventType);
    }

    public sealed class AtemSDKComparisonMonitor
    {
        public ComparisonState State { get; }

        private readonly List<Action> _cleanupCallbacks = new List<Action>();

        public AtemSDKComparisonMonitor(IBMDSwitcher switcher)
        {
            State = new ComparisonState
            {
                Auxiliaries = new Dictionary<AuxiliaryId, ComparisonAuxiliaryState>(),
                MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>(),
                Colors = new Dictionary<ColorGeneratorId, ComparisonColorState>(),
            };
            
            SetupInputs(switcher);
            SetupMixEffects(switcher);
            SetupSerialPorts(switcher);
            SetupMultiViews(switcher);
            SetupDownstreamKeyers(switcher);
            SetupMediaPlayers(switcher);
            SetupMediaPool(switcher);
            SetupMacroPool(switcher);
            SetupAudio(switcher);

            var cb = new SwitcherPropertiesCallback(State, switcher, FireCommandKey);
            switcher.AddCallback(cb);
            _cleanupCallbacks.Add(() => switcher.RemoveCallback(cb));
            TriggerAllChanged(cb);

        }

        ~AtemSDKComparisonMonitor()
        {
            _cleanupCallbacks.ForEach(cb => cb());
        }

        private void FireCommandKey(CommandQueueKey key)
        {
            OnStateChange?.Invoke(this, key);
        }

        public delegate void StateChangeHandler(object sender, CommandQueueKey key);
        public event StateChangeHandler OnStateChange;
        
        private void TriggerAllChanged<T>(INotify<T> cb, params T[] skip)
        {
            Enum.GetValues(typeof(T)).OfType<T>().Where(v => !skip.Contains(v)).ForEach(cb.Notify);
        }

        private void SetupAudio(IBMDSwitcher switcher)
        {
            var mixer = switcher as IBMDSwitcherAudioMixer;

            var cb = new AudioMixerCallback(State.Audio.ProgramOut, mixer, FireCommandKey);
            mixer.AddCallback(cb);
            _cleanupCallbacks.Add(() => mixer.RemoveCallback(cb));
            TriggerAllChanged(cb);

            Guid itId = typeof(IBMDSwitcherAudioInputIterator).GUID;
            mixer.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherAudioInputIterator iterator = (IBMDSwitcherAudioInputIterator)Marshal.GetObjectForIUnknown(itPtr);

            int id = 0;
            for (iterator.Next(out IBMDSwitcherAudioInput port); port != null; iterator.Next(out port))
            {
                port.GetAudioInputId(out long inputId);
                State.Audio.Inputs[inputId] = new ComparisonAudioInputState();

                var cbi = new AudioMixerInputCallback(State.Audio.Inputs[inputId], (AudioSource)inputId, port, FireCommandKey);
                port.AddCallback(cbi);
                _cleanupCallbacks.Add(() => port.RemoveCallback(cbi));
                TriggerAllChanged(cbi);

                id++;
            }

            itId = typeof(IBMDSwitcherAudioMonitorOutputIterator).GUID;
            mixer.CreateIterator(ref itId, out itPtr);
            IBMDSwitcherAudioMonitorOutputIterator monIt = (IBMDSwitcherAudioMonitorOutputIterator)Marshal.GetObjectForIUnknown(itPtr);

            var result = new List<IBMDSwitcherAudioMonitorOutput>();
            uint id2 = 0;
            for (monIt.Next(out IBMDSwitcherAudioMonitorOutput r); r != null; monIt.Next(out r))
            {
                State.Audio.Monitors[id2] = new ComparisonAudioMonitorOutputState();

                var cbi = new AudioMixerMonitorOutputCallback(State.Audio.Monitors[id2], r, FireCommandKey);
                r.AddCallback(cbi);
                _cleanupCallbacks.Add(() => r.RemoveCallback(cbi));
                TriggerAllChanged(cbi);

                id2++;
            }

            var talkback = switcher as IBMDSwitcherTalkback;
            if (talkback != null)
            {
                var cbt = new TalkbackCallback(State.Audio.Talkback, talkback);
                talkback.AddCallback(cbt);
                _cleanupCallbacks.Add(() => talkback.RemoveCallback(cbt));
                cbt.NotifyAll(State.Audio.Inputs.Keys);
            }

            // TODO others
        }

        private void SetupSerialPorts(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherSerialPortIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherSerialPortIterator iterator = (IBMDSwitcherSerialPortIterator)Marshal.GetObjectForIUnknown(itPtr);

            int id = 0;
            for (iterator.Next(out IBMDSwitcherSerialPort port); port != null; iterator.Next(out port))
            {
                Assert.Equal(0, id);
                
                var cb = new SerialPortPropertiesCallback(State.Settings, port, FireCommandKey);
                port.AddCallback(cb);
                _cleanupCallbacks.Add(() => port.RemoveCallback(cb));
                TriggerAllChanged(cb);

                id++;
            }
        }

        private void SetupMultiViews(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherMultiViewIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMultiViewIterator iterator = (IBMDSwitcherMultiViewIterator)Marshal.GetObjectForIUnknown(itPtr);

            uint id = 0;
            for (iterator.Next(out IBMDSwitcherMultiView mv); mv != null; iterator.Next(out mv))
            {
                State.Settings.MultiViews[id] = new ComparisonSettingsMultiViewState();
                var cb = new MultiViewPropertiesCallback(State.Settings.MultiViews[id], id, mv, FireCommandKey);
                mv.AddCallback(cb);
                _cleanupCallbacks.Add(() => mv.RemoveCallback(cb));
                TriggerAllChanged(cb);

                id++;
            }
        }

        private void SetupMediaPlayers(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherMediaPlayerIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMediaPlayerIterator iterator = (IBMDSwitcherMediaPlayerIterator)Marshal.GetObjectForIUnknown(itPtr);

            MediaPlayerId id = 0;
            for (iterator.Next(out IBMDSwitcherMediaPlayer media); media != null; iterator.Next(out media))
            {
                State.MediaPlayers[id] = new ComparisonMediaPlayerState();
                var cb = new MediaPlayerCallback(State.MediaPlayers[id], id, media, FireCommandKey);
                media.AddCallback(cb);
                _cleanupCallbacks.Add(() => media.RemoveCallback(cb));
                cb.Notify();

                id++;
            }
        }
        
        private void SetupMediaPool(IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMediaPool;

            // General
            // TODO

            // Stills
            pool.GetStills(out IBMDSwitcherStills stills);

            var cbs = new MediaPoolStillsCallback(State.MediaPool, stills);
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
            for (uint i = 0; i < clipCount; i++)
            {
                pool.GetClip(i, out IBMDSwitcherClip clip);

                State.MediaPool.Clips[i] = new ComparisonMediaPoolClipState();
                var cbc = new MediaPoolClipCallback(State.MediaPool.Clips[i], clip);
                clip.AddCallback(cbc);
                _cleanupCallbacks.Add(() => clip.RemoveCallback(cbc));

                cbc.Init();
                TriggerAllChanged(cbc);
            }
        }

        private void SetupMacroPool(IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMacroPool;
            
            var cbs = new MacroPoolCallback(State.Macros, pool, FireCommandKey);
            pool.AddCallback(cbs);
            _cleanupCallbacks.Add(() => pool.RemoveCallback(cbs));

            pool.GetMaxCount(out uint count);
            for (uint i = 0; i < count; i++)
            {
                State.Macros.Pool[i] = new ComparisonMacroItemState();
                Enum.GetValues(typeof(_BMDSwitcherMacroPoolEventType)).OfType<_BMDSwitcherMacroPoolEventType>().ForEach(e => cbs.Notify(e, i, null));
            }

            var ctrl = switcher as IBMDSwitcherMacroControl;

            var cbs2 = new MacroControlCallback(State.Macros, ctrl, FireCommandKey);
            ctrl.AddCallback(cbs2);
            _cleanupCallbacks.Add(() => ctrl.RemoveCallback(cbs2));

            TriggerAllChanged(cbs2);
        }

        private void SetupDownstreamKeyers(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherDownstreamKeyIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherDownstreamKeyIterator iterator = (IBMDSwitcherDownstreamKeyIterator)Marshal.GetObjectForIUnknown(itPtr);

            DownstreamKeyId id = 0;
            for (iterator.Next(out IBMDSwitcherDownstreamKey key); key != null; iterator.Next(out key))
            {
                State.DownstreamKeyers[id] = new ComparisonDownstreamKeyerState();
                var cb = new DownstreamKeyerPropertiesCallback(State.DownstreamKeyers[id], id, key, FireCommandKey);
                key.AddCallback(cb);
                _cleanupCallbacks.Add(() => key.RemoveCallback(cb));
                TriggerAllChanged(cb);

                id++;
            }
        }

        private void SetupMixEffects(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator) Marshal.GetObjectForIUnknown(itPtr);

            var id = MixEffectBlockId.One;
            for (iterator.Next(out IBMDSwitcherMixEffectBlock me); me != null; iterator.Next(out me))
            {
                State.MixEffects[id] = new ComparisonMixEffectState();

                var cb = new MixEffectPropertiesCallback(State.MixEffects[id], id, me, FireCommandKey);
                me.AddCallback(cb);
                _cleanupCallbacks.Add(() => me.RemoveCallback(cb));
                TriggerAllChanged(cb);

                SetupMixEffectKeyer(me, id);

                SetupMixEffectTransition(me, id);

                id++;
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
            ComparisonMixEffectTransitionState st = State.MixEffects[id].Transition;

            var cb = new MixEffectTransitionPropertiesCallback(st, id, trans, FireCommandKey);
            trans.AddCallback(cb);
            _cleanupCallbacks.Add(() => trans.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionMix(IBMDSwitcherTransitionMixParameters dip, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionMixState st = State.MixEffects[id].Transition.Mix;

            var cb = new MixEffectTransitionMixCallback(st, id, dip, FireCommandKey);
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionDip(IBMDSwitcherTransitionDipParameters dip, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionDipState st = State.MixEffects[id].Transition.Dip;

            var cb = new MixEffectTransitionDipCallback(st, id, dip, FireCommandKey);
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionWipe(IBMDSwitcherTransitionWipeParameters wipe, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionWipeState st = State.MixEffects[id].Transition.Wipe;

            var cb = new MixEffectTransitionWipeCallback(st, id, wipe, FireCommandKey);
            wipe.AddCallback(cb);
            _cleanupCallbacks.Add(() => wipe.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionStinger(IBMDSwitcherTransitionStingerParameters stinger, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionStingerState st = State.MixEffects[id].Transition.Stinger;

            var cb = new MixEffectTransitionStingerCallback(st, id, stinger, FireCommandKey);
            stinger.AddCallback(cb);
            _cleanupCallbacks.Add(() => stinger.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionDVE(IBMDSwitcherTransitionDVEParameters dve, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionDVEState st = State.MixEffects[id].Transition.DVE;

            var cb = new MixEffectTransitionDVECallback(st, id, dve, FireCommandKey);
            dve.AddCallback(cb);
            _cleanupCallbacks.Add(() => dve.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectKeyer(IBMDSwitcherMixEffectBlock me, MixEffectBlockId id)
        {
            Guid itId = typeof(IBMDSwitcherKeyIterator).GUID;
            me.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherKeyIterator iterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(itPtr);

            var keyId = UpstreamKeyId.One;
            for (iterator.Next(out IBMDSwitcherKey keyer); keyer != null; iterator.Next(out keyer))
            {
                State.MixEffects[id].Keyers[keyId] = new ComparisonMixEffectKeyerState();
                SetupMixEffectKeyerProps(keyer, id, keyId);

                if (keyer is IBMDSwitcherKeyLumaParameters luma)
                    SetupMixEffectLumaKeyer(luma, id, keyId);
                if (keyer is IBMDSwitcherKeyChromaParameters chroma)
                    SetupMixEffectChromaKeyer(chroma, id, keyId);
                if (keyer is IBMDSwitcherKeyPatternParameters pattern)
                    SetupMixEffectPatternKeyer(pattern, id, keyId);
                if (keyer is IBMDSwitcherKeyDVEParameters dve)
                    SetupMixEffectDVEKeyer(dve, id, keyId);
                if (keyer is IBMDSwitcherKeyFlyParameters fly)
                    SetupMixEffectFlyKeyer(fly, id, keyId);
                
                keyId++;
            }
        }

        private void SetupMixEffectKeyerProps(IBMDSwitcherKey props, MixEffectBlockId id, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerState keyer = State.MixEffects[id].Keyers[keyId];

            var cb = new MixEffectKeyerCallback(keyer, id, keyId, props, FireCommandKey);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb, _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeCanBeDVEKeyChanged);
        }

        private void SetupMixEffectLumaKeyer(IBMDSwitcherKeyLumaParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerLumaState luma = State.MixEffects[meId].Keyers[keyId].Luma;

            var cb = new MixEffectKeyerLumaCallback(luma, meId, keyId, props, FireCommandKey);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectChromaKeyer(IBMDSwitcherKeyChromaParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerChromaState chroma = State.MixEffects[meId].Keyers[keyId].Chroma;

            var cb = new MixEffectKeyerChromaCallback(chroma, meId, keyId, props, FireCommandKey);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectPatternKeyer(IBMDSwitcherKeyPatternParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerPatternState pattern = State.MixEffects[meId].Keyers[keyId].Pattern;

            var cb = new MixEffectKeyerPatternCallback(pattern, meId, keyId, props, FireCommandKey);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectDVEKeyer(IBMDSwitcherKeyDVEParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerDVEState dve = State.MixEffects[meId].Keyers[keyId].DVE;

            var cb = new MixEffectKeyerDVECallback(dve, meId, keyId, props, FireCommandKey);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectFlyKeyer(IBMDSwitcherKeyFlyParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerFlyState fly = State.MixEffects[meId].Keyers[keyId].Fly;

            var cb = new MixEffectKeyerFlyCallback(fly, meId, keyId, props, FireCommandKey);
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

            var cb2 = new MixEffectKeyerFlyKeyFrameCallback(fly.Frames[FlyKeyKeyFrameId.One], meId, keyId, FlyKeyKeyFrameId.One, keyframeA, FireCommandKey);
            keyframeA.AddCallback(cb2);
            _cleanupCallbacks.Add(() => keyframeA.RemoveCallback(cb2));
            TriggerAllChanged(cb2);

            var cb3 = new MixEffectKeyerFlyKeyFrameCallback(fly.Frames[FlyKeyKeyFrameId.Two], meId, keyId, FlyKeyKeyFrameId.Two, keyframeB, FireCommandKey);
            keyframeB.AddCallback(cb3);
            _cleanupCallbacks.Add(() => keyframeB.RemoveCallback(cb3));
            TriggerAllChanged(cb3);
        }

        private void SetupInputs(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherInputIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherInputIterator iterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(itPtr);

            for (iterator.Next(out IBMDSwitcherInput input); input != null; iterator.Next(out input))
            {
                input.GetInputId(out long id);
                var src = (VideoSource) id;

                SetInputProperties(src, input);

                if (input is IBMDSwitcherInputAux aux)
                    SetupAuxiliary(src, aux);
                if (input is IBMDSwitcherInputColor col)
                    SetupColor(src, col);
                if (input is IBMDSwitcherInputSuperSource ssrc)
                    SetupSuperSource(ssrc);
            }
        }

        private void SetInputProperties(VideoSource id, IBMDSwitcherInput inp)
        {
            var c = new ComparisonInputState();
            State.Inputs[id] = c;
            var cb = new InputCallback(c, id, inp, FireCommandKey);
            inp.AddCallback(cb);
            _cleanupCallbacks.Add(() => inp.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupAuxiliary(VideoSource id, IBMDSwitcherInputAux aux)
        {
            var c = new ComparisonAuxiliaryState();
            State.Auxiliaries[AtemEnumMaps.GetAuxId(id)] = c;
            var cb = new AuxiliaryCallback(c, AtemEnumMaps.GetAuxId(id), aux, FireCommandKey);
            aux.AddCallback(cb);
            _cleanupCallbacks.Add(() => aux.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupColor(VideoSource id, IBMDSwitcherInputColor col)
        {
            var c = new ComparisonColorState();
            State.Colors[AtemEnumMaps.GetSourceIdForGen(id)] = c;
            var cb = new ColorCallback(c, AtemEnumMaps.GetSourceIdForGen(id), col, FireCommandKey);
            col.AddCallback(cb);
            _cleanupCallbacks.Add(() => col.RemoveCallback(cb));
            
            TriggerAllChanged(cb);
        }

        private void SetupSuperSource(IBMDSwitcherInputSuperSource ssrc)
        {
            // TODO - properly
            SuperSourceId ssrcId = SuperSourceId.One;

            State.SuperSource = new ComparisonSuperSourceState();
            var cb = new SuperSourceCallback(State.SuperSource, ssrc, FireCommandKey);
            ssrc.AddCallback(cb);
            _cleanupCallbacks.Add(() => ssrc.RemoveCallback(cb));
            TriggerAllChanged(cb);

            var ssrc2 = ssrc as IBMDSwitcherSuperSourceBorder;
            var cb3 = new SuperSourceBorderCallback(State.SuperSource, ssrc2, FireCommandKey);
            ssrc2.AddCallback(cb3);
            _cleanupCallbacks.Add(() => ssrc2.RemoveCallback(cb3));
            TriggerAllChanged(cb3);

            Guid itId = typeof(IBMDSwitcherSuperSourceBoxIterator).GUID;
            ssrc.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherSuperSourceBoxIterator iterator = (IBMDSwitcherSuperSourceBoxIterator)Marshal.GetObjectForIUnknown(itPtr);

            SuperSourceBoxId id = 0;
            for (iterator.Next(out IBMDSwitcherSuperSourceBox box); box != null; iterator.Next(out box))
            {
                State.SuperSource.Boxes[id] = new ComparisonSuperSourceBoxState();
                var cb2 = new SuperSourceBoxCallback(State.SuperSource.Boxes[id], ssrcId, id, box, FireCommandKey);
                box.AddCallback(cb2);
                _cleanupCallbacks.Add(() => box.RemoveCallback(cb2));

                TriggerAllChanged(cb2);

                id++;
            }
        }

    }
}