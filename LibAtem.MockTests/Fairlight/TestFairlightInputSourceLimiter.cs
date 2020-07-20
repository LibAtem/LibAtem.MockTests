using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightInputSourceLimiter
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightInputSourceLimiter(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherFairlightAudioLimiter GetLimiter(IBMDSwitcherFairlightAudioSource src)
        {
            IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = TestFairlightInputSource.GetDynamics(src);
            var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
            Assert.NotNull(limiter);
            return limiter;
        }

        [Fact]
        public void TestLimiterEnabled()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceLimiterSetCommand,
                        FairlightMixerSourceLimiterGetCommand>("LimiterEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(src);

                    srcState.Dynamics.Limiter.LimiterEnabled = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetEnabled(i % 2); });
                });
            });
        }

        [Fact]
        public void TestThreshold()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceLimiterSetCommand,
                        FairlightMixerSourceLimiterGetCommand>("Threshold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(src);

                    var target = Randomiser.Range(-30, 0);
                    srcState.Dynamics.Limiter.Threshold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetThreshold(target); });
                });
            });
        }

        [Fact]
        public void TestAttack()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceLimiterSetCommand,
                        FairlightMixerSourceLimiterGetCommand>("Attack");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(src);

                    var target = Randomiser.Range(0.7, 30);
                    srcState.Dynamics.Limiter.Attack = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetAttack(target); });
                });
            });
        }

        [Fact]
        public void TestHold()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceLimiterSetCommand,
                        FairlightMixerSourceLimiterGetCommand>("Hold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(src);

                    var target = Randomiser.Range(0, 4000);
                    srcState.Dynamics.Limiter.Hold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetHold(target); });
                });
            });
        }

        [Fact]
        public void TestRelease()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<FairlightMixerSourceLimiterSetCommand,
                        FairlightMixerSourceLimiterGetCommand>("Release");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(src);

                    var target = Randomiser.Range(50, 4000);
                    srcState.Dynamics.Limiter.Release = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetRelease(target); });
                });
            });
        }

        [Fact]
        public void TestReset()
        {
            var target = new FairlightMixerSourceDynamicsResetCommand() { Limiter = true };
            var handler = CommandGenerator.MatchCommand(target);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(src);

                    target.Index = (AudioSource) inputId;
                    target.SourceId = srcState.SourceId;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { limiter.Reset(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }, 1);
            });
        }
    }
}