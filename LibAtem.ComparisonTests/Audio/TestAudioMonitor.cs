using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.ComparisonTests.MixEffects;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Audio
{
    [Collection("Client")]
    public class TestAudioMonitor : ComparisonTestBase
    {
        public TestAudioMonitor(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        protected List<IBMDSwitcherAudioMonitorOutput> GetMonitors()
        {
            var mixer = Client.SdkSwitcher as IBMDSwitcherAudioMixer;
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
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                var sdkCount = GetMonitors().Count;
                Assert.Equal(sdkCount, Client.LibState.Audio.Monitors.Count);

                helper.AssertStatesMatch();
            }
        }

        [Fact]
        public void TestEnabled()
        {
            if (GetMonitor() == null)
                return;

            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ICommand Setter(bool v) => new AudioMixerMonitorSetCommand()
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Enabled,
                    Enabled = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.Audio.Monitors[0].Enabled = v;

                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, new[] { false, true });
            }
        }

        [Fact]
        public void TestMute()
        {
            if (GetMonitor() == null)
                return;

            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ICommand Setter(bool v) => new AudioMixerMonitorSetCommand()
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Mute,
                    Mute = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.Audio.Monitors[0].Mute = v;

                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, new[] { false, true });
            }
        }

        [Fact]
        public void TestDim()
        {
            if (GetMonitor() == null)
                return;

            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ICommand Setter(bool v) => new AudioMixerMonitorSetCommand()
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Dim,
                    Dim = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.Audio.Monitors[0].Dim = v;

                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, new[] {true, false });
            }
        }

        [Fact]
        public void TestSolo()
        {
            if (GetMonitor() == null)
                return;

            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ICommand Setter(bool v) => new AudioMixerMonitorSetCommand()
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Solo,
                    Solo = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.Audio.Monitors[0].Solo = v;

                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, new[] { true, false });
            }
        }

        [Fact]
        public void TestGain()
        {
            if (GetMonitor() == null)
                return;

            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                double[] testValues = { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };

                ICommand Setter(double v) => new AudioMixerMonitorSetCommand()
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.Gain,
                    Gain = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.Audio.Monitors[0].Gain = v;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
            }
        }

        [Fact]
        public void TestSoloInput()
        {
            if (GetMonitor() == null)
                return;
            
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                AudioSource[] testValues = Client.LibState.Audio.Inputs.Keys.Select(i => (AudioSource)i).ToArray();
                AudioSource[] badValues = Enum.GetValues(typeof(AudioSource)).OfType<AudioSource>().Except(testValues).ToArray();

                ICommand Setter(AudioSource v) => new AudioMixerMonitorSetCommand()
                {
                    Mask = AudioMixerMonitorSetCommand.MaskFlags.SoloSource,
                    SoloSource = v,
                };

                void UpdateExpectedState(ComparisonState state, AudioSource v) => state.Audio.Monitors[0].SoloInput = v;
                void UpdateFailedState(ComparisonState state, AudioSource v) => state.Audio.Monitors[0].SoloInput = AudioSource.Input1;

                ValueTypeComparer<AudioSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<AudioSource>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

    }
}
