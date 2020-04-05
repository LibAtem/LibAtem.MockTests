using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.ClassicAudio
{
    [Collection("ServerClientPool")]
    public class TestAudioMonitors
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAudioMonitors(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherAudioMonitorOutput GetMonitor(AtemMockServerWrapper helper)
        {
            var mixer = TestAudioProgramOut.GetAudioMixer(helper);

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioMonitorOutputIterator>(mixer.CreateIterator);
            var headphones = AtemSDKConverter.IterateList<IBMDSwitcherAudioMonitorOutput, IBMDSwitcherAudioMonitorOutput>(iterator.Next, (p, i) => p);

            return headphones.Single();
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerMonitorSetCommand, AudioMixerMonitorGetCommand>("Gain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.MonitorOutputState monState = stateBefore.Audio.MonitorOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    monState.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetGain(target); });
                }
            });
        }

        [Fact]
        public void TestEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerMonitorSetCommand, AudioMixerMonitorGetCommand>("Enabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.MonitorOutputState monState = stateBefore.Audio.MonitorOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    bool target = i % 2 == 0;
                    monState.Enabled = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetMonitorEnable(target ? 1 : 0); });
                }
            });
        }

        [Fact]
        public void TestMute()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerMonitorSetCommand, AudioMixerMonitorGetCommand>("Mute");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.MonitorOutputState monState = stateBefore.Audio.MonitorOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    bool target = i % 2 == 0;
                    monState.Mute = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetMute(target ? 1 : 0); });
                }
            });
        }

        [Fact]
        public void TestSolo()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerMonitorSetCommand, AudioMixerMonitorGetCommand>("Solo");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.MonitorOutputState monState = stateBefore.Audio.MonitorOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    bool target = i % 2 == 0;
                    monState.Solo = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetSolo(target ? 1 : 0); });
                }
            });
        }

        [Fact]
        public void TestSoloSource()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerMonitorSetCommand, AudioMixerMonitorGetCommand>("SoloSource");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.MonitorOutputState monState = stateBefore.Audio.MonitorOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    long target = Randomiser.RangeInt(10);
                    monState.SoloSource = (AudioSource)target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetSoloInput(target); });
                }
            });
        }

        [Fact]
        public void TestDim()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerMonitorSetCommand, AudioMixerMonitorGetCommand>("Dim");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.MonitorOutputState monState = stateBefore.Audio.MonitorOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    bool target = i % 2 == 0;
                    monState.Dim = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetDim(target ? 1 : 0); });
                }
            });
        }

        [Fact]
        public void TestDimLevel()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerMonitorSetCommand, AudioMixerMonitorGetCommand>("DimLevel");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.MonitorOutputState monState = stateBefore.Audio.MonitorOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    uint target = Randomiser.RangeInt(100);
                    monState.DimLevel = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetDimLevel(target / 100d); });
                }
            });
        }

        [Fact]
        public void TestResetPeakLevels()
        {
            var expected = new AudioMixerResetPeaksCommand { Mask = AudioMixerResetPeaksCommand.MaskFlags.Monitor };
            var handler = CommandGenerator.MatchCommand(expected, "Input");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMonitors, helper =>
            {
                    uint timeBefore = helper.Server.CurrentTime;

                    IBMDSwitcherAudioMonitorOutput monitor = GetMonitor(helper);

                    helper.SendAndWaitForChange(null, () => { monitor.ResetLevelNotificationPeaks(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }
    }
}