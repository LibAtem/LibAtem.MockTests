using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightProgramOut
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightProgramOut(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        public static IBMDSwitcherFairlightAudioMixer GetFairlightMixer(AtemMockServerWrapper helper)
        {
            var mixer = helper.Helper.SdkSwitcher as IBMDSwitcherFairlightAudioMixer;
            Assert.NotNull(mixer);
            return mixer;
        }

        public static IBMDSwitcherFairlightAudioDynamicsProcessor GetDynamics(AtemMockServerWrapper helper)
        {
            var mixer = GetFairlightMixer(helper);
            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(mixer.GetMasterOutEffect);
            Assert.NotNull(dynamics);
            return dynamics;
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterSetCommand, FairlightMixerMasterGetCommand>("Gain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    stateBefore.Fairlight.ProgramOut.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        mixer.SetMasterOutFaderGain(target);
                    });
                }
            });
        }

        [Fact]
        public void TestFollowFadeToBlack()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterSetCommand, FairlightMixerMasterGetCommand>("FollowFadeToBlack");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    stateBefore.Fairlight.ProgramOut.FollowFadeToBlack = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        mixer.SetMasterOutFollowFadeToBlack(i % 2);
                    });
                }
            });
        }

        [Fact]
        public void TestMakeUp()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterSetCommand, FairlightMixerMasterGetCommand>("MakeUpGain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = GetDynamics(helper);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range(0, 20);
                    stateBefore.Fairlight.ProgramOut.Dynamics.MakeUpGain = target;
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        dynamics.SetMakeupGain(target);
                    });
                }
            });
        }

        [Fact]
        public void TestAudioFollowVideoCrossfadeTransitionEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerMasterPropertiesSetCommand, FairlightMixerMasterPropertiesGetCommand>("AudioFollowVideoCrossfadeTransitionEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    stateBefore.Fairlight.ProgramOut.AudioFollowVideoCrossfadeTransitionEnabled = i % 2 > 0;
                    helper.SendAndWaitForChange(stateBefore,
                        () => { mixer.SetAudioFollowVideoCrossfadeTransition(i % 2); });
                }
            });
        }

        private class MasterLevelCallback : IBMDSwitcherFairlightAudioMixerCallback
        {
            public double[] Levels { get; private set; } = new double[0];
            public double[] Peaks { get; private set; } = new double[0];

            public void Reset()
            {
                Levels = Peaks = new double[0];
            }

            public void Notify(_BMDSwitcherFairlightAudioMixerEventType eventType)
            {
                // Ignore
            }

            public void MasterOutLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
            {
                Levels = Randomiser.ConvertDoubleArray(numLevels, ref levels);
                Peaks = Randomiser.ConvertDoubleArray(numPeakLevels, ref peakLevels);
            }
        }

        [Fact]
        public void TestLevelsAndPeaks()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);

                var cb = new MasterLevelCallback();
                using (new UseCallback<MasterLevelCallback>(cb, mixer.AddCallback, mixer.RemoveCallback))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        cb.Reset();

                        AtemState expectedState = helper.Helper.BuildLibState();

                        var testCmd = new FairlightMixerMasterLevelsCommand
                        {
                            LeftLevel = Randomiser.Range(-100, 0),
                            RightLevel = Randomiser.Range(-100, 0),
                            LeftPeak = Randomiser.Range(-100, 0),
                            RightPeak = Randomiser.Range(-100, 0),
                        };

                        expectedState.Fairlight.ProgramOut.Levels = new FairlightAudioState.LevelsState
                        {
                            Levels = new[] { testCmd.LeftLevel, testCmd.RightLevel },
                            Peaks = new[] { testCmd.LeftPeak, testCmd.RightPeak },
                            DynamicsInputLevels = new double[2],
                            DynamicsInputPeaks = new double[2],
                            DynamicsOutputLevels = new double[2],
                            DynamicsOutputPeaks = new double[2],
                        };

                        helper.SendAndWaitForChange(expectedState, () =>
                        {
                            helper.Server.SendCommands(testCmd);
                        }, -1, (sdkState, libState) =>
                        {
                            sdkState.Fairlight.ProgramOut.Levels = new FairlightAudioState.LevelsState
                            {
                                Levels = cb.Levels,
                                Peaks = cb.Peaks,
                                DynamicsInputLevels = new double[2],
                                DynamicsInputPeaks = new double[2],
                                DynamicsOutputLevels = new double[2],
                                DynamicsOutputPeaks = new double[2],
                            };
                        });
                    }
                }
            });
        }

        public class DynamicsLevelCallback : IBMDSwitcherFairlightAudioDynamicsProcessorCallback
        {
            public double[] InputLevels { get; private set; } = new double[0];
            public double[] InputPeaks { get; private set; } = new double[0];
            public double[] OutputLevels { get; private set; } = new double[0];
            public double[] OutputPeaks { get; private set; } = new double[0];

            public void Reset()
            {
                InputLevels = InputPeaks = OutputLevels = OutputPeaks = new double[0];
            }


            public void Notify(_BMDSwitcherFairlightAudioDynamicsProcessorEventType eventType)
            {
                // Ignore
            }

            public void InputLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
            {
                InputLevels = Randomiser.ConvertDoubleArray(numLevels, ref levels);
                InputPeaks = Randomiser.ConvertDoubleArray(numPeakLevels, ref peakLevels);
            }

            public void OutputLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
            {
                OutputLevels = Randomiser.ConvertDoubleArray(numLevels, ref levels);
                OutputPeaks = Randomiser.ConvertDoubleArray(numPeakLevels, ref peakLevels);
            }
        }

        [Fact]
        public void TestDynamicsLevelsAndPeaks()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);

                var cb = new DynamicsLevelCallback();
                var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(mixer.GetMasterOutEffect);
                using (new UseCallback<DynamicsLevelCallback>(cb, dynamics.AddCallback, dynamics.RemoveCallback))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        cb.Reset();

                        AtemState expectedState = helper.Helper.BuildLibState();
                        FairlightAudioState.ProgramOutState pgmOutState = expectedState.Fairlight.ProgramOut;

                        var testCmd = new FairlightMixerMasterLevelsCommand
                        {
                            InputLeftLevel = Randomiser.Range(-100, 0),
                            InputRightLevel = Randomiser.Range(-100, 0),
                            InputLeftPeak = Randomiser.Range(-100, 0),
                            InputRightPeak = Randomiser.Range(-100, 0),
                            OutputLeftLevel = Randomiser.Range(-100, 0),
                            OutputRightLevel = Randomiser.Range(-100, 0),
                            OutputLeftPeak = Randomiser.Range(-100, 0),
                            OutputRightPeak = Randomiser.Range(-100, 0),
                        };

                        expectedState.Fairlight.ProgramOut.Levels = new FairlightAudioState.LevelsState
                        {
                            Levels = new double[2],
                            Peaks = new double[2],
                            DynamicsInputLevels = new[] { testCmd.InputLeftLevel, testCmd.InputRightLevel },
                            DynamicsInputPeaks = new[] { testCmd.InputLeftPeak, testCmd.InputRightPeak },
                            DynamicsOutputLevels = new[] { testCmd.OutputLeftLevel, testCmd.OutputRightLevel },
                            DynamicsOutputPeaks = new[] { testCmd.OutputLeftPeak, testCmd.OutputRightPeak },
                        };

                        helper.SendAndWaitForChange(expectedState, () =>
                        {
                            helper.Server.SendCommands(testCmd);
                        }, -1, (sdkState, libState) =>
                        {
                            sdkState.Fairlight.ProgramOut.Levels = new FairlightAudioState.LevelsState
                            {
                                Levels = new double[2],
                                Peaks = new double[2],
                                DynamicsInputLevels = cb.InputLevels,
                                DynamicsInputPeaks = cb.InputPeaks,
                                DynamicsOutputLevels = cb.OutputLevels,
                                DynamicsOutputPeaks = cb.OutputPeaks,
                            };
                        });
                    }
                }
            });
        }

        public class DynamicEffectsLevelCallback : IBMDSwitcherFairlightAudioCompressorCallback, IBMDSwitcherFairlightAudioLimiterCallback, IBMDSwitcherFairlightAudioExpanderCallback
        {
            public double[] GainReduction { get; private set; } = new double[0];

            public void Reset()
            {
                GainReduction = new double[0];
            }

            public void Notify(_BMDSwitcherFairlightAudioCompressorEventType eventType)
            {
                // Ignore
            }

            public void GainReductionLevelNotification(uint numLevels, ref double levels)
            {
                GainReduction = Randomiser.ConvertDoubleArray(numLevels, ref levels);
            }

            public void Notify(_BMDSwitcherFairlightAudioLimiterEventType eventType)
            {
                // Ignore
            }

            public void Notify(_BMDSwitcherFairlightAudioExpanderEventType eventType)
            {
                // Ignore
            }
        }

        [Fact]
        public void TestDynamicsGainReduction()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                IBMDSwitcherFairlightAudioMixer mixer = GetFairlightMixer(helper);

                var cbCompressor = new DynamicEffectsLevelCallback();
                var cbLimiter = new DynamicEffectsLevelCallback();
                var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(mixer.GetMasterOutEffect);
                var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
                var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);

                using (new UseCallback<DynamicEffectsLevelCallback>(cbCompressor, compressor.AddCallback, compressor.RemoveCallback))
                using (new UseCallback<DynamicEffectsLevelCallback>(cbLimiter, limiter.AddCallback, limiter.RemoveCallback))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        cbCompressor.Reset();
                        cbLimiter.Reset();

                        AtemState expectedState = helper.Helper.BuildLibState();

                        var testCmd = new FairlightMixerMasterLevelsCommand
                        {
                            CompressorGainReduction = Randomiser.Range(-100, 0),
                            LimiterGainReduction = Randomiser.Range(-100, 0)
                        };

                        expectedState.Fairlight.ProgramOut.Levels = new FairlightAudioState.LevelsState
                        {
                            Levels = new double[2],
                            Peaks = new double[2],
                            DynamicsInputLevels = new double[2],
                            DynamicsInputPeaks = new double[2],
                            DynamicsOutputLevels = new double[2],
                            DynamicsOutputPeaks = new double[2],
                            CompressorGainReductionLevel = testCmd.CompressorGainReduction,
                            LimiterGainReductionLevel = testCmd.LimiterGainReduction
                        };

                        helper.SendAndWaitForChange(expectedState, () =>
                        {
                            helper.Server.SendCommands(testCmd);
                        }, -1, (sdkState, libState) =>
                        {
                            sdkState.Fairlight.ProgramOut.Levels = new FairlightAudioState.LevelsState
                            {
                                Levels = new double[2],
                                Peaks = new double[2],
                                DynamicsInputLevels = new double[2],
                                DynamicsInputPeaks = new double[2],
                                DynamicsOutputLevels = new double[2],
                                DynamicsOutputPeaks = new double[2],
                                CompressorGainReductionLevel = cbCompressor.GainReduction.Single(),
                                LimiterGainReductionLevel = cbLimiter.GainReduction.Single()
                            };
                        });
                    }
                }
            });
        }

    }
}
