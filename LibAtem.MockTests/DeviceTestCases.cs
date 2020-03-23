using System;

namespace LibAtem.MockTests
{
    internal static class DeviceTestCases
    {
#if ATEM_v8_1_1
        public static readonly ProtocolVersion Version = ProtocolVersion.V8_1_1;
        public static readonly string Mini = "mini-v8.1.1";
#else
        public static readonly ProtocolVersion Version = ProtocolVersion.V8_0_1;
        public static readonly string Mini = "mini-v8.1";
#endif

        public static readonly string Constellation_8_0_2 = "constellation-v8.0.2";
        public static readonly string Legacy2ME_8_0_1 = "2me-v8.1";


        public static readonly string[] All = { Mini, Constellation_8_0_2, Legacy2ME_8_0_1 };


        public static readonly string[] ChromaKeyer = { Legacy2ME_8_0_1 };
        public static readonly string[] AdvancedChromaKeyer = { Mini, Constellation_8_0_2 };
        public static readonly string[] SuperSource = { Constellation_8_0_2, Legacy2ME_8_0_1 };

        // Audio
        public static readonly string[] FairlightMain = { Mini, Constellation_8_0_2 };
        public static readonly string[] FairlightAnalog = { Mini };
        public static readonly string[] FairlightXLR = { Constellation_8_0_2 };
        public static readonly string[] FairlightDelay = { Constellation_8_0_2 };
    }
}
