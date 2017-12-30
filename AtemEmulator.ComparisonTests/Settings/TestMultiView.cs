using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.XmlState;
using Xunit;
using Xunit.Abstractions;
using MultiView = LibAtem.XmlState.Settings.MultiView;

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
                Assert.Equal(MultiView.WindowCount, count);
            }
        }

        [Fact]
        public void TestMultiviewLayout()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    MultiViewLayout? Getter() => helper.FindWithMatching(new MultiviewPropertiesGetCommand {MultiviewIndex = sdkProps.Item1})?.Layout;

                    ICommand Setter(MultiViewLayout v) => new MultiviewPropertiesSetCommand
                    {
                        MultiviewIndex = sdkProps.Item1,
                        Mask = MultiviewPropertiesSetCommand.MaskFlags.Layout,
                        Layout = v,
                    };

                    MultiViewLayout[] newVals = Enum.GetValues(typeof(MultiViewLayout)).OfType<MultiViewLayout>().ToArray();

                    EnumValueComparer<MultiViewLayout, _BMDSwitcherMultiViewLayout>.Run(helper, LayoutMap, Setter, sdkProps.Item2.GetLayout, Getter, newVals);
                }
            }
        }
        
        [Fact]
        public void TestMultiviewProgramPreviewSwapped()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    sdkProps.Item2.SupportsProgramPreviewSwap(out int canTest);
                    if (canTest == 0)
                        continue;

                    bool? Getter() => helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = sdkProps.Item1 })?.ProgramPreviewSwapped;

                    ICommand Setter(bool v) => new MultiviewPropertiesSetCommand
                    {
                        MultiviewIndex = sdkProps.Item1,
                        Mask = MultiviewPropertiesSetCommand.MaskFlags.ProgramPreviewSwapped,
                        ProgramPreviewSwapped = v,
                    };

                    bool[] newVals = {true, false};

                    BoolValueComparer.Run(helper, Setter, sdkProps.Item2.GetProgramPreviewSwapped, Getter, newVals);
                }
            }
        }

        [Fact]
        public void TestMultiviewToggleSafeArea()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    sdkProps.Item2.CanToggleSafeAreaEnabled(out int canTest);
                    if (canTest == 0)
                        continue;

                    bool? Getter() => helper.FindWithMatching(new MultiviewPropertiesGetCommand { MultiviewIndex = sdkProps.Item1 })?.SafeAreaEnabled;

                    ICommand Setter(bool v) => new MultiviewPropertiesSetCommand
                    {
                        MultiviewIndex = sdkProps.Item1,
                        Mask = MultiviewPropertiesSetCommand.MaskFlags.SafeAreaEnabled,
                        SafeAreaEnabled = v,
                    };

                    bool[] newVals = { true, false };

                    BoolValueComparer.Run(helper, Setter, sdkProps.Item2.GetSafeAreaEnabled, Getter, newVals);
                }
            }
        }

        [Fact]
        public void TestMultiviewSources()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach (Tuple<uint, IBMDSwitcherMultiView> sdkProps in GetMultiviewers())
                {
                    long[] badValuesPvwPgm = VideoSourceLists.All.Select(s => (long)s).ToArray();
                    long[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.Multiviewer)).Select(s => (long)s).ToArray();
                    long[] badValues = VideoSourceLists.All.Select(s => (long)s).Where(s => !testValues.Contains(s)).ToArray();

                    uint unroutableWindows = _client.Profile.MultiView.CanRouteInputs ? 2 : MultiView.WindowCount;
                    // Pvw/Pgm/unroutable windows
                    for (uint i = 0; i < unroutableWindows; i++)
                    {
                        ICommand Setter(long v) => new MultiviewWindowInputSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            Source = (VideoSource)v,
                        };

                        long? Getter() => (long?)helper.FindWithMatching(new MultiviewWindowInputGetCommand { MultiviewIndex = sdkProps.Item1, WindowIndex = i })?.Source;

                        void SdkGetter(out long src) => sdkProps.Item2.GetWindowInput(i, out src);

                        ValueTypeComparer<long>.Fail(helper, Setter, SdkGetter, Getter, badValuesPvwPgm);
                    }
                    
                    // Routable windows
                    for (uint i = unroutableWindows; i < MultiView.WindowCount; i++)
                    {
                        ICommand Setter(long v) => new MultiviewWindowInputSetCommand()
                        {
                            MultiviewIndex = sdkProps.Item1,
                            WindowIndex = i,
                            Source = (VideoSource) v,
                        };

                        long? Getter() => (long?) helper.FindWithMatching(new MultiviewWindowInputGetCommand {MultiviewIndex = sdkProps.Item1, WindowIndex = i})?.Source;

                        void SdkGetter(out long src)
                        {
                            sdkProps.Item2.GetWindowInput(i, out src);
                        }

                        ValueTypeComparer<long>.Run(helper, Setter, SdkGetter, Getter, testValues);
                        ValueTypeComparer<long>.Fail(helper, Setter, SdkGetter, Getter, badValues);
                    }
                }
            }
        }
        
        [Fact]
        public void TestMultiviewVuMeter()
        {
            using (var helper = new AtemComparisonHelper(_client))
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