using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;

namespace LibAtem.MockTests.SdkState
{
    public static class CameraControlBuilder
    {
        public delegate void GetGetter<T>(uint device, uint category, uint parameter, ref uint count, out T values);

        private static T[] GetGeneric<T>(IBMDSwitcherCameraControl camera, uint device, uint category, uint parameter, uint expectedCount, _BMDSwitcherCameraControlParameterType expectedType, GetGetter<T> getter)
        {
            camera.GetParameterInfo(device, category, parameter,
                out _BMDSwitcherCameraControlParameterType type, out uint count);
            Assert.Equal(expectedType, type);
            Assert.Equal(expectedCount, count);

            uint count2 = count;
            getter(device, category, parameter, ref count2, out T values);
            Assert.Equal(expectedCount, count2);

            return Randomiser.ConvertSdkArray(count2, ref values);
        }

        private static sbyte[] GetSInt8(IBMDSwitcherCameraControl camera, uint device, uint category, uint parameter, uint expectedCount)
        {
            return GetGeneric<sbyte>(camera, device, category, parameter, expectedCount,
                _BMDSwitcherCameraControlParameterType
                    .bmdSwitcherCameraControlParameterTypeSigned8Bit, camera.GetInt8s);
        }

        private static short[] GetSInt16(IBMDSwitcherCameraControl camera, uint device, uint category, uint parameter, uint expectedCount)
        {
            return GetGeneric<short>(camera, device, category, parameter, expectedCount,
                _BMDSwitcherCameraControlParameterType
                    .bmdSwitcherCameraControlParameterTypeSigned16Bit, camera.GetInt16s);
        }

        private static int[] GetSInt32(IBMDSwitcherCameraControl camera, uint device, uint category, uint parameter, uint expectedCount)
        {
            return GetGeneric<int>(camera, device, category, parameter, expectedCount,
                _BMDSwitcherCameraControlParameterType
                    .bmdSwitcherCameraControlParameterTypeSigned32Bit, camera.GetInt32s);
        }

        private static double[] GetFloats(IBMDSwitcherCameraControl camera, uint device, uint category, uint parameter, uint expectedCount)
        {
            return GetGeneric<double>(camera, device, category, parameter, expectedCount,
                _BMDSwitcherCameraControlParameterType
                    .bmdSwitcherCameraControlParameterTypeFixedPoint16Bit, camera.GetFloats);
        }


        public static void Build(AtemState state, IBMDSwitcherCameraControl camera)
        {

            state.CameraControl = new Dictionary<long, CameraControlState>();
            /*
            foreach (VideoSource videoSource in state.Settings.Inputs.Keys)
            {
                var cState = state.CameraControl[(long) videoSource] = new CameraControlState();
            }
            */

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherCameraControlParameterIterator>(camera.CreateIterator);
            var lastDevice = UInt32.MaxValue;
            var lastCategory = 0u;
            var lastParameter = 0u;
            for (iterator.Next(out var device, out var category, out var parameter); device != 0; iterator.Next(out device, out category, out parameter))
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

                switch ((AdjustmentDomain)category)
                {
                    case AdjustmentDomain.Lens:
                        switch ((LensFeature)parameter)
                        {
                            case LensFeature.Focus:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 1);
                                cState.Lens.Focus = (int) values[0]; // TODO
                                break;
                            }
                            case LensFeature.AutoFocus:
                                // TODO
                                break;
                            case LensFeature.Iris:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 1);
                                cState.Lens.Iris = (uint) values[0]; // TODO
                                break;
                            }
                            case LensFeature.Zoom:
                                // TODO
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case AdjustmentDomain.Camera:
                        switch ((CameraFeature) parameter)
                        {
                            case CameraFeature.PositiveGain:
                            {
                                sbyte[] values = GetSInt8(camera, device, category, parameter, 1);
                                cState.Camera.PositiveGain = (uint) values[0];
                                break;
                            }
                            case CameraFeature.WhiteBalance:
                            {
                                short[] values = GetSInt16(camera, device, category, parameter, 1);
                                cState.Camera.WhiteBalance = (uint) values[0];
                                break;
                            }
                            case CameraFeature.Shutter:
                            {
                                int[] values = GetSInt32(camera, device, category, parameter, 1);
                                cState.Camera.Shutter = (uint)values[0];
                                break;
                            }
                            case CameraFeature.Detail:
                            {
                                sbyte[] values = GetSInt8(camera, device, category, parameter, 1);
                                cState.Camera.Detail = (CameraDetail)values[0];
                                break;
                            }
                            case CameraFeature.Gain:
                            {
                                sbyte[] values = GetSInt8(camera, device, category, parameter, 1);
                                cState.Camera.Gain = (int)values[0];
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case AdjustmentDomain.ColourBars:
                        if (parameter == 4)
                        {
                            sbyte[] values = GetSInt8(camera, device, category, parameter, 1);
                            // TODO - should this really be converting from int to bool?
                            cState.ColorBars = values[0] == 1;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case AdjustmentDomain.Chip:
                        switch ((ChipFeature) parameter)
                        {
                            case ChipFeature.Lift:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 4);
                                cState.Chip.Lift.R = (int)values[0];
                                cState.Chip.Lift.G = (int)values[1];
                                cState.Chip.Lift.B = (int)values[2];
                                cState.Chip.Lift.Y = (int)values[3];
                                break;
                            }
                            case ChipFeature.Gamma:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 4);
                                cState.Chip.Gamma.R = (int)values[0];
                                cState.Chip.Gamma.G = (int)values[1];
                                cState.Chip.Gamma.B = (int)values[2];
                                cState.Chip.Gamma.Y = (int)values[3];
                                break;
                            }
                            case ChipFeature.Aperture:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 4);
                                cState.Chip.Aperture = (uint) values[0]; // TODO
                                break;
                            }
                            case ChipFeature.Gain:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 4);
                                cState.Chip.Gain.R = (int)values[0];
                                cState.Chip.Gain.G = (int)values[1];
                                cState.Chip.Gain.B = (int)values[2];
                                cState.Chip.Gain.Y = (int)values[3];
                                break;
                            }
                            case ChipFeature.Contrast:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 2);
                                cState.Chip.Contrast = (uint)values[0]; // TODO
                                break;
                            }
                            case ChipFeature.Lum:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 1);
                                cState.Chip.LumMix = (uint)values[0];
                                break;
                            }
                            case ChipFeature.HueSaturation:
                            {
                                double[] values = GetFloats(camera, device, category, parameter, 2);
                                cState.Chip.Hue = (int)values[0];
                                cState.Chip.Saturation = (uint)values[1];
                                break;
                            }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    //default:
                    //throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}