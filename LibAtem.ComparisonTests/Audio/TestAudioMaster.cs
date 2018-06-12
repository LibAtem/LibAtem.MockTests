using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.ComparisonTests.MixEffects;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using System;
using Xunit;
using Xunit.Abstractions;
using static LibAtem.ComparisonTests.Util.MediaPoolUtil;

namespace LibAtem.ComparisonTests.Audio
{
    [Collection("Client")]
    public class TestAudioMaster : ComparisonTestBase
    {
        public TestAudioMaster(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        [Fact]
        public void TestAudioFollowVideo()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ICommand Setter(bool v) => new AudioMixerMasterSetCommand()
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.FollowFadeToBlack,
                    FollowFadeToBlack = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.Audio.ProgramOutFollowFadeToBlack = v;

                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, new[] { true, false });
            }
        }

        [Fact]
        public void TestBalance()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                double[] testValues = { -50, -49.99, -49, 10, 0, 35.84, 49.99, 50 };
                double[] badValues = { -50.01, -51, 50.01, 51 };

                ICommand Setter(double v) => new AudioMixerMasterSetCommand()
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.Balance,
                    Balance = v,
                };

                using (new ValueDefaults(helper, Setter(0))) // We need to do this, as it cannot be set via the stock client
                {
                    void UpdateExpectedState(ComparisonState state, double v) => state.Audio.ProgramOutBalance = v;
                    void UpdateBadState(ComparisonState state, double v) => state.Audio.ProgramOutBalance = v < -50 ? -50 : 50;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateBadState, badValues);
                }
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                double[] testValues = { -60, -59, -45, -10, 0, 0.1, 6, 5.99, 5.9, -60.1, 6.01, -65, -90, double.NegativeInfinity };

                ICommand Setter(double v) => new AudioMixerMasterSetCommand()
                {
                    Mask = AudioMixerMasterSetCommand.MaskFlags.Gain,
                    Gain = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.Audio.ProgramOutGain = v;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
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

                ComparisonAudioState mixer = helper.SdkState.Audio;
                Assert.False(double.IsNegativeInfinity(mixer.ProgramLeft));
                Assert.False(double.IsNegativeInfinity(mixer.ProgramRight));
                Assert.False(double.IsNegativeInfinity(mixer.ProgramPeakLeft));
                Assert.False(double.IsNegativeInfinity(mixer.ProgramPeakRight));
                Assert.Equal(-1.9, mixer.ProgramLeft, 1);
                Assert.Equal(mixer.ProgramLeft, mixer.ProgramRight, 1);
                Assert.Equal(mixer.ProgramPeakLeft, mixer.ProgramPeakRight, 1);
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

                ComparisonAudioState mixer2 = helper.SdkState.Audio;
                Assert.Equal(-11.9, mixer2.ProgramLeft, 1);
                Assert.Equal(mixer2.ProgramLeft, mixer2.ProgramRight, 1);
                Assert.Equal(mixer2.ProgramPeakLeft, mixer2.ProgramPeakRight, 1);
                Assert.Equal(mixer.ProgramPeakLeft, mixer2.ProgramPeakLeft, 1); // Shouldnt have changed
                helper.AssertStatesMatch();

                // Reset peaks and ensure they changed
                helper.SendAndWaitForMatching(levelsKey, new AudioMixerResetPeaksCommand
                {
                    Mask = AudioMixerResetPeaksCommand.MaskFlags.Master,
                });
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                ComparisonAudioState mixer3 = helper.SdkState.Audio;
                Assert.Equal(mixer2.ProgramLeft, mixer3.ProgramLeft, 1); // Shouldnt have changed
                Assert.Equal(mixer3.ProgramLeft, mixer3.ProgramRight, 1);
                Assert.Equal(-11.9, mixer3.ProgramPeakLeft, 1);
                Assert.Equal(mixer3.ProgramPeakLeft, mixer3.ProgramPeakRight, 1);
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

                ComparisonAudioState mixer4 = helper.LibState.Audio;
                Assert.True(mixer4.ProgramLeft < -70);
                Assert.True(mixer4.ProgramRight < -70);
                Assert.True(mixer4.ProgramPeakLeft < -70);
                Assert.True(mixer4.ProgramPeakRight < -70);
            }
        }
        
        [Fact]
        public void TestResetAllPeaks()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            using (var player = new MediaPlayingHelper(helper))
            using (new SendAudioLevelsHelper(helper))
            {
                var levelsKey = new CommandQueueKey(new AudioMixerLevelsCommand());
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                ComparisonAudioState mixer = helper.SdkState.Audio;
                ComparisonAudioInputState input = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.False(double.IsNegativeInfinity(mixer.ProgramPeakLeft));
                Assert.False(double.IsNegativeInfinity(mixer.ProgramPeakRight));
                Assert.False(double.IsNegativeInfinity(input.PeakLeft));
                Assert.False(double.IsNegativeInfinity(input.PeakRight));
                helper.AssertStatesMatch();

                // Stop the clip
                player.Dispose();
                helper.Sleep(240);
                helper.SendAndWaitForMatching(levelsKey, null);

                // Ensure peaks are still high
                ComparisonAudioState mixer2 = helper.SdkState.Audio;
                ComparisonAudioInputState input2 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.False(mixer2.ProgramPeakLeft < -60);
                Assert.False(mixer2.ProgramPeakRight < -60);
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

                ComparisonAudioState mixer3 = helper.SdkState.Audio;
                ComparisonAudioInputState input3 = helper.SdkState.Audio.Inputs[(long)AudioSource.MP1];
                Assert.True(mixer3.ProgramPeakLeft < -60);
                Assert.True(mixer3.ProgramPeakRight < -60);
                Assert.True(input3.PeakLeft < -60);
                Assert.True(input3.PeakRight < -60);
                helper.AssertStatesMatch();
            }
        }
    }
}
