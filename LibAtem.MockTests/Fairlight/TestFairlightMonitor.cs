using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightMonitor
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightMonitor(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        public static IBMDSwitcherFairlightAudioHeadphoneOutput GetMonitor(AtemMockServerWrapper helper)
        {
            IBMDSwitcherFairlightAudioMixer mixer = TestFairlightProgramOut.GetFairlightMixer(helper);
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioHeadphoneOutputIterator>(mixer.CreateIterator);

            var monitors = AtemSDKConverter.IterateList<IBMDSwitcherFairlightAudioHeadphoneOutput, IBMDSwitcherFairlightAudioHeadphoneOutput>(iterator.Next, (s, i) => s);
            return monitors.SingleOrDefault();
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMonitorSetCommand, FairlightMixerMonitorGetCommand>("Gain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                var monitor = GetMonitor(helper);
                if (monitor == null) return;
                tested = true;

                AtemState stateBefore = helper.Helper.LibState;
                FairlightAudioState.MonitorOutputState monState = stateBefore.Fairlight.Monitors.Single();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(-60, 6);
                    monState.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetGain(target); });
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInputMasterGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMonitorSetCommand, FairlightMixerMonitorGetCommand>("InputMasterGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                var monitor = GetMonitor(helper);
                if (monitor == null) return;
                tested = true;

                AtemState stateBefore = helper.Helper.LibState;
                FairlightAudioState.MonitorOutputState monState = stateBefore.Fairlight.Monitors.Single();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(-60, 6);
                    monState.InputMasterGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetInputMasterOutGain(target); });
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInputTalkbackGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMonitorSetCommand, FairlightMixerMonitorGetCommand>("InputTalkbackGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                var monitor = GetMonitor(helper);
                if (monitor == null) return;
                tested = true;

                AtemState stateBefore = helper.Helper.LibState;
                FairlightAudioState.MonitorOutputState monState = stateBefore.Fairlight.Monitors.Single();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(-60, 6);
                    monState.InputTalkbackGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetInputTalkbackGain(target); });
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInputSidetoneGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMonitorSetCommand, FairlightMixerMonitorGetCommand>("InputSidetoneGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                var monitor = GetMonitor(helper);
                if (monitor == null) return;
                tested = true;

                AtemState stateBefore = helper.Helper.LibState;
                FairlightAudioState.MonitorOutputState monState = stateBefore.Fairlight.Monitors.Single();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(-60, 6);
                    monState.InputSidetoneGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { monitor.SetInputSidetoneGain(target); });
                }
            });
            Assert.True(tested);
        }
    }
}