using System;

namespace LibAtem.MockTests
{
    internal static class DeviceTestCases
    {
#if ATEM_v8_1
        public static readonly ProtocolVersion Version = ProtocolVersion.V8_0_1;
        public static readonly string Mini = "mini-v8.1";
        public static readonly string Constellation = "constellation-v8.0.2";
        public static readonly string Legacy2ME = "2me-v8.1";
#elif ATEM_v8_1_1
        public static readonly ProtocolVersion Version = ProtocolVersion.V8_1_1;
        public static readonly string Mini = "mini-v8.1.1";
        public static readonly string Constellation = "";
        public static readonly string Legacy2ME = "2me-v8.1.2";
#endif

        public static readonly string[] All = { Mini, Constellation, Legacy2ME };


        public static readonly string[] ChromaKeyer = { Legacy2ME };
        public static readonly string[] AdvancedChromaKeyer = { Mini, Constellation };
        public static readonly string[] SuperSource = { Constellation, Legacy2ME };

        // Audio
        public static readonly string[] FairlightMain = { Mini, Constellation };
#if ATEM_v8_1
        public static readonly string[] FairlightAnalog = { Mini };
        public static readonly string[] FairlightXLR = { Constellation };
#elif ATEM_v8_1_1
        public static readonly string[] FairlightAnalog = { Mini, Constellation };
#endif
        public static readonly string[] FairlightDelay = { Constellation };
    }
}
