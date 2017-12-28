using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Common;
using LibAtem.XmlState.Settings;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.Settings
{
    [Collection("Client")]
    public class TestMultiView
    {
        private static readonly IReadOnlyDictionary<MultiViewLayout, _BMDSwitcherMultiViewLayout> LayoutMap;

        static TestMultiView()
        {
            LayoutMap = new Dictionary<MultiViewLayout, _BMDSwitcherMultiViewLayout>
            {
                {MultiViewLayout.ProgramBottom, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramBottom},
                {MultiViewLayout.ProgramLeft, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramLeft},
                {MultiViewLayout.ProgramRight, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramRight},
                {MultiViewLayout.ProgramTop, _BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramTop},
            };
        }

        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestMultiView(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        [Fact]
        public void EnsureLayoutMapIsComplete()
        {
            EnumMap.EnsureIsComplete(LayoutMap);
        }

        private static List<IBMDSwitcherMultiView> GetMultiviewers(AtemComparisonHelper helper)
        {
            Guid itId = typeof(IBMDSwitcherMultiViewIterator).GUID;
            helper.SdkSwitcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMultiViewIterator
                iterator = (IBMDSwitcherMultiViewIterator) Marshal.GetObjectForIUnknown(itPtr);

            List<IBMDSwitcherMultiView> result = new List<IBMDSwitcherMultiView>();
            for (iterator.Next(out IBMDSwitcherMultiView r); r != null; iterator.Next(out r))
                result.Add(r);

            return result;
        }

        [Fact]
        public void TestMultiviewCount()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                List<IBMDSwitcherMultiView> multiviewers = GetMultiviewers(helper);
                Assert.Equal((int) helper.Profile.MultiView.Count, multiviewers.Count);
            }
        }

        [Fact]
        public void TestMultiviewProperties()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                List<IBMDSwitcherMultiView> multiviewers = GetMultiviewers(helper);

                var failures = new List<string>();

                for (uint i = 0; i < multiviewers.Count; i++)
                {
                    IBMDSwitcherMultiView sdkProps = multiviewers[(int) i];

                    // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                    // We track this another way
                    sdkProps.GetInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                    if (availabilityMask != (_BMDSwitcherInputAvailability) ((int)SourceAvailability.Multiviewer << 2))
                        failures.Add("Incorrect SourceAvailability value");

                    var mvProps = helper.FindWithMatching(new MultiviewPropertiesGetCommand {MultiviewIndex = i});
                    if (mvProps == null)
                    {
                        failures.Add(string.Format("{0}: Failed to find MV props. Skipping", i));
                        continue;
                    }

                    failures.AddRange(CheckLayout(helper, sdkProps, i));

                    sdkProps.GetWindowCount(out uint count);
                    if (count != MultiView.WindowCount)
                        failures.Add(string.Format("{0}: Incorrect window count. {1}, {2}", i, MultiView.WindowCount, count));
                    
                    failures.AddRange(CheckWindowSource(helper, sdkProps, i));

                    // TODO - these parts of the test havent been tested yet, due to available hardware
                    failures.AddRange(CheckProgramPreviewSwapped(helper, sdkProps, i));
                    failures.AddRange(CheckToggleSafeArea(helper, sdkProps, i));

                    sdkProps.SupportsVuMeters(out int supportsVu);
                    if (helper.Profile.MultiView.VuMeters != (supportsVu != 0))
                        failures.Add(string.Format("{0}: Incorrect SupportsVuMeters. {1}, {2}", i, helper.Profile.MultiView.VuMeters, supportsVu != 0));

                    // TODO - implement these
                    //GetVuMeterEnabled Check if the VU meter is currently visible on a specified window.
                    //GetVuMeterOpacity Get the current MultiView VU meter opacity.
                }

                failures.ForEach(f => _output.WriteLine(f));
                Assert.Equal(new List<string>(), failures);
            }
        }

        //        [Fact]
        //        public void TestWindowInputVuMeter()
        //        {
        //            // TODO - before this is done, all of VideoSource will need annotating with whther they support vu meter or not.
        //            // It will also require a device which supports vu meters
        //            //CurrentInputSupportsVuMeter Check if the current input of a specified MultiView window supports VU meters.
        //        }
        private IEnumerable<string> CheckWindowSource(AtemComparisonHelper helper, IBMDSwitcherMultiView sdkProps, uint id)
        {
            sdkProps.CanRouteInputs(out int canRoute);
            if (helper.Profile.MultiView.CanRouteInputs != (canRoute != 0))
                yield return string.Format("{0}: Incorrect CanRouteInputs. {1}, {2}", id, helper.Profile.MultiView.CanRouteInputs, canRoute != 0);
            if (canRoute == 0)
            {
                _output.WriteLine("Doesnt support CanRouteInputs. Skipping some checks");
                yield break;
            }

            foreach (string s in CheckWindowSourcesMatch(helper, sdkProps, id))
                yield return s;

            helper.SendCommand(new MultiviewWindowInputSetCommand
            {
                MultiviewIndex = id,
                WindowIndex = 1,
                Source = VideoSource.Color1,
            });
            helper.Sleep();
            VideoSource?[] expected = {null, VideoSource.Color1, null, null, null, null, null, null, null, null};
            string[] errs = CheckWindowSourcesMatch(helper, sdkProps, id, expected).ToArray();
            if (errs.Length != 1 || !errs[0].StartsWith(string.Format("{0}.{1}: Incorrect window source", id, 1)))
                yield return string.Format("{0}.{1}: Shouldnt be able to set source of pvw/pgm", id, 1);

            helper.SendCommand(new MultiviewWindowInputSetCommand
            {
                MultiviewIndex = id,
                WindowIndex = 3,
                Source = VideoSource.Color1,
            });
            helper.Sleep();
            expected = new VideoSource?[] {null, null, null, VideoSource.Color1, null, null, null, null, null, null};
            foreach (string s in CheckWindowSourcesMatch(helper, sdkProps, id, expected))
                yield return s;

            helper.SendCommand(new MultiviewWindowInputSetCommand
            {
                MultiviewIndex = id,
                WindowIndex = 3,
                Source = VideoSource.Color2,
            });
            helper.Sleep();
            expected = new VideoSource?[] { null, null, null, VideoSource.Color2, null, null, null, null, null, null };
            foreach (string s in CheckWindowSourcesMatch(helper, sdkProps, id, expected))
                yield return s;

            helper.SendCommand(new MultiviewWindowInputSetCommand
            {
                MultiviewIndex = id,
                WindowIndex = 7,
                Source = VideoSource.MediaPlayer1,
            });
            helper.Sleep();
            expected = new VideoSource?[] { null, null, null, null, null, null, null, VideoSource.MediaPlayer1, null, null };
            foreach (string s in CheckWindowSourcesMatch(helper, sdkProps, id, expected))
                yield return s;

            helper.SendCommand(new MultiviewWindowInputSetCommand
            {
                MultiviewIndex = id,
                WindowIndex = 7,
                Source = VideoSource.Input1,
            });
            helper.Sleep();
            expected = new VideoSource?[] { null, null, null, null, null, null, null, VideoSource.Input1, null, null };
            foreach (string s in CheckWindowSourcesMatch(helper, sdkProps, id, expected))
                yield return s;
        }

        private static IEnumerable<string> CheckWindowSourcesMatch(AtemComparisonHelper helper, IBMDSwitcherMultiView sdkProps, uint id, VideoSource?[] expected=null)
        {
            sdkProps.GetWindowCount(out uint windowCount);
            for (uint i = 0; i < windowCount; i++)
            {
                sdkProps.GetWindowInput(i, out long input);

                var winCmd = helper.FindWithMatching(new MultiviewWindowInputGetCommand { MultiviewIndex = id, WindowIndex = i});
                if (winCmd == null || winCmd.Source != (VideoSource) input)
                    yield return string.Format("{0}.{1}: Incorrect window source. {2}, {3}", id, i, winCmd?.Source, (VideoSource) input);

                VideoSource? current = expected?[(int) i];
                if (current != null && winCmd != null && winCmd.Source != current)
                    yield return string.Format("{0}.{1}: Incorrect window source. {2}, {3}", id, i, winCmd.Source, current);
            }
        }

        private static IEnumerable<string> CheckLayout(AtemComparisonHelper helper, IBMDSwitcherMultiView sdkProps, uint id)
        {
            var mvCmd = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetLayout(out _BMDSwitcherMultiViewLayout layout);
            if (layout != LayoutMap[mvCmd.Layout])
                yield return string.Format("{0}: Incorrect layout. {1}, {2}", id, mvCmd.Layout, layout);

            // Invert it
            helper.SendCommand(new MultiviewPropertiesSetCommand
            {
                MultiviewIndex = id,
                Mask = MultiviewPropertiesSetCommand.MaskFlags.Layout,
                Layout = MultiViewLayout.ProgramLeft,
            });
            helper.Sleep();

            var mvCmd2 = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetLayout(out layout);
            if (layout != LayoutMap[mvCmd2.Layout])
                yield return string.Format("{0}: Incorrect Layout. {1}, {2}", id, mvCmd2.Layout, layout);
            if (LayoutMap[mvCmd2.Layout] == LayoutMap[mvCmd.Layout])
                yield return string.Format("{0}: Layout didnt set!. {1}", id, mvCmd.Layout);

            // Set it back again
            helper.SendCommand(new MultiviewPropertiesSetCommand
            {
                MultiviewIndex = id,
                Mask = MultiviewPropertiesSetCommand.MaskFlags.Layout,
                Layout = MultiViewLayout.ProgramTop,
            });
            helper.Sleep();

            mvCmd = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetLayout(out layout);
            if (layout != LayoutMap[mvCmd.Layout])
                yield return string.Format("{0}: Incorrect Layout. {1}, {2}", id, mvCmd.Layout, layout);
            if (LayoutMap[mvCmd2.Layout] == LayoutMap[mvCmd.Layout])
                yield return string.Format("{0}: Layout didnt set!. {1}", id, mvCmd.Layout);
        }

        private IEnumerable<string> CheckProgramPreviewSwapped(AtemComparisonHelper helper, IBMDSwitcherMultiView sdkProps, uint id)
        {
            sdkProps.SupportsProgramPreviewSwap(out int canSwapPreviewProgram);
            if (helper.Profile.MultiView.CanSwapPreviewProgram != (canSwapPreviewProgram != 0))
                yield return string.Format("{0}: Incorrect CanSwapPreviewProgram. {1}, {2}", id, helper.Profile.MultiView.CanSwapPreviewProgram, canSwapPreviewProgram != 0);

            if (canSwapPreviewProgram == 0)
            {
                _output.WriteLine("Doesnt support CanSwapPreviewProgram. Skipping some checks");
                yield break;
            }

            var mvCmd = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetProgramPreviewSwapped(out int swapped);
            if (mvCmd.ProgramPreviewSwapped != (swapped != 0))
                yield return string.Format("{0}: Incorrect ProgramPreviewSwapped. {1}, {2}", id, mvCmd.ProgramPreviewSwapped, swapped != 0);

            // Invert it
            helper.SendCommand(new MultiviewPropertiesSetCommand
            {
                MultiviewIndex = id, 
                Mask = MultiviewPropertiesSetCommand.MaskFlags.ProgramPreviewSwapped,
                ProgramPreviewSwapped = !mvCmd.ProgramPreviewSwapped,
            });
            helper.Sleep();

            var mvCmd2 = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetProgramPreviewSwapped(out swapped);
            if (mvCmd.ProgramPreviewSwapped != (swapped != 0))
                yield return string.Format("{0}: Incorrect ProgramPreviewSwapped. {1}, {2}", id, mvCmd.ProgramPreviewSwapped, swapped != 0);
            if (mvCmd2.ProgramPreviewSwapped == mvCmd.ProgramPreviewSwapped)
                yield return string.Format("{0}: ProgramPreviewSwapped didnt set!. {1}", id, !mvCmd.ProgramPreviewSwapped);

            // Set it back again
            helper.SendCommand(new MultiviewPropertiesSetCommand
            {
                MultiviewIndex = id,
                Mask = MultiviewPropertiesSetCommand.MaskFlags.ProgramPreviewSwapped,
                ProgramPreviewSwapped = !mvCmd.ProgramPreviewSwapped,
            });
            helper.Sleep();

            mvCmd = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetProgramPreviewSwapped(out swapped);
            if (mvCmd.ProgramPreviewSwapped != (swapped != 0))
                yield return string.Format("{0}: Incorrect ProgramPreviewSwapped. {1}, {2}", id, mvCmd.ProgramPreviewSwapped, swapped != 0);
            if (mvCmd2.ProgramPreviewSwapped == mvCmd.ProgramPreviewSwapped)
                yield return string.Format("{0}: ProgramPreviewSwapped didnt set!. {1}", id, !mvCmd.ProgramPreviewSwapped);
        }

        private IEnumerable<string> CheckToggleSafeArea(AtemComparisonHelper helper, IBMDSwitcherMultiView sdkProps, uint id)
        {
            sdkProps.CanToggleSafeAreaEnabled(out int canToggleSafeArea);
            if (helper.Profile.MultiView.CanToggleSafeArea != (canToggleSafeArea != 0))
                yield return string.Format("{0}: Incorrect CanToggleSafeArea. {1}, {2}", id, helper.Profile.MultiView.CanToggleSafeArea, canToggleSafeArea != 0);

            if (canToggleSafeArea == 0)
            {
                _output.WriteLine("Doesnt support CanToggleSafeAreaEnabled. Skipping some checks");
                yield break;
            }

            var mvCmd = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetSafeAreaEnabled(out int enabled);
            if (mvCmd.SafeAreaEnabled != (enabled != 0))
                yield return string.Format("{0}: Incorrect SafeAreaEnabled. {1}, {2}", id, mvCmd.SafeAreaEnabled, enabled != 0);

            // Invert it
            helper.SendCommand(new MultiviewPropertiesSetCommand
            {
                MultiviewIndex = id,
                Mask = MultiviewPropertiesSetCommand.MaskFlags.SafeAreaEnabled,
                SafeAreaEnabled = !mvCmd.SafeAreaEnabled,
            });
            helper.Sleep();

            var mvCmd2 = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetSafeAreaEnabled(out enabled);
            if (mvCmd.SafeAreaEnabled != (enabled != 0))
                yield return string.Format("{0}: Incorrect SafeAreaEnabled. {1}, {2}", id, mvCmd.SafeAreaEnabled, enabled != 0);
            if (mvCmd2.SafeAreaEnabled == mvCmd.SafeAreaEnabled)
                yield return string.Format("{0}: SafeAreaEnabled didnt set!. {1}", id, !mvCmd.SafeAreaEnabled);

            // Set it back again
            helper.SendCommand(new MultiviewPropertiesSetCommand
            {
                MultiviewIndex = id,
                Mask = MultiviewPropertiesSetCommand.MaskFlags.SafeAreaEnabled,
                SafeAreaEnabled = !mvCmd.SafeAreaEnabled,
            });
            helper.Sleep();

            mvCmd = helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = id });
            sdkProps.GetSafeAreaEnabled(out enabled);
            if (mvCmd.SafeAreaEnabled != (enabled != 0))
                yield return string.Format("{0}: Incorrect SafeAreaEnabled. {1}, {2}", id, mvCmd.SafeAreaEnabled, enabled != 0);
            if (mvCmd2.SafeAreaEnabled == mvCmd.SafeAreaEnabled)
                yield return string.Format("{0}: SafeAreaEnabled didnt set!. {1}", id, !mvCmd.SafeAreaEnabled);
        }
    }
}