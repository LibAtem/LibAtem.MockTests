using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings;
using LibAtem.Commands.Settings.HyperDeck;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestHyperdecks
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestHyperdecks(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private List<IBMDSwitcherHyperDeck> GetHyperDecks(AtemMockServerWrapper helper)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherHyperDeckIterator>(helper.SdkClient.SdkSwitcher.CreateIterator);

            var result = new List<IBMDSwitcherHyperDeck>();
            for (iterator.Next(out IBMDSwitcherHyperDeck r); r != null; iterator.Next(out r))
            {
                result.Add(r);
            }

            return result;
        }

        // TODO NetworkAddress

        private byte[] RandomIP()
        {
            return new[]
            {
                (byte) Randomiser.RangeInt(255),
                (byte) Randomiser.RangeInt(255),
                (byte) Randomiser.RangeInt(255),
                (byte) Randomiser.RangeInt(255),
            };
        }

        [Fact]
        public void TestNetworkAddress()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckSettingsSetCommand, HyperDeckSettingsGetCommand>(
                    "NetworkAddress");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        deck.GetId(out long id);
                        AtemState stateBefore = helper.Helper.BuildLibState();

                        HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];
                        byte[] randomIp = RandomIP();
                        hyperdeckState.Settings.NetworkAddress = IPUtil.IPToString(randomIp);
                        uint ipVal = BitConverter.ToUInt32(randomIp.Reverse().ToArray());

                        helper.SendAndWaitForChange(stateBefore,
                            () => { deck.SetNetworkAddress(ipVal); });
                    }
                }
            });
        }

        [Fact]
        public void TestInput()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckSettingsSetCommand, HyperDeckSettingsGetCommand>(
                    "Input");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                var possibleValues = helper.Helper.BuildLibState().Settings.Inputs
                    .Where(s => s.Value.Properties.InternalPortType == InternalPortType.External).Select(s => s.Key)
                    .ToList();
                possibleValues.Add(VideoSource.Black);

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    List<VideoSource> selectedValues = Randomiser.SelectionOfGroup(possibleValues).ToList();
                    foreach (VideoSource src in selectedValues)
                    {
                        deck.GetId(out long id);
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        
                        HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int) id];
                        hyperdeckState.Settings.Input = src;

                        helper.SendAndWaitForChange(stateBefore, () => { deck.SetSwitcherInput((long) src); });
                    }
                }
            });
        }

        [Fact]
        public void TestAutoRoll()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckSettingsSetCommand, HyperDeckSettingsGetCommand>(
                    "AutoRoll");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        deck.GetId(out long id);
                        AtemState stateBefore = helper.Helper.BuildLibState();

                        HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];
                        hyperdeckState.Settings.AutoRoll = !hyperdeckState.Settings.AutoRoll;

                        helper.SendAndWaitForChange(stateBefore,
                            () => { deck.SetAutoRollOnTake(hyperdeckState.Settings.AutoRoll ? 1 : 0); });
                    }
                }
            });
        }

        [Fact]
        public void TestAutoRollFrameDelay()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckSettingsSetCommand, HyperDeckSettingsGetCommand>(
                    "AutoRollFrameDelay");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        deck.GetId(out long id);
                        AtemState stateBefore = helper.Helper.BuildLibState();

                        HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];
                        hyperdeckState.Settings.AutoRollFrameDelay = Randomiser.RangeInt(60);

                        helper.SendAndWaitForChange(stateBefore,
                            () =>
                            {
                                deck.SetAutoRollOnTakeFrameDelay((ushort) hyperdeckState.Settings.AutoRollFrameDelay);
                            });
                    }
                }
            });
        }

        [Fact]
        public void TestLoopedPlayback()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckCXCPCommand, HyperDeckRXCPCommand>(
                    "Loop");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        deck.GetId(out long id);
                        AtemState stateBefore = helper.Helper.BuildLibState();

                        HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];
                        hyperdeckState.Player.Loop = !hyperdeckState.Player.Loop;

                        helper.SendAndWaitForChange(stateBefore,
                            () =>
                            {
                                deck.SetLoopedPlayback(hyperdeckState.Player.Loop ? 1 : 0);
                            });
                    }
                }
            });
        }

        [Fact]
        public void TestSingleClipPlayback()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckCXCPCommand, HyperDeckRXCPCommand>(
                    "SingleClip");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        deck.GetId(out long id);
                        AtemState stateBefore = helper.Helper.BuildLibState();

                        HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];
                        hyperdeckState.Player.SingleClip = !hyperdeckState.Player.SingleClip;

                        helper.SendAndWaitForChange(stateBefore,
                            () =>
                            {
                                deck.SetSingleClipPlayback(hyperdeckState.Player.SingleClip ? 1 : 0);
                            });
                    }
                }
            });
        }

        [Fact]
        public void TestConnection()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<HyperDeckCXCPCommand, HyperDeckRXCPCommand>(
                new[] { "State" });

            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                List<HyperDeckSettingsGetCommand> previousStates = helper.Server.GetParsedDataDump().OfType<HyperDeckSettingsGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand cmd = previousStates.Single(c => c.Id == id);
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    for (int i = 0; i < 5; i++)
                    {
                        HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];
                        hyperdeckState.Settings.Status = cmd.Status = Randomiser.EnumValue<HyperDeckConnectionStatus>();
                        hyperdeckState.Player.State = HyperDeckPlayerState.Idle;

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }

        [Fact]
        public void TestPlayerState()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();
                List<HyperDeckRXCPCommand> previousStates = allCommands.OfType<HyperDeckRXCPCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckRXCPCommand cmd = previousStates.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    HyperDeckSettingsGetCommand connCmd = settingsCommands.Single(c => c.Id == id);
                    connCmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int) id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int) id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, connCmd);

                    stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];

                    for (int i = 0; i < 5; i++)
                    {
                        hyperdeckState.Player.State = cmd.State = Randomiser.EnumValue<HyperDeckPlayerState>();

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }

        [Fact]
        public void TestStop()
        {
            var expectedCmd = new HyperDeckCXCPCommand
            {
                Mask = HyperDeckCXCPCommand.MaskFlags.State,
                State = HyperDeckPlayerState.Idle,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    expectedCmd.Id = (uint)id;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        deck.Stop();
                    });
                }
            });
        }

        [Fact]
        public void TestPlay()
        {
            var expectedCmd = new HyperDeckCXCPCommand
            {
                Mask = HyperDeckCXCPCommand.MaskFlags.State | HyperDeckCXCPCommand.MaskFlags.PlaybackSpeed,
                State = HyperDeckPlayerState.Playing,
                PlaybackSpeed = 100,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    expectedCmd.Id = (uint) id;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        deck.Play();
                    });
                }
            });
        }

        [Fact]
        public void TestRecord()
        {
            var expectedCmd = new HyperDeckCXCPCommand
            {
                Mask = HyperDeckCXCPCommand.MaskFlags.State,
                State = HyperDeckPlayerState.Recording,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    expectedCmd.Id = (uint)id;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        deck.Record();
                    });
                }
            });
        }

        [Fact]
        public void TestShuttle()
        {
            var expectedCmd = new HyperDeckCXCPCommand
            {
                Mask = HyperDeckCXCPCommand.MaskFlags.State | HyperDeckCXCPCommand.MaskFlags.PlaybackSpeed,
                State = HyperDeckPlayerState.Playing,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    expectedCmd.Id = (uint)id;

                    for (int i = 0; i < 5; i++)
                    {
                        expectedCmd.PlaybackSpeed = Randomiser.RangeInt(-100, 200);
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            deck.Shuttle(expectedCmd.PlaybackSpeed);
                        });
                        
                    }

                }
            });
        }

        [Fact]
        public void TestJog() // TODO fix
        {
            var expectedCmd = new HyperDeckCXCPCommand
            {
                Mask = HyperDeckCXCPCommand.MaskFlags.Jog,
            };
            var handler = CommandGenerator.MatchCommand(expectedCmd, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    expectedCmd.Id = (uint)id;

                    expectedCmd.Jog = Randomiser.RangeInt(-100, 100);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        deck.Jog(expectedCmd.Jog);
                    });
                }
            });
        }

        [Fact]
        public void TestRemoteEnabled()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                List<HyperDeckSettingsGetCommand> settingsCommands = helper.Server.GetParsedDataDump().OfType<HyperDeckSettingsGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    cmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];

                    for (int i = 0; i < 5; i++)
                    {
                        hyperdeckState.Settings.IsRemoteEnabled =
                            cmd.IsRemoteEnabled = !hyperdeckState.Settings.IsRemoteEnabled;

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }

        [Fact]
        public void TestStorageMediaCount()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                List<HyperDeckSettingsGetCommand> settingsCommands = helper.Server.GetParsedDataDump().OfType<HyperDeckSettingsGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    cmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];

                    for (int i = 0; i < 5; i++)
                    {
                        cmd.StorageMediaCount = (uint) Randomiser.RangeInt(1, 5);
                        hyperdeckState.Settings.StorageMedia = Enumerable.Range(0, (int) cmd.StorageMediaCount)
                            .Select(o => HyperDeckStorageStatus.Unavailable).ToList();

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }

        [Fact]
        public void TestStorageMediaStatus()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                List<HyperDeckSettingsGetCommand> settingsCommands = helper.Server.GetParsedDataDump().OfType<HyperDeckSettingsGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    cmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];

                    for (int i = 0; i < 5; i++)
                    {
                        cmd.StorageMediaCount = (uint)Randomiser.RangeInt(1, 5);
                        hyperdeckState.Settings.StorageMedia = Enumerable.Range(0, (int)cmd.StorageMediaCount)
                            .Select(o => HyperDeckStorageStatus.Unavailable).ToList();

                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                }
            });
        }

        [Fact]
        public void TestActiveStorageMedia()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckCXSSCommand, HyperDeckRXSSCommand>(
                    "ActiveStorageMedia");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState deckState = stateBefore.Hyperdecks[(int) id];

                    for (int i = 0; i < 5; i++)
                    {
                        deckState.Settings.ActiveStorageMedia = Randomiser.RangeInt(-1, 3);

                        helper.SendAndWaitForChange(stateBefore,
                            () => { deck.SetActiveStorageMedia(deckState.Settings.ActiveStorageMedia); });
                    }
                }
            });
        }

        [Fact]
        public void TestClipsInfo()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                List<HyperDeckSettingsGetCommand> settingsCommands = helper.Server.GetParsedDataDump().OfType<HyperDeckSettingsGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    cmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];

                    for (int i = 0; i < 5; i++)
                    {
                        // Change the length
                        var newCmd = new HyperDeckClipCountCommand
                        {
                            Id = (uint) id,
                            ClipCount = (uint) Randomiser.RangeInt(2, 5)
                        };
                        hyperdeckState.Clips = UpdaterUtil
                          .CreateList(newCmd.ClipCount, o => new HyperdeckState.ClipState());

                        helper.SendFromServerAndWaitForChange(stateBefore, newCmd,-1,  (sdkState, libState) =>
                        {
                            // Sdk likes to randomly give back some stale data, so lets focus on just the length
                            sdkState.Hyperdecks[(int) id].Clips = UpdaterUtil
                                .CreateList((uint) sdkState.Hyperdecks[(int) id].Clips.Count,
                                    o => new HyperdeckState.ClipState());
                        });

                        // Now fill in some clip info
                        var infoCmd = new HyperDeckClipInfoCommand
                        {
                            HyperdeckId = (uint) id,
                            ClipId = 1,
                            Name = Randomiser.String(64),

                            StartHour = (uint) Randomiser.RangeInt(2, 8),
                            StartMinute = (uint)Randomiser.RangeInt(2, 50),
                            StartSecond = (uint)Randomiser.RangeInt(2, 50),
                            StartFrame = (uint)Randomiser.RangeInt(2, 50),

                            EndHour = (uint)Randomiser.RangeInt(4, 18),
                            EndMinute = (uint)Randomiser.RangeInt(2, 50),
                            EndSecond = (uint)Randomiser.RangeInt(2, 50),
                            EndFrame = (uint)Randomiser.RangeInt(2, 50),

                            DurationHour = (uint)Randomiser.RangeInt(10, 20),
                            DurationMinute = (uint)Randomiser.RangeInt(2, 50),
                            DurationSecond = (uint)Randomiser.RangeInt(2, 50),
                            DurationFrame = (uint)Randomiser.RangeInt(2, 50),
                        };
                        hyperdeckState.Clips[(int) infoCmd.ClipId].Name = infoCmd.Name;
                        hyperdeckState.Clips[(int) infoCmd.ClipId].Duration = new HyperdeckState.Time(
                            infoCmd.DurationHour, infoCmd.DurationMinute, infoCmd.DurationSecond,
                            infoCmd.DurationFrame);
                        hyperdeckState.Clips[(int) infoCmd.ClipId].TimelineStart =
                            new HyperdeckState.Time(infoCmd.StartHour, infoCmd.StartMinute, infoCmd.StartSecond,
                                infoCmd.StartFrame);
                        hyperdeckState.Clips[(int) infoCmd.ClipId].TimelineEnd = new HyperdeckState.Time(
                            infoCmd.EndHour, infoCmd.EndMinute, infoCmd.EndSecond, infoCmd.EndFrame);

                        helper.SendFromServerAndWaitForChange(stateBefore, infoCmd);
                    }
                }
            });
        }
    }
}