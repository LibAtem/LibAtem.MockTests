using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
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
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(_client.SdkSwitcher.CreateIterator);

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
        private abstract class MultiviewTestDefinition<T> : TestDefinitionBase<MultiviewPropertiesSetV8Command, T>
        {
            protected readonly uint _id;
            protected readonly IBMDSwitcherMultiView _sdk;

            public MultiviewTestDefinition(AtemComparisonHelper helper, Tuple<uint,  IBMDSwitcherMultiView> mv) : base(helper)
            {
                _id = mv.Item1;
                _sdk = mv.Item2;
            }

            public override void SetupCommand(MultiviewPropertiesSetV8Command cmd)
            {
                cmd.MultiviewIndex = _id;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MultiViewerState.PropertiesState obj = state.Settings.MultiViewers[(int)_id].Properties;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"Settings.MultiViewers.{_id:D}.Properties";
            }
        }

        private class MultiviewLayoutTestDefinition : MultiviewTestDefinition<MultiViewLayoutV8>
        {
            public MultiviewLayoutTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv) : base(helper, mv)
            {
                // mv.Item2.SupportsQuadrantLayout
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLayout(_BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramRight);

            public override string PropertyName => "Layout";
            public override MultiViewLayoutV8 MangleBadValue(MultiViewLayoutV8 v) => v;

            public override MultiViewLayoutV8[] GoodValues
            {
                get
                {
                    var rawValues = Enum.GetValues(typeof(MultiViewLayoutV8)).OfType<MultiViewLayoutV8>();

                    _sdk.SupportsQuadrantLayout(out int supportsQuad);
                    if (supportsQuad == 0)
                    {
                        // Only non-power of 2
                        rawValues = rawValues.Where(x => x != 0 && (x & (x - 1)) != 0);
                    }

                    return rawValues.ToArray();
                }
            }
            public override MultiViewLayoutV8[] BadValues => Enum.GetValues(typeof(MultiViewLayoutV8)).OfType<MultiViewLayoutV8>().Except(GoodValues).ToArray();

            public override void UpdateExpectedState(AtemState state, bool goodValue, MultiViewLayoutV8 v)
            {
                if (goodValue)
                {
                    base.UpdateExpectedState(state, goodValue, v);
                }
            }
        }

        [Fact]
        public void TestLayout()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetMultiviewers().ForEach(k => new MultiviewLayoutTestDefinition(helper, k).Run());
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

        private class MultiviewProgramPreviewSwappedTestDefinition : MultiviewTestDefinition<bool>
        {
            public MultiviewProgramPreviewSwappedTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv) : base(helper, mv)
            {
                mv.Item2.SupportsProgramPreviewSwap(out int canTest);
                Skip.If(canTest == 0, "Model does not support Multiview.ProgramPreviewSwapped");
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetProgramPreviewSwapped(0);

            public override string PropertyName => "ProgramPreviewSwapped";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                var props = state.Settings.MultiViewers[(int)_id];
                props.Properties.ProgramPreviewSwapped = v;

                var tmp = props.Windows[0].Source;
                props.Windows[0].Source = props.Windows[1].Source;
                props.Windows[1].Source = tmp;

                var tmp2 = props.Windows[0].SupportsVuMeter;
                props.Windows[0].SupportsVuMeter = props.Windows[1].SupportsVuMeter;
                props.Windows[1].SupportsVuMeter = tmp2;
            }
        }

        [SkippableFact]
        public void TestProgramPreviewSwapped()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetMultiviewers().ForEach(k => new MultiviewProgramPreviewSwappedTestDefinition(helper, k).Run());
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

        /*
        private class MultiviewToggleSafeAreaTestDefinition : MultiviewTestDefinition<bool>
        {
            public MultiviewToggleSafeAreaTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv) : base(helper, mv)
            {
                mv.Item2.SupportsProgramPreviewSwap(out int canTest);
                Skip.If(canTest == 0, "Model does not support Multiview.ProgramPreviewSwapped");
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSafeAreaEnabled(0);

            public override string PropertyName => "SafeAreaEnabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestToggleSafeArea()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetMultiviewers().ForEach(k => new MultiviewToggleSafeAreaTestDefinition(helper, k).Run());
        }*/

        private class MultiviewUnroutableWindowSourcesTestDefinition : TestDefinitionBase<MultiviewWindowInputSetCommand, VideoSource>
        {
            private readonly uint _id;
            private readonly uint _window;
            private readonly IBMDSwitcherMultiView _sdk;
            private readonly bool _quick;

            public MultiviewUnroutableWindowSourcesTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv, uint window, bool quick) : base(helper)
            {
                _id = mv.Item1;
                _window = window;
                _sdk = mv.Item2;
                _quick = quick;
            }

            public override void Prepare()
            {
            }

            public override string PropertyName => "Source";

            public override void SetupCommand(MultiviewWindowInputSetCommand cmd)
            {
                cmd.MultiviewIndex = _id;
                cmd.WindowIndex = _window;
            }

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
            }

            public override VideoSource[] GoodValues => new VideoSource[0];
            public override VideoSource[] BadValues
            {
                get
                {
                    if (_quick)
                        return new VideoSource[] { VideoSource.ColorBars, VideoSource.Input1 };
                    return VideoSourceLists.All.ToArray();
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                yield break;
            }
        }

        private class MultiviewPvwPgmSetVuMeterTestDefinition : TestDefinitionBase<MultiviewWindowVuMeterSetCommand, bool>
        {
            private readonly uint _id;
            private readonly uint _window;
            private readonly IBMDSwitcherMultiView _sdk;

            public MultiviewPvwPgmSetVuMeterTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv, uint window) : base(helper)
            {
                _id = mv.Item1;
                _window = window;
                _sdk = mv.Item2;
            }

            public override void Prepare()
            {
            }

            public override string PropertyName => "VuEnabled";

            public override void SetupCommand(MultiviewWindowVuMeterSetCommand cmd)
            {
                cmd.MultiviewIndex = _id;
                cmd.WindowIndex = _window;
            }

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                _sdk.SupportsVuMeters(out int supportVuMeters);
                if (supportVuMeters != 0)
                {
                    var mv = state.Settings.MultiViewers[(int)_id];
                    mv.Windows[(int)_window].VuMeter = mv.Properties.ProgramPreviewSwapped ? (_window == 0) && v : (_window == 1) && v;
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, bool v)
            {
                yield break;
            }
        }
        private class MultiviewRoutableWindowSourcesTestDefinition : TestDefinitionBase<MultiviewWindowInputSetCommand, VideoSource>
        {
            private readonly uint _id;
            private readonly uint _window;
            private readonly IBMDSwitcherMultiView _sdk;
            private readonly bool _quick;

            public MultiviewRoutableWindowSourcesTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv, uint window, bool quick) : base(helper)
            {
                _id = mv.Item1;
                _window = window;
                _sdk = mv.Item2;
                _quick = quick;
            }

            public override string PropertyName => "Source";

            public override void SetupCommand(MultiviewWindowInputSetCommand cmd)
            {
                cmd.MultiviewIndex = _id;
                cmd.WindowIndex = _window;
            }

            public override void Prepare() => _sdk.SetWindowInput(_window, (long)VideoSource.ColorBars);

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.Settings.MultiViewers[(int)_id].Windows[(int)_window].Source = v;
            }

            public override VideoSource[] GoodValues
            {
                get
                {
                    var ignorePortTypes = new List<InternalPortType>();
                    ignorePortTypes.Add(InternalPortType.Mask); // TODO - dynamic based on model

                    if (_quick)
                        return new[] { VideoSource.Black, VideoSource.ColorBars };
                    return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, ignorePortTypes.ToArray()) && s.IsAvailable(SourceAvailability.Multiviewer)).ToArray();
                }
            }
            public override VideoSource[] BadValues
            {
                get
                {
                    if (_quick)
                        return new VideoSource[0];
                    return base.BadValues;
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return $"Settings.MultiViewers.{_id:D}.Windows.{_window:D}";
            }
        }
        private class MultiviewRoutableWindowVuMeterTestDefinition : TestDefinitionBase<MultiviewWindowVuMeterSetCommand, bool>
        {
            private readonly uint _id;
            private readonly uint _window;
            private readonly IBMDSwitcherMultiView _sdk;

            public MultiviewRoutableWindowVuMeterTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv, uint window) : base(helper)
            {
                _id = mv.Item1;
                _window = window;
                _sdk = mv.Item2;
            }

            public override void Prepare() => _sdk.SetWindowInput(_window, (long)VideoSource.Input1);

            public override string PropertyName => "VuEnabled";

            public override void SetupCommand(MultiviewWindowVuMeterSetCommand cmd)
            {
                cmd.MultiviewIndex = _id;
                cmd.WindowIndex = _window;
            }

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                _sdk.SupportsVuMeters(out int supportsVuMeter);
                if (supportsVuMeter != 0) _sdk.CurrentInputSupportsVuMeter(_window, out supportsVuMeter);

                state.Settings.MultiViewers[(int)_id].Windows[(int)_window].VuMeter = supportsVuMeter != 0 ? v : false;
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, bool v)
            {
                yield break;
            }
        }

        [Fact]
        public void TestMultiviewSources()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Skip.If(_client.SdkState.Settings.MultiViewers.Count == 0, "Model does not support Multiviewers");

                uint windowCount = (uint)_client.SdkState.Settings.MultiViewers.First().Windows.Count;
                uint unroutableWindows = _client.Profile.MultiView.CanRouteInputs ? 2 : windowCount;
                if (windowCount > 10)
                    unroutableWindows = 0;

                var multiViewers = GetMultiviewers();
                {
                    Tuple<uint, IBMDSwitcherMultiView> mv = multiViewers.First();

                    mv.Item2.CanRouteInputs(out int canRouteInputs);
                    Assert.Equal(canRouteInputs != 0, _client.Profile.MultiView.CanRouteInputs);                        
                    
                    // Pvw/Pgm/unroutable windows
                    for (uint i = 0; i < unroutableWindows; i++)
                    {
                        new MultiviewUnroutableWindowSourcesTestDefinition(helper, mv, i, false).Run();
                        new MultiviewPvwPgmSetVuMeterTestDefinition(helper, mv, i).Run();
                    }

                    // Quick test for every routable window (we assume they are all equal)
                    for (uint i = unroutableWindows; i < windowCount; i++)
                    {
                        new MultiviewRoutableWindowSourcesTestDefinition(helper, mv, i, true).Run();
                        new MultiviewRoutableWindowVuMeterTestDefinition(helper, mv, i).Run();
                    }

                    // Run full test for one window
                    const uint SampleWindow = 5;
                    new MultiviewRoutableWindowSourcesTestDefinition(helper, mv, SampleWindow, false).Run();
                    // TODO - CurrentInputSupportsVuMeter 
                }

                // Now quickly check everything else
                foreach (Tuple<uint, IBMDSwitcherMultiView> mv in multiViewers.Skip(1))
                {
                    VideoSource[] badValuesPvwPgm = new[] { VideoSource.Input1 };
                    
                    // Pvw/Pgm/unroutable windows
                    for (uint i = 0; i < unroutableWindows; i++)
                    {
                        new MultiviewUnroutableWindowSourcesTestDefinition(helper, mv, i, true).Run();
                        new MultiviewPvwPgmSetVuMeterTestDefinition(helper, mv, i).Run();
                    }
                    
                    // Quick test for every routable window (we assume they are all equal)
                    for (uint i = unroutableWindows; i < windowCount; i++)
                    {
                        new MultiviewRoutableWindowSourcesTestDefinition(helper, mv, i, true).Run();
                        new MultiviewRoutableWindowVuMeterTestDefinition(helper, mv, i).Run();
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

        private class MultiviewVuOpacityTestDefinition : TestDefinitionBase<MultiviewVuOpacityCommand, double>
        {
            private readonly uint _id;
            private readonly IBMDSwitcherMultiView _sdk;

            public MultiviewVuOpacityTestDefinition(AtemComparisonHelper helper, Tuple<uint, IBMDSwitcherMultiView> mv) : base(helper)
            {
                _id = mv.Item1;
                _sdk = mv.Item2;

                mv.Item2.SupportsVuMeters(out int canTest);
                Skip.If(canTest == 0, "Model does not support Multiview.SupportsVuMeters");
            }

            public override void Prepare()
            {
            }

            public override string PropertyName => "Opacity";

            public override void SetupCommand(MultiviewVuOpacityCommand cmd)
            {
                cmd.MultiviewIndex = _id;
            }
            
            public override void UpdateExpectedState(AtemState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.Settings.MultiViewers[(int)_id].VuMeterOpacity = v;
                else
                    state.Settings.MultiViewers[(int)_id].VuMeterOpacity = v <= 10 && v >= -0.1 ? 10 : 100;
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, double v)
            {
                yield return $"Settings.MultiViewers.{_id:D}";
            }

            public override double[] GoodValues => new double[] { 10, 87, 14, 99, 100, 11 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -1, -10, 9 };
        }
        
        [SkippableFact]
        public void TestMultiviewVuMeterOpacity()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetMultiviewers().ForEach(k => new MultiviewVuOpacityTestDefinition(helper, k).Run());
        }
    }
}