using System.Collections.Generic;

namespace LibAtem.ComparisonTests2.State
{
    public static class ComparisonStateSettings
    {
        public static bool TrackMediaClipFrames { get; set; }
        public static List<string> IgnoreNodes { get; }

        static ComparisonStateSettings()
        {
            IgnoreNodes = new List<string>();
        }
    }
}