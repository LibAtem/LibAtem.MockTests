using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Common;

namespace LibAtem.ComparisonTests.State
{
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

            Guid itId = typeof(IBMDSwitcherInputIterator).GUID;
            switcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherInputIterator iterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(itPtr);

            SetupInputs(iterator);

        }

        ~AtemSDKComparisonMonitor()
        {
            _cleanupCallbacks.ForEach(cb => cb());
        }

        private void SetupInputs(IBMDSwitcherInputIterator iterator)
        {
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

        #region Auxiliary

        private sealed class AuxCallback : IBMDSwitcherInputAuxCallback
        {
            private readonly ComparisonAuxiliaryState _state;
            private readonly IBMDSwitcherInputAux _aux;

            public AuxCallback(ComparisonAuxiliaryState state, IBMDSwitcherInputAux aux)
            {
                _state = state;
                _aux = aux;
            }

            public void Notify(_BMDSwitcherInputAuxEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherInputAuxEventType.bmdSwitcherInputAuxEventTypeInputSourceChanged:
                        _aux.GetInputSource(out long source);
                        _state.Source = (VideoSource) source;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        // TODO - duplicated method
        private static AuxiliaryId GetAuxId(VideoSource id)
        {
            if (id >= VideoSource.Auxilary1 && id <= VideoSource.Auxilary6)
                return (AuxiliaryId)(id - VideoSource.Auxilary1);

            throw new Exception("Not an Aux");
        }

        private void SetupAuxiliary(VideoSource id, IBMDSwitcherInputAux aux)
        {
            aux.GetInputSource(out long input);
            var c = new ComparisonAuxiliaryState
            {
                Source = (VideoSource) input
            };

            State.Auxiliaries[GetAuxId(id)] = c;
            var cb = new AuxCallback(c, aux);
            aux.AddCallback(cb);
            _cleanupCallbacks.Add(() => aux.RemoveCallback(cb));
        }

        #endregion Auxiliary

        #region Color

        private sealed class ColorCallback : IBMDSwitcherInputColorCallback
        {
            private readonly ComparisonColorState _state;
            private readonly IBMDSwitcherInputColor _color;

            public ColorCallback(ComparisonColorState state, IBMDSwitcherInputColor color)
            {
                _state = state;
                _color = color;
            }

            public void Notify(_BMDSwitcherInputColorEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeHueChanged:
                        _color.GetHue(out double hue);
                        _state.Hue = hue;
                        break;
                    case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeSaturationChanged:
                        _color.GetSaturation(out double saturation);
                        _state.Saturation = saturation * 100;
                        break;
                    case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeLumaChanged:
                        _color.GetLuma(out double luma);
                        _state.Luma = luma * 100;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        // TODO - duplicated method
        private static ColorGeneratorId GetSourceIdForGen(VideoSource id)
        {
            switch (id)
            {
                case VideoSource.Color1:
                    return ColorGeneratorId.One;
                case VideoSource.Color2:
                    return ColorGeneratorId.Two;
                default:
                    throw new Exception("Not a ColorGen");
            }
        }

        private void SetupColor(VideoSource id, IBMDSwitcherInputColor col)
        {
            col.GetHue(out double hue);
            col.GetSaturation(out double saturation);
            col.GetLuma(out double luma);
            var c = new ComparisonColorState()
            {
                Hue = hue,
                Saturation = saturation * 100,
                Luma = luma * 100
            };

            State.Colors[GetSourceIdForGen(id)] = c;
            var cb = new ColorCallback(c, col);
            col.AddCallback(cb);
            _cleanupCallbacks.Add(() => col.RemoveCallback(cb));
        }

        #endregion Color
    }
}