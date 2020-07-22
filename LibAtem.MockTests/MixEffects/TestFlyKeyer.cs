using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestFlyKeyer : MixEffectsTestBase
    {
        public TestFlyKeyer(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestFlyEnabled()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyTypeSetCommand, MixEffectKeyPropertiesGetCommand>("FlyEnabled");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    bool target = i % 2 == 0;
                    keyerBefore.Properties.FlyEnabled = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetFly(target ? 1 : 0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCanFly()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    var cmd = helper.Server.GetParsedDataDump().OfType<MixEffectKeyPropertiesGetCommand>()
                        .Single(c => c.MixEffectIndex == meId && c.KeyerIndex == keyId);

                    bool target = !keyerBefore.Properties.CanFlyKey;
                    keyerBefore.Properties.CanFlyKey = cmd.CanFlyKey = target;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("Rate");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(250);
                    keyerBefore.DVE.Rate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetRate(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSizeX()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("SizeX");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 99.99);
                    keyerBefore.DVE.SizeX = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetSizeX(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSizeY()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("SizeY");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 99.99);
                    keyerBefore.DVE.SizeY = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetSizeY(target); });
                });
            });
            Assert.True(tested);
        }

        /*
        [Fact]
        public void TestCanScaleUp()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    var cmd = helper.Server.GetParsedDataDump().OfType<MixEffectKeyDVEGetCommand>()
                        .Single(c => c.MixEffectIndex == meId && c.KeyerIndex == keyId);

                    bool target = !keyerBefore.DVE.CanScaleUp;
                    keyerBefore.DVE.CanScaleUp = cmd.CanScaleUp = target;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                });
            });
            Assert.True(tested);
        }
        */

        [Fact]
        public void TestPositionX()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("PositionX");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(-1000, 1000);
                    keyerBefore.DVE.PositionX = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetPositionX(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPositionY()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("PositionY");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(-1000, 1000);
                    keyerBefore.DVE.PositionY = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetPositionY(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestRotation()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("Rotation");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    sdkKeyer.GetCanRotate(out int canRotate);
                    if (canRotate != 0)
                    {
                        tested = true;

                        double target = Randomiser.Range(-1000, 1000, 10);
                        keyerBefore.DVE.Rotation = target;
                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetRotation(target); });
                    }
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestResetRotation()
        {
            bool tested = false;
            var expectedCmd = new MixEffectKeyDVESetCommand
            {
                Mask = MixEffectKeyDVESetCommand.MaskFlags.Rotation,
                Rotation = 0,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    sdkKeyer.GetCanRotate(out int canRotate);
                    if (canRotate != 0)
                    {
                        tested = true;

                        expectedCmd.MixEffectIndex = meId;
                        expectedCmd.KeyerIndex = keyId;

                        keyerBefore.DVE.Rotation = expectedCmd.Rotation;

                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.ResetRotation(); });
                    }
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestResetDVE()
        {
            bool tested = false;
            var expectedCmd = new MixEffectKeyDVESetCommand
            {
                Mask = MixEffectKeyDVESetCommand.MaskFlags.SizeX | MixEffectKeyDVESetCommand.MaskFlags.SizeY |
                       MixEffectKeyDVESetCommand.MaskFlags.PositionX | MixEffectKeyDVESetCommand.MaskFlags.PositionY |
                       MixEffectKeyDVESetCommand.MaskFlags.Rotation,
                SizeX = 0.5,
                SizeY = 0.5,
                PositionX = 0,
                PositionY = 0,
                Rotation = 0,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    sdkKeyer.GetCanRotate(out int canRotate);
                    if (canRotate != 0)
                    {
                        tested = true;

                        expectedCmd.MixEffectIndex = meId;
                        expectedCmd.KeyerIndex = keyId;

                        keyerBefore.DVE.SizeX = expectedCmd.SizeX;
                        keyerBefore.DVE.SizeY = expectedCmd.SizeY;
                        keyerBefore.DVE.PositionX = expectedCmd.PositionX;
                        keyerBefore.DVE.PositionY = expectedCmd.PositionY;
                        keyerBefore.DVE.Rotation = expectedCmd.Rotation;

                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.ResetDVE(); });
                    }
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestResetDVEFull()
        {
            bool tested = false;
            var expectedCmd = new MixEffectKeyDVESetCommand
            {
                Mask = MixEffectKeyDVESetCommand.MaskFlags.SizeX | MixEffectKeyDVESetCommand.MaskFlags.SizeY |
                       MixEffectKeyDVESetCommand.MaskFlags.PositionX | MixEffectKeyDVESetCommand.MaskFlags.PositionY |
                       MixEffectKeyDVESetCommand.MaskFlags.Rotation,
                SizeX = 1,
                SizeY = 1,
                PositionX = 0,
                PositionY = 0,
                Rotation = 0,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    sdkKeyer.GetCanRotate(out int canRotate);
                    if (canRotate != 0)
                    {
                        tested = true;

                        expectedCmd.MixEffectIndex = meId;
                        expectedCmd.KeyerIndex = keyId;

                        keyerBefore.DVE.SizeX = expectedCmd.SizeX;
                        keyerBefore.DVE.SizeY = expectedCmd.SizeY;
                        keyerBefore.DVE.PositionX = expectedCmd.PositionX;
                        keyerBefore.DVE.PositionY = expectedCmd.PositionY;
                        keyerBefore.DVE.Rotation = expectedCmd.Rotation;

                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.ResetDVEFull(); });
                    }
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestIsASet()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    var cmd = helper.Server.GetParsedDataDump().OfType<MixEffectKeyFlyPropertiesGetCommand>()
                        .Single(c => c.MixEffectIndex == meId && c.KeyerIndex == keyId);

                    bool target = !keyerBefore.FlyProperties.IsASet;
                    keyerBefore.FlyProperties.IsASet = cmd.IsASet = target;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestIsBSet()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    var cmd = helper.Server.GetParsedDataDump().OfType<MixEffectKeyFlyPropertiesGetCommand>()
                        .Single(c => c.MixEffectIndex == meId && c.KeyerIndex == keyId);

                    bool target = !keyerBefore.FlyProperties.IsBSet;
                    keyerBefore.FlyProperties.IsBSet = cmd.IsBSet = target;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestIsRunning()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    var cmd = helper.Server.GetParsedDataDump().OfType<MixEffectKeyFlyPropertiesGetCommand>()
                        .Single(c => c.MixEffectIndex == meId && c.KeyerIndex == keyId);

                    for (int o = 0; o < 5; o++)
                    {
                        var targetFrame = Randomiser.RangeInt(10) > 7
                            ? FlyKeyKeyFrameType.RunToInfinite
                            : Randomiser.EnumValue<FlyKeyKeyFrameType>();
                        var targetInfinite = Randomiser.EnumValue<FlyKeyLocation>();

                        keyerBefore.FlyProperties.RunningToKeyFrame = cmd.RunningToKeyFrame = targetFrame;
                        keyerBefore.FlyProperties.RunningToInfinite = cmd.RunningToInfinite =
                            targetFrame == FlyKeyKeyFrameType.RunToInfinite ? targetInfinite : 0;

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestRunToKeyFrame()
        {
            bool tested = false;
            var expectedCmd = new MixEffectKeyFlyRunSetCommand();
            bool ignoreInfinite = false;
            Func<Lazy<ImmutableList<ICommand>>, ICommand, IEnumerable<ICommand>> handler = (previousCommands, cmd) =>
            {
                string[] ignoreProps = ignoreInfinite ? new[] {"RunToInfinite"} : new string[] { };
                if (CommandGenerator.ValidateCommandMatches(cmd, expectedCmd, false, ignoreProps))
                {
                    // Accept it
                    return new ICommand[] { null };
                }

                return new ICommand[0];
            };
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    expectedCmd.MixEffectIndex = meId;
                    expectedCmd.KeyerIndex = keyId;

                    for (int o = 0; o < 5; o++)
                    {
                        var targetFrame = Randomiser.RangeInt(2) > 0
                            ? FlyKeyKeyFrameType.RunToInfinite
                            : Randomiser.EnumValue<FlyKeyKeyFrameType>(FlyKeyKeyFrameType.None);
                        var targetInfinite = Randomiser.EnumValue<FlyKeyLocation>();
                        
                        ignoreInfinite = targetFrame != FlyKeyKeyFrameType.RunToInfinite;

                        expectedCmd.KeyFrame = targetFrame;
                        expectedCmd.RunToInfinite = targetInfinite;
                        /*expectedCmd.Mask = targetFrame == FlyKeyKeyFrameType.RunToInfinite
                            ? MixEffectKeyFlyRunSetCommand.MaskFlags.RunToInfinite |
                              MixEffectKeyFlyRunSetCommand.MaskFlags.KeyFrame
                            : MixEffectKeyFlyRunSetCommand.MaskFlags.KeyFrame;*/

                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            _BMDSwitcherFlyKeyFrame destination;
                            switch (targetFrame)
                            {
                                case FlyKeyKeyFrameType.None:
                                case FlyKeyKeyFrameType.A:
                                    destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA;
                                    break;
                                case FlyKeyKeyFrameType.B:
                                    destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB;
                                    break;
                                case FlyKeyKeyFrameType.Full:
                                    destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull;
                                    break;
                                case FlyKeyKeyFrameType.RunToInfinite:
                                    switch (targetInfinite)
                                    {
                                        case FlyKeyLocation.CentreOfKey:
                                            destination = _BMDSwitcherFlyKeyFrame
                                                .bmdSwitcherFlyKeyFrameInfinityCentreOfKey;
                                            break;
                                        case FlyKeyLocation.TopLeft:
                                            destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityTopLeft;
                                            break;
                                        case FlyKeyLocation.TopCentre:
                                            destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityTop;
                                            break;
                                        case FlyKeyLocation.TopRight:
                                            destination = _BMDSwitcherFlyKeyFrame
                                                .bmdSwitcherFlyKeyFrameInfinityTopRight;
                                            break;
                                        case FlyKeyLocation.MiddleLeft:
                                            destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityLeft;
                                            break;
                                        case FlyKeyLocation.MiddleCentre:
                                            destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityCentre;
                                            break;
                                        case FlyKeyLocation.MiddleRight:
                                            destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityRight;
                                            break;
                                        case FlyKeyLocation.BottomLeft:
                                            destination = _BMDSwitcherFlyKeyFrame
                                                .bmdSwitcherFlyKeyFrameInfinityBottomLeft;
                                            break;
                                        case FlyKeyLocation.BottomCentre:
                                            destination = _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameInfinityBottom;
                                            break;
                                        case FlyKeyLocation.BottomRight:
                                            destination = _BMDSwitcherFlyKeyFrame
                                                .bmdSwitcherFlyKeyFrameInfinityBottomRight;
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            sdkKeyer.RunToKeyFrame(destination);
                        });
                    }
                });
            });
            Assert.True(tested);
        }


        [Fact]
        public void TestStoreAsKeyFrame()
        {
            bool tested = false;
            var expectedCmd = new MixEffectKeyFlyKeyframeStoreCommand();
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyFlyParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    sdkKeyer.GetCanRotate(out int canRotate);
                    if (canRotate != 0)
                    {
                        tested = true;

                        expectedCmd.MixEffectIndex = meId;
                        expectedCmd.KeyerIndex = keyId;
                        expectedCmd.KeyFrame = Randomiser.EnumValue<FlyKeyKeyFrameId>();

                        var kfId = expectedCmd.KeyFrame == FlyKeyKeyFrameId.One
                            ? _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA
                            : _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB;

                        helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.StoreAsKeyFrame(kfId); });
                    }
                });
            });
            Assert.True(tested);
        }



    }
}