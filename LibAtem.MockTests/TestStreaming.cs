using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Streaming;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestStreaming
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestStreaming(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestServiceName()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingServiceSetCommand, StreamingServiceGetCommand>("ServiceName");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();
 
                for (int i = 0; i < 5; i++)
                {
                    string target = Randomiser.String(64);
                    stateBefore.Streaming.Settings.ServiceName = target;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetServiceName(target); });
                }
            });
        }

        [Fact]
        public void TestUrl()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingServiceSetCommand, StreamingServiceGetCommand>("Url");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    string target = Randomiser.String(512);
                    stateBefore.Streaming.Settings.Url = target;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetUrl(target); });
                }
            });
        }

        [Fact]
        public void TestKey()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingServiceSetCommand, StreamingServiceGetCommand>("Key");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    string target = Randomiser.String(512);
                    stateBefore.Streaming.Settings.Key = target;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetKey(target); });
                }
            });
        }

        /*
         TODO - this uses rounded/fixed values?
        [Fact]
        public void TestBitrates()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingServiceSetCommand, StreamingServiceGetCommand>("Bitrates");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint targetLow = Randomiser.RangeInt(1u << 30);
                    uint targetHigh = Randomiser.RangeInt(1u << 30);
                    stateBefore.Streaming.Settings.LowBitrate = targetLow;
                    stateBefore.Streaming.Settings.HighBitrate = targetHigh;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetBitrates(targetLow, targetHigh); });
                }
            });
        }
        */
        /*
        [Fact]
        public void TestStartStopStreaming()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingActiveSetCommand, StreamingStateCommand>("IsStreaming", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();


                for (int i = 0; i < 250; i++)
                {
                    var startCommand = new CommandBuilder("SRSS");
                    startCommand.AddByte((byte) Randomiser.RangeInt(255), (byte) Randomiser.RangeInt(255),
                        (byte) Randomiser.RangeInt(255), (byte) Randomiser.RangeInt(255),
                        (byte) Randomiser.RangeInt(255), (byte) Randomiser.RangeInt(255),
                        (byte) Randomiser.RangeInt(255), (byte) Randomiser.RangeInt(255));
                        
                    //startCommand.AddUInt32((uint)_BMDSwitcherStreamRTMPState.bmdSwitcherStreamRTMPStateConnecting);
                    //startCommand.Pad(2);
                    //stateBefore.Streaming.Status.IsStreaming = true;

                    helper.SendBufferFromServerAndWaitForChange(stateBefore, startCommand.ToByteArray());

                    //helper.SendAndWaitForChange(stateBefore, () => { switcher.StartStreaming(); });

                    //stateBefore.Streaming.Status.IsStreaming = false;

                    //helper.SendAndWaitForChange(stateBefore, () => { switcher.StopStreaming(); });
                    Assert.True(helper.Helper.TestResult);
                }
            });
        }*/

        [Fact]
        public void TestDuration()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingActiveSetCommand, StreamingStateCommand>("IsStreaming", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();
                
                for (int i = 0; i < 5; i++)
                {
                    var durationCommand = new StreamingTimecodeCommand
                    {
                        Hour = Randomiser.RangeInt(23),
                        Minute = Randomiser.RangeInt(59),
                        Second = Randomiser.RangeInt(59),
                        Frame = Randomiser.RangeInt(59),
                        IsDropFrame = i % 2 != 0
                    };

                    stateBefore.Streaming.Status.Duration = new Timecode
                    {
                        Hour = durationCommand.Hour,
                        Minute = durationCommand.Minute,
                        Second = durationCommand.Second,
                        Frame = durationCommand.Frame,
                        DropFrame = durationCommand.IsDropFrame,
                    };

                    helper.SendFromServerAndWaitForChange(stateBefore, durationCommand);
                }
            });
        }

        [Fact]
        public void TestStreamingState()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingActiveSetCommand, StreamingStateCommand>("IsStreaming", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    var cmd = new StreamingStateCommand
                    {
                        StreamingStatus = Randomiser.EnumValue<StreamingStatus>()
                    };

                    stateBefore.Streaming.Status.State = cmd.StreamingStatus;

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }
    }
}