using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Recording;
using LibAtem.Commands.Streaming;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestRecording
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestRecording(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

#if !ATEM_v8_1

        [Fact]
        public void TestFilename()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingSettingsSetCommand, RecordingSettingsGetCommand>("Filename");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Recording, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    string target = Randomiser.String(128);
                    stateBefore.Recording.Properties.Filename = target;

                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetFilename(target); });
                }
            });
        }

        [Fact]
        public void TestRecordInAllCameras()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingSettingsSetCommand, RecordingSettingsGetCommand>("RecordInAllCameras");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Recording, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    stateBefore.Recording.Properties.RecordInAllCameras =
                        !stateBefore.Recording.Properties.RecordInAllCameras;

                    helper.SendAndWaitForChange(stateBefore,
                        () =>
                        {
                            switcher.SetRecordInAllCameras(stateBefore.Recording.Properties.RecordInAllCameras ? 1 : 0);
                        });
                }
            });
        }

        [Fact]
        public void TestISORecordAllInputs()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingISOCommand, RecordingISOCommand>("ISORecordAllInputs", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Recording, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    stateBefore.Recording.ISORecordAllInputs = !stateBefore.Recording.ISORecordAllInputs;

                    helper.SendAndWaitForChange(stateBefore,
                        () =>
                        {
                            switcher.SetRecordAllISOInputs(stateBefore.Recording.ISORecordAllInputs ? 1 : 0);
                        });
                }
            });
        }

        [Fact]
        public void TestStartRecording()
        {
            var handler = CommandGenerator.MatchCommand(new RecordingStatusSetCommand {IsRecording = true});
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { switcher.StartRecording(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestStopRecording()
        {
            var handler = CommandGenerator.MatchCommand(new RecordingStatusSetCommand { IsRecording = false});
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                var stateBefore = helper.Helper.BuildLibState();
                InitDisk(helper, stateBefore, 0);
                InitDisk(helper, stateBefore, 1);

                InitRecording(helper, stateBefore, 0, 1);

                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { switcher.StopRecording(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestTotalRecordingTimeAvailable()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingStatusSetCommand, RecordingStatusGetCommand>("IsRecording", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();
                stateBefore.Recording.Status.Error = RecordingError.NoMedia;

                for (int i = 0; i < 10; i++)
                {
                    var cmd = new RecordingStatusGetCommand
                    {
                        TotalRecordingTimeAvailable = Randomiser.RangeInt(int.MaxValue)
                    };
                    stateBefore.Recording.Status.TotalRecordingTimeAvailable = cmd.TotalRecordingTimeAvailable;

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestRecordingState()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.Recording, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                InitDisk(helper, stateBefore);
                InitRecording(helper, stateBefore);
                
                stateBefore.Recording.Status.TotalRecordingTimeAvailable = 0;

                for (int i = 0; i < 10; i++)
                {
                    stateBefore.Recording.Status.State = Randomiser.EnumValue<RecordingStatus>();
                    stateBefore.Recording.Status.Error = Randomiser.EnumValue<RecordingError>();
                    var cmd = new RecordingStatusGetCommand()
                    {
                        Status = stateBefore.Recording.Status.State,
                        Error = stateBefore.Recording.Status.Error,
                    };
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestWorkingSetDisk1()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<RecordingSettingsSetCommand, RecordingSettingsGetCommand>(
                    "WorkingSet1DiskId");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                var stateBefore = helper.Helper.BuildLibState();
                for (int i = 0; i < 10; i++)
                {
                    InitDisk(helper, stateBefore, (uint) i);
                }

                for (int i = 0; i < 5; i++)
                {
                    var disk = stateBefore.Recording.Properties.WorkingSet1DiskId = Randomiser.RangeInt(10);
                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetWorkingSetDisk(0, disk); });
                }
            });
        }

        [Fact]
        public void TestWorkingSetDisk2()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<RecordingSettingsSetCommand, RecordingSettingsGetCommand>(
                    "WorkingSet2DiskId");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                var stateBefore = helper.Helper.BuildLibState();
                for (int i = 0; i < 10; i++)
                {
                    InitDisk(helper, stateBefore, (uint)i);
                }

                for (int i = 0; i < 5; i++)
                {
                    var disk = stateBefore.Recording.Properties.WorkingSet2DiskId = Randomiser.RangeInt(10);
                    helper.SendAndWaitForChange(stateBefore, () => { switcher.SetWorkingSetDisk(1, disk); });
                }
            });
        }
        
        [Fact]
        public void TestSwitchDisk()
        {
            var handler = CommandGenerator.MatchCommand(new RecordingSwitchDiskCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                var stateBefore = helper.Helper.BuildLibState();
                InitDisk(helper, stateBefore, 0);
                InitDisk(helper, stateBefore, 1);

                InitRecording(helper, stateBefore, 0, 1);

                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { switcher.SwitchDisk(); });

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
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                InitDisk(helper, stateBefore);
                InitRecording(helper, stateBefore);

                for (int i = 0; i < 5; i++)
                {
                    var durationCommand = new RecordingDurationCommand
                    {
                        Hour = Randomiser.RangeInt(23),
                        Minute = Randomiser.RangeInt(59),
                        Second = Randomiser.RangeInt(59),
                        Frame = Randomiser.RangeInt(59),
                        IsDropFrame = i % 2 != 0
                    };

                    stateBefore.Recording.Status.Duration = new Timecode
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

        private static void InitRecording(AtemMockServerWrapper helper, AtemState stateBefore, uint diskId = 0, uint diskId2 = 0)
        {
            var parsedCmds = helper.Server.GetParsedDataDump();
            var cmd = parsedCmds.OfType<RecordingSettingsGetCommand>().SingleOrDefault();
            stateBefore.Recording.Properties.WorkingSet1DiskId = cmd.WorkingSet1DiskId = diskId;
            stateBefore.Recording.Properties.WorkingSet2DiskId = cmd.WorkingSet2DiskId = diskId2;

            helper.SendFromServerAndWaitForChange(stateBefore, cmd);

            stateBefore.Recording.Status.State = RecordingStatus.Recording;
            stateBefore.Recording.Status.Error = RecordingError.None;

            var rtms = parsedCmds.OfType<RecordingStatusGetCommand>().SingleOrDefault();
            rtms.Status = stateBefore.Recording.Status.State;
            rtms.Error = stateBefore.Recording.Status.Error;

            helper.SendFromServerAndWaitForChange(stateBefore, rtms);
        }

        [Fact]
        public void TestRequestDuration()
        {
            var handler = CommandGenerator.MatchCommand(new RecordingRequestDurationCommand());
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherRecordAV;
                Assert.NotNull(switcher);

                var stateBefore = helper.Helper.BuildLibState();
                InitDisk(helper, stateBefore);
                InitRecording(helper, stateBefore);

                for (int i = 0; i < 5; i++)
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(null, () => { switcher.RequestDuration(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        #region Disk

        private RecordingDiskInfoCommand InitDisk(AtemMockServerWrapper helper, AtemState stateBefore, uint id = 0)
        {
            var cmd = new RecordingDiskInfoCommand
            {
                DiskId = id,
                Status = RecordingDiskStatus.Idle,
                VolumeName = "",
            };
            stateBefore.Recording.Disks[id] = new RecordingState.RecordingDiskState
            {
                DiskId = id,
                Status = RecordingDiskStatus.Idle,
                VolumeName = "",
            };

            helper.SendFromServerAndWaitForChange(stateBefore, cmd);

            return cmd;
        }

        [Fact]
        public void TestBasicDisk()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingStatusSetCommand, RecordingStatusGetCommand>("IsRecording", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    InitDisk(helper, stateBefore);
                }
            });
        }

        [Fact]
        public void TestDiskStatus()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingStatusSetCommand, RecordingStatusGetCommand>("IsRecording", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                var cmd = InitDisk(helper, stateBefore);

                for (int i = 0; i < 10; i++)
                {
                    stateBefore.Recording.Disks[cmd.DiskId].Status =
                        cmd.Status = Randomiser.EnumValue<RecordingDiskStatus>();

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestDiskName()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingStatusSetCommand, RecordingStatusGetCommand>("IsRecording", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                var cmd = InitDisk(helper, stateBefore);

                for (int i = 0; i < 10; i++)
                {
                    stateBefore.Recording.Disks[cmd.DiskId].VolumeName =
                        cmd.VolumeName = Randomiser.String(64);

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestDiskRecordingTimeAvailable()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingStatusSetCommand, RecordingStatusGetCommand>("IsRecording", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                var cmd = InitDisk(helper, stateBefore);

                for (int i = 0; i < 10; i++)
                {
                    stateBefore.Recording.Disks[cmd.DiskId].RecordingTimeAvailable =
                        cmd.RecordingTimeAvailable = Randomiser.RangeInt(int.MaxValue);

                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        [Fact]
        public void TestDiskRemoval()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<RecordingStatusSetCommand, RecordingStatusGetCommand>("IsRecording", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.Streaming, helper =>
            {
                var switcher = helper.SdkClient.SdkSwitcher as IBMDSwitcherStreamRTMP;
                Assert.NotNull(switcher);

                AtemState stateBefore = helper.Helper.BuildLibState();

                // Init a disk that will persit
                InitDisk(helper, stateBefore, 99);

                for (int i = 0; i < 10; i++)
                {
                    var id = Randomiser.RangeInt(20);
                    var cmd = InitDisk(helper, stateBefore, id);

                    // Simulate removal
                    cmd.IsDelete = true;
                    stateBefore.Recording.Disks.Remove(id);
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                }
            });
        }

        #endregion Disk
#endif
    }
}