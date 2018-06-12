using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.ComparisonTests.MixEffects;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using static LibAtem.ComparisonTests.Util.MediaPoolUtil;

namespace LibAtem.ComparisonTests.Audio
{
    [Collection("Client")]
    public class TestAudioInput : ComparisonTestBase
    {
        public TestAudioInput(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        protected List<Tuple<AudioSource, IBMDSwitcherAudioInput>> GetInputs()
        {
            var mixer = Client.SdkSwitcher as IBMDSwitcherAudioMixer;
            Assert.NotNull(mixer);

            Guid itId = typeof(IBMDSwitcherAudioInputIterator).GUID;
            mixer.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherAudioInputIterator iterator = (IBMDSwitcherAudioInputIterator)Marshal.GetObjectForIUnknown(itPtr);

            var result = new List<Tuple<AudioSource, IBMDSwitcherAudioInput>>();
            for (iterator.Next(out IBMDSwitcherAudioInput r); r != null; iterator.Next(out r))
            {
                r.GetAudioInputId(out long id);
                result.Add(Tuple.Create((AudioSource)id, r));
            }

            return result;
        }

        [Fact]
        public void TestIdsValid()
        {
            foreach (var inp in GetInputs())
            {
                Assert.True(inp.Item1.IsValid());
            }

            long[] sdkIds = GetInputs().Select(i => (long)i.Item1).OrderBy(i => i).ToArray();
            long[] libIds = Client.LibState.Audio.Inputs.Keys.OrderBy(i => i).ToArray();
            Assert.True(sdkIds.SequenceEqual(libIds));
        }

        [Fact]
        public void TestMixOption()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var inp in GetInputs())
                {
                    var fullProps = Client.FindWithMatching(new AudioMixerInputGetCommand()
                    {
                        Index = inp.Item1
                    });
                    if (fullProps == null)
                    {
                        Output.WriteLine("Missing {0} in LibAtem", inp.Item1);
                        continue;
                    }

                    AudioMixOption[] testValues;
                    switch (fullProps.SourceType)
                    {
                        case AudioSourceType.ExternalAudio:
                            testValues = new[] { AudioMixOption.On, AudioMixOption.Off };
                            break;
                        case AudioSourceType.ExternalVideo:
                            testValues = Enum.GetValues(typeof(AudioMixOption)).OfType<AudioMixOption>().ToArray();
                            break;
                        case AudioSourceType.MediaPlayer:
                            testValues = new[] { AudioMixOption.AudioFollowVideo, AudioMixOption.Off };
                            break;
                        default:
                            Output.WriteLine("Invalid AudioSourceType: {0}", fullProps.SourceType);
                            continue;
                    }

                    var badValues = Enum.GetValues(typeof(AudioMixOption)).OfType<AudioMixOption>().Except(testValues).ToArray();

                    ICommand Setter(AudioMixOption v) => new AudioMixerInputSetCommand()
                    {
                        Mask = AudioMixerInputSetCommand.MaskFlags.MixOption,
                        Index = inp.Item1,
                        MixOption = v,
                    };

                    void UpdateExpectedState(ComparisonState state, AudioMixOption v) {
                        var props = state.Audio.Inputs[(long)inp.Item1];
                        props.MixOption = v;
                        switch (v)
                        {
                            case AudioMixOption.Off:
                                props.IsMixedIn = false;
                                break;
                            case AudioMixOption.On:
                                props.IsMixedIn = true;
                                break;
                            case AudioMixOption.AudioFollowVideo:
                                var video = state.Inputs[inp.Item1.GetVideoSource() ?? VideoSource.Input1];
                                props.IsMixedIn = video.ProgramTally;
                                break;
                        }
                    }

                    ValueTypeComparer<AudioMixOption>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<AudioMixOption>.Fail(helper, Setter, badValues);
                }
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var inp in GetInputs())
                {
                    double[] testValues = { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };

                    ICommand Setter(double v) => new AudioMixerInputSetCommand()
                    {
                        Mask = AudioMixerInputSetCommand.MaskFlags.Gain,
                        Index = inp.Item1,
                        Gain = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.Audio.Inputs[(long)inp.Item1].Gain = v;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestBalance()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var inp in GetInputs())
                {
                    double[] testValues = { -50, -49.99, -49, 10, 0, 35.84, 49.99, 50 };
                    double[] badValues = { -50.01, -51, 50.01, 51 };

                    ICommand Setter(double v) => new AudioMixerInputSetCommand()
                    {
                        Mask = AudioMixerInputSetCommand.MaskFlags.Balance,
                        Index = inp.Item1,
                        Balance = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.Audio.Inputs[(long)inp.Item1].Balance = v;
                    void UpdateBadState(ComparisonState state, double v) => state.Audio.Inputs[(long)inp.Item1].Balance = v < -50 ? -50 : 50;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateBadState, badValues);
                }
            }
        }

        [Fact]
        public void TestLevels()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            using (new MediaPlayingHelper(helper))
            using (new SendAudioLevelsHelper(helper))
            {
                var levelsKey = new CommandQueueKey(new AudioMixerLevelsCommand());
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                ComparisonAudioInputState mixer = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.False(double.IsNegativeInfinity(mixer.LevelLeft));
                Assert.False(double.IsNegativeInfinity(mixer.LevelRight));
                Assert.False(double.IsNegativeInfinity(mixer.PeakLeft));
                Assert.False(double.IsNegativeInfinity(mixer.PeakRight));
                Assert.Equal(-1.9, mixer.LevelLeft, 1);
                Assert.Equal(mixer.LevelLeft, mixer.LevelRight, 1);
                Assert.Equal(mixer.PeakLeft, mixer.PeakRight, 1);
                helper.AssertStatesMatch();

                // Drop the volume, and check it got quieter
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerInputSetCommand
                {
                    Index = Common.AudioSource.MP1,
                    Mask = AudioMixerInputSetCommand.MaskFlags.Gain,
                    Gain = -10
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                ComparisonAudioInputState mixer2 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.Equal(-11.9, mixer2.LevelLeft, 1);
                Assert.Equal(mixer2.LevelLeft, mixer2.LevelRight, 1);
                Assert.Equal(mixer2.PeakLeft, mixer2.PeakRight, 1);
                Assert.Equal(mixer.PeakLeft, mixer2.PeakLeft, 1); // Shouldnt have changed
                helper.AssertStatesMatch();

                // Reset peaks and ensure they changed
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerResetPeaksCommand
                {
                    Mask = AudioMixerResetPeaksCommand.MaskFlags.Input,
                    Source = AudioSource.MP1
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                ComparisonAudioInputState mixer3 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.Equal(mixer2.LevelLeft, mixer3.LevelLeft, 1); // Shouldnt have changed
                Assert.Equal(mixer3.LevelLeft, mixer3.LevelRight, 1);
                Assert.Equal(-11.9, mixer3.PeakLeft, 1);
                Assert.Equal(mixer3.PeakLeft, mixer3.PeakRight, 1);
                // helper.AssertStatesMatch();

                // Mute the media player
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerInputSetCommand
                {
                    Index = Common.AudioSource.MP1,
                    Mask = AudioMixerInputSetCommand.MaskFlags.MixOption,
                    MixOption = Common.AudioMixOption.Off
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerResetPeaksCommand
                {
                    Mask = AudioMixerResetPeaksCommand.MaskFlags.Input,
                    Source = AudioSource.MP1
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                ComparisonAudioInputState mixer4 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.Equal(mixer3.LevelLeft, mixer4.LevelLeft, 1);
                Assert.Equal(mixer3.LevelRight, mixer4.LevelRight, 1);
                Assert.Equal(mixer3.PeakLeft, mixer4.PeakLeft, 1);
                Assert.Equal(mixer3.PeakRight, mixer4.PeakRight, 1);
            }
        }
    }
}
