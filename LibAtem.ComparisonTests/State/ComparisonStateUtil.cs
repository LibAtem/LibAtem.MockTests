using LibAtem.Common;
using LibAtem.State;
using System;
using System.Collections.Generic;
using System.Linq;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State
{
    public static class AtemStateUtil
    {
        public static void UpdateVideoTally(AtemState state)
        {
            Tuple<List<VideoSource>, List<VideoSource>> me1 = CalculateTallyForMixEffect(state.MixEffects[(int)MixEffectBlockId.One]);
            List<VideoSource> program = me1.Item1;
            List<VideoSource> preview = me1.Item2;

            state.DownstreamKeyers.ForEach((i, dsk) =>
            {
                if (dsk.State.OnAir)
                {
                    program.Add(dsk.Sources.FillSource);
                    program.Add(dsk.Sources.CutSource);
                }
                if (!dsk.State.OnAir && dsk.Properties.Tie)
                {
                    preview.Add(dsk.Sources.FillSource);
                    preview.Add(dsk.Sources.CutSource);
                }
                // TODO - some more cases need filling out
            });

            if (program.Contains(VideoSource.ME2Prog))
                program.AddRange(CalculateTallyForMixEffect(state.MixEffects[(int)MixEffectBlockId.Two]).Item1);
            else if (preview.Contains(VideoSource.ME2Prog))
                preview.AddRange(CalculateTallyForMixEffect(state.MixEffects[(int)MixEffectBlockId.Two]).Item1);
            if (program.Contains(VideoSource.ME2Prev))
                program.AddRange(CalculateTallyForMixEffect(state.MixEffects[(int)MixEffectBlockId.Two]).Item2);
            else if (preview.Contains(VideoSource.ME2Prev))
                preview.AddRange(CalculateTallyForMixEffect(state.MixEffects[(int)MixEffectBlockId.Two]).Item2);

            // TODO - repeat for me3 & me4

            HashSet<VideoSource> programSet = program.ToHashSet();
            HashSet<VideoSource> previewSet = preview.ToHashSet();

            foreach (KeyValuePair<VideoSource, InputState> inp in state.Settings.Inputs)
            {
                inp.Value.Tally.PreviewTally = previewSet.Contains(inp.Key);
                inp.Value.Tally.ProgramTally = programSet.Contains(inp.Key);
            }
        }

        private static Tuple<List<VideoSource>, List<VideoSource>> CalculateTallyForMixEffect(MixEffectState state)
        {
            var program = new List<VideoSource>();
            var preview = new List<VideoSource>();

            preview.Add(state.Sources.Preview);
            program.Add(state.Sources.Program);
            //program.Add(state.Preview);

            state.Keyers.ForEach((i, keyer) =>
            {
                var keyerId = (UpstreamKeyId)i;
                if (keyer.OnAir)
                {
                    program.AddRange(CalculateSourcesForKeyer(keyer));
                    preview.AddRange(CalculateSourcesForKeyer(keyer));
                }
                if (!keyer.OnAir && state.Transition.Properties.Selection.HasFlag(keyerId.ToTransitionLayerKey()))
                    preview.AddRange(CalculateSourcesForKeyer(keyer));

                // TODO - some more cases need filling out to handle in transition better
            });

            return Tuple.Create(program, preview);
        }
        
        private static IEnumerable<VideoSource> CalculateSourcesForKeyer(MixEffectState.KeyerState state)
        {
            yield return state.Properties.FillSource;

            switch (state.Properties.Mode)
            {
                case MixEffectKeyType.Luma:
                    yield return state.Properties.CutSource;
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