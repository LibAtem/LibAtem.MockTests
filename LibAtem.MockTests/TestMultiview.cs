using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;
using AtemSDKConverter = LibAtem.ComparisonTests.AtemSDKConverter;

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
                                mv.Item2.SetLayout((_BMDSwitcherMultiViewLayout) newValue);
                            });
                    }
                }
            });
        }

        [Fact(Skip = "No supporting model captures")]
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

                    for (int i = 0; i < 5; i++)
                    {
                        bool newValue = i % 2 != 0;
                        stateBefore.Settings.MultiViewers[(int)mv.Item1].Properties.ProgramPreviewSwapped = newValue;
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
                        ? new[] {0, 1}
                        : Randomiser.SelectionOfGroup(Enumerable.Range(0, 16).ToList()).ToArray();

                    foreach (int window in windows)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        for (int i = 0; i < 5; i++)
                        {
                            bool newValue = i % 2 == 0;
                            stateBefore.Settings.MultiViewers[(int) mv.Item1].Windows[window].SafeAreaEnabled = newValue;

                            helper.SendAndWaitForChange(stateBefore,
                                () => { mv.Item2.SetSafeAreaEnabled((uint) window, newValue ? 1 : 0); });
                        }
                    }
                }
            });
        }

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
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewToggleSafeArea, helper =>
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in GetMultiviewers(helper))
                {
                    mv.Item2.SupportsVuMeters(out int supportsVu);
                    Assert.Equal(1, supportsVu);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    for (int i = 0; i < 5; i++)
                    {
                        double newValue = Randomiser.Range(0, 100);
                        stateBefore.Settings.MultiViewers[(int) mv.Item1].VuMeterOpacity = newValue;

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
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MultiviewToggleSafeArea, helper =>
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
                            stateBefore.Settings.MultiViewers[(int)mv.Item1].Windows[window].VuMeter = newValue;

                            helper.SendAndWaitForChange(stateBefore,
                                () => { mv.Item2.SetVuMeterEnabled((uint)window, newValue ? 1 : 0); });
                        }
                    }
                }
            });
        }

        /*
        [Fact]
        public void TestSource()
        {
            AtemMockServerWrapper.Each(_output, _pool, SourceCommandHandler, DeviceTestCases.All, helper =>
            {
                List<Tuple<uint, IBMDSwitcherMultiView>> multiviewers = GetMultiviewers(helper);

                foreach (va auxSource in chosenIds)
                {
                    AuxiliaryId auxId = AtemEnumMaps.GetAuxId(auxSource);
                    IBMDSwitcherInputAux aux = allAuxes[auxSource];

                    // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                    // We track this another way
                    aux.GetInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                    Assert.Equal(availabilityMask, (_BMDSwitcherInputAvailability)((int)SourceAvailability.Auxiliary << 2));

                    AtemState stateBefore = helper.Helper.BuildLibState();

                    List<VideoSource> deviceSources = stateBefore.Settings.Inputs.Keys.ToList();

                    VideoSource[] validSources = deviceSources.Where(s =>
                        s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask) &&
                        s.IsAvailable(SourceAvailability.Auxiliary)).ToArray();
                    var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                    foreach (VideoSource src in sampleSources)
                    {
                        stateBefore.Auxiliaries[(int)auxId].Source = src;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            aux.SetInputSource((long)src);
                        });
                    }
                }
            });
        }
        private static IEnumerable<ICommand> SourceCommandHandler(ImmutableList<ICommand> previousCommands, ICommand cmd)
        {
            if (cmd is MultiviewWindowInputSetCommand inpCmd)
            {
                var previous = previousCommands.OfType<MultiviewWindowInputGetCommand>().Last(a =>
                    a.MultiviewIndex == inpCmd.MultiviewIndex && a.WindowIndex == inpCmd.WindowIndex);
                Assert.NotNull(previous);

                previous.Source = inpCmd.Source;
                yield return previous;
            }
        }
        */

    }
}