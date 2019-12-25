using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Common;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightInputSourceExpander
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightInputSourceExpander(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherFairlightAudioExpander GetExpander(IBMDSwitcherFairlightAudioSource src)
        {
            IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = TestFairlightInputSource.GetDynamics(src);
            var expander = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(dynamics.GetProcessor);
            Assert.NotNull(expander);
            return expander;
        }

        [Fact]
        public void TestExpanderEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("ExpanderEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    srcState.Dynamics.Expander.ExpanderEnabled = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetEnabled(i % 2); });
                });
            });
        }

        [Fact]
        public void TestGateEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("GateEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    srcState.Dynamics.Expander.GateEnabled = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetGateMode(i % 2); });
                });
            });
        }

        [Fact]
        public void TestThreshold()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("Threshold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    var target = Randomiser.Range(-30, 0);
                    srcState.Dynamics.Expander.Threshold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetThreshold(target); });
                });
            });
        }

        [Fact]
        public void TestAttack()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("Attack");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    var target = Randomiser.Range(0.7, 30);
                    srcState.Dynamics.Expander.Attack = target;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetAttack(target); });
                });
            });
        }

        [Fact]
        public void TestHold()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("Hold");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    var target = Randomiser.Range(0, 4000);
                    srcState.Dynamics.Expander.Hold = target;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetHold(target); });
                });
            });
        }

        [Fact]
        public void TestRelease()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("Release");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    var target = Randomiser.Range(50, 4000);
                    srcState.Dynamics.Expander.Release = target;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetRelease(target); });
                });
            });
        }

        [Fact]
        public void TestRange()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("Range");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    var target = Randomiser.Range(0, 60);
                    srcState.Dynamics.Expander.Range = target;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetRange(target); });
                });
            });
        }

        [Fact]
        public void TestRatio()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceExpanderSetCommand, FairlightMixerSourceExpanderGetCommand>("Ratio");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander = GetExpander(src);

                    var target = Randomiser.Range(1.1, 3);
                    srcState.Dynamics.Expander.Ratio = target;
                    helper.SendAndWaitForChange(stateBefore, () => { expander.SetRatio(target); });
                });
            });
        }

        [Fact]
        public void TestReset()
        {
            var target = new FairlightMixerSourceDynamicsResetCommand() { Expander = true };
            var handler = CommandGenerator.MatchCommand(target);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioExpander expander= GetExpander(src);

                    target.Index = (AudioSource)inputId;
                    target.SourceId = srcState.SourceId;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { expander.Reset(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }, 1);
            });
        }
    }
}