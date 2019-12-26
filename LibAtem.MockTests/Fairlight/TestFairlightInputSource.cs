using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Common;
using LibAtem.ComparisonTests;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightInputSource
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightInputSource(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherFairlightAudioSource GetSource(IBMDSwitcherFairlightAudioInput input, long? targetId = null)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioSourceIterator>(input.CreateIterator);
            if (targetId.HasValue)
            {
                iterator.GetById(targetId.Value, out IBMDSwitcherFairlightAudioSource src);
                return src;
            }
            else
            {
                iterator.Next(out IBMDSwitcherFairlightAudioSource src);
                return src;
            }
        }

        private static IBMDSwitcherFairlightAudioSource GetSource(AtemMockServerWrapper helper, long inputId,
            long? sourceId = null)
        {
            return GetSource(TestFairlightInput.GetInput(helper, inputId), sourceId);
        }

        public static IBMDSwitcherFairlightAudioDynamicsProcessor GetDynamics(IBMDSwitcherFairlightAudioSource src)
        {
            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(src.GetEffect);
            Assert.NotNull(dynamics);
            return dynamics;
        }

        public static IBMDSwitcherFairlightAudioEqualizer GetEqualizer(IBMDSwitcherFairlightAudioSource src)
        {
            var eq = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(src.GetEffect);
            Assert.NotNull(eq);
            return eq;
        }

        public static void EachRandomSource(AtemMockServerWrapper helper, Action<AtemState, FairlightAudioState.InputSourceState, long, IBMDSwitcherFairlightAudioSource, int> fcn, int maxIterations = 5, bool useAll = false)
        {
            List<long> useIds = helper.Helper.LibState.Fairlight.Inputs.Keys.ToList();
            if (!useAll) useIds = Randomiser.SelectionOfGroup(useIds, 2).ToList();

            foreach (long id in useIds)
            {
                helper.Helper.SyncStates();

                IBMDSwitcherFairlightAudioSource src = GetSource(helper, id);
                src.GetId(out long sourceId);

                AtemState stateBefore = helper.Helper.LibState;
                FairlightAudioState.InputSourceState srcState = stateBefore.Fairlight.Inputs[id].Sources.Single(s => s.SourceId == sourceId);

                for (int i = 0; i < maxIterations; i++)
                {
                    fcn(stateBefore, srcState, id, src, i);
                }
            }
        }

        // TODO - test modifying multiple sources

        [Fact]
        public void TestFaderGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("FaderGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    tested = true;
                    var target = Randomiser.Range();
                    srcState.FaderGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetFaderGain(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("Gain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    tested = true;
                    var target = Randomiser.Range();
                    srcState.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetInputGain(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBalance()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("Balance");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    tested = true;
                    var target = Randomiser.Range(-100, 100);
                    srcState.Balance = target;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetPan(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMixOption()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("MixOption");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    var testConfigs = AtemSDKConverter.GetFlagsValues(src.GetSupportedMixOptions,
                        AtemEnumMaps.FairlightAudioMixOptionMap);
                    // Need more than 1 config to allow for switching around
                    if (1 == testConfigs.Count) return;
                    tested = true;
                    var target = testConfigs[i % testConfigs.Count];

                    srcState.MixOption = target.Item2;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetMixOption(target.Item1); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMakeUpGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("MakeUpGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = GetDynamics(src);
                    tested = true;

                    var target = Randomiser.Range(0, 20);
                    srcState.Dynamics.MakeUpGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { dynamics.SetMakeupGain(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestEqualizerEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("EqualizerEnabled");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioEqualizer eq = GetEqualizer(src);
                    tested = true;

                    srcState.Equalizer.Enabled = i % 2 == 1;
                    helper.SendAndWaitForChange(stateBefore, () => { eq.SetEnabled(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestEqualizerGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("EqualizerGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioEqualizer eq = GetEqualizer(src);
                    tested = true;

                    var target = Randomiser.Range(-20, 20);
                    srcState.Equalizer.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { eq.SetGain(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestEqualizerReset()
        {
            var target = new FairlightMixerSourceEqualizerResetCommand { Equalizer = true };
            var handler = CommandGenerator.MatchCommand(target);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioEqualizer eq = GetEqualizer(src);

                    target.Index = (AudioSource)inputId;
                    target.SourceId = srcState.SourceId;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { eq.Reset(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }, 1);
            });
        }

        [Fact]
        public void TestDynamicsReset()
        {
            var target = new FairlightMixerSourceDynamicsResetCommand { Dynamics = true };
            var handler = CommandGenerator.MatchCommand(target);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = GetDynamics(src);

                    target.Index = (AudioSource)inputId;
                    target.SourceId = srcState.SourceId;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { dynamics.Reset(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }, 1);
            });
        }

        [Fact]
        public void TestDynamicsResetInputPeakLevels()
        {
            var expected = new FairlightMixerSourceResetPeakLevelsCommand { DynamicsInput = true };
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = GetDynamics(src);

                    expected.Index = (AudioSource)inputId;
                    expected.SourceId = srcState.SourceId;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { dynamics.ResetInputPeakLevels(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                });
            });
        }

        [Fact]
        public void TestDynamicsResetOutputPeakLevels()
        {
            var expected = new FairlightMixerSourceResetPeakLevelsCommand { DynamicsOutput = true };
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = GetDynamics(src);

                    expected.Index = (AudioSource)inputId;
                    expected.SourceId = srcState.SourceId;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { dynamics.ResetOutputPeakLevels(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                });
            });
        }

        [Fact]
        public void TestFramesDelay()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("FramesDelay");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightDelay, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    src.GetMaxDelayFrames(out ushort maxDelay);
                    //_output.WriteLine("{0} = {1}", inputId, maxDelay);
                    if (maxDelay <= 1) return;
                    tested = true;

                    var target = 1 + Randomiser.RangeInt((uint) (maxDelay - 1));
                    srcState.FramesDelay = target;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetDelayFrames((ushort) target); });
                }, 5, true);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestHasStereoSimulation()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("StereoSimulation");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightDelay, helper =>
            {
                var rawPackets = WiresharkParser.BuildCommands(helper.Server.CurrentVersion, helper.Server.CurrentCase);
                var rawCommands = WiresharkParser.ParseToCommands(helper.Server.CurrentVersion, rawPackets);
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    src.HasStereoSimulation(out int available);
                    if (available != 0) return;
                    tested = true;

                    var srcCommand = rawCommands.OfType<FairlightMixerSourceGetCommand>().Single(c => c.Index == (AudioSource)inputId && c.SourceId == srcState.SourceId);
                    srcCommand.HasStereoSimulation = true;

                    srcState.HasStereoSimulation = true;
                    helper.SendAndWaitForChange(stateBefore, () => { helper.Server.SendCommands(srcCommand); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestStereoSimulation()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("StereoSimulation");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightDelay, helper =>
            {
                var rawPackets = WiresharkParser.BuildCommands(helper.Server.CurrentVersion, helper.Server.CurrentCase);
                var rawCommands = WiresharkParser.ParseToCommands(helper.Server.CurrentVersion, rawPackets);
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        var target = Randomiser.Range(0, 100, 100);
                        srcState.StereoSimulation = target;
                        src.SetStereoSimulationIntensity(target);
                    });
                });
            });
        }

        [Fact]
        public void TestResetPeakLevels()
        {
            var expected = new FairlightMixerSourceResetPeakLevelsCommand { Output = true };
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    expected.Index = (AudioSource)inputId;
                    expected.SourceId = srcState.SourceId;

                    helper.SendAndWaitForChange(null, () => { src.ResetOutputPeakLevels(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                });
            });
        }
        
        private class MainLevelCallback : IBMDSwitcherFairlightAudioSourceCallback
        {
            public double[] Levels { get; private set; } = new double[0];
            public double[] Peaks { get; private set; } = new double[0];

            public void Reset()
            {
                Levels = Peaks = new double[0];
            }

            public void Notify(_BMDSwitcherFairlightAudioSourceEventType eventType)
            {
                // Ignore
            }

            public void OutputLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
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
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    var cb = new MainLevelCallback();
                    using (new UseCallback<MainLevelCallback>(cb, src.AddCallback, src.RemoveCallback))
                    {
                        cb.Reset();

                        long sourceId = srcState.SourceId;
                        var testCmd = new FairlightMixerSourceLevelsCommand
                        {
                            Index = (AudioSource)inputId,
                            SourceId = sourceId,

                            LeftLevel = Randomiser.Range(-100, 0),
                            RightLevel = Randomiser.Range(-100, 0),
                            LeftPeak = Randomiser.Range(-100, 0),
                            RightPeak = Randomiser.Range(-100, 0),
                        };

                        srcState.Levels = new FairlightAudioState.LevelsState
                        {
                            Levels = new[] { testCmd.LeftLevel, testCmd.RightLevel },
                            Peaks = new[] { testCmd.LeftPeak, testCmd.RightPeak },
                            DynamicsInputLevels = new double[2],
                            DynamicsInputPeaks = new double[2],
                            DynamicsOutputLevels = new double[2],
                            DynamicsOutputPeaks = new double[2],
                        };

                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            helper.Server.SendCommands(testCmd);
                        }, -1, (sdkState, libState) =>
                        {
                            var srcState = sdkState.Fairlight.Inputs[(long)testCmd.Index].Sources.Single(s => s.SourceId == testCmd.SourceId);
                            srcState.Levels = new FairlightAudioState.LevelsState
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
                });
            });
        }

        [Fact]
        public void TestDynamicsLevelsAndPeaks()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    var cb = new TestFairlightProgramOut.DynamicsLevelCallback();
                    var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(src.GetEffect);
                    using (new UseCallback<TestFairlightProgramOut.DynamicsLevelCallback>(cb, dynamics.AddCallback, dynamics.RemoveCallback))
                    {
                        cb.Reset();

                        long sourceId = srcState.SourceId;
                        var testCmd = new FairlightMixerSourceLevelsCommand
                        {
                            Index = (AudioSource)inputId,
                            SourceId = sourceId,

                            InputLeftLevel = Randomiser.Range(-100, 0),
                            InputLeftPeak = Randomiser.Range(-100, 0),
                            InputRightLevel = Randomiser.Range(-100, 0),
                            InputRightPeak = Randomiser.Range(-100, 0),
                            OutputLeftLevel = Randomiser.Range(-100, 0),
                            OutputLeftPeak = Randomiser.Range(-100, 0),
                            OutputRightLevel = Randomiser.Range(-100, 0),
                            OutputRightPeak = Randomiser.Range(-100, 0),
                        };

                        srcState.Levels = new FairlightAudioState.LevelsState
                        {
                            Levels = new double[2],
                            Peaks = new double[2],
                            DynamicsInputLevels = new[] { testCmd.InputLeftLevel, testCmd.InputRightLevel },
                            DynamicsInputPeaks = new[] { testCmd.InputLeftPeak, testCmd.InputRightPeak },
                            DynamicsOutputLevels = new[] { testCmd.OutputLeftLevel, testCmd.OutputRightLevel },
                            DynamicsOutputPeaks = new[] { testCmd.OutputLeftPeak, testCmd.OutputRightPeak },
                        };

                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            helper.Server.SendCommands(testCmd);
                        }, -1, (sdkState, libState) =>
                        {
                            var srcState = sdkState.Fairlight.Inputs[(long)testCmd.Index].Sources.Single(s => s.SourceId == testCmd.SourceId);
                            srcState.Levels = new FairlightAudioState.LevelsState
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
                });
            });
        }

        [Fact]
        public void TestDynamicsGainReduction()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
                {
                    var cbCompressor = new TestFairlightProgramOut.DynamicEffectsLevelCallback();
                    var cbLimiter = new TestFairlightProgramOut.DynamicEffectsLevelCallback();
                    var cbExpander = new TestFairlightProgramOut.DynamicEffectsLevelCallback();
                    var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(src.GetEffect);
                    var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
                    var expander = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(dynamics.GetProcessor);
                    var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);

                    using (new UseCallback<TestFairlightProgramOut.DynamicEffectsLevelCallback>(cbCompressor, compressor.AddCallback, compressor.RemoveCallback))
                    using (new UseCallback<TestFairlightProgramOut.DynamicEffectsLevelCallback>(cbLimiter, limiter.AddCallback, limiter.RemoveCallback))
                    using (new UseCallback<TestFairlightProgramOut.DynamicEffectsLevelCallback>(cbExpander, expander.AddCallback, expander.RemoveCallback))
                    {
                        long sourceId = srcState.SourceId;
                        var testCmd = new FairlightMixerSourceLevelsCommand
                        {
                            Index = (AudioSource)inputId,
                            SourceId = sourceId,

                            CompressorGainReduction = Randomiser.Range(-100, 0),
                            LimiterGainReduction = Randomiser.Range(-100, 0),
                            ExpanderGainReduction = Randomiser.Range(-100, 0)
                        };

                        srcState.Levels = new FairlightAudioState.LevelsState
                        {
                            Levels = new double[2],
                            Peaks = new double[2],
                            DynamicsInputLevels = new double[2],
                            DynamicsInputPeaks = new double[2],
                            DynamicsOutputLevels = new double[2],
                            DynamicsOutputPeaks = new double[2],
                            CompressorGainReductionLevel = testCmd.CompressorGainReduction,
                            LimiterGainReductionLevel = testCmd.LimiterGainReduction,
                            ExpanderGainReductionLevel = testCmd.ExpanderGainReduction
                        };

                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            helper.Server.SendCommands(testCmd);
                        }, -1, (sdkState, libState) =>
                        {
                            var srcState = sdkState.Fairlight.Inputs[(long)testCmd.Index].Sources.Single(s => s.SourceId == testCmd.SourceId);
                            srcState.Levels = new FairlightAudioState.LevelsState
                            {
                                Levels = new double[2],
                                Peaks = new double[2],
                                DynamicsInputLevels = new double[2],
                                DynamicsInputPeaks = new double[2],
                                DynamicsOutputLevels = new double[2],
                                DynamicsOutputPeaks = new double[2],
                                CompressorGainReductionLevel = cbCompressor.GainReduction.Single(),
                                LimiterGainReductionLevel = cbLimiter.GainReduction.Single(),
                                ExpanderGainReductionLevel = cbExpander.GainReduction.Single(),
                            };
                        });
                    }
                });
            });
        }

    }
}