using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightProgramOutCompressor
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightProgramOutCompressor(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherFairlightAudioCompressor GetCompressor(AtemMockServerWrapper helper)
        {
            IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = TestFairlightProgramOut.GetDynamics(helper);
            var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);
            Assert.NotNull(compressor);
            return compressor;
        }

        [Fact]
        public void TestCompressorEnabled()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerMasterCompressorSetCommand,
                        FairlightMixerMasterCompressorGetCommand>("CompressorEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    stateBefore.Fairlight.ProgramOut.Dynamics.Compressor.CompressorEnabled = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetEnabled(i % 2); });
                }
            });
        }

        [Fact]
        public void TestThreshold()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerMasterCompressorSetCommand,
                        FairlightMixerMasterCompressorGetCommand>("Threshold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(-50, 0);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Compressor.Threshold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetThreshold(target); });
                }
            });
        }

        [Fact]
        public void TestRatio()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerMasterCompressorSetCommand,
                        FairlightMixerMasterCompressorGetCommand>("Ratio");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(1.2, 20);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Compressor.Ratio = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetRatio(target); });
                }
            });
        }

        [Fact]
        public void TestAttack()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerMasterCompressorSetCommand,
                        FairlightMixerMasterCompressorGetCommand>("Attack");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(0.7, 100);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Compressor.Attack = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetAttack(target); });
                }
            });
        }

        [Fact]
        public void TestHold()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerMasterCompressorSetCommand,
                        FairlightMixerMasterCompressorGetCommand>("Hold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(0, 4000);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Compressor.Hold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetHold(target); });
                }
            });
        }

        [Fact]
        public void TestRelease()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerMasterCompressorSetCommand,
                        FairlightMixerMasterCompressorGetCommand>("Release");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(50, 4000);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Compressor.Release = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetRelease(target); });
                }
            });
        }

        [Fact]
        public void TestReset()
        {
            var target = new FairlightMixerMasterDynamicsResetCommand { Compressor = true };
            var handler = CommandGenerator.MatchCommand(target);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(helper);

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(null, () => { compressor.Reset(); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }
    }
}