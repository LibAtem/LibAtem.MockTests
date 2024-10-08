﻿using System;
using System.Collections.Generic;
using System.Linq;
using LibAtem.MockTests.Util;

namespace LibAtem.MockTests
{
    internal static class DeviceTestCases
    {
#if ATEM_v8_1
        public static readonly ProtocolVersion Version = ProtocolVersion.V8_0_1;
        public static readonly string MiniExtremeIso = "";
        public static readonly string Mini = "mini-v8.1";
        public static readonly string Constellation = "constellation-v8.0.2";
        public static readonly string Constellation2ME = "";
        public static readonly string TwoME = "2me-v8.1";
        public static readonly string TwoME4K = "2me4k-v8.0.1";
        public static readonly string FourME4K = "";
        public static readonly string TVSHD = "tvshd-v8.1.0";
        public static readonly string TVS = "tvs-v8.1.0";
        public static readonly string TVSHD8 = "";
#elif ATEM_v8_1_1
        public static readonly ProtocolVersion Version = ProtocolVersion.V8_1_1;
        public static readonly string MiniExtremeIso = "mini-extreme-iso-v8.6.1";
        public static readonly string Mini = "mini-v8.3";
        public static readonly string Constellation = "constellation-8k-v9.4";
        public static readonly string Constellation2MEHD = "constellation-2me-hd-v8.7";
        public static readonly string TwoME = "2me-v8.3";
        public static readonly string TwoME4K = "";
        public static readonly string FourME4K = "4me-bs4k-v8.2";
        public static readonly string TVSHD = "tvshd-v8.2.0";
        public static readonly string TVS = "tvs-v8.1.1";
        public static readonly string TVSHD8 = "tvs-hd8-v9.0";
#endif

        public static readonly string[] All = { MiniExtremeIso, Mini, Constellation, Constellation2MEHD, TwoME, TVSHD, TVS, TwoME4K, FourME4K, TVSHD8 };
        public static readonly string[] DownConvertSDMode = { TwoME };
        public static readonly string[] DownConvertHDMode = { FourME4K };
        public static readonly string[] AutoVideoMode = {Mini, MiniExtremeIso };
        public static readonly string[] MacroTransfer = All.Where(t => t != "").Take(1).ToArray();

        public static readonly string[] ChromaKeyer = { TwoME };
        public static readonly string[] AdvancedChromaKeyer = { Mini, MiniExtremeIso, Constellation };
        public static readonly string[] SuperSource = { Constellation, TwoME, MiniExtremeIso };
        public static readonly string[] SuperSourceCascade = { Constellation };

        public static readonly string[] Multiview = { TVS, TwoME, Constellation };
        public static readonly string[] MultiviewRouteInputs = {TwoME, Constellation};
        public static readonly string[] MultiviewSwapProgramPreview = { TwoME4K, FourME4K };
        public static readonly string[] MultiviewToggleSafeArea = { TwoME4K, FourME4K, Constellation };
        public static readonly string[] MultiviewVuMeters = { TwoME4K, FourME4K, Constellation };
        public static readonly string[] MultiviewLabelSample = { TwoME4K, TwoME, Constellation, MiniExtremeIso, Mini };
        public static readonly string[] MultiviewBorders = { Constellation };

        public static readonly string[] CameraControl = {TwoME, Constellation};
        public static readonly string[] SerialPort = { TwoME, Constellation };
        public static readonly string[] SDI3G = {Constellation, TwoME4K};
        public static readonly string[] MixMinusOutputs = {TVSHD};
        public static readonly string[] Talkback = {Constellation}; // TODO - more
        public static readonly string[] TimeCodeMode = {Mini, MiniExtremeIso };

        public static readonly string[] MediaPlayer = All;
        public static readonly string[] MediaPlayerStillTransfer =
            (new List<string> {Mini, TwoME, Constellation, TVS}).Where(t => t != "").Take(1).ToArray();
        public static readonly string[] MediaPlayerStillCapture = { Mini };
        public static readonly string[] MediaPlayerClips = { TwoME, Constellation, TwoME4K, FourME4K };

        public static readonly string[] HyperDecks = Randomiser.SelectionOfGroup(All.ToList()).ToArray();

        public static readonly string[] Streaming = { MiniExtremeIso };
        public static readonly string[] Recording = { MiniExtremeIso };

        // Audio
        public static readonly string[] FairlightMain = { Mini, MiniExtremeIso, Constellation };
#if ATEM_v8_1
        public static readonly string[] FairlightAnalog = { Mini };
        public static readonly string[] FairlightXLR = { Constellation };
#else
        public static readonly string[] FairlightAnalog = { Mini, MiniExtremeIso, Constellation };
#endif
        public static readonly string[] FairlightDelay = { Constellation };

        public static readonly string[] ClassicAudioMain = { TwoME, TVSHD, TVS };
        public static readonly string[] ClassicAudioHeadphones = { TVSHD };
        public static readonly string[] ClassicAudioMonitors = { TwoME4K, FourME4K };
        public static readonly string[] ClassicAudioXLRLevel = { TVSHD };

        public static readonly string[] AudioRouting = { TVSHD8 };

        public static readonly string[] DisplayClock = { Constellation2MEHD };

    }
}
