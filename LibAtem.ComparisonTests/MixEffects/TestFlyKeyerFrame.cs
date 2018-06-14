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
                    // Limits not enforced by atem
                    double[] testValues = { 0, 87.4, 0.01, 100, 999.99, 1000 };
                    // double[] badValues = { -0.01, -1, -10, 1000.1, 1010 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.XSize,
                        XSize = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeX = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeX = 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
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
                    // Limits not enforced by atem
                    double[] testValues = { 0, 87.4, 0.01, 100, 999.99, 1000 };
                    // double[] badValues = { -0.01, -1, -10, 1000.1, 1010 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.YSize,
                        YSize = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeY = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].SizeY = 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
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

        [Fact]
        public void TestBorderInnerWidth()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    double[] testValues = { 0, 15.9, 0.01, 16 };
                    // double[] badValues = { -0.01, -1, -10, 16.01, 16.1, 17 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderInnerWidth,
                        BorderInnerWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].InnerWidth = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].InnerWidth = v < 0 ? v + 655.36 : v;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderOuterWidth()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    double[] testValues = { 0, 15.9, 0.01, 16 };
                    // double[] badValues = { -0.01, -1, -10, 16.01, 16.1, 17 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderOuterWidth,
                        BorderOuterWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].OuterWidth = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].OuterWidth = v < 0 ? v + 655.36 : v;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderOuterSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    uint[] testValues = { 0, 15, 1, 99, 100 };
                    // uint[] badValues = { 101, 110, 255 };

                    ICommand Setter(uint v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderOuterSoftness,
                        BorderOuterSoftness = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].OuterSoftness = v;
                    // void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].OuterSoftness = 254;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderInnerSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    uint[] testValues = { 0, 15, 1, 99, 100 };
                    // uint[] badValues = { 101, 110, 255 };

                    ICommand Setter(uint v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderInnerSoftness,
                        BorderInnerSoftness = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].InnerSoftness = v;
                    // void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].InnerSoftness = 254;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    uint[] testValues = { 0, 15, 1, 99, 100 };
                    // uint[] badValues = { 101, 110, 255 };

                    ICommand Setter(uint v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderBevelSoftness,
                        BorderBevelSoftness = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BevelSoftness = v;
                    // void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BevelSoftness = 254;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    uint[] testValues = { 0, 15, 1, 99, 100 };
                    // uint[] badValues = { 101, 110, 255 };

                    ICommand Setter(uint v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderBevelPosition,
                        BorderBevelPosition = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BevelPosition = v;
                    // void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BevelPosition = 254;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        /* TODO - sdk does not fire any changed event, so this test does not work
        [Fact]
        public void TestBorderOpacity()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // stock client has a much stricter range
                    uint[] testValues = { 0, 15, 1, 99, 100, 254, 125 };
                    uint[] badValues = { 255 };

                    ICommand Setter(uint v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderOpacity,
                        BorderOpacity = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderOpacity = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderOpacity = 254;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }*/

        [Fact]
        public void TestBorderHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    double[] testValues = { 0, 123, 233.4, 359.9 };
                    // double[] badValues = { 360, 360.1, 361, -1, -0.01 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderHue,
                        BorderHue = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderHue = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderHue = 254;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    double[] testValues = { 0, 0.1, 99.9, 100 };
                    // double[] badValues = { 101, 100.1, 110, -1, -0.1 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderSaturation,
                        BorderSaturation = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderSaturation = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderSaturation = 254;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    double[] testValues = { 0, 0.1, 99.9, 100 };
                    // double[] badValues = { 101, 100.1, 110, -1, -0.1 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.BorderLuma,
                        BorderLuma = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderLuma = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].BorderLuma = 254;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    double[] testValues = { 0, 123, 233.4, 359.9 };
                    // double[] badValues = { 360, 360.1, 361, -1, -0.1 };

                    ICommand Setter(double v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.LightSourceDirection,
                        LightSourceDirection = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].LightSourceDirection = v;
                    // void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].LightSourceDirection = v >= 360 ? 360 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var frame in GetFrames())
                {
                    // Limits not enforced by atem
                    uint[] testValues = { 10, 11, 28, 99, 100 };
                    // uint[] badValues = { 0, 9, 101, 123, 255 };

                    ICommand Setter(uint v) => new MixEffectKeyFlyKeyframeSetCommand
                    {
                        MixEffectIndex = frame.Item1,
                        KeyerIndex = frame.Item2,
                        KeyFrame = frame.Item3,
                        Mask = MixEffectKeyFlyKeyframeSetCommand.MaskFlags.LightSourceAltitude,
                        LightSourceAltitude = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].LightSourceAltitude = v;
                    // void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[frame.Item1].Keyers[frame.Item2].Fly.Frames[frame.Item3].LightSourceAltitude = (uint)(v < 10 ? 10 : 100);

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }


    }
}