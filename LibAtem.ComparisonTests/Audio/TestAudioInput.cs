using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Commands.MixEffects;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.State;
using LibAtem.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Audio
{
    [Collection("Client")]
    public class TestAudioInput
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestAudioInput(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        protected List<Tuple<AudioSource, IBMDSwitcherAudioInput>> GetInputs()
        {
            var mixer = _client.SdkSwitcher as IBMDSwitcherAudioMixer;
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
            GetInputs().ForEach(i => Assert.True(i.Item1.IsValid()));

            long[] sdkIds = GetInputs().Select(i => (long)i.Item1).OrderBy(i => i).ToArray();
            long[] libIds = _client.LibState.Audio.Inputs.Keys.OrderBy(i => i).ToArray();
            Assert.True(sdkIds.SequenceEqual(libIds));
        }

        private abstract class AudioInputTestDefinition<T> : TestDefinitionBase<AudioMixerInputSetCommand, T>
        {
            protected readonly AudioSource _id;
            protected readonly IBMDSwitcherAudioInput _sdk;

            public AudioInputTestDefinition(AtemComparisonHelper helper, Tuple<AudioSource, IBMDSwitcherAudioInput> prop) : base(helper)
            {
                _id = prop.Item1;
                _sdk = prop.Item2;
            }

            public override void SetupCommand(AudioMixerInputSetCommand cmd)
            {
                cmd.Index = _id;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                AudioState.InputState obj = state.Audio.Inputs[(long)_id];
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"Audio.Inputs.{_id}";
            }
        }

        private class AudioInputMixOptionTestDefinition : AudioInputTestDefinition<AudioMixOption>
        {
            public AudioInputMixOptionTestDefinition(AtemComparisonHelper helper, Tuple<AudioSource, IBMDSwitcherAudioInput> prop) : base(helper, prop)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetMixOption(AtemEnumMaps.AudioMixOptionMap[GoodValues.Last()]);

            public override string PropertyName => "MixOption";
            public override AudioMixOption MangleBadValue(AudioMixOption v) => v;
            public override void UpdateExpectedState(AtemState state, bool goodValue, AudioMixOption v)
            {
                if (goodValue)
                {
                    var props = state.Audio.Inputs[(long)_id];
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
                            VideoSource? source = _id.GetVideoSource();
                            if (source.HasValue)
                            {
                                props.IsMixedIn = state.Settings.Inputs[source.Value].Tally.ProgramTally;
                                if (VideoSourceLists.MediaPlayers.Contains(source.Value))
                                    props.IsMixedIn |= state.Settings.Inputs[source.Value + 1].Tally.ProgramTally;
                            }
                            break;
                    }
                }
            }

            public override AudioMixOption[] GoodValues
            {
                get
                {
                    var state = _helper.LibState.Audio.Inputs[(long)_id];
                    var fullProps = _helper.Client.FindWithMatching(new AudioMixerInputGetCommand() { Index = _id });
                    switch (fullProps.SourceType)
                    {
                        case AudioSourceType.ExternalAudio:
                            return new[] { AudioMixOption.On, AudioMixOption.Off };
                        case AudioSourceType.ExternalVideo:
                            return Enum.GetValues(typeof(AudioMixOption)).OfType<AudioMixOption>().ToArray();
                        case AudioSourceType.MediaPlayer:
                            return new[] { AudioMixOption.AudioFollowVideo, AudioMixOption.Off };
                        default:
                            throw new InvalidOperationException("Invalid AudioSourceType: " + fullProps.SourceType);
                    }
                }
            }
            public override AudioMixOption[] BadValues => Enum.GetValues(typeof(AudioMixOption)).OfType<AudioMixOption>().Except(GoodValues).ToArray();

            public override IEnumerable<string> ExpectedCommands(bool goodValue, AudioMixOption v)
            {
                foreach (var c in base.ExpectedCommands(goodValue, v))
                    yield return c;

                if (goodValue)
                    yield return $"Audio.Inputs.{_id}.Tally";
            }
        }
        [Fact]
        public void TestMixOption()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetInputs().ForEach(i => new AudioInputMixOptionTestDefinition(helper, i).Run());
        }

        private class AudioInputGainTestDefinition : AudioInputTestDefinition<double>
        {
            public AudioInputGainTestDefinition(AtemComparisonHelper helper, Tuple<AudioSource, IBMDSwitcherAudioInput> prop) : base(helper, prop)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "Gain";
            public override double MangleBadValue(double v) => v;

            public override double[] GoodValues => new double[] { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };
        }
        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetInputs().ForEach(i => new AudioInputGainTestDefinition(helper, i).Run());
        }

        private class AudioInputBalanceTestDefinition : AudioInputTestDefinition<double>
        {
            public AudioInputBalanceTestDefinition(AtemComparisonHelper helper, Tuple<AudioSource, IBMDSwitcherAudioInput> prop) : base(helper, prop)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBalance(20);

            public override string PropertyName => "Balance";
            public override double MangleBadValue(double v) => v < -50 ? -50 : 50;

            public override double[] GoodValues => new double[] { -50, -49.99, -49, 10, 0, 35.84, 49.99, 50 };
            public override double[] BadValues => new double[] { -50.01, -51, 50.01, 51 };
        }
        [Fact]
        public void TestBalance()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetInputs().ForEach(i => new AudioInputBalanceTestDefinition(helper, i).Run());
        }

        [Fact]
        public void TestLevels()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            using (new MediaPoolUtil.SolidClipUploadHelper(helper, 0, "clip", 1))
            using (new MediaPoolUtil.AudioClipUploadHelper(helper, 0, "tone24bit", 5292))
            using (new MediaPoolUtil.MediaPlayingHelper(helper, MediaPlayerId.One, 0))
            using (new SendAudioLevelsHelper(helper))
            {
                var levelsKey = $"Audio.Inputs.{AudioSource.MP1}.Levels";
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, new ProgramInputSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Source = VideoSource.MediaPlayer1
                });

                AudioState.InputState mixer = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.False(double.IsNegativeInfinity(mixer.LevelLeft));
                Assert.False(double.IsNegativeInfinity(mixer.LevelRight));
                Assert.False(double.IsNegativeInfinity(mixer.PeakLeft));
                Assert.False(double.IsNegativeInfinity(mixer.PeakRight));
                Assert.Equal(-0.3, mixer.LevelLeft, 1);
                Assert.Equal(mixer.LevelLeft, mixer.LevelRight, 1);
                Assert.Equal(mixer.PeakLeft, mixer.PeakRight, 1);
                helper.AssertStatesMatch();

                // Drop the volume, and check it got quieter
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerInputSetCommand
                {
                    Index = AudioSource.MP1,
                    Mask = AudioMixerInputSetCommand.MaskFlags.Gain,
                    Gain = -10
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.InputState mixer2 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.Equal(-10.3, mixer2.LevelLeft, 1);
                Assert.Equal(mixer2.LevelLeft, mixer2.LevelRight, 1);
                Assert.Equal(mixer2.PeakLeft, mixer2.PeakRight, 1);
                Assert.Equal(mixer.PeakLeft, mixer2.PeakLeft, 1); // Shouldnt have changed
                helper.AssertStatesMatch();

                // Change balance and ensure it applied
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerInputSetCommand
                {
                    Index = AudioSource.MP1,
                    Mask = AudioMixerInputSetCommand.MaskFlags.Balance,
                    Balance = -20
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.InputState mixer5 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.Equal(-14.7, mixer5.LevelRight, 1);
                Assert.Equal(mixer2.LevelLeft, mixer5.LevelLeft, 1);
                Assert.Equal(mixer5.PeakLeft, mixer5.PeakRight, 1);
                Assert.NotEqual(mixer2.PeakLeft, mixer5.PeakLeft, 1); // Doing a pan should reset
                helper.AssertStatesMatch();

                // Reset peaks and ensure they changed
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerInputSetCommand
                {
                    Index = AudioSource.MP1,
                    Mask = AudioMixerInputSetCommand.MaskFlags.Balance,
                    Balance = 0
                });
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerResetPeaksCommand
                {
                    Mask = AudioMixerResetPeaksCommand.MaskFlags.Input,
                    Input = AudioSource.MP1
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.InputState mixer3 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.Equal(mixer2.LevelLeft, mixer3.LevelLeft, 1); // Shouldnt have changed
                Assert.Equal(mixer3.LevelLeft, mixer3.LevelRight, 1);
                Assert.Equal(-10.3, mixer3.PeakLeft, 1);
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
                    Input = AudioSource.MP1
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.InputState mixer4 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.Equal(mixer3.LevelLeft, mixer4.LevelLeft, 1);
                Assert.Equal(mixer3.LevelRight, mixer4.LevelRight, 1);
                Assert.Equal(mixer3.PeakLeft, mixer4.PeakLeft, 1);
                Assert.Equal(mixer3.PeakRight, mixer4.PeakRight, 1);
            }
        }
    }
}
