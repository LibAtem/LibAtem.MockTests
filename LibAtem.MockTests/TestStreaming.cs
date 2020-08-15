using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Streaming;
using LibAtem.Common;
using LibAtem.MockTests.Util;
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

#if !ATEM_v8_1

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

        [Fact]
        public void TestVideoBitrates()
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
                    stateBefore.Streaming.Settings.LowVideoBitrate = targetLow;
                    stateBefore.Streaming.Settings.HighVideoBitrate = targetHigh;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetVideoBitrates(targetLow, targetHigh); });
                }
            });
        }


        [Fact]
        public void TestAudioBitrates()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<StreamingAudioBitratesCommand, StreamingAudioBitratesCommand>("Bitrates", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint targetLow = Randomiser.RangeInt(1u << 30);
                    uint targetHigh = Randomiser.RangeInt(1u << 30);
                    stateBefore.Streaming.Settings.LowAudioBitrate = targetLow;
                    stateBefore.Streaming.Settings.HighAudioBitrate = targetHigh;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetAudioBitrates(targetLow, targetHigh); });
                }
            });
        }

        [Fact]
        public void TestStartStreaming()
        {
            var handler = CommandGenerator.MatchCommand(new StreamingActiveSetCommand {IsStreaming = true});
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { switcher.StartStreaming(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestStopStreaming()
        {
            var handler = CommandGenerator.MatchCommand(new StreamingActiveSetCommand { IsStreaming = false });
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                // Set streaming
                var stateBefore = helper.Helper.BuildLibState();
                var streamCmd = new StreamingStateCommand
                    { StreamingStatus = StreamingStatusExt.EncodeStreamingStatus(StreamingStatus.Streaming, StreamingError.None) };
                stateBefore.Streaming.Status.State = StreamingStatus.Streaming;
                helper.SendFromServerAndWaitForChange(stateBefore, streamCmd);


                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { switcher.StopStreaming(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestDuration()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.Streaming, helper =>
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
        public void TestRequestDuration()
        {
            var handler = CommandGenerator.MatchCommand(new StreamingRequestDurationCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { switcher.RequestDuration(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
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

                for (int i = 0; i < 10; i++)
                {
                    stateBefore.Streaming.Status.State = Randomiser.EnumValue<StreamingStatus>();
                    stateBefore.Streaming.Status.Error = Randomiser.EnumValue<StreamingError>();
                    var cmd = new StreamingStateCommand
                    {
                        StreamingStatus =
                            StreamingStatusExt.EncodeStreamingStatus(stateBefore.Streaming.Status.State,
                                stateBefore.Streaming.Status.Error)
                    };

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestAuthentication()
        {
            var handler =
                CommandGenerator
                    .CreateAutoCommandHandler<StreamingAuthenticationCommand, StreamingAuthenticationCommand>(
                        new[] {"Username", "Password"}, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    string target1 = Randomiser.String(64);
                    string target2 = Randomiser.String(64);
                    stateBefore.Streaming.Authentication.Username = target1;
                    stateBefore.Streaming.Authentication.Password = target2;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetAuthentication(target1, target2); });
                }
            });
        }

        [Fact]
        public void TestEncodingBitrate()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                // Set streaming
                var streamCmd = new StreamingStateCommand
                    {StreamingStatus = StreamingStatusExt.EncodeStreamingStatus(StreamingStatus.Streaming, StreamingError.None)};
                stateBefore.Streaming.Status.State = StreamingStatus.Streaming;
                helper.SendFromServerAndWaitForChange(stateBefore, streamCmd);

                for (int i = 0; i < 5; i++)
                {
                    var cmd = new StreamingStatsCommand
                    {
                        EncodingBitrate = Randomiser.RangeInt(int.MaxValue)
                    };
                    stateBefore.Streaming.Stats.EncodingBitrate = cmd.EncodingBitrate;

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestCacheUsed()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                // Set streaming
                var streamCmd = new StreamingStateCommand
                    { StreamingStatus = StreamingStatusExt.EncodeStreamingStatus(StreamingStatus.Streaming, StreamingError.None) };
                stateBefore.Streaming.Status.State = StreamingStatus.Streaming;
                helper.SendFromServerAndWaitForChange(stateBefore, streamCmd);

                for (int i = 0; i < 5; i++)
                {
                    var cmd = new StreamingStatsCommand
                    {
                        CacheUsed = (uint)Randomiser.RangeInt(0, 100)
                    };
                    stateBefore.Streaming.Stats.CacheUsed = cmd.CacheUsed;

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }
#endif
    }
}