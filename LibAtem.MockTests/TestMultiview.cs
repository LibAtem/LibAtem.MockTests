﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestMultiview
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestMultiview(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private List<Tuple<uint, IBMDSwitcherMultiView>> GetMultiviewers(AtemMockServerWrapper helper)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(helper.SdkClient.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<uint, IBMDSwitcherMultiView>>();
            uint index = 0;
            for (iterator.Next(out IBMDSwitcherMultiView r); r != null; iterator.Next(out r))
            {
                result.Add(Tuple.Create(index, r));
                index++;
            }

            return result;
        }

        [Fact]
        public void TestLayout()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewPropertiesSetV8Command, MultiviewPropertiesGetV8Command>("Layout");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Multiview, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.SupportsQuadrantLayout(out int supportsQuadrant);

                    MultiViewLayoutV8[] possibles = supportsQuadrant != 0
                        ? Enum.GetValues(typeof(MultiViewLayoutV8)).OfType<MultiViewLayoutV8>().ToArray()
                        : new[]
                        {
                            MultiViewLayoutV8.ProgramLeft, MultiViewLayoutV8.ProgramBottom,
                            MultiViewLayoutV8.ProgramTop, MultiViewLayoutV8.ProgramRight
                        };

                    AtemState stateBefore = helper.Helper.BuildLibState();

                    for (int i = 0; i < 5; i++)
                    {
                        MultiViewLayoutV8 newValue = possibles[i % possibles.Length];
                        stateBefore.Settings.MultiViewers[(int)mv.Item1].Properties.Layout = newValue;
                        helper.SendAndWaitForChange(stateBefore, () =>
                            {
                                mv.Item2.SetLayout((_BMDSwitcherMultiViewLayout)newValue);
                            });
                    }
                }
            });
        }

        [Fact]
        public void TestSwapProgramPreview()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewPropertiesSetV8Command, MultiviewPropertiesGetV8Command>("ProgramPreviewSwapped");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewSwapProgramPreview, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.SupportsProgramPreviewSwap(out int supportsSwap);
                    Assert.Equal(1, supportsSwap);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    MultiViewerState mvState = stateBefore.Settings.MultiViewers[(int)mv.Item1];

                    for (int i = 0; i < 5; i++)
                    {
                        bool newValue = i % 2 != 0;
                        mvState.Properties.ProgramPreviewSwapped = newValue;

                        /*
                        bool tmp = mvState.Windows[0].SupportsVuMeter;
                        mvState.Windows[0].SupportsVuMeter = mvState.Windows[1].SupportsVuMeter;
                        mvState.Windows[1].SupportsVuMeter = tmp;
                        */
                        // mvState.Windows[0]

                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            mv.Item2.SetProgramPreviewSwapped(newValue ? 1 : 0);
                        });
                    }
                }
            });
        }

        [Fact]
        public void TestToggleSafeAreaEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewWindowSafeAreaCommand, MultiviewWindowSafeAreaCommand>("SafeAreaEnabled", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewToggleSafeArea, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.CanToggleSafeAreaEnabled(out int supportsToggle);
                    Assert.Equal(1, supportsToggle);

                    mv.Item2.SupportsQuadrantLayout(out int supportsQuadrant);

                    int[] windows = supportsQuadrant == 0
                        ? new[] { 0, 1 }
                        : Randomiser.SelectionOfGroup(Enumerable.Range(0, 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        for (int i = 0; i < 5; i++)
                        {
                            bool newValue = i % 2 == 0;
                            stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window].SafeAreaEnabled = newValue;

                            helper.SendAndWaitForChange(stateBefore,
                                () => { mv.Item2.SetSafeAreaEnabled((uint)window, newValue ? 1 : 0); });
                        }
                    }
                }
            });
        }

        // TODO - try again at tests for the various Can* methods

        /*
        SDK doesnt appear to acknowldge changes in this property, so it is not possible to test like this.
        [Fact]
        public void TestCanToggleSafeArea()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.Multiview, helper =>
            {
                MultiviewerConfigV8Command cmd = helper.Server.GetParsedDataDump().OfType<MultiviewerConfigV8Command>().Last();

                AtemState stateBefore = helper.Helper.BuildLibState();
                for (int i = 0; i < 5; i++)
                {
                    bool newValue = i % 2 == 0;
                    stateBefore.Settings.MultiViewers.ForEach(mv =>
                    {
                        mv.SupportsToggleSafeArea = newValue;
                        // Clean out properties that get wiped out in lib state handler
                        mv.Windows = new List<MultiViewerState.WindowState>();
                        mv.Properties.Layout = MultiViewLayoutV8.Default;
                        mv.Properties.ProgramPreviewSwapped = false;
                    });
                    cmd.CanToggleSafeArea = newValue;

                    helper.SendAndWaitForChange(stateBefore,
                        () =>
                        {
                            helper.Server.SendCommands(cmd);
                        }, -1, (sdkState, libState) =>
                        {
                            sdkState.Settings.MultiViewers.ForEach(mv =>
                                {
                                    mv.Windows = new List<MultiViewerState.WindowState>();
                                    mv.Properties.Layout = MultiViewLayoutV8.Default;
                                    mv.Properties.ProgramPreviewSwapped = false;
                                });
                            libState.Settings.MultiViewers.ForEach(mv =>
                                {
                                    mv.Windows = new List<MultiViewerState.WindowState>();
                                    mv.Properties.Layout = MultiViewLayoutV8.Default;
                                    mv.Properties.ProgramPreviewSwapped = false;
                                });
                        });
                }
            });
        }
        */

        [Fact]
        public void TestVuMeterOpacity()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewVuOpacityCommand, MultiviewVuOpacityCommand>("Opacity", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewVuMeters, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.SupportsVuMeters(out int supportsVu);
                    Assert.Equal(1, supportsVu);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    for (int i = 0; i < 5; i++)
                    {
                        double newValue = Randomiser.Range(0, 100);
                        stateBefore.Settings.MultiViewers[(int)mv.Item1].VuMeterOpacity = newValue;

                        helper.SendAndWaitForChange(stateBefore,
                            () => { mv.Item2.SetVuMeterOpacity(newValue / 100); });
                    }
                }
            });
        }

        [Fact]
        public void TestVuMeterEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewWindowVuMeterSetCommand, MultiviewWindowVuMeterGetCommand>("VuEnabled", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewVuMeters, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.SupportsVuMeters(out int supportsVu);
                    Assert.Equal(1, supportsVu);

                    mv.Item2.SupportsQuadrantLayout(out int supportsQuadrant);
                    int[] windows = Randomiser
                        .SelectionOfGroup(Enumerable.Range(0, supportsQuadrant == 0 ? 10 : 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        for (int i = 0; i < 5; i++)
                        {
                            bool newValue = i % 2 == 0;
                            stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window].VuMeterEnabled = newValue;

                            helper.SendAndWaitForChange(stateBefore,
                                () => { mv.Item2.SetVuMeterEnabled((uint)window, newValue ? 1 : 0); });
                        }
                    }
                }
            });
        }

        [Fact]
        public void TestWindowSupportsVuMeterEnabled()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MultiviewVuMeters, helper =>
            {
                var cmds = helper.Server.GetParsedDataDump().OfType<MultiviewWindowInputGetCommand>().ToList();
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.SupportsVuMeters(out int supportsVu);
                    Assert.Equal(1, supportsVu);

                    mv.Item2.SupportsQuadrantLayout(out int supportsQuadrant);
                    int[] windows = Randomiser
                        .SelectionOfGroup(Enumerable.Range(0, supportsQuadrant == 0 ? 10 : 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        MultiViewerState.WindowState windowState = stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window];

                        MultiviewWindowInputGetCommand cmd = cmds.Single(c => c.WindowIndex == window && c.MultiviewIndex == mv.Item1);

                        for (int i = 0; i < 5; i++)
                        {
                            windowState.SupportsVuMeter = cmd.SupportVuMeter = !windowState.SupportsVuMeter;

                            helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                        }
                    }
                }
            });
        }

        [Fact]
        public void TestWindowSupportsSafeAreaEnabled()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MultiviewToggleSafeArea, helper =>
            {
                var cmds = helper.Server.GetParsedDataDump().OfType<MultiviewWindowInputGetCommand>().ToList();
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.CanToggleSafeAreaEnabled(out int supported);
                    Assert.Equal(1, supported);

                    mv.Item2.SupportsQuadrantLayout(out int supportsQuadrant);
                    int[] windows = Randomiser
                        .SelectionOfGroup(Enumerable.Range(0, supportsQuadrant == 0 ? 10 : 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        MultiViewerState.WindowState windowState = stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window];

                        MultiviewWindowInputGetCommand cmd = cmds.Single(c => c.WindowIndex == window && c.MultiviewIndex == mv.Item1);

                        for (int i = 0; i < 5; i++)
                        {
                            windowState.SupportsSafeArea = cmd.SupportsSafeArea = !windowState.SupportsSafeArea;

                            helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                        }
                    }
                }
            });
        }

        /*
         This doesnt appear to like updating, even with a full data dump
        [Fact]
        public void TestSupportVuMeters()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MultiviewVuMeters, helper =>
            {
                ImmutableList<ICommand> parsedDump = helper.Server.GetParsedDataDump();
                MultiviewerConfigV8Command cmd = parsedDump.OfType<MultiviewerConfigV8Command>().Single();
                
                for (int i = 0; i < 4; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    InfoState.MultiViewInfoState mvState = stateBefore.Info.MultiViewers;
                    mvState.SupportsVuMeters = cmd.SupportsVuMeters = !mvState.SupportsVuMeters;

                    // Dummy to trigger a state change
                    helper.SendFromServerAndWaitForChange(stateBefore, new MixEffectCutCommand());

                    helper.SendFromServerAndWaitForChange(stateBefore, parsedDump, 5000);
                }
            });
        }
        */

        [Fact]
        public void TestSource()
        {
            AtemMockServerWrapper.Each(_output, _pool, SourceCommandHandler, DeviceTestCases.MultiviewRouteInputs, helper =>
            {
                VideoSource[] validSources = helper.Helper.BuildLibState().Settings.Inputs
                    .Where(i => i.Value.Properties.SourceAvailability.HasFlag(SourceAvailability.Multiviewer))
                    .Select(i => i.Key).ToArray();

                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.CanRouteInputs(out int supported);
                    Assert.Equal(1, supported);

                    mv.Item2.SupportsQuadrantLayout(out int supportsQuadrant);
                    int[] windows = Randomiser
                        .SelectionOfGroup(Enumerable.Range(supportsQuadrant == 0 ? 2 : 0, supportsQuadrant == 0 ? 8 : 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        MultiViewerState.WindowState windowState = stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window];

                        //var sampleSources = VideoSourceUtil.TakeSelection(validSources);
                        var sampleSources = Randomiser.SelectionOfGroup(validSources.ToList(), 5);
                        foreach (VideoSource src in sampleSources)
                        {
                            windowState.Source = src;

                            helper.SendAndWaitForChange(stateBefore, () =>
                            {
                                mv.Item2.SetWindowInput((uint)window, (long)src);
                            });
                        }
                    }
                }
            });
        }
        private static IEnumerable<ICommand> SourceCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is MultiviewWindowInputSetCommand inpCmd)
            {
                var previous = previousCommands.Value.OfType<MultiviewWindowInputGetCommand>().Last(a =>
                    a.MultiviewIndex == inpCmd.MultiviewIndex && a.WindowIndex == inpCmd.WindowIndex);
                Assert.NotNull(previous);

                previous.Source = inpCmd.Source;
                yield return previous;
            }
        }

        [Fact]
        public void TestWindowLabelVisible()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewWindowOverlaySetCommand, MultiviewWindowOverlayGetCommand>("LabelVisible", true);

            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewBorders, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    // TODO: re-enable this
                    // mv.Item2.CanChangeOverlayProperties(out int supported);
                    // Assert.Equal(1, supported);

                    int[] windows = Randomiser
                        .SelectionOfGroup(Enumerable.Range(0, 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        MultiViewerState.WindowState windowState = stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window];

                        for (int i = 0; i < 5; i++)
                        {
                            bool newValue = i % 2 == 0;
                            windowState.LabelVisible = newValue;

                            helper.SendAndWaitForChange(stateBefore, () =>
                            {
                                mv.Item2.SetLabelVisible((uint)window, newValue ? 1 : 0);
                            });
                        }
                    }
                }
            });
        }

        [Fact]
        public void TestWindowBorderVisible()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewWindowOverlaySetCommand, MultiviewWindowOverlayGetCommand>("BorderVisible", true);

            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewBorders, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    // TODO: re-enable this
                    // mv.Item2.CanChangeOverlayProperties(out int supported);
                    // Assert.Equal(1, supported);

                    int[] windows = Randomiser
                        .SelectionOfGroup(Enumerable.Range(0, 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        MultiViewerState.WindowState windowState = stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window];

                        for (int i = 0; i < 5; i++)
                        {
                            bool newValue = i % 2 == 0;
                            windowState.BorderVisible = newValue;

                            helper.SendAndWaitForChange(stateBefore, () =>
                            {
                                mv.Item2.SetBorderVisible((uint)window, newValue ? 1 : 0);
                            });
                        }
                    }
                }
            });
        }

        [Fact]
        public void TestBorderColor()
        {
            var expectedCmd = new MultiviewBorderColorGetCommand();
            var handler = CommandGenerator.EchoCommand(expectedCmd);

            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewBorders, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    // TODO: re-enable this
                    // mv.Item2.CanChangeOverlayProperties(out int supported);
                    // Assert.Equal(1, supported);


                    AtemState stateBefore = helper.Helper.BuildLibState();
                    MultiViewerState mvState = stateBefore.Settings.MultiViewers[(int)mv.Item1];

                    for (int i = 0; i < 5; i++)
                    {
                        expectedCmd.MultiviewIndex = mv.Item1;
                        expectedCmd.Red = Randomiser.Range(0, 1, 100);
                        expectedCmd.Green = Randomiser.Range(0, 1, 100);
                        expectedCmd.Blue = Randomiser.Range(0, 1, 100);
                        expectedCmd.Alpha = Randomiser.Range(0, 1, 100);

                        mvState.BorderColor = new MultiViewerState.BorderColorState
                        {
                            Red = expectedCmd.Red,
                            Green = expectedCmd.Green,
                            Blue = expectedCmd.Blue,
                            Alpha = expectedCmd.Alpha
                        };

                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            mv.Item2.SetBorderColor(expectedCmd.Red, expectedCmd.Green, expectedCmd.Blue, expectedCmd.Alpha);
                        });
                        
                    }
                }
            });
        }
    }
}