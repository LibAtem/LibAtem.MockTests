using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestFlyKeyerFrame : ComparisonTestBase
    {
        public TestFlyKeyerFrame(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private List<Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters>> GetFrames()
        {
            var res = new List<Tuple<MixEffectBlockId, UpstreamKeyId, FlyKeyKeyFrameId, IBMDSwitcherKeyFlyKeyFrameParameters>>();
            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyFlyParameters> key in GetKeyers<IBMDSwitcherKeyFlyParameters>())
            {
                key.Item3.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out IBMDSwitcherKeyFlyKeyFrameParameters frameA);
                key.Item3.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out IBMDSwitcherKeyFlyKeyFrameParameters frameB);

                res.Add(Tuple.Create(key.Item1, key.Item2, FlyKeyKeyFrameId.One, frameA));
                res.Add(Tuple.Create(key.Item1, key.Item2, FlyKeyKeyFrameId.Two, frameB));
            }

            return res;
        }

        [Fact]
        public void TestSizeX()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // stock client has a much stricter range
                    double[] testValues = { 0, 87.4, 0.01, 100, 999.99, 32767.9 };
                    double[] badValues = { -0.01, -1, -10, 32768, 40000 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.XSize,
                        XSize = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeX = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeX = 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestSizeY()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // stock client has a much stricter range
                    double[] testValues = { 0, 87.4, 0.01, 100, 999.99, 32767.9 };
                    double[] badValues = { -0.01, -1, -10, 32768, 40000 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.YSize,
                        YSize = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeY = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeY = 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestPositionX()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    double[] testValues = { -456, 0, 567, -32767.9, 32767.9 };
                    double[] badValues = { 32768, 33000, -32768 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.XPosition,
                        XPosition = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].PositionX = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].PositionX = v >= 32768 ? v - 2 * 32768 : v + 2 * 32768;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestPositionY()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    double[] testValues = { -456, 0, 567, -32767.9, 32767.9 };
                    double[] badValues = { 32768, 33000, -32768 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.YPosition,
                        YPosition = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].PositionY = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].PositionY = v >= 32768 ? v - 2 * 32768 : v + 2 * 32768;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestRotation()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    double[] testValues = { -456, 0, 567, -32767.9, 32767.9 };
                    double[] badValues = { 32768, 33000, -32768 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.Rotation,
                        Rotation = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].Rotation = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].Rotation = v >= 32768 ? v - 2 * 32768 : v + 2 * 32768;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }


    }
}