using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.ComparisonTests2.MixEffects;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.Audio
{
    [Collection("Client")]
    public class TestAudioMonitor
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestAudioMonitor(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        protected List<IBMDSwitcherAudioMonitorOutput> GetMonitors()
        {
            var mixer = _client.SdkSwitcher as IBMDSwitcherAudioMixer;
            Assert.NotNull(mixer);

            Guid itId = typeof(IBMDSwitcherAudioMonitorOutputIterator).GUID;
            mixer.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherAudioMonitorOutputIterator iterator = (IBMDSwitcherAudioMonitorOutputIterator)Marshal.GetObjectForIUnknown(itPtr);

            var result = new List<IBMDSwitcherAudioMonitorOutput>();
            for (iterator.Next(out IBMDSwitcherAudioMonitorOutput r); r != null; iterator.Next(out r))
                result.Add(r);

            return result;
        }

        protected IBMDSwitcherAudioMonitorOutput GetMonitor()
        {
            // We are only prepared for up to one, so fail if there are more
            var monitor = GetMonitors().SingleOrDefault();
            Skip.If(monitor == null, "Model does not support monitor");
            return monitor;
        }

        [Fact]
        public void TestCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                var sdkCount = GetMonitors().Count;
                Assert.Equal(sdkCount, _client.LibState.Audio.Monitors.Count);

                helper.AssertStatesMatch();
            }
        }

        private abstract class AudioMonitorTestDefinition<T> : TestDefinitionBase<AudioMixerMonitorSetCommand, T>
        {
            protected readonly IBMDSwitcherAudioMonitorOutput _sdk;

            public AudioMonitorTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                AudioState.MonitorOutputState obj = state.Audio.Monitors[0];
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new AudioMixerMonitorGetCommand());
            }
        }

        private class AudioMonitorEnabledTestDefinition : AudioMonitorTestDefinition<bool>
        {
            public AudioMonitorEnabledTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetMonitorEnable(0);

            public override string PropertyName => "Enabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new AudioMonitorEnabledTestDefinition(helper, GetMonitor()).Run();
        }

        private class AudioMonitorMuteTestDefinition : AudioMonitorTestDefinition<bool>
        {
            public AudioMonitorMuteTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetMute(0);

            public override string PropertyName => "Mute";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestMute()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new AudioMonitorMuteTestDefinition(helper, GetMonitor()).Run();
        }

        private class AudioMonitorDimTestDefinition : AudioMonitorTestDefinition<bool>
        {
            public AudioMonitorDimTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetDim(0);

            public override string PropertyName => "Dim";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestDim()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor();
                Skip.If(monitor == null, "Model does not support monitor");

                new AudioMonitorDimTestDefinition(helper, monitor).Run();
            }
        }

        private class AudioMonitorSoloTestDefinition : AudioMonitorTestDefinition<bool>
        {
            public AudioMonitorSoloTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSolo(0);

            public override string PropertyName => "Solo";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestSolo()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new AudioMonitorSoloTestDefinition(helper, GetMonitor()).Run();
        }

        private class AudioMonitorGainTestDefinition : AudioMonitorTestDefinition<double>
        {
            public AudioMonitorGainTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "Gain";
            public override double MangleBadValue(double v) => v;

            public override double[] GoodValues => new double[] { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };
        }

        [SkippableFact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new AudioMonitorGainTestDefinition(helper, GetMonitor()).Run();
        }

        private class AudioMonitorSoloInputTestDefinition : AudioMonitorTestDefinition<AudioSource>
        {
            public AudioMonitorSoloInputTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSoloInput((long)AudioSource.XLR);

            public override string PropertyName => "SoloSource";
            public override void UpdateExpectedState(AtemState state, bool goodValue, AudioSource v)
            {
                AudioState.MonitorOutputState obj = state.Audio.Monitors[0];
                if (goodValue)
                {
                    SetCommandProperty(obj, PropertyName, v);
                }
            }

            public override AudioSource MangleBadValue(AudioSource v) => v;

            public override AudioSource[] GoodValues =>  _helper.LibState.Audio.Inputs.Keys.Select(i => (AudioSource)i).ToArray();
            public override AudioSource[] BadValues => Enum.GetValues(typeof(AudioSource)).OfType<AudioSource>().Except(GoodValues).ToArray();
        }

        [SkippableFact]
        public void TestSoloInput()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new AudioMonitorSoloInputTestDefinition(helper, GetMonitor()).Run();
        }
    }
}
