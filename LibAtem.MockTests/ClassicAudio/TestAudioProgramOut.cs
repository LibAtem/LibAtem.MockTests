using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.ClassicAudio
{
    [Collection("ServerClientPool")]
    public class TestAudioProgramOut
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAudioProgramOut(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        public static IBMDSwitcherAudioMixer GetAudioMixer(AtemMockServerWrapper helper)
        {
            var mixer = helper.Helper.SdkSwitcher as IBMDSwitcherAudioMixer;
            Assert.NotNull(mixer);
            return mixer;
        }

        [Fact]
        public void TestGain()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<AudioMixerMasterSetCommand, AudioMixerMasterGetCommand>("Gain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IBMDSwitcherAudioMixer mixer = GetAudioMixer(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range();
                    stateBefore.Audio.ProgramOut.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { mixer.SetProgramOutGain(target); });
                }
            });
        }

        [Fact]
        public void TestBalance()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<AudioMixerMasterSetCommand, AudioMixerMasterGetCommand>(
                    "Balance");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IBMDSwitcherAudioMixer mixer = GetAudioMixer(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    double target = Randomiser.Range(-50, 50);
                    stateBefore.Audio.ProgramOut.Balance = target;
                    helper.SendAndWaitForChange(stateBefore, () => { mixer.SetProgramOutBalance(target / 50); });
                }
            });
        }

        [Fact]
        public void TestFollowFadeToBlack()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<AudioMixerMasterSetCommand, AudioMixerMasterGetCommand>(
                    "FollowFadeToBlack");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IBMDSwitcherAudioMixer mixer = GetAudioMixer(helper);
                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    int target = i % 2;
                    stateBefore.Audio.ProgramOut.FollowFadeToBlack = target != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { mixer.SetProgramOutFollowFadeToBlack(target); });
                }
            });
        }

        // TODO GetAudioFollowVideoCrossfadeTransition


        private class ProgramOutLevelCallback : IBMDSwitcherAudioMixerCallback
        {
            public double[] Levels { get; private set; } = new double[0];
            public double[] Peaks { get; private set; } = new double[0];

            public void Reset()
            {
                Levels = Peaks = new double[0];
            }

            public void Notify(_BMDSwitcherAudioMixerEventType eventType)
            {
                // Ignore
            }

            public void ProgramOutLevelNotification(double left, double right, double peakLeft, double peakRight)
            {
                Levels = new[] { left, right };
                Peaks = new[] { peakLeft, peakRight };
            }
        }

        [Fact]
        public void TestLevelsAndPeaks()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IBMDSwitcherAudioMixer mixer = GetAudioMixer(helper);

                var cb = new ProgramOutLevelCallback();
                using (new UseCallback<ProgramOutLevelCallback>(cb, mixer.AddCallback, mixer.RemoveCallback))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        cb.Reset();

                        AtemState expectedState = helper.Helper.BuildLibState();

                        var testCmd = new AudioMixerLevelsCommand()
                        {
                            MasterLeftLevel = Randomiser.Range(-100, 0),
                            MasterRightLevel = Randomiser.Range(-100, 0),
                            MasterLeftPeak = Randomiser.Range(-100, 0),
                            MasterRightPeak = Randomiser.Range(-100, 0),
                        };

                        expectedState.Audio.ProgramOut.Levels = new AudioState.LevelsState
                        {
                            Levels = new[] { testCmd.MasterLeftLevel, testCmd.MasterRightLevel },
                            Peaks = new[] { testCmd.MasterLeftPeak, testCmd.MasterRightPeak },
                        };

                        helper.SendAndWaitForChange(expectedState, () =>
                        {
                            helper.Server.SendCommands(testCmd);
                        }, -1, (sdkState, libState) =>
                        {
                            sdkState.Audio.ProgramOut.Levels = new AudioState.LevelsState
                            {
                                Levels = cb.Levels,
                                Peaks = cb.Peaks,
                            };
                        });

                    }
                }
            });
        }
    }
}