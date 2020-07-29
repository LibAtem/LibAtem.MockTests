using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.CameraControl;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.State.Util;

namespace LibAtem.MockTests.SdkState
{
    public static class CameraControlBuilder
    {
        public static CameraControlGetCommand BuildCommand(IBMDSwitcherCameraControl camera, uint device, uint category, uint parameter)
        {
            camera.GetParameterInfo(device, category, parameter,
                    out _BMDSwitcherCameraControlParameterType type, out uint count);

            CameraControlDataType newType;
            switch (type)
            {
                case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeVoidBool:
                    newType = CameraControlDataType.Bool;
                    break;
                case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned8Bit:
                    newType = CameraControlDataType.SInt8;
                    break;
                case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned16Bit:
                    newType = CameraControlDataType.SInt16;
                    break;
                case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned32Bit:
                    newType = CameraControlDataType.SInt32;
                    break;
                case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned64Bit:
                    newType = CameraControlDataType.SInt64;
                    break;
                case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeUTF8:
                    newType = CameraControlDataType.String;
                    break;
                case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeFixedPoint16Bit:
                    newType = CameraControlDataType.Float;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            camera.GetParameterPeriodicFlushEnabled(device, category, parameter, out int flushEnabled);

            var cmd = new CameraControlGetCommand
            {
                Input = (VideoSource) device,
                Category = category,
                Parameter = parameter,
                Type = newType,
                PeriodicFlushEnabled = flushEnabled != 0,
            };

            switch (cmd.Type)
            {
                case CameraControlDataType.Bool:
                    {
                        uint count2 = count;
                        camera.GetFlags(device, category, parameter, ref count2, out int values);
                        int[] intVals = Randomiser.ConvertSdkArray(count2, ref values);
                        var sbyteVals = new sbyte[count2];
                        Buffer.BlockCopy(intVals, 0, sbyteVals, 0, (int) count2);
                        cmd.BoolData = sbyteVals.Select(v => v != 0).ToArray();
                        break;
                    }
                case CameraControlDataType.SInt8:
                    {
                        uint count2 = count;
                        camera.GetInt8s(device, category, parameter, ref count2, out sbyte values);
                        cmd.IntData = Randomiser.ConvertSdkArray(count2, ref values).Select(v => (int)v).ToArray();
                        break;
                    }
                case CameraControlDataType.SInt16:
                    {
                        uint count2 = count;
                        camera.GetInt16s(device, category, parameter, ref count2, out short values);
                        cmd.IntData = Randomiser.ConvertSdkArray(count2, ref values).Select(v => (int)v).ToArray();
                        break;
                    }
                case CameraControlDataType.SInt32:
                    {
                        uint count2 = count;
                        camera.GetInt32s(device, category, parameter, ref count2, out int values);
                        cmd.IntData = Randomiser.ConvertSdkArray(count2, ref values);
                        break;
                    }
                case CameraControlDataType.SInt64:
                    {
                        uint count2 = count;
                        camera.GetInt64s(device, category, parameter, ref count2, out long values);
                        cmd.LongData = Randomiser.ConvertSdkArray(count2, ref values);
                        break;
                    }
                case CameraControlDataType.String:
                    {
                        camera.GetString(device, category, parameter, out string value);
                        cmd.StringData = value;
                        break;
                    }
                case CameraControlDataType.Float:
                    {
                        uint count2 = count;
                        camera.GetFloats(device, category, parameter, ref count2, out double values);
                        cmd.FloatData = Randomiser.ConvertSdkArray(count2, ref values);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return cmd;
        }

        public static void Build(AtemState state, IBMDSwitcherCameraControl camera, AtemStateBuilderSettings updateSettings)
        {
            camera.GetPeriodicFlushInterval(out uint interval);
            state.CameraControl.PeriodicFlushInterval = interval;

            IBMDSwitcherCameraControlParameterIterator iterator =
                AtemSDKConverter.CastSdk<IBMDSwitcherCameraControlParameterIterator>(camera.CreateIterator);
            uint lastDevice = uint.MaxValue;
            uint lastCategory = 0u;
            uint lastParameter = 0u;
            for (iterator.Next(out var device, out var category, out var parameter);
                ;
                iterator.Next(out device, out category, out parameter))
            {
                if (device == lastDevice && category == lastCategory && parameter == lastParameter)
                    break;

                lastDevice = device;
                lastCategory = category;
                lastParameter = parameter;

                if (device == 0) continue;

                if (!state.CameraControl.Cameras.TryGetValue(device, out CameraControlState.CameraState cState))
                {
                    cState = state.CameraControl.Cameras[device] = new CameraControlState.CameraState();
                }

                CameraControlGetCommand cmd = BuildCommand(camera, device, category, parameter);
                CameraControlUtil.ApplyToState(cState, cmd, updateSettings.IgnoreUnknownCameraControlProperties);
            }
        }
    }
}