using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightInputSourceCompressor
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightInputSourceCompressor(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherFairlightAudioCompressor GetCompressor(IBMDSwitcherFairlightAudioSource src)
        {
            IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = TestFairlightInputSource.GetDynamics(src);
            var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);
            Assert.NotNull(compressor);
            return compressor;
        }

        [Fact]
        public void TestCompressorEnabled()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceCompressorSetCommand,
                        FairlightMixerSourceCompressorGetCommand>("CompressorEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(src);
                    srcState.Dynamics.Compressor.CompressorEnabled = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetEnabled(i % 2); });
                });
            });
        }

        [Fact]
        public void TestThreshold()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceCompressorSetCommand,
                        FairlightMixerSourceCompressorGetCommand>("Threshold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(src);

                    var target = Randomiser.Range(-50, 0);
                    srcState.Dynamics.Compressor.Threshold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetThreshold(target); });
                });
            });
        }

        [Fact]
        public void TestRatio()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceCompressorSetCommand,
                        FairlightMixerSourceCompressorGetCommand>("Ratio");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(src);

                    var target = Randomiser.Range(1.2, 20);
                    srcState.Dynamics.Compressor.Ratio = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetRatio(target); });
                });
            });
        }

        [Fact]
        public void TestAttack()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceCompressorSetCommand,
                        FairlightMixerSourceCompressorGetCommand>("Attack");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(src);

                    var target = Randomiser.Range(0.7, 100);
                    srcState.Dynamics.Compressor.Attack = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetAttack(target); });
                });
            });
        }

        [Fact]
        public void TestHold()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceCompressorSetCommand,
                        FairlightMixerSourceCompressorGetCommand>("Hold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(src);

                    var target = Randomiser.Range(0, 4000);
                    srcState.Dynamics.Compressor.Hold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetHold(target); });
                });
            });
        }

        [Fact]
        public void TestRelease()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceCompressorSetCommand,
                        FairlightMixerSourceCompressorGetCommand>("Release");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioCompressor compressor = GetCompressor(src);

                    var target = Randomiser.Range(50, 4000);
                    srcState.Dynamics.Compressor.Release = target;
                    helper.SendAndWaitForChange(stateBefore, () => { compressor.SetRelease(target); });
                });
            });
        }

        /*
        [Fact]
        public void TestReset()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterLimiterSetCommand, FairlightMixerMasterLimiterGetCommand>("Release");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(helper);

                uint timeBefore = helper.Server.CurrentTime;

                helper.SendAndWaitForChange(null, () => { limiter.Reset(); });

                // It should have sent a response, but we dont expect any comparable data
                Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
            });
        }
        */
    }
}