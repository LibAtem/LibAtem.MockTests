using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Talkback;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestTalkback
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestTalkback(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private void EachTalkback(AtemMockServerWrapper helper, Action<AtemState, SettingsState.TalkbackState, IBMDSwitcherTalkback, uint> fcn)
        {
            AtemState stateBefore = helper.Helper.BuildLibState();
            var it = AtemSDKConverter.CastSdk<IBMDSwitcherTalkbackIterator>(helper.SdkClient.SdkSwitcher
                .CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherTalkback>(it.Next, (talkback, i) =>
            {
                SettingsState.TalkbackState talkbackState = stateBefore.Settings.Talkback[(int) i];
                fcn(stateBefore, talkbackState, talkback, i);
            });
        }

        private List<long> SampleOfInputs(SettingsState.TalkbackState state)
        {
            return Randomiser.SelectionOfGroup(state.Inputs.Keys.Select(k => (long) k).ToList(), 3).ToList();
        }

#if !ATEM_v8_1
        [Fact]
        public void TestMuteSDI()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<TalkbackMixerPropertiesSetCommand,
                        TalkbackMixerPropertiesGetCommand>("MuteSDI");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Talkback, helper =>
            {
                EachTalkback(helper, (stateBefore, tbState, talkback, index) =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        tbState.MuteSDI = !tbState.MuteSDI;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            talkback.SetMuteSDI(tbState.MuteSDI ? 1 : 0);
                        });
                    }
                });
            });
        }

        [Fact]
        public void TestInputCanMuteSDI()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<TalkbackMixerInputPropertiesSetCommand,
                        TalkbackMixerInputPropertiesGetCommand>("MuteSDI");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Talkback, helper =>
            {
                var allCommands = helper.Server.GetParsedDataDump().OfType<TalkbackMixerInputPropertiesGetCommand>()
                    .ToList();
                EachTalkback(helper, (stateBefore, tbState, talkback, index) =>
                {
                    foreach (long inputId in SampleOfInputs(tbState))
                    {
                        stateBefore = helper.Helper.BuildLibState();
                        var inputState = stateBefore.Settings.Talkback[(int)index].Inputs[(VideoSource)inputId];

                        var cmd = allCommands.Single(
                            c => c.Channel == (TalkbackChannel) index && c.Index == (VideoSource) inputId);

                        for (int i = 0; i < 5; i++)
                        {
                            cmd.InputCanMuteSDI = inputState.InputCanMuteSDI = !inputState.InputCanMuteSDI;
                            helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                        }
                    }
                });
            });
        }

        [Fact]
        public void TestCurrentInputSupportsMuteSDI()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<TalkbackMixerInputPropertiesSetCommand,
                        TalkbackMixerInputPropertiesGetCommand>("MuteSDI");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Talkback, helper =>
            {
                var allCommands = helper.Server.GetParsedDataDump().OfType<TalkbackMixerInputPropertiesGetCommand>()
                    .ToList();
                EachTalkback(helper, (stateBefore, tbState, talkback, index) =>
                {
                    foreach (long inputId in SampleOfInputs(tbState))
                    {
                        var inputState = stateBefore.Settings.Talkback[(int)index].Inputs[(VideoSource)inputId];

                        var cmd = allCommands.Single(
                            c => c.Channel == (TalkbackChannel)index && c.Index == (VideoSource)inputId);

                        bool origVal = inputState.InputCanMuteSDI;
                        for (int i = 0; i < 5; i++)
                        {
                            cmd.CurrentInputSupportsMuteSDI = inputState.CurrentInputSupportsMuteSDI = !inputState.CurrentInputSupportsMuteSDI;
                            inputState.InputCanMuteSDI = inputState.CurrentInputSupportsMuteSDI ? origVal : false;

                            helper.SendFromServerAndWaitForChange(stateBefore, cmd, -1, (sdkState, libState) =>
                            {
                                libState.Settings.Talkback.ForEach(tb =>
                                {
                                    tb.Inputs.ForEach(inp =>
                                    {
                                        if (!inp.Value.CurrentInputSupportsMuteSDI)
                                            inp.Value.InputCanMuteSDI = false;
                                    });
                                });
                            });
                        }
                    }
                });
            });
        }

        [Fact]
        public void TestInputMuteSDI()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<TalkbackMixerInputPropertiesSetCommand,
                        TalkbackMixerInputPropertiesGetCommand>("MuteSDI");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Talkback, helper =>
            {
                EachTalkback(helper, (stateBefore, tbState, talkback, index) =>
                {
                    foreach (long inputId in SampleOfInputs(tbState))
                    {
                        var inputState = stateBefore.Settings.Talkback[(int) index].Inputs[(VideoSource) inputId];
                        Assert.True(inputState.InputCanMuteSDI);
                        Assert.True(inputState.CurrentInputSupportsMuteSDI);

                        for (int i = 0; i < 5; i++)
                        {
                            inputState.MuteSDI = !inputState.MuteSDI;
                            helper.SendAndWaitForChange(stateBefore, () =>
                            {
                                talkback.SetInputMuteSDI(inputId, inputState.MuteSDI ? 1 : 0);
                            });
                        }
                    }
                });
            });
        }
#endif
    }
}