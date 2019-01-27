using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.ComparisonTests2.MixEffects;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
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
            return GetMonitors().SingleOrDefault();
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

        private class AudioMonitorEnabledTestDefinition : TestDefinitionBase<bool>
        {
            protected readonly IBMDSwitcherAudioMonitorOutput _sdk;

            public AudioMonitorEnabledTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetMonitorEnable(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new AudioMixerMonitorSetCommand
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Enabled,
                    Enabled = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.Audio.Monitors[0].Enabled = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, bool v)
            {
                yield return new CommandQueueKey(new AudioMixerMonitorGetCommand());
            }
        }

        [SkippableFact]
        public void TestEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor();
                Skip.If(monitor == null, "Model does not support monitor");

                new AudioMonitorEnabledTestDefinition(helper, monitor).Run();
            }
        }

        private class AudioMonitorMuteTestDefinition : TestDefinitionBase<bool>
        {
            protected readonly IBMDSwitcherAudioMonitorOutput _sdk;

            public AudioMonitorMuteTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetMute(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new AudioMixerMonitorSetCommand
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Mute,
                    Mute = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.Audio.Monitors[0].Mute = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, bool v)
            {
                yield return new CommandQueueKey(new AudioMixerMonitorGetCommand());
            }
        }

        [SkippableFact]
        public void TestMute()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor();
                Skip.If(monitor == null, "Model does not support monitor");

                new AudioMonitorMuteTestDefinition(helper, monitor).Run();
            }
        }

        private class AudioMonitorDimTestDefinition : TestDefinitionBase<bool>
        {
            protected readonly IBMDSwitcherAudioMonitorOutput _sdk;

            public AudioMonitorDimTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetDim(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new AudioMixerMonitorSetCommand
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Dim,
                    Dim = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.Audio.Monitors[0].Dim = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, bool v)
            {
                yield return new CommandQueueKey(new AudioMixerMonitorGetCommand());
            }
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

        private class AudioMonitorSoloTestDefinition : TestDefinitionBase<bool>
        {
            protected readonly IBMDSwitcherAudioMonitorOutput _sdk;

            public AudioMonitorSoloTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSolo(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new AudioMixerMonitorSetCommand
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Solo,
                    Solo = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.Audio.Monitors[0].Solo = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, bool v)
            {
                yield return new CommandQueueKey(new AudioMixerMonitorGetCommand());
            }
        }

        [SkippableFact]
        public void TestSolo()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor();
                Skip.If(monitor == null, "Model does not support monitor");

                new AudioMonitorSoloTestDefinition(helper, monitor).Run();
            }
        }

        private class AudioMonitorGainTestDefinition : TestDefinitionBase<double>
        {
            protected readonly IBMDSwitcherAudioMonitorOutput _sdk;

            public AudioMonitorGainTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetGain(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new AudioMixerMonitorSetCommand
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Gain,
                    Gain = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.Audio.Monitors[0].Gain = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, double v)
            {
                yield return new CommandQueueKey(new AudioMixerMonitorGetCommand());
            }

            public override double[] GoodValues()
            {
                return new double[] { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };
            }
        }

        [SkippableFact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor();
                Skip.If(monitor == null, "Model does not support monitor");

                new AudioMonitorGainTestDefinition(helper, monitor).Run();
            }
        }

        private class AudioMonitorSoloInputTestDefinition : TestDefinitionBase<AudioSource>
        {
            protected readonly IBMDSwitcherAudioMonitorOutput _sdk;

            public AudioMonitorSoloInputTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMonitorOutput sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSoloInput((long)AudioSource.XLR);
            }

            public override ICommand GenerateCommand(AudioSource v)
            {
                return new AudioMixerMonitorSetCommand
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.SoloSource,
                    SoloSource = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, AudioSource v)
            {
                if (goodValue)
                    state.Audio.Monitors[0].SoloInput = v;
                else
                    state.Audio.Monitors[0].SoloInput = AudioSource.Input1;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, AudioSource v)
            {
                yield return new CommandQueueKey(new AudioMixerMonitorGetCommand());
            }

            public override AudioSource[] GoodValues()
            {
                return _helper.LibState.Audio.Inputs.Keys.Select(i => (AudioSource)i).ToArray();
            }

            public override AudioSource[] BadValues()
            {
                return Enum.GetValues(typeof(AudioSource)).OfType<AudioSource>().Except(GoodValues()).ToArray();
            }
        }

        [SkippableFact]
        public void TestSoloInput()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor();
                Skip.If(monitor == null, "Model does not support monitor");

                new AudioMonitorSoloInputTestDefinition(helper, monitor).Run();
            }
        }
    }
}
