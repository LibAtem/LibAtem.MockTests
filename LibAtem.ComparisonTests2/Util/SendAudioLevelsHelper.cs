using System;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio;

namespace LibAtem.ComparisonTests2.Util
{
    public sealed class SendAudioLevelsHelper : IDisposable
    {
        private readonly AtemComparisonHelper _helper;

        public SendAudioLevelsHelper(AtemComparisonHelper helper)
        {
            _helper = helper;

            _helper.SendCommand(new AudioMixerSendLevelsCommand { SendLevels = true });

            var mixer = (IBMDSwitcherAudioMixer)_helper.SdkSwitcher;
            mixer.SetAllLevelNotificationsEnable(1);
        }

        public void Dispose()
        {
            _helper.SendCommand(new AudioMixerSendLevelsCommand { SendLevels = false });

            var mixer = (IBMDSwitcherAudioMixer)_helper.SdkSwitcher;
            mixer.SetAllLevelNotificationsEnable(0);
        }
    }

}