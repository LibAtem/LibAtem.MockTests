using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Commands.MixEffects;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.State;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.Audio
{
    [Collection("Client")]
    public class TestAudioMaster
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestAudioMaster(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        private abstract class AudioMasterTestDefinition<T> : TestDefinitionBase<AudioMixerMasterSetCommand, T>
        {
            protected readonly IBMDSwitcherAudioMixer _sdk;

            public AudioMasterTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMixer sdk) : base(helper)
            {
                _sdk = sdk;
            }

            public abstract T MangleBadValue(T v);

            public sealed override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                AudioState.ProgramOutState obj = state.Audio.ProgramOut;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new AudioMixerMasterGetCommand());
            }
        }

        private class AudioMasterAudioFollowVideoTestDefinition : AudioMasterTestDefinition<bool>
        {
            public AudioMasterAudioFollowVideoTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMixer sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetProgramOutFollowFadeToBlack(0);

            public override string PropertyName => "FollowFadeToBlack";
            public override bool MangleBadValue(bool v) => v;
        }
        [Fact]
        public void TestAudioFollowVideo()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new AudioMasterAudioFollowVideoTestDefinition(helper, helper.SdkSwitcher as IBMDSwitcherAudioMixer).Run();
        }

        private class AudioMasterBalanceTestDefinition : AudioMasterTestDefinition<double>
        {
            public AudioMasterBalanceTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMixer sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetProgramOutBalance(20);

            public override string PropertyName => "Balance";
            public override double MangleBadValue(double v) => v < -50 ? -50 : 50;

            public override double[] GoodValues => new double[] { -50, -49.99, -49, 10, 0, 35.84, 49.99, 50 };
            public override double[] BadValues => new double[] { -50.01, -51, 50.01, 51 };
        }
        [Fact]
        public void TestBalance()
        {
            var resetCmd = new AudioMixerMasterSetCommand
            {
                Mask = AudioMixerMasterSetCommand.MaskFlags.Balance,
                Balance = 0
            };

            using (var helper = new AtemComparisonHelper(_client, _output))
                using (new ValueDefaults(helper, resetCmd)) // We need to do this, as it cannot be set via the stock client
                    new AudioMasterBalanceTestDefinition(helper, helper.SdkSwitcher as IBMDSwitcherAudioMixer).Run();
        }

        private class AudioMasterGainTestDefinition : AudioMasterTestDefinition<double>
        {
            public AudioMasterGainTestDefinition(AtemComparisonHelper helper, IBMDSwitcherAudioMixer sdk) : base(helper, sdk)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetProgramOutGain(20);

            public override string PropertyName => "Gain";
            public override double MangleBadValue(double v) => v;

            public override double[] GoodValues => new double[] { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };
        }
        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new AudioMasterGainTestDefinition(helper, helper.SdkSwitcher as IBMDSwitcherAudioMixer).Run();
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
                helper.SendCommand(new ProgramInputSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Source = VideoSource.MediaPlayer1
                },
                new AudioMixerMasterSetCommand
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.Gain | AudioMixerMasterSetCommand.MaskFlags.Balance | AudioMixerMasterSetCommand.MaskFlags.FollowFadeToBlack,
                    Gain = 0,
                    Balance = 0,
                    FollowFadeToBlack = false
                });

                var levelsKey = new CommandQueueKey(new AudioMixerLevelsCommand());
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);


                AudioState.ProgramOutState mixer = helper.SdkState.Audio.ProgramOut;
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
                    Index = Common.AudioSource.MP1,
                    Mask = AudioMixerInputSetCommand.MaskFlags.Gain,
                    Gain = -10
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.ProgramOutState mixer2 = helper.SdkState.Audio.ProgramOut;
                Assert.Equal(-10.3, mixer2.LevelLeft, 1);
                Assert.Equal(mixer2.LevelLeft, mixer2.LevelRight, 1);
                Assert.Equal(mixer2.PeakLeft, mixer2.PeakRight, 1);
                Assert.Equal(mixer.PeakLeft, mixer2.PeakLeft, 1); // Shouldnt have changed
                helper.AssertStatesMatch();

                // Reset peaks and ensure they changed
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerResetPeaksCommand
                {
                    Mask = AudioMixerResetPeaksCommand.MaskFlags.Master,
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.ProgramOutState mixer3 = helper.SdkState.Audio.ProgramOut;
                Assert.Equal(mixer2.LevelLeft, mixer3.LevelLeft, 1); // Shouldnt have changed
                Assert.Equal(mixer3.LevelLeft, mixer3.LevelRight, 1);
                Assert.Equal(-10.3, mixer3.PeakLeft, 1);
                Assert.Equal(mixer3.PeakLeft, mixer3.PeakRight, 1);
                helper.AssertStatesMatch();

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
                    Mask = AudioMixerResetPeaksCommand.MaskFlags.Master,
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.ProgramOutState mixer4 = helper.LibState.Audio.ProgramOut;
                Assert.True(mixer4.LevelLeft < -70);
                Assert.True(mixer4.LevelRight < -70);
                Assert.True(mixer4.PeakLeft < -70);
                Assert.True(mixer4.PeakRight < -70);
            }
        }
        
        [Fact]
        public void TestResetAllPeaks()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            using (new MediaPoolUtil.SolidClipUploadHelper(helper, 0, "clip", 1))
            using (new MediaPoolUtil.AudioClipUploadHelper(helper, 0, "tone24bit", 5292))
            using (var player = new MediaPoolUtil.MediaPlayingHelper(helper, MediaPlayerId.One, 0))
            using (new SendAudioLevelsHelper(helper))
            {
                helper.SendCommand(new ProgramInputSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Source = VideoSource.MediaPlayer1
                },
                new AudioMixerMasterSetCommand
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.Gain | AudioMixerMasterSetCommand.MaskFlags.Balance | AudioMixerMasterSetCommand.MaskFlags.FollowFadeToBlack,
                    Gain = 0,
                    Balance = 0,
                    FollowFadeToBlack = false
                });

                var levelsKey = new CommandQueueKey(new AudioMixerLevelsCommand());
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.ProgramOutState mixer = helper.SdkState.Audio.ProgramOut;
                AudioState.InputState input = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.False(double.IsNegativeInfinity(mixer.PeakLeft));
                Assert.False(double.IsNegativeInfinity(mixer.PeakRight));
                Assert.False(double.IsNegativeInfinity(input.PeakLeft));
                Assert.False(double.IsNegativeInfinity(input.PeakRight));
                helper.AssertStatesMatch();

                // Stop the clip
                player.Dispose();
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                // Ensure peaks are still high
                AudioState.ProgramOutState mixer2 = helper.SdkState.Audio.ProgramOut;
                AudioState.InputState input2 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.False(mixer2.PeakLeft < -60);
                Assert.False(mixer2.PeakRight < -60);
                Assert.False(input2.PeakLeft < -60);
                Assert.False(input2.PeakRight < -60);
                helper.AssertStatesMatch();

                // Reset peaks
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerResetPeaksCommand
                {
                    Mask = AudioMixerResetPeaksCommand.MaskFlags.All,
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                AudioState.ProgramOutState mixer3 = helper.SdkState.Audio.ProgramOut;
                AudioState.InputState input3 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.True(mixer3.PeakLeft < -60);
                Assert.True(mixer3.PeakRight < -60);
                Assert.True(input3.PeakLeft < -60);
                Assert.True(input3.PeakRight < -60);
                helper.AssertStatesMatch();
            }
        }
    }
}
