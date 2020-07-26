using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.CameraControl;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestCameraControl
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestCameraControl(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static void FillRandomData(CameraControlGetCommand cmd)
        {
            cmd.Input = (VideoSource) (Randomiser.RangeInt(20) + 50);
            cmd.Category = Randomiser.RangeInt(10) + 20;
            cmd.Parameter = Randomiser.RangeInt(10) + 15;

            cmd.Type = Randomiser.EnumValue<CameraControlDataType>();
            switch (cmd.Type)
            {
                case CameraControlDataType.SInt8:
                    cmd.IntData = Enumerable.Range(0, Randomiser.RangeInt(1, 6))
                        .Select(i => Randomiser.RangeInt(sbyte.MinValue, sbyte.MaxValue))
                        .ToArray();
                    break;
                case CameraControlDataType.SInt16:
                    cmd.IntData = Enumerable.Range(0, Randomiser.RangeInt(1, 4))
                        .Select(i => Randomiser.RangeInt(short.MinValue, short.MaxValue))
                        .ToArray();
                    break;
                case CameraControlDataType.SInt32:
                    cmd.IntData = Enumerable.Range(0, Randomiser.RangeInt(1, 2))
                        .Select(i => Randomiser.RangeInt(-500000, 500000))
                        .ToArray();
                    break;
                case CameraControlDataType.Float:
                    cmd.FloatData = Enumerable.Range(0, Randomiser.RangeInt(1, 4))
                        .Select(i => Randomiser.Range(0, 1))
                        .ToArray();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void AreEqual(CameraControlGetCommand a, CameraControlGetCommand b)
        {
            Assert.Equal(a.Input, b.Input);
            Assert.Equal(a.Category, b.Category);
            Assert.Equal(a.Parameter, b.Parameter);
            Assert.Equal(a.Type, b.Type);

            Assert.Equal(a.IntData != null, b.IntData != null);
            Assert.Equal(a.FloatData != null, b.FloatData != null);
            if (a.IntData != null)
            {
                for (int i = 0; i < a.IntData.Length; i++)
                    Assert.Equal(a.IntData[i], b.IntData[i]);
            }
            if (a.FloatData != null)
            {
                for (int i = 0; i < a.FloatData.Length; i++)
                    Assert.True(Math.Abs(a.FloatData[i] - b.FloatData[i]) < 0.01);
            }
        }
        
        private static void FillRandomOffsetData(CameraControlGetCommand cmd)
        {
            switch (cmd.Type)
            {
                case CameraControlDataType.SInt8:
                    cmd.IntData = cmd.IntData
                        .Select(i => Randomiser.RangeInt(sbyte.MinValue - i, sbyte.MaxValue - i))
                        .ToArray();
                    break;
                case CameraControlDataType.SInt16:
                    cmd.IntData = cmd.IntData
                        .Select(i => Randomiser.RangeInt(short.MinValue - i, short.MaxValue - i))
                        .ToArray();
                    break;
                case CameraControlDataType.SInt32:
                    cmd.IntData = cmd.IntData
                        .Select(i => Randomiser.RangeInt(-50000, 50000))
                        .ToArray();
                    break;
                case CameraControlDataType.Float:
                    cmd.FloatData = cmd.FloatData
                        .Select(i => Randomiser.Range(0 - i, 1 - i))
                        .ToArray();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private class CameraControlReceiver : IDisposable
        {
            private readonly AtemMockServerWrapper _helper;

            public CameraControlReceiver(AtemMockServerWrapper helper)
            {
                _helper = helper;
                _helper.Helper.OnLibAtemCommand += Receive;
            }

            public void Dispose()
            {
                _helper.Helper.OnLibAtemCommand -= Receive;
            }

            public CameraControlGetCommand LastCommand { get; private set; }

            private void Receive(object sender, ICommand cmd0)
            {
                if (cmd0 is CameraControlGetCommand cmd)
                {
                    LastCommand = cmd;
                }
            }
        }

        [Fact]
        public void TestSdkDecoding()
        {
            var handler = CommandGenerator.MatchCommand(new StartupStateSaveCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.CameraControl, helper =>
            {
                helper.Helper.StateSettings.IgnoreUnknownCameraControlProperties = true;
                IBMDSwitcherCameraControl camera = helper.SdkClient.SdkSwitcher as IBMDSwitcherCameraControl;
                Assert.NotNull(camera);

                using var watcher = new CameraControlReceiver(helper);

                for (int i = 0; i < 20; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    // Generate and send some data
                    CameraControlGetCommand cmd = new CameraControlGetCommand();
                    FillRandomData(cmd);

                    if (!stateBefore.CameraControl.ContainsKey((long) cmd.Input))
                        stateBefore.CameraControl[(long) cmd.Input] = new CameraControlState();
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    // Check libatem encoding
                    CameraControlGetCommand libCmd = watcher.LastCommand;
                    Assert.NotNull(libCmd);
                    AreEqual(cmd, libCmd);

                    // Pull the value out of the sdk, and ensure it is the same
                    CameraControlGetCommand sdkCmd =
                        CameraControlBuilder.BuildCommand(camera, (uint) cmd.Input, cmd.Category, cmd.Parameter);

                    AreEqual(cmd, sdkCmd);
                }
            });
        }

        private static void ApplyOffsets(CameraControlGetCommand getCmd, CameraControlGetCommand deltaCmd)
        {
            Assert.False(deltaCmd.IntData == null && deltaCmd.FloatData == null);
            if (deltaCmd.IntData != null)
            {
                Assert.NotNull(getCmd.IntData);
                Assert.Equal(getCmd.IntData.Length, deltaCmd.IntData.Length);

                var newVals = new int[getCmd.IntData.Length];
                for (int i = 0; i < newVals.Length; i++)
                    newVals[i] = getCmd.IntData[i] + deltaCmd.IntData[i];
                getCmd.IntData = newVals;
            }
            if (deltaCmd.FloatData != null)
            {
                Assert.NotNull(getCmd.FloatData);
                Assert.Equal(getCmd.FloatData.Length, deltaCmd.FloatData.Length);

                var newVals = new double[getCmd.FloatData.Length];
                for (int i = 0; i < getCmd.FloatData.Length; i++)
                    newVals[i] = getCmd.FloatData[i] + deltaCmd.FloatData[i];
                getCmd.FloatData = newVals;
            }
        }

        private static CameraControlGetCommand CopyCommand(CameraControlGetCommand cmd)
        {
            return new CameraControlGetCommand
            {
                Input = cmd.Input,
                Category = cmd.Category,
                Parameter = cmd.Parameter,
                Type = cmd.Type,
                IntData = cmd.IntData,
                FloatData = cmd.FloatData
            };
        }

        private CameraControlGetCommand _prevCmd;
        private IEnumerable<ICommand> CameraCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is CameraControlSetCommand setCmd)
            {
                CameraControlGetCommand getCmd = CopyCommand(setCmd);

                // If relative, we need to be more clever
                if (setCmd.Relative)
                {
                    Assert.Equal(setCmd.Input, _prevCmd.Input);
                    Assert.Equal(setCmd.Category, _prevCmd.Category);
                    Assert.Equal(setCmd.Parameter, _prevCmd.Parameter);
                    Assert.Equal(setCmd.Type, _prevCmd.Type);

                    getCmd.IntData = _prevCmd.IntData;
                    getCmd.FloatData = _prevCmd.FloatData;

                    ApplyOffsets(getCmd, setCmd);
                }

                yield return getCmd;
            }
        }

        [Fact]
        public void TestSdkEncoding()
        {
            AtemMockServerWrapper.Each(_output, _pool, CameraCommandHandler, DeviceTestCases.CameraControl, helper =>
            {
                helper.Helper.StateSettings.IgnoreUnknownCameraControlProperties = true;
                IBMDSwitcherCameraControl camera = helper.SdkClient.SdkSwitcher as IBMDSwitcherCameraControl;
                Assert.NotNull(camera);

                using var watcher = new CameraControlReceiver(helper);

                for (int i = 0; i < 20; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    // Generate and send some data
                    CameraControlGetCommand cmd = new CameraControlGetCommand();
                    FillRandomData(cmd);

                    if (!stateBefore.CameraControl.ContainsKey((long)cmd.Input))
                        stateBefore.CameraControl[(long)cmd.Input] = new CameraControlState();

                    IntPtr data;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        switch (cmd.Type)
                        {
                            case CameraControlDataType.SInt8:
                            {
                                data = Randomiser.BuildSdkArray(sizeof(sbyte), cmd.IntData);
                                unsafe
                                {
                                    var ptr = (sbyte*) data.ToPointer();
                                    camera.SetInt8s((uint) cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint) cmd.IntData.Length, ref *ptr);
                                }
                                break;
                            }
                            case CameraControlDataType.SInt16:
                            {
                                data = Randomiser.BuildSdkArray(sizeof(short), cmd.IntData);
                                unsafe
                                {
                                    var ptr = (short*)data.ToPointer();
                                    camera.SetInt16s((uint)cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint)cmd.IntData.Length, ref *ptr);
                                }
                                break;
                            }
                            case CameraControlDataType.SInt32:
                            {
                                data = Randomiser.BuildSdkArray(sizeof(int), cmd.IntData);
                                unsafe
                                {
                                    var ptr = (int*)data.ToPointer();
                                    camera.SetInt32s((uint)cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint)cmd.IntData.Length, ref *ptr);
                                }
                                break;
                            }
                            case CameraControlDataType.Float:
                            {
                                data = Randomiser.BuildSdkArray(cmd.FloatData);
                                unsafe
                                {
                                    var ptr = (double*)data.ToPointer();
                                    camera.SetFloats((uint)cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint)cmd.FloatData.Length, ref *ptr);
                                }
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });

                    // Check libatem encoding
                    CameraControlGetCommand libCmd = watcher.LastCommand;
                    Assert.NotNull(libCmd);
                    AreEqual(cmd, libCmd);

                    // Pull the value out of the sdk, and ensure it is the same
                    CameraControlGetCommand sdkCmd =
                        CameraControlBuilder.BuildCommand(camera, (uint)cmd.Input, cmd.Category, cmd.Parameter);

                    AreEqual(cmd, sdkCmd);
                }
            });
        }

        [Fact]
        public void TestSdkRelativeEncoding()
        {
            AtemMockServerWrapper.Each(_output, _pool, CameraCommandHandler, DeviceTestCases.CameraControl, helper =>
            {
                helper.Helper.StateSettings.IgnoreUnknownCameraControlProperties = true;
                IBMDSwitcherCameraControl camera = helper.SdkClient.SdkSwitcher as IBMDSwitcherCameraControl;
                Assert.NotNull(camera);

                using var watcher = new CameraControlReceiver(helper);

                for (int i = 0; i < 20; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    // Generate and send some data
                    CameraControlGetCommand refCmd = new CameraControlGetCommand();
                    FillRandomData(refCmd);

                    if (!stateBefore.CameraControl.ContainsKey((long)refCmd.Input))
                        stateBefore.CameraControl[(long)refCmd.Input] = new CameraControlState();

                    helper.SendFromServerAndWaitForChange(stateBefore, refCmd);
                    _prevCmd = refCmd;

                    {
                        // Pull the value out of the sdk, and ensure it is the same
                        CameraControlGetCommand sdkCmd =
                            CameraControlBuilder.BuildCommand(camera, (uint)refCmd.Input, refCmd.Category, refCmd.Parameter);

                        AreEqual(refCmd, sdkCmd);
                    }

                    CameraControlGetCommand cmd = CopyCommand(refCmd);
                    FillRandomOffsetData(cmd);

                    // Now do the sdk offset
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        switch (cmd.Type)
                        {
                            case CameraControlDataType.SInt8:
                            {
                                IntPtr data = Randomiser.BuildSdkArray(sizeof(sbyte), cmd.IntData);
                                unsafe
                                {
                                    var ptr = (sbyte*)data.ToPointer();
                                    camera.OffsetInt8s((uint)cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint)cmd.IntData.Length, ref *ptr);
                                }
                                break;
                            }
                            case CameraControlDataType.SInt16:
                            {
                                IntPtr data = Randomiser.BuildSdkArray(sizeof(short), cmd.IntData);
                                unsafe
                                {
                                    var ptr = (short*)data.ToPointer();
                                    camera.OffsetInt16s((uint)cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint)cmd.IntData.Length, ref *ptr);
                                }
                                break;
                            }
                            case CameraControlDataType.SInt32:
                            {
                                IntPtr data = Randomiser.BuildSdkArray(sizeof(int), cmd.IntData);
                                unsafe
                                {
                                    var ptr = (int*)data.ToPointer();
                                    camera.OffsetInt32s((uint)cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint)cmd.IntData.Length, ref *ptr);
                                }
                                break;
                            }
                            case CameraControlDataType.Float:
                            {
                                IntPtr data = Randomiser.BuildSdkArray(cmd.FloatData);
                                unsafe
                                {
                                    var ptr = (double*)data.ToPointer();
                                    camera.OffsetFloats((uint)cmd.Input, cmd.Category, cmd.Parameter,
                                        (uint)cmd.FloatData.Length, ref *ptr);
                                }
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });

                    var completeCmd = CopyCommand(refCmd);
                    ApplyOffsets(completeCmd, cmd);

                    // Check libatem encoding
                    CameraControlGetCommand libCmd = watcher.LastCommand;
                    Assert.NotNull(libCmd);
                    AreEqual(completeCmd, libCmd);

                    {
                        // Pull the value out of the sdk, and ensure it is the same
                        CameraControlGetCommand sdkCmd =
                            CameraControlBuilder.BuildCommand(camera, (uint)refCmd.Input, refCmd.Category, refCmd.Parameter);

                        AreEqual(completeCmd, sdkCmd);
                    }
                }
            });
        }



    }
}