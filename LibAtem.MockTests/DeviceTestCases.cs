using System;

namespace LibAtem.MockTests
{
    using TestCaseId = Tuple<ProtocolVersion, string>;

    internal static class DeviceTestCases
    {
        public static readonly TestCaseId Mini_8_1 = Tuple.Create(ProtocolVersion.V8_0_1, "8.1-mini");
        public static readonly TestCaseId Constellation_8_0_2 = Tuple.Create(ProtocolVersion.V8_0_1, "8.0.2-constellation");
        public static readonly TestCaseId Legacy2ME_8_0_1 = Tuple.Create(ProtocolVersion.V8_0_1, "8.0.1-2me");


        public static readonly TestCaseId[] All = { Mini_8_1, Constellation_8_0_2, Legacy2ME_8_0_1 };


        public static readonly TestCaseId[] AdvancedChromaKeyer = { Mini_8_1, Constellation_8_0_2 };

        // Audio
        public static readonly TestCaseId[] FairlightMain = { Mini_8_1, Constellation_8_0_2 };
        public static readonly TestCaseId[] FairlightAnalog = { Mini_8_1 };
        public static readonly TestCaseId[] FairlightXLR = { Constellation_8_0_2 };
        public static readonly TestCaseId[] FairlightDelay = { Constellation_8_0_2 };
    }
}
