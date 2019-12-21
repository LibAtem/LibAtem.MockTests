using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    public class TestFairlightProgramOutLimiter
    {
        private readonly ITestOutputHelper _output;

        public TestFairlightProgramOutLimiter(ITestOutputHelper output)
        {
            _output = output;
        }

        private static IBMDSwitcherFairlightAudioLimiter GetLimiter(AtemMockServerWrapper helper)
        {
            IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = TestFairlightProgramOut.GetDynamics(helper);
            var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
            Assert.NotNull(limiter);
            return limiter;
        }

        [Fact]
        public void TestLimiterEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterLimiterSetCommand, FairlightMixerMasterLimiterGetCommand>("LimiterEnabled");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(helper);

                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    stateBefore.Fairlight.ProgramOut.Dynamics.Limiter.LimiterEnabled = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetEnabled(i % 2); });
                }
            });
        }

        [Fact]
        public void TestThreshold()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterLimiterSetCommand, FairlightMixerMasterLimiterGetCommand>("Threshold");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(helper);

                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(-30, 0);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Limiter.Threshold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetThreshold(target); });
                }
            });
        }

        [Fact]
        public void TestAttack()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterLimiterSetCommand, FairlightMixerMasterLimiterGetCommand>("Attack");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(helper);

                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(0.7, 30);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Limiter.Attack = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetAttack(target); });
                }
            });
        }

        [Fact]
        public void TestHold()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterLimiterSetCommand, FairlightMixerMasterLimiterGetCommand>("Hold");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(helper);

                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(0, 4000);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Limiter.Hold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetHold(target); });
                }
            });
        }

        [Fact]
        public void TestRelease()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterLimiterSetCommand, FairlightMixerMasterLimiterGetCommand>("Release");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioLimiter limiter = GetLimiter(helper);

                AtemState stateBefore = helper.Helper.LibState;

                for (int i = 0; i < 5; i++)
                {
                    var target = Randomiser.Range(50, 4000);
                    stateBefore.Fairlight.ProgramOut.Dynamics.Limiter.Release = target;
                    helper.SendAndWaitForChange(stateBefore, () => { limiter.SetRelease(target); });
                }
            });
        }

        /*
        [Fact]
        public void TestReset()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterLimiterSetCommand, FairlightMixerMasterLimiterGetCommand>("Release");
            AtemMockServerWrapper.Each(_output, handler, DeviceTestCases.FairlightMain, helper =>
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