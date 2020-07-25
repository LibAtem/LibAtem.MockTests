using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.CameraControl;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.State.Util;

namespace LibAtem.MockTests.SdkState
{
    public static class CameraControlBuilder
    {
        public static void Build(AtemState state, IBMDSwitcherCameraControl camera)
        {
            state.CameraControl = new Dictionary<long, CameraControlState>();

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherCameraControlParameterIterator>(camera.CreateIterator);
            var lastDevice = uint.MaxValue;
            var lastCategory = 0u;
            var lastParameter = 0u;
            for (iterator.Next(out var device, out var category, out var parameter);
                device != 0;
                iterator.Next(out device, out category, out parameter))
            {
                if (device == lastDevice && category == lastCategory && parameter == lastParameter)
                    break;

                lastDevice = device;
                lastCategory = category;
                lastParameter = parameter;

                if (!state.CameraControl.TryGetValue(device, out CameraControlState cState))
                {
                    cState = state.CameraControl[device] = new CameraControlState();
                }

                camera.GetParameterInfo(device, category, parameter,
                    out _BMDSwitcherCameraControlParameterType type, out uint count);

                CameraControlGetCommand.DataType newType;
                switch (type)
                {
                    // case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeVoidBool:
                    //     break;
                    case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned8Bit:
                        newType = CameraControlGetCommand.DataType.SInt8;
                        break;
                    case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned16Bit:
                        newType = CameraControlGetCommand.DataType.SInt16;
                        break;
                    case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned32Bit:
                        newType = CameraControlGetCommand.DataType.SInt32;
                        break;
                    // case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeSigned64Bit:
                    //     break;
                    // case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeUTF8:
                    //     break;
                    case _BMDSwitcherCameraControlParameterType.bmdSwitcherCameraControlParameterTypeFixedPoint16Bit:
                        newType = CameraControlGetCommand.DataType.Float;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

               var cmd = new CameraControlGetCommand
                {
                    Input = (VideoSource) device,
                    Category = category,
                    Parameter = parameter,
                    Type = newType,
                };

                switch (cmd.Type)
                {
                    case CameraControlGetCommand.DataType.SInt8:
                    {
                        uint count2 = count;
                        camera.GetInt8s(device, category, parameter, ref count2, out sbyte values);
                        cmd.IntData = Randomiser.ConvertSdkArray(count2, ref values).Select(v => (int) v).ToArray();
                        break;
                    }
                    case CameraControlGetCommand.DataType.SInt16:
                    {
                        uint count2 = count;
                        camera.GetInt16s(device, category, parameter, ref count2, out short values);
                        cmd.IntData = Randomiser.ConvertSdkArray(count2, ref values).Select(v => (int) v).ToArray();
                        break;
                    }
                    case CameraControlGetCommand.DataType.SInt32:
                    {
                        uint count2 = count;
                        camera.GetInt32s(device, category, parameter, ref count2, out int values);
                        cmd.IntData = Randomiser.ConvertSdkArray(count2, ref values);
                        break;
                    }
                    case CameraControlGetCommand.DataType.Float:
                    {
                        uint count2 = count;
                        camera.GetFloats(device, category, parameter, ref count2, out double values);
                        cmd.FloatData = Randomiser.ConvertSdkArray(count2, ref values);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                CameraControlUtil.ApplyToState(cState, cmd);
            }
        }
    }
}