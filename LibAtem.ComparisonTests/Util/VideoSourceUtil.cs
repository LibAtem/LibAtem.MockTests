using LibAtem.Common;
using LibAtem.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibAtem.ComparisonTests.Util
{
    public static class VideoSourceUtil
    {
        private static IEnumerable<VideoSource> SelectionOfGroup(List<VideoSource> sources, int randomCount = 3)
        {
            VideoSource min = sources.Min();
            VideoSource max = sources.Max();
            yield return min;
            yield return max;

            sources.Remove(min);
            sources.Remove(max);

            var rand = new Random();

            for (int i = 0; i < randomCount && sources.Count > 0; i++)
            {
                int ind = rand.Next(0, sources.Count);
                yield return sources[ind];
                sources.RemoveAt(ind);
            }
        }

        public static VideoSource[] TakeSelection(VideoSource[] possibleSources)
        {
            var inputs = possibleSources.Where(src =>
            {
                VideoSourceTypeAttribute props = src.GetAttribute<VideoSource, VideoSourceTypeAttribute>();
                return (props != null && props.PortType == InternalPortType.External);
            }).ToList();
            var auxes = possibleSources.Where(src =>
            {
                VideoSourceTypeAttribute props = src.GetAttribute<VideoSource, VideoSourceTypeAttribute>();
                return (props != null && props.PortType == InternalPortType.Auxiliary);
            }).ToList();

            List<VideoSource> result = possibleSources.Except(inputs).Except(auxes).ToList();

            // Choose some random sources
            if (inputs.Count > 0)
                result.AddRange(SelectionOfGroup(inputs));
            if (auxes.Count > 0)
                result.AddRange(SelectionOfGroup(auxes));

            return result.ToArray();
        }

        public static VideoSource[] TakeBadSelection(VideoSource[] possibleSources)
        {
            var badSources = VideoSourceLists.All.Where(s => !possibleSources.Contains(s)).ToArray();
            return TakeSelection(badSources);
        }

    }
}
