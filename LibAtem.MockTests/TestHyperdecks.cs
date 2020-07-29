using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
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

        private static byte[] RandomIP()
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
                CommandGenerator.CreateAutoCommandHandler<HyperDeckPlayerSetCommand, HyperDeckPlayerGetCommand>(
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
                CommandGenerator.CreateAutoCommandHandler<HyperDeckPlayerSetCommand, HyperDeckPlayerGetCommand>(
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
            var handler = CommandGenerator.CreateAutoCommandHandler<HyperDeckPlayerSetCommand, HyperDeckPlayerGetCommand>(
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
                List<HyperDeckPlayerGetCommand> previousStates = allCommands.OfType<HyperDeckPlayerGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckPlayerGetCommand cmd = previousStates.Single(c => c.Id == id);

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
            var expectedCmd = new HyperDeckPlayerSetCommand
            {
                Mask = HyperDeckPlayerSetCommand.MaskFlags.State,
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
            var expectedCmd = new HyperDeckPlayerSetCommand
            {
                Mask = HyperDeckPlayerSetCommand.MaskFlags.State | HyperDeckPlayerSetCommand.MaskFlags.PlaybackSpeed,
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
            var expectedCmd = new HyperDeckPlayerSetCommand
            {
                Mask = HyperDeckPlayerSetCommand.MaskFlags.State,
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
            var expectedCmd = new HyperDeckPlayerSetCommand
            {
                Mask = HyperDeckPlayerSetCommand.MaskFlags.State | HyperDeckPlayerSetCommand.MaskFlags.PlaybackSpeed,
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
        public void TestJog()
        {
            var expectedCmd = new HyperDeckPlayerSetCommand
            {
                Mask = HyperDeckPlayerSetCommand.MaskFlags.Jog,
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
                        hyperdeckState.Settings.StorageMediaCount =
                            cmd.StorageMediaCount = (uint) Randomiser.RangeInt(1, 5);

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
                    hyperdeckState.Storage.ActiveStorageMedia = 0;

                    var storageCmd = new HyperDeckStorageGetCommand
                    {
                        Id = (uint) id,
                        ActiveStorageMedia = hyperdeckState.Storage.ActiveStorageMedia,
                        CurrentClipId = hyperdeckState.Storage.CurrentClipId,
                        FrameRate = hyperdeckState.Storage.FrameRate,
                        TimeScale = hyperdeckState.Storage.TimeScale,
                    };

                    for (int i = 0; i < 5; i++)
                    {
                        hyperdeckState.Storage.ActiveStorageStatus = storageCmd.ActiveStorageStatus =
                            Randomiser.EnumValue<HyperDeckStorageStatus>();

                        helper.SendFromServerAndWaitForChange(stateBefore, storageCmd);
                    }
                }
            });
        }

        [Fact]
        public void TestActiveStorageMedia()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckStorageSetCommand, HyperDeckStorageGetCommand>(
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
                        deckState.Storage.ActiveStorageMedia = Randomiser.RangeInt(-1, 3);

                        helper.SendAndWaitForChange(stateBefore,
                            () => { deck.SetActiveStorageMedia(deckState.Storage.ActiveStorageMedia); });
                    }
                }
            });
        }

        [Fact]
        public void TestClipsInfo()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();

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

                            TimelineStart = new HyperDeckTime
                            {
                                Hour = (uint) Randomiser.RangeInt(2, 8),
                                Minute = (uint) Randomiser.RangeInt(2, 50),
                                Second = (uint) Randomiser.RangeInt(2, 50),
                                Frame = (uint) Randomiser.RangeInt(2, 50),
                            },

                            TimelineEnd = new HyperDeckTime
                            {
                                Hour = (uint) Randomiser.RangeInt(4, 18),
                                Minute = (uint) Randomiser.RangeInt(2, 50),
                                Second = (uint) Randomiser.RangeInt(2, 50),
                                Frame = (uint) Randomiser.RangeInt(2, 50),
                            },

                            Duration = new HyperDeckTime
                            {
                                Hour = (uint) Randomiser.RangeInt(10, 20),
                                Minute = (uint) Randomiser.RangeInt(2, 50),
                                Second = (uint) Randomiser.RangeInt(2, 50),
                                Frame = (uint) Randomiser.RangeInt(2, 50),
                            },
                        };
                        hyperdeckState.Clips[(int) infoCmd.ClipId].Name = infoCmd.Name;
                        hyperdeckState.Clips[(int) infoCmd.ClipId].Duration = infoCmd.Duration;
                        hyperdeckState.Clips[(int) infoCmd.ClipId].TimelineStart = infoCmd.TimelineStart;
                        hyperdeckState.Clips[(int) infoCmd.ClipId].TimelineEnd = infoCmd.TimelineEnd;

                        helper.SendFromServerAndWaitForChange(stateBefore, infoCmd);
                    }
                }
            });
        }

        [Fact]
        public void TestCurrentClipId()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckStorageSetCommand, HyperDeckStorageGetCommand>(
                    "CurrentClipId");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState deckState = stateBefore.Hyperdecks[(int)id];

                    for (int i = 0; i < 5; i++)
                    {
                        deckState.Storage.CurrentClipId = Randomiser.RangeInt(-1, 3);

                        helper.SendAndWaitForChange(stateBefore,
                            () => { deck.SetCurrentClip(deckState.Storage.CurrentClipId); });
                    }
                }
            });
        }

        [Fact]
        public void TestFramerate()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand connCmd = settingsCommands.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    connCmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, connCmd);

                    stateBefore = helper.Helper.BuildLibState();
                    HyperdeckState hyperdeckState = stateBefore.Hyperdecks[(int)id];


                    for (int i = 0; i < 5; i++)
                    {
                        hyperdeckState.Storage.FrameRate = (uint) Randomiser.RangeInt(1000, 500000);
                        hyperdeckState.Storage.TimeScale = (uint) Randomiser.RangeInt(1000, 500000);

                        helper.SendFromServerAndWaitForChange(stateBefore, new HyperDeckStorageGetCommand
                        {
                            Id = (uint)id,
                            ActiveStorageMedia = -1,
                            CurrentClipId = -1,
                            FrameRate = hyperdeckState.Storage.FrameRate,
                            TimeScale = hyperdeckState.Storage.TimeScale,
                        });
                    }
                }
            });
        }

        [Fact]
        public void TestIsInterlacedVideo()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();
                List<HyperDeckStorageGetCommand> sourceCommands = allCommands.OfType<HyperDeckStorageGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);
                    HyperDeckStorageGetCommand srcCmd = sourceCommands.Single(c => c.Id == id);

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
                        hyperdeckState.Storage.IsInterlaced =
                            srcCmd.IsInterlaced = !hyperdeckState.Storage.IsInterlaced;

                        helper.SendFromServerAndWaitForChange(stateBefore, srcCmd);
                    }
                }
            });
        }

        [Fact]
        public void TestIsDropFrameTimecode()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();
                List<HyperDeckStorageGetCommand> sourceCommands = allCommands.OfType<HyperDeckStorageGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);
                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);
                    HyperDeckStorageGetCommand srcCmd = sourceCommands.Single(c => c.Id == id);

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
                        hyperdeckState.Storage.IsDropFrameTimecode =
                            srcCmd.IsDropFrameTimecode = !hyperdeckState.Storage.IsDropFrameTimecode;

                        helper.SendFromServerAndWaitForChange(stateBefore, srcCmd);
                    }
                }
            });
        }

        [Fact]
        public void TestCurrentClipTime()
        {
            var handler =
                CommandGenerator.CreateAutoCommandHandler<HyperDeckPlayerSetCommand, HyperDeckPlayerGetCommand>(
                    "ClipTime");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();

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

                    // Define a clip
                    stateBefore.Hyperdecks[(int) id].Clips =
                        UpdaterUtil.CreateList(1, i => new HyperdeckState.ClipState());
                    helper.SendFromServerAndWaitForChange(stateBefore, new HyperDeckClipCountCommand
                    {
                        Id = (uint) id,
                        ClipCount = 1,
                    });
                    var clipCmd = new HyperDeckClipInfoCommand
                    {
                        HyperdeckId = (uint) id,
                        ClipId = 0,
                        Name = "something 123",
                        Duration = new HyperDeckTime {Hour = 24},
                        TimelineStart = new HyperDeckTime(),
                        TimelineEnd = new HyperDeckTime {Hour = 24},
                    };
                    AtemStateBuilder.Update(stateBefore, clipCmd);
                    helper.SendFromServerAndWaitForChange(stateBefore, clipCmd);
                    stateBefore = helper.Helper.BuildLibState();

                    // Set the clip to be playing
                    HyperDeckStorageGetCommand playCmd = new HyperDeckStorageGetCommand
                    {
                        Id = (uint) id,
                        ActiveStorageMedia = 0,
                        CurrentClipId = 0,
                        FrameRate = 50000,
                        TimeScale = 1000,
                        RemainingRecordTime = new HyperDeckTime()
                    };
                    AtemStateBuilder.Update(stateBefore, playCmd);
                    helper.SendFromServerAndWaitForChange(stateBefore, playCmd);
                    stateBefore = helper.Helper.BuildLibState();

                    HyperdeckState deckState = stateBefore.Hyperdecks[(int)id];

                    // Now try the stuff
                    for (int i = 0; i < 5; i++)
                    {
                        uint hours = (uint) Randomiser.RangeInt(1, 20);
                        uint minutes = (uint) Randomiser.RangeInt(1, 59);
                        uint seconds = (uint) Randomiser.RangeInt(1, 59);
                        uint frames = (uint) Randomiser.RangeInt(1, 20);
                        deckState.Player.TimelineTime = new HyperDeckTime();
                        deckState.Player.ClipTime = new HyperDeckTime
                            {Hour = hours, Minute = minutes, Second = seconds, Frame = frames};

                        helper.SendAndWaitForChange(stateBefore,
                            () =>
                            {
                                deck.SetCurrentClipTime((ushort) hours, (byte) minutes, (byte) seconds, (byte) frames);
                            });
                    }
                }
            });
        }

        [Fact]
        public void TestCurrentTimelineTime()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();
                List<HyperDeckPlayerGetCommand> playerCommands = allCommands.OfType<HyperDeckPlayerGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);

                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);
                    HyperDeckPlayerGetCommand playCmd = playerCommands.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    cmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    // Define a clip
                    stateBefore.Hyperdecks[(int)id].Clips =
                        UpdaterUtil.CreateList(1, i => new HyperdeckState.ClipState());
                    helper.SendFromServerAndWaitForChange(stateBefore, new HyperDeckClipCountCommand
                    {
                        Id = (uint)id,
                        ClipCount = 1,
                    });
                    var clipCmd = new HyperDeckClipInfoCommand
                    {
                        HyperdeckId = (uint)id,
                        ClipId = 0,
                        Name = "something 123",
                        Duration = new HyperDeckTime { Hour = 24 },
                        TimelineStart = new HyperDeckTime(),
                        TimelineEnd = new HyperDeckTime { Hour = 24 },
                    };
                    AtemStateBuilder.Update(stateBefore, clipCmd);
                    helper.SendFromServerAndWaitForChange(stateBefore, clipCmd);
                    stateBefore = helper.Helper.BuildLibState();

                    // Set the clip to be playing
                    HyperDeckStorageGetCommand srcCmd = new HyperDeckStorageGetCommand
                    {
                        Id = (uint) id,
                        ActiveStorageMedia = 0,
                        CurrentClipId = 0,
                        FrameRate = 50000,
                        TimeScale = 1000,
                        RemainingRecordTime = new HyperDeckTime(),
                    };
                    AtemStateBuilder.Update(stateBefore, srcCmd);
                    helper.SendFromServerAndWaitForChange(stateBefore, srcCmd);
                    stateBefore = helper.Helper.BuildLibState();

                    HyperdeckState deckState = stateBefore.Hyperdecks[(int)id];

                    // Now try the stuff
                    for (int i = 0; i < 5; i++)
                    {
                        uint hours = (uint)Randomiser.RangeInt(1, 20);
                        uint minutes = (uint)Randomiser.RangeInt(1, 59);
                        uint seconds = (uint)Randomiser.RangeInt(1, 59);
                        uint frames = (uint)Randomiser.RangeInt(1, 20);
                        deckState.Player.ClipTime = new HyperDeckTime();
                        playCmd.TimelineTime = deckState.Player.TimelineTime = new HyperDeckTime
                            {Hour = hours, Minute = minutes, Second = seconds, Frame = frames};

                        helper.SendFromServerAndWaitForChange(stateBefore, playCmd);
                    }
                }
            });
        }

        [Fact]
        public void TestRemainingRecordTime()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.HyperDecks, helper =>
            {
                ImmutableList<ICommand> allCommands = helper.Server.GetParsedDataDump();
                List<HyperDeckSettingsGetCommand> settingsCommands = allCommands.OfType<HyperDeckSettingsGetCommand>().ToList();
                List<HyperDeckStorageGetCommand> sourceCommands = allCommands.OfType<HyperDeckStorageGetCommand>().ToList();

                foreach (IBMDSwitcherHyperDeck deck in GetHyperDecks(helper))
                {
                    deck.GetId(out long id);

                    HyperDeckSettingsGetCommand cmd = settingsCommands.Single(c => c.Id == id);
                    HyperDeckStorageGetCommand srcCmd = sourceCommands.Single(c => c.Id == id);

                    // Force it to be connected
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    cmd.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Settings.Status = HyperDeckConnectionStatus.Connected;
                    stateBefore.Hyperdecks[(int)id].Player.State = HyperDeckPlayerState.Idle;
                    helper.SendFromServerAndWaitForChange(stateBefore, cmd);

                    HyperdeckState deckState = stateBefore.Hyperdecks[(int)id];

                    // Now try the stuff
                    for (int i = 0; i < 5; i++)
                    {
                        uint hours = (uint)Randomiser.RangeInt(1, 20);
                        uint minutes = (uint)Randomiser.RangeInt(1, 59);
                        uint seconds = (uint)Randomiser.RangeInt(1, 59);
                        uint frames = (uint)Randomiser.RangeInt(1, 20);
                        srcCmd.RemainingRecordTime = deckState.Storage.RemainingRecordTime = new HyperDeckTime
                            {Hour = hours, Minute = minutes, Second = seconds, Frame = frames};

                        helper.SendFromServerAndWaitForChange(stateBefore, srcCmd);
                    }
                }
            });
        }

    }
}