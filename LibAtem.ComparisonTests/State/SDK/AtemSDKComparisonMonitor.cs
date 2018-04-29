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
                
                SetupMixEffectKeyer(me, id);

                SetupMixEffectTransition(me, id);

                id++;
            }
        }

        private void SetupMixEffectTransition(IBMDSwitcherMixEffectBlock me, MixEffectBlockId id)
        {
            var dip = me as IBMDSwitcherTransitionDipParameters;
            if (dip != null)
                SetupMixEffectTransitionDip(dip, id);
            var dve = me as IBMDSwitcherTransitionDVEParameters;
            if (dve != null)
                SetupMixEffectTransitionDVE(dve, id);
        }

        private void SetupMixEffectTransitionDip(IBMDSwitcherTransitionDipParameters dip, MixEffectBlockId id)
        {
            ComparisonMixEffectTransitionDipState st = State.MixEffects[id].Transition.Dip;

            var cb = new MixEffectTransitionDipCallback(st, dip);
            dip.AddCallback(cb);
            _cleanupCallbacks.Add(() => dip.RemoveCallback(cb));

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

                var chroma = keyer as IBMDSwitcherKeyChromaParameters;
                if (chroma != null)
                    SetupMixEffectChromaKeyer(chroma, id, keyId);
                var dve = keyer as IBMDSwitcherKeyDVEParameters;
                if (dve != null)
                    SetupMixEffectDVEKeyer(dve, id, keyId);
                var fly = keyer as IBMDSwitcherKeyFlyParameters;
                if (fly != null)
                    SetupMixEffectFlyKeyer(fly, id, keyId);

                keyId++;
            }
        }

        private void SetupMixEffectChromaKeyer(IBMDSwitcherKeyChromaParameters props, MixEffectBlockId meId, UpstreamKeyId keyId)
        {
            ComparisonMixEffectKeyerChromaState chroma = State.MixEffects[meId].Keyers[keyId].Chroma;
            
            var cb = new MixEffectKeyerChromaCallback(chroma, props);
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