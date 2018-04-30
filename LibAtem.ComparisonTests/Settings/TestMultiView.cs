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
                    }

                    bool[] newVals = { true, false };
                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, newVals);
                }
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
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    VideoSource[] badValuesPvwPgm = VideoSourceLists.All.ToArray();
                    // TODO - 2me4k can route mask types to multiviewer
                    VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.Multiviewer)).ToArray();
                    VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                    uint unroutableWindows = _client.Profile.MultiView.CanRouteInputs ? 2 : Constants.MultiViewWindowCount;
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
                    
                    // Routable windows
                    for (uint i = unroutableWindows; i < Constants.MultiViewWindowCount; i++)
                    {
                        ICommand Setter(VideoSource v) => new MultiviewWindowInputSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            Source = v,
                        };

                        void UpdateExpectedState(ComparisonState state, VideoSource v) => state.Settings.MultiViews[sdkProps.Item1].Windows[(int)i].Source = v;

                        ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                        ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValues);
                    }
                }
            }
        }
        
        [Fact]
        public void TestMultiviewVuMeter()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    sdkProps.Item2.SupportsVuMeters(out int supportsVu);
                    Assert.Equal(helper.Profile.MultiView.VuMeters, (supportsVu != 0));

                    // TODO - implement these
                    //GetVuMeterEnabled Check if the VU meter is currently visible on a specified window.
                    //GetVuMeterOpacity Get the current MultiView VU meter opacity.
                }
            }
        }

        //        [Fact]
        //        public void TestWindowInputVuMeter()
        //        {
        //            // TODO - before this is done, all of VideoSource will need annotating with whther they support vu meter or not.
        //            // It will also require a device which supports vu meters
        //            //CurrentInputSupportsVuMeter Check if the current input of a specified MultiView window supports VU meters.
        //        }
    }
}