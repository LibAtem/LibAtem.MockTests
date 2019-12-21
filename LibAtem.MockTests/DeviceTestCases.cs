using System;
using System.Collections.Generic;
using System.Text;

namespace LibAtem.MockTests
{
    internal static class DeviceTestCases
    {
        public static readonly string HandshakeMini = "8.1-mini";
        public static readonly string HandshakeConstellation = "8.0.2-constellation";
        public static readonly string Handshake2ME = "8.0.1-2me";

        // Audio
        public static readonly string[] FairlightMain = {HandshakeMini, HandshakeConstellation};
    }
}
