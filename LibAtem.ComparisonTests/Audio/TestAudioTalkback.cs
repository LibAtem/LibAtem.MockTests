using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.ComparisonTests.MixEffects;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Audio
{
    [Collection("Client")]
    public class TestAudioTalkback : ComparisonTestBase
    {
        public TestAudioTalkback(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }
        
        protected IBMDSwitcherTalkback GetTalkback()
        {
            return (IBMDSwitcherTalkback)Client.SdkSwitcher;
        }

        [Fact]
        public void TestMuteSDI()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ICommand Setter(bool v) => new AudioMixerTalkbackPropertiesSetCommand()
                {
                    Mask = AudioMixerTalkbackPropertiesSetCommand.MaskFlags.MuteSDI,
                    MuteSDI = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.Audio.Talkback.MuteSDI = v;

                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, new[] { true, false });
            }
        }

        // TODO - test mutesdi for inputs
    }
}
