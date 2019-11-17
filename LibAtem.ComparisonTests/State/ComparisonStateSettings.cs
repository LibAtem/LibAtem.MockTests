using System.Collections.Generic;

namespace LibAtem.ComparisonTests2.State
{
    public static class AtemStateSettings
    {
        public static bool TrackMediaClipFrames { get; set; }
        public static List<string> IgnoreNodes { get; }

        static AtemStateSettings()
        {
            IgnoreNodes = new List<string>();
        }
    }
}