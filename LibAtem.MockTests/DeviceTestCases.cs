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


        public static readonly string[] All = { HandshakeMini, HandshakeConstellation, Handshake2ME };


        public static readonly string[] AdvancedChromaKeyer = { HandshakeMini, HandshakeConstellation };

        // Audio
        public static readonly string[] FairlightMain = {HandshakeMini, HandshakeConstellation};
    }
}
