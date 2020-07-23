using LibAtem.Common;
using LibAtem.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibAtem.MockTests.Util
{
    public static class VideoSourceUtil
    {
        private static IEnumerable<T> SelectionOfGroup<T>(List<T> sources, int randomCount = 3)
        {
            T min = sources.Min();
            T max = sources.Max();
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
                    src.GetAttribute<VideoSource, VideoSourceTypeAttribute>()?.PortType == InternalPortType.External)
                .ToList();
            var auxes = possibleSources.Where(src =>
                    src.GetAttribute<VideoSource, VideoSourceTypeAttribute>()?.PortType == InternalPortType.Auxiliary)
                .ToList();

            List<VideoSource> result = possibleSources.Except(inputs).Except(auxes).ToList();

            // Choose some random sources
            if (inputs.Count > 0)
                result.AddRange(SelectionOfGroup(inputs));
            if (auxes.Count > 0)
                result.AddRange(SelectionOfGroup(auxes));

            return result.ToArray();
        }

/*
        public static AudioSource[] TakeSelection(AudioSource[] possibleSources)
        {
            var inputs = possibleSources.Where(src =>
            {
                AudioSourceTypeAttribute props = src.GetAttribute<AudioSource, AudioSourceTypeAttribute>();
                return props != null && props.Type == AudioSourceType.ExternalVideo;
            }).ToList();

            List<AudioSource> result = possibleSources.Except(inputs).ToList();

            // Choose some random sources
            if (inputs.Count > 0)
                result.AddRange(SelectionOfGroup(inputs));

            return result.ToArray();
        }

        public static AudioSource[] TakeBadSelection(AudioSource[] possibleSources)
        {
            var badSources = Enum.GetValues(typeof(AudioSource)).OfType<AudioSource>().Where(s => !possibleSources.Contains(s)).ToArray();
            return TakeSelection(badSources);
        }
        */
    }
}
