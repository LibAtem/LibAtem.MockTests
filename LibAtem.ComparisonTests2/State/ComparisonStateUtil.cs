using LibAtem.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibAtem.ComparisonTests2.State
{
    public static class ComparisonStateUtil
    {
        public static void UpdateVideoTally(ComparisonState state)
        {
            Tuple<List<VideoSource>, List<VideoSource>> me1 = CalculateTallyForMixEffect(state.MixEffects[MixEffectBlockId.One]);
            List<VideoSource> program = me1.Item1;
            List<VideoSource> preview = me1.Item2;

            foreach (KeyValuePair<DownstreamKeyId, ComparisonDownstreamKeyerState> dsk in state.DownstreamKeyers)
            {
                if (dsk.Value.OnAir)
                {
                    program.Add(dsk.Value.FillSource);
                    program.Add(dsk.Value.CutSource);
                }
                if (!dsk.Value.OnAir && dsk.Value.Tie)
                {
                    preview.Add(dsk.Value.FillSource);
                    preview.Add(dsk.Value.CutSource);
                }
                // TODO - some more cases need filling out
            }

            if (program.Contains(VideoSource.ME2Prog))
                program.AddRange(CalculateTallyForMixEffect(state.MixEffects[MixEffectBlockId.Two]).Item1);
            else if (preview.Contains(VideoSource.ME2Prog))
                preview.AddRange(CalculateTallyForMixEffect(state.MixEffects[MixEffectBlockId.Two]).Item1);
            if (program.Contains(VideoSource.ME2Prev))
                program.AddRange(CalculateTallyForMixEffect(state.MixEffects[MixEffectBlockId.Two]).Item2);
            else if (preview.Contains(VideoSource.ME2Prev))
                preview.AddRange(CalculateTallyForMixEffect(state.MixEffects[MixEffectBlockId.Two]).Item2);

            // TODO - repeat for me3 & me4

            HashSet<VideoSource> programSet = program.ToHashSet();
            HashSet<VideoSource> previewSet = preview.ToHashSet();

            foreach (KeyValuePair<VideoSource, ComparisonInputState> inp in state.Inputs)
            {
                inp.Value.PreviewTally = previewSet.Contains(inp.Key);
                inp.Value.ProgramTally = programSet.Contains(inp.Key);
            }
        }

        private static Tuple<List<VideoSource>, List<VideoSource>> CalculateTallyForMixEffect(ComparisonMixEffectState state)
        {
            var program = new List<VideoSource>();
            var preview = new List<VideoSource>();

            preview.Add(state.Preview);
            program.Add(state.Program);
            //program.Add(state.Preview);

            foreach (KeyValuePair<UpstreamKeyId, ComparisonMixEffectKeyerState> keyer in state.Keyers)
            {
                if (keyer.Value.OnAir)
                {
                    program.AddRange(CalculateSourcesForKeyer(keyer.Value));
                    preview.AddRange(CalculateSourcesForKeyer(keyer.Value));
                }
                if (!keyer.Value.OnAir && state.Transition.Selection.HasFlag(keyer.Key.ToTransitionLayerKey()))
                    preview.AddRange(CalculateSourcesForKeyer(keyer.Value));

                // TODO - some more cases need filling out to handle in transition better
            }

            return Tuple.Create(program, preview);
        }
        
        private static IEnumerable<VideoSource> CalculateSourcesForKeyer(ComparisonMixEffectKeyerState state)
        {
            yield return state.FillSource;

            switch (state.Type)
            {
                case MixEffectKeyType.Luma:
                    yield return state.CutSource;
                    break;
                case MixEffectKeyType.Chroma:
                case MixEffectKeyType.Pattern:
                case MixEffectKeyType.DVE:
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
    }
}