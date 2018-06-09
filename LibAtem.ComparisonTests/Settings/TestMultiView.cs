using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Settings
{
    [Collection("Client")]
    public class TestMultiView
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestMultiView(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        private List<Tuple<uint, IBMDSwitcherMultiView>> GetMultiviewers()
        {
            Guid itId = typeof(IBMDSwitcherMultiViewIterator).GUID;
            _client.SdkSwitcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMultiViewIterator
                iterator = (IBMDSwitcherMultiViewIterator) Marshal.GetObjectForIUnknown(itPtr);

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
        public void TestMultiviewCount()
        {
            List<Tuple<uint, IBMDSwitcherMultiView>> multiviewers = GetMultiviewers();
            Assert.Equal((int) _client.Profile.MultiView.Count, multiviewers.Count);
        }

        [Fact]
        public void TestMultiviewWindowCount()
        {
            foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
            {
                sdkProps.Item2.GetWindowCount(out uint count);
                Assert.Equal(Constants.MultiViewWindowCount, count);
            }
        }

        [Fact]
        public void TestMultiviewLayout()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    ICommand Setter(MultiViewLayout v) => new MultiviewPropertiesSetCommand
                    {
                        MultiviewIndex = sdkProps.Item1,
                        Mask = MultiviewPropertiesSetCommand.MaskFlags.Layout,
                        Layout = v,
                    };

                    void UpdateExpectedState(ComparisonState state, MultiViewLayout v) => state.Settings.MultiViews[sdkProps.Item1].Layout = v;

                    MultiViewLayout[] newVals = Enum.GetValues(typeof(MultiViewLayout)).OfType<MultiViewLayout>().ToArray();
                    ValueTypeComparer<MultiViewLayout>.Run(helper, Setter, UpdateExpectedState, newVals);
                }
            }
        }

        [Fact]
        public void TestSupportsProgramPreviewSwap()
        {
            foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
            {
                sdkProps.Item2.SupportsProgramPreviewSwap(out int canToggle);
                Assert.Equal(_client.Profile.MultiView.CanSwapPreviewProgram, canToggle == 1);
            }
        }

        [Fact]
        public void TestMultiviewProgramPreviewSwapped()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    sdkProps.Item2.SupportsProgramPreviewSwap(out int canTest);
                    if (canTest == 0)
                        continue;

                    ICommand Setter(bool v) => new MultiviewPropertiesSetCommand
                    {
                        MultiviewIndex = sdkProps.Item1,
                        Mask = MultiviewPropertiesSetCommand.MaskFlags.ProgramPreviewSwapped,
                        ProgramPreviewSwapped = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        var props = state.Settings.MultiViews[sdkProps.Item1];
                        props.ProgramPreviewSwapped = v;

                        var tmp = props.Windows[0].Source;
                        props.Windows[0].Source = props.Windows[1].Source;
                        props.Windows[1].Source = tmp;

                        var tmp2 = props.Windows[0].SupportsVuMeter;
                        props.Windows[0].SupportsVuMeter = props.Windows[1].SupportsVuMeter;
                        props.Windows[1].SupportsVuMeter = tmp2;
                    }

                    bool[] newVals = { true, false };
                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, newVals);
                }
            }
        }

        [Fact]
        public void TestCanToggleSafeArea()
        {
            foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
            {
                sdkProps.Item2.CanToggleSafeAreaEnabled(out int canToggle);
                Assert.Equal(_client.Profile.MultiView.CanToggleSafeArea, canToggle == 1);
            }
        }

        [Fact]
        public void TestMultiviewToggleSafeArea()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    sdkProps.Item2.CanToggleSafeAreaEnabled(out int canTest);
                    Assert.Equal(helper.Profile.MultiView.CanToggleSafeArea, (canTest != 0));
                    if (canTest == 0)
                        continue;

                    ICommand Setter(bool v) => new MultiviewPropertiesSetCommand
                    {
                        MultiviewIndex = sdkProps.Item1,
                        Mask = MultiviewPropertiesSetCommand.MaskFlags.SafeAreaEnabled,
                        SafeAreaEnabled = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.Settings.MultiViews[sdkProps.Item1].SafeAreaEnabled = v;

                    bool[] newVals = { true, false };
                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, newVals);
                }
            }
        }

        [Fact]
        public void TestMultiviewSources()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                uint unroutableWindows = _client.Profile.MultiView.CanRouteInputs ? 2 : Constants.MultiViewWindowCount;

                var multiViewers = GetMultiviewers();
                {
                    Tuple<uint, IBMDSwitcherMultiView> sdkProps = multiViewers.First();

                    sdkProps.Item2.CanRouteInputs(out int canRouteInputs);
                    Assert.True((canRouteInputs != 0) == _client.Profile.MultiView.CanRouteInputs);                        

                    VideoSource[] badValuesPvwPgm = VideoSourceLists.All.ToArray();
                    // TODO - 2me4k can route mask types to multiviewer
                    VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile/*, InternalPortType.Mask*/) && s.IsAvailable(SourceAvailability.Multiviewer)).ToArray();
                    VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                    // Pvw/Pgm/unroutable windows
                    for (uint i = 0; i < unroutableWindows; i++)
                    {
                        ICommand Setter(VideoSource v) => new MultiviewWindowInputSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            Source = v,
                        };

                        ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValuesPvwPgm);

                        // Vu meter enabled
                        ICommand VuSetter(bool v) => new MultiviewWindowVuMeterSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            VuEnabled = v,
                        };

                        void UpdateExpectedVuState(ComparisonState state, bool v)
                        {
                            var mv = state.Settings.MultiViews[sdkProps.Item1];
                            mv.Windows[(int)i].VuMeter = mv.ProgramPreviewSwapped ? (i == 0) && v : (i == 1) && v;
                        }

                        ValueTypeComparer<bool>.Run(helper, VuSetter, UpdateExpectedVuState, new[] { true, false });
                    }

                    // Quick test for every routable window (we assume they are all equal)
                    for (uint i = unroutableWindows; i < Constants.MultiViewWindowCount; i++)
                    {
                        ICommand Setter(VideoSource v) => new MultiviewWindowInputSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            Source = v,
                        };

                        void UpdateExpectedState(ComparisonState state, VideoSource v)
                        {
                            var window = state.Settings.MultiViews[sdkProps.Item1].Windows[(int)i];
                            window.SupportsVuMeter = v.SupportsVuMeter();
                            window.Source = v;
                        }

                        ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, new[] { VideoSource.Black, VideoSource.ColorBars });

                        // Set vumeter enabled/disabled
                        {
                            sdkProps.Item2.SetWindowInput(i, (long)VideoSource.Input1);
                            helper.Sleep();

                            ICommand VuSetter(bool v) => new MultiviewWindowVuMeterSetCommand()
                            {
                                MultiviewIndex = sdkProps.Item1,
                                WindowIndex = i,
                                VuEnabled = v,
                            };

                            sdkProps.Item2.CurrentInputSupportsVuMeter(i, out int supportsVuMeter);
                            void UpdateExpectedVuState(ComparisonState state, bool v) => state.Settings.MultiViews[sdkProps.Item1].Windows[(int)i].VuMeter = supportsVuMeter != 0 ? v : false;

                            ValueTypeComparer<bool>.Run(helper, VuSetter, UpdateExpectedVuState, new[] { true, false });
                        }
                    }

                    // Run full test for one window
                    const uint SampleWindow = 5;
                    ICommand Setter2(VideoSource v) => new MultiviewWindowInputSetCommand()
                    {
                        MultiviewIndex = sdkProps.Item1,
                        WindowIndex = SampleWindow,
                        Source = v,
                    };

                    // TODO - CurrentInputSupportsVuMeter 

                    void UpdateExpectedState2(ComparisonState state, VideoSource v)
                    {
                        var win = state.Settings.MultiViews[sdkProps.Item1].Windows[(int)SampleWindow];
                        win.Source = v;
                        win.SupportsVuMeter = v.SupportsVuMeter();
                    }

                    ValueTypeComparer<VideoSource>.Run(helper, Setter2, UpdateExpectedState2, testValues);
                    ValueTypeComparer<VideoSource>.Fail(helper, Setter2, badValues);
                }

                // Now quickly check everything else
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in multiViewers.Skip(1))
                {
                    VideoSource[] badValuesPvwPgm = new[] { VideoSource.Input1 };
                    
                    // Pvw/Pgm/unroutable windows
                    for (uint i = 0; i < unroutableWindows; i++)
                    {
                        ICommand Setter(VideoSource v) => new MultiviewWindowInputSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            Source = v,
                        };

                        ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValuesPvwPgm);
                    }
                    
                    // Quick test for every routable window (we assume they are all equal)
                    for (uint i = unroutableWindows; i < Constants.MultiViewWindowCount; i++)
                    {
                        ICommand Setter(VideoSource v) => new MultiviewWindowInputSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            Source = v,
                        };

                        void UpdateExpectedState(ComparisonState state, VideoSource v)
                        {
                            var win = state.Settings.MultiViews[sdkProps.Item1].Windows[(int)i];
                            win.Source = v;
                            win.SupportsVuMeter = v.SupportsVuMeter();
                        }

                        ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, new[] { VideoSource.Black, VideoSource.ColorBars });
                    }
                }
            }
        }
        
        [Fact]
        public void TestSupportsVuMeters()
        {
            foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
            {
                sdkProps.Item2.SupportsVuMeters(out int supports);
                Assert.Equal(_client.Profile.MultiView.VuMeters, supports == 1);
            }
        }
        
        [Fact]
        public void TestMultiviewVuMeterOpacity()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    sdkProps.Item2.SupportsVuMeters(out int supportsVu);
                    if (supportsVu == 0)
                        continue;

                    double[] testValues = { 10, 87, 14, 99, 100, 11 };
                    double[] badValues = { 100.1, 110, 101, -1, -10, 9 };

                    ICommand Setter(double v) => new MultiviewVuOpacityCommand()
                    {
                        MultiviewIndex = sdkProps.Item1,
                        Opacity = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.Settings.MultiViews[sdkProps.Item1].VuMeterOpacity = v;
                    void UpdateBadState(ComparisonState state, double v) => state.Settings.MultiViews[sdkProps.Item1].VuMeterOpacity = v <= 10 && v >= -0.1 ? 10 : 100;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateBadState, badValues);
                }
            }
        }
    }
}