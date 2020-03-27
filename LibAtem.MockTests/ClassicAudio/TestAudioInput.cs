using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.ClassicAudio
{
    [Collection("ServerClientPool")]
    public class TestAudioInput
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAudioInput(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        public static IBMDSwitcherAudioInput GetInput(AtemMockServerWrapper helper, long targetId)
        {
            IBMDSwitcherAudioMixer mixer = TestAudioProgramOut.GetAudioMixer(helper);
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioInputIterator>(mixer.CreateIterator);

            iterator.GetById(targetId, out IBMDSwitcherAudioInput input);
            Assert.NotNull(input);
            return input;
        }

#if ATEM_v8_1 || ATEM_v8_1_1
        private static Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> CreateAutoCommandHandler(string name)
        {
            return CommandGenerator.CreateAutoCommandHandler<AudioMixerInputSetCommand, AudioMixerInputGetV8Command>(name);
        }

        private static AudioMixerInputGetV8Command RawGetCommand(ImmutableList<ICommand> rawCommands, long id)
        {
            return rawCommands.OfType<AudioMixerInputGetV8Command>().Single(c => c.Index == (AudioSource)id);
        }
#endif

        [Fact]
        public void TestGain()
        {
            var handler = CreateAutoCommandHandler("Gain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    tested = true;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range();
                        inputState.Gain = target;
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetGain(target); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBalance()
        {
            var handler = CreateAutoCommandHandler("Balance");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    tested = true;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(-50, 50);
                        inputState.Balance = target;
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetBalance(target / 50); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMixOption()
        {
            var handler = CreateAutoCommandHandler("MixOption");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    tested = true;
                    
                    for (int i = 0; i < 5; i++)
                    {
                        AudioMixOption target = Randomiser.EnumValue<AudioMixOption>();
                        if (id >= 2000 && target == AudioMixOption.On)
                        {
                            // Not supported
                            continue;
                        }

                        _BMDSwitcherAudioMixOption target2 = AtemEnumMaps.AudioMixOptionMap[target];
                        inputState.MixOption = target;
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetMixOption(target2); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSourceType()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.ClassicAudioMain, helper =>
            {
                var rawCommands = helper.Server.GetParsedDataDump();

                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    tested = true;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    var srcCommand = RawGetCommand(rawCommands, id);
                    for (int i = 0; i < 5; i++)
                    {
                        AudioSourceType target = Randomiser.EnumValue<AudioSourceType>();
                        inputState.SourceType = target;
                        srcCommand.SourceType = target;

                        helper.SendAndWaitForChange(stateBefore, () => { helper.Server.SendCommands(srcCommand); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPortType()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.ClassicAudioMain, helper =>
            {
                var rawCommands = helper.Server.GetParsedDataDump();

                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    tested = true;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    var srcCommand = RawGetCommand(rawCommands, id);
                    for (int i = 0; i < 5; i++)
                    {
                        AudioPortType target = Randomiser.EnumValue(AudioPortType.Unknown);
                        inputState.PortType = target;
                        srcCommand.PortType = target;

                        helper.SendAndWaitForChange(stateBefore, () => { helper.Server.SendCommands(srcCommand); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestResetPeakLevels()
        {
            bool tested = false;
            var expected = new AudioMixerResetPeaksCommand { Mask = AudioMixerResetPeaksCommand.MaskFlags.Input };
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    tested = true;
                    uint timeBefore = helper.Server.CurrentTime;

                    expected.Input = (AudioSource) id;

                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    helper.SendAndWaitForChange(null, () => { input.ResetLevelNotificationPeaks(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
            Assert.True(tested);
        }

        private class LevelCallback : IBMDSwitcherAudioInputCallback
        {
            public double[] Levels { get; private set; } = new double[0];
            public double[] Peaks { get; private set; } = new double[0];

            public void Reset()
            {
                Levels = Peaks = new double[0];
            }

            public void Notify(_BMDSwitcherAudioInputEventType eventType)
            {
                // Ignore
            }

            public void LevelNotification(double left, double right, double peakLeft, double peakRight)
            {
                Levels = new[] {left, right};
                Peaks = new[] {peakLeft, peakRight};
            }
        }

        public class DisposableList : IDisposable
        {
            public List<IDisposable> Items = new List<IDisposable>();

            public void Dispose()
            {
                Items.ForEach(i => i.Dispose());
                Items.Clear();
            }
        }

        [Fact]
        public void TestLevelsAndPeaks()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.ClassicAudioMain, helper =>
            {
                using var items = new DisposableList();

                IReadOnlyList<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                var callbacks = new Dictionary<AudioSource, LevelCallback>(useIds.Count);
                // Setup callbacks
                foreach (long id in useIds)
                {
                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    var cb = new LevelCallback();
                    cb.Reset();
                    items.Items.Add(new UseCallback<LevelCallback>(cb, input.AddCallback, input.RemoveCallback));
                    callbacks[(AudioSource) id] = cb;
                }

                // Now run the tests
                for (int i = 0; i < 5; i++)
                {
                    tested = true;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    stateBefore.Audio.ProgramOut.Levels = null;
                    var inputsState = stateBefore.Audio.Inputs;

                    var testCmd = new AudioMixerLevelsCommand();
                    foreach (long tmpId in useIds)
                    {
                        var levels = new AudioMixerLevelInput((AudioSource) tmpId)
                        {
                            LeftLevel = Randomiser.Range(-100, 0),
                            RightLevel = Randomiser.Range(-100, 0),
                            LeftPeak = Randomiser.Range(-100, 0),
                            RightPeak = Randomiser.Range(-100, 0),
                        };
                        testCmd.Inputs.Add(levels);

                        inputsState[tmpId].Levels = new AudioState.LevelsState
                        {
                            Levels = new[] { levels.LeftLevel, levels.RightLevel },
                            Peaks = new[] { levels.LeftPeak, levels.RightPeak },
                        };
                    }

                    helper.SendAndWaitForChange(stateBefore, () => { helper.Server.SendCommands(testCmd); }, -1,
                        (sdkState, libState) =>
                        {
                            libState.Audio.ProgramOut.Levels = null;

                            foreach (KeyValuePair<AudioSource, LevelCallback> i in callbacks)
                            {
                                sdkState.Audio.Inputs[(long) i.Key].Levels = new AudioState.LevelsState
                                {
                                    Levels = i.Value.Levels,
                                    Peaks = i.Value.Peaks,
                                };
                            }
                        });
                }
            });
            Assert.True(tested);
        }

    }
}