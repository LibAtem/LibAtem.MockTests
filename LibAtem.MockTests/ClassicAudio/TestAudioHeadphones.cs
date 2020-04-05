using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.ClassicAudio
{
    [Collection("ServerClientPool")]
    public class TestAudioHeadphones
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAudioHeadphones(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherAudioHeadphoneOutput GetHeadphones(AtemMockServerWrapper helper)
        {
            var mixer = TestAudioProgramOut.GetAudioMixer(helper);

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioHeadphoneOutputIterator>(mixer.CreateIterator);
            var headphones = AtemSDKConverter.IterateList<IBMDSwitcherAudioHeadphoneOutput, IBMDSwitcherAudioHeadphoneOutput>(iterator.Next, (p, i) => p);

            return headphones.Single();
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerHeadphoneSetCommand, AudioMixerHeadphoneGetCommand>("Gain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioHeadphones, helper =>
            {
                IBMDSwitcherAudioHeadphoneOutput headphones = GetHeadphones(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.HeadphoneOutputState hpState = stateBefore.Audio.HeadphoneOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    hpState.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { headphones.SetGain(target); });
                }
            });
        }

        [Fact]
        public void TestProgramOutGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerHeadphoneSetCommand, AudioMixerHeadphoneGetCommand>("ProgramOutGain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioHeadphones, helper =>
            {
                IBMDSwitcherAudioHeadphoneOutput headphones = GetHeadphones(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.HeadphoneOutputState hpState = stateBefore.Audio.HeadphoneOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    hpState.ProgramOutGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { headphones.SetInputProgramOutGain(target); });
                }
            });
        }

        [Fact]
        public void TestSidetoneGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerHeadphoneSetCommand, AudioMixerHeadphoneGetCommand>("SidetoneGain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioHeadphones, helper =>
            {
                IBMDSwitcherAudioHeadphoneOutput headphones = GetHeadphones(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.HeadphoneOutputState hpState = stateBefore.Audio.HeadphoneOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    hpState.SidetoneGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { headphones.SetInputSidetoneGain(target); });
                }
            });
        }

        [Fact]
        public void TestTalkbackGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<AudioMixerHeadphoneSetCommand, AudioMixerHeadphoneGetCommand>("TalkbackGain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioHeadphones, helper =>
            {
                IBMDSwitcherAudioHeadphoneOutput headphones = GetHeadphones(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();
                AudioState.HeadphoneOutputState hpState = stateBefore.Audio.HeadphoneOutputs.Single();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    hpState.TalkbackGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { headphones.SetInputTalkbackGain(target); });
                }
            });
        }
    }
}