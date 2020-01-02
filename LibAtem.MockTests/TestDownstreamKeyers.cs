using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestDownstreamKeyers
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestDownstreamKeyers(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }
        private static List<Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey>> GetKeyers(AtemMockServerWrapper helper)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherDownstreamKeyIterator>(helper.Helper.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey>>();
            DownstreamKeyId index = 0;
            for (iterator.Next(out IBMDSwitcherDownstreamKey r); r != null; iterator.Next(out r))
            {
                result.Add(Tuple.Create(index, r));
                index++;
            }

            return result;
        }

        private static void EachKeyer(AtemMockServerWrapper helper, Action<AtemState, DownstreamKeyerState, IBMDSwitcherDownstreamKey, DownstreamKeyId, int> fcn, int iterations = 5)
        {
            foreach (Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> c in GetKeyers(helper))
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                DownstreamKeyerState dskBefore = stateBefore.DownstreamKeyers[(int)c.Item1];
                Assert.NotNull(dskBefore);

                for (int i = 0; i < iterations; i++)
                {
                    fcn(stateBefore, dskBefore, c.Item2, c.Item1, i);
                }
            }
        }

        [Fact]
        public void TestTie()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyTieSetCommand, DownstreamKeyPropertiesGetCommand>("Tie", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.Properties.Tie = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetTie(i % 2); });
                });
            });
        }

        [Fact]
        public void TestOnAir()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyOnAirSetCommand, DownstreamKeyStateGetV8Command>("OnAir", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.State.OnAir = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetOnAir(i % 2); });
                });
            });
        }

        [Fact]
        public void TestRate()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyRateSetCommand, DownstreamKeyPropertiesGetCommand>("Rate", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    uint target = Randomiser.RangeInt(250);
                    state.Properties.Rate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetRate(target); });
                });
            });
        }

        [Fact]
        public void TestPreMultipliedKey()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyGeneralSetCommand, DownstreamKeyPropertiesGetCommand>("PreMultipliedKey");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.Properties.PreMultipliedKey = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetPreMultiplied(i % 2); });
                });
            });
        }

        [Fact]
        public void TestClip()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyGeneralSetCommand, DownstreamKeyPropertiesGetCommand>("Clip");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    double target = Randomiser.Range(0, 100, 10);
                    state.Properties.Clip = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetClip(target / 100); });
                });
            });
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyGeneralSetCommand, DownstreamKeyPropertiesGetCommand>("Gain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    double target = Randomiser.Range(0, 100, 10);
                    state.Properties.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetGain(target / 100); });
                });
            });
        }

        [Fact]
        public void TestInvert()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyGeneralSetCommand, DownstreamKeyPropertiesGetCommand>("Invert");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.Properties.Invert = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetInverse(i % 2); });
                });
            });
        }

        [Fact]
        public void TestMaskEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyMaskSetCommand, DownstreamKeyPropertiesGetCommand>("MaskEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.Properties.MaskEnabled = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetMasked(i % 2); });
                });
            });
        }

        [Fact]
        public void TestMaskTop()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyMaskSetCommand, DownstreamKeyPropertiesGetCommand>("MaskTop");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    double target = Randomiser.Range(-9, 9, 1000);
                    state.Properties.MaskTop = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetMaskTop(target); });
                });
            });
        }

        [Fact]
        public void TestMaskBottom()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyMaskSetCommand, DownstreamKeyPropertiesGetCommand>("MaskBottom");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    double target = Randomiser.Range(-9, 9, 1000);
                    state.Properties.MaskBottom = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetMaskBottom(target); });
                });
            });
        }

        [Fact]
        public void TestMaskLeft()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyMaskSetCommand, DownstreamKeyPropertiesGetCommand>("MaskLeft");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    double target = Randomiser.Range(-9, 9, 1000);
                    state.Properties.MaskLeft = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetMaskLeft(target); });
                });
            });
        }

        [Fact]
        public void TestMaskRight()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyMaskSetCommand, DownstreamKeyPropertiesGetCommand>("MaskRight");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    double target = Randomiser.Range(-9, 9, 1000);
                    state.Properties.MaskRight= target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetMaskRight(target); });
                });
            });
        }

        [Fact]
        public void TestResetMask()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyMaskSetCommand, DownstreamKeyPropertiesGetCommand>(new[] { "MaskRight", "MaskLeft", "MaskTop", "MaskBottom" });
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.Properties.MaskRight = 16;
                    state.Properties.MaskLeft = -16;
                    state.Properties.MaskTop = 9;
                    state.Properties.MaskBottom = -9;
                    helper.SendAndWaitForChange(stateBefore, () => { props.ResetMask(); });
                });
            });
        }

        [Fact]
        public void TestFillSource()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyFillSourceSetCommand, DownstreamKeySourceGetCommand>("FillSource", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                VideoSource[] validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput | InternalPortType.SuperSource) &&
                    s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    // TODO GetFillInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    state.Sources.FillSource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetInputFill((long)target); });
                }, sampleSources.Length);
            });
        }

        [Fact]
        public void TestCutSource()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownstreamKeyCutSourceSetCommand, DownstreamKeySourceGetCommand>("CutSource", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                VideoSource[] validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput | InternalPortType.SuperSource) &&
                    s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    // TODO GetCutInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    state.Sources.CutSource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetInputCut((long)target); });
                }, sampleSources.Length);
            });
        }

        [Fact]
        public void TestAuto()
        {
            var expected = new DownstreamKeyAutoV8Command();
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    expected.Index = id;
                    helper.SendAndWaitForChange(stateBefore, () => { props.PerformAutoTransition(); });
                });
            });
        }

        [Fact]
        public void TestAutoInDirection()
        {
            var expected = new DownstreamKeyAutoV8Command
            {
                Mask = DownstreamKeyAutoV8Command.MaskFlags.IsTowardsOnAir
            };
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    expected.Index = id;
                    expected.IsTowardsOnAir = i % 2 != 0;

                    helper.SendAndWaitForChange(stateBefore, () => { props.PerformAutoTransitionInDirection(i % 2); });
                });
            });
        }

        [Fact]
        public void TestInTransition()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.State.InTransition = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new DownstreamKeyStateGetV8Command
                        {
                            Index = id,
                            OnAir = state.State.OnAir,
                            InTransition = i % 2 != 0,
                            IsAuto = state.State.IsAuto,
                            IsTowardsOnAir = state.State.IsTowardsOnAir,
                            RemainingFrames = state.State.RemainingFrames,
                        });
                    });
                });
            });
        }

        [Fact]
        public void TestIsAuto()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.State.IsAuto = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new DownstreamKeyStateGetV8Command
                        {
                            Index = id,
                            OnAir = state.State.OnAir,
                            InTransition = state.State.InTransition,
                            IsAuto = i % 2 != 0,
                            IsTowardsOnAir = state.State.IsTowardsOnAir,
                            RemainingFrames = state.State.RemainingFrames,
                        });
                    });
                });
            });
        }

        [Fact]
        public void TestIsTowardsOnAir()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    state.State.IsTowardsOnAir = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new DownstreamKeyStateGetV8Command
                        {
                            Index = id,
                            OnAir = state.State.OnAir,
                            InTransition = state.State.InTransition,
                            IsAuto = state.State.IsAuto,
                            IsTowardsOnAir = i % 2 != 0,
                            RemainingFrames = state.State.RemainingFrames,
                        });
                    });
                });
            });
        }

        [Fact]
        public void TestRemainingFrames()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                EachKeyer(helper, (stateBefore, state, props, id, i) =>
                {
                    var target = Randomiser.RangeInt(250);
                    state.State.RemainingFrames = target;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new DownstreamKeyStateGetV8Command
                        {
                            Index = id,
                            OnAir = state.State.OnAir,
                            InTransition = state.State.InTransition,
                            IsAuto = state.State.IsAuto,
                            IsTowardsOnAir = state.State.IsTowardsOnAir,
                            RemainingFrames = target,
                        });
                    });
                });
            });
        }




    }
}