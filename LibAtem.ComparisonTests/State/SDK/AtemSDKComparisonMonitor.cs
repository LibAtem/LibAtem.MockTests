using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
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

        }

        ~AtemSDKComparisonMonitor()
        {
            _cleanupCallbacks.ForEach(cb => cb());
        }

        private void TriggerAllChanged<T>(INotify<T> cb, params T[] skip)
        {
            Enum.GetValues(typeof(T)).OfType<T>().Where(v => !skip.Contains(v)).ForEach(cb.Notify);
        }

        private void SetupMixEffects(IBMDSwitcher switcher)
        {
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator) Marshal.GetObjectForIUnknown(itPtr);

            var id = MixEffectBlockId.One;
            for (iterator.Next(out IBMDSwitcherMixEffectBlock me); me != null; iterator.Next(out me))
            {
                me.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out long pgm);
                me.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out long pvw);

                State.MixEffects[id] = new ComparisonMixEffectState()
                {
                    Preview = (VideoSource) pvw,
                    Program = (VideoSource) pgm,
                };

                var cb = new MixEffectPropertiesCallback(State.MixEffects[(MixEffectBlockId)id], me);
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

            var cb = new MixEffectTransitionPropertiesCallback(st, trans);
            trans.AddCallback(cb);
            _cleanupCallbacks.Add(() => trans.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionMix(IBMDSwitcherTransitionMixParameters dip, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionMixState st = State.MixEffects[id].Transition.Mix;

            var cb = new MixEffectTransitionMixCallback(st, dip);
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionDip(IBMDSwitcherTransitionDipParameters dip, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionDipState st = State.MixEffects[id].Transition.Dip;

            var cb = new MixEffectTransitionDipCallback(st, dip);
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionWipe(IBMDSwitcherTransitionWipeParameters wipe, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionWipeState st = State.MixEffects[id].Transition.Wipe;

            var cb = new MixEffectTransitionWipeCallback(st, wipe);
            wipe.AddCallback(cb);
            _cleanupCallbacks.Add(() => wipe.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionStinger(IBMDSwitcherTransitionStingerParameters stinger, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionStingerState st = State.MixEffects[id].Transition.Stinger;

            var cb = new MixEffectTransitionStingerCallback(st, stinger);
            stinger.AddCallback(cb);
            _cleanupCallbacks.Add(() => stinger.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectTransitionDVE(IBMDSwitcherTransitionDVEParameters dve, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionDVEState st = State.MixEffects[id].Transition.DVE;

            var cb = new MixEffectTransitionDVECallback(st, dve);
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

            var cb = new MixEffectKeyerCallback(keyer, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb, _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeCanBeDVEKeyChanged);
        }

        private void SetupMixEffectLumaKeyer(IBMDSwitcherKeyLumaParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerLumaState luma = State.MixEffects[meId].Keyers[keyId].Luma;

            var cb = new MixEffectKeyerLumaCallback(luma, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectChromaKeyer(IBMDSwitcherKeyChromaParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerChromaState chroma = State.MixEffects[meId].Keyers[keyId].Chroma;

            var cb = new MixEffectKeyerChromaCallback(chroma, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectPatternKeyer(IBMDSwitcherKeyPatternParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerPatternState pattern = State.MixEffects[meId].Keyers[keyId].Pattern;

            var cb = new MixEffectKeyerPatternCallback(pattern, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectDVEKeyer(IBMDSwitcherKeyDVEParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerDVEState dve = State.MixEffects[meId].Keyers[keyId].DVE;

            var cb = new MixEffectKeyerDVECallback(dve, props);
            props.AddCallback(cb);
            _cleanupCallbacks.Add(() => props.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupMixEffectFlyKeyer(IBMDSwitcherKeyFlyParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerFlyState fly = State.MixEffects[meId].Keyers[keyId].Fly;

            var cb = new MixEffectKeyerFlyCallback(fly, props);
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

                // TODO - normal input stuff

                var aux = input as IBMDSwitcherInputAux;
                if (aux != null)
                    SetupAuxiliary(src, aux);

                var col = input as IBMDSwitcherInputColor;
                if (col != null)
                    SetupColor(src, col);
            }
        }

        private void SetupAuxiliary(VideoSource id, IBMDSwitcherInputAux aux)
        {
            var c = new ComparisonAuxiliaryState();
            State.Auxiliaries[AtemEnumMaps.GetAuxId(id)] = c;
            var cb = new AuxiliaryCallback(c, aux);
            aux.AddCallback(cb);
            _cleanupCallbacks.Add(() => aux.RemoveCallback(cb));

            TriggerAllChanged(cb);
        }

        private void SetupColor(VideoSource id, IBMDSwitcherInputColor col)
        {
            var c = new ComparisonColorState();
            State.Colors[AtemEnumMaps.GetSourceIdForGen(id)] = c;
            var cb = new ColorCallback(c, col);
            col.AddCallback(cb);
            _cleanupCallbacks.Add(() => col.RemoveCallback(cb));
            
            TriggerAllChanged(cb);
        }
        
    }
}