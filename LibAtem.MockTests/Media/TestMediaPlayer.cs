using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Media;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Media
{
    [Collection("ServerClientPool")]
    public class TestMediaPlayer
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestMediaPlayer(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static List<Tuple<uint, IBMDSwitcherMediaPlayer>> GetMediaPlayers(AtemMockServerWrapper helper)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMediaPlayerIterator>(helper.SdkClient.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<uint, IBMDSwitcherMediaPlayer>>();
            uint index = 0;
            for (iterator.Next(out IBMDSwitcherMediaPlayer r); r != null; iterator.Next(out r))
            {
                result.Add(Tuple.Create(index, r));
                index++;
            }

            return result;
        }

        [Fact]
        public void TestLoop()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MediaPlayerClipStatusSetCommand, MediaPlayerClipStatusGetCommand>("Loop");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                var tested = false;
                var players = GetMediaPlayers(helper);

                foreach (Tuple<uint, IBMDSwitcherMediaPlayer> player in players)
                {
                    tested = true;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    MediaPlayerState playerState = stateBefore.MediaPlayers[(int) player.Item1];

                    for (int i = 0; i < 5; i++)
                    {
                        bool loop = i % 2 == 0;
                        playerState.ClipStatus.Loop = loop;
                        
                        helper.SendAndWaitForChange(stateBefore, () => { player.Item2.SetLoop(loop ? 1 : 0); });
                    }
                }
                Assert.True(tested);
            });
        }

        [Fact]
        public void TestPlaying()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MediaPlayerClipStatusSetCommand, MediaPlayerClipStatusGetCommand>("Playing");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MediaPlayerClips, helper =>
            {
                var tested = false;
                var players = GetMediaPlayers(helper);

                foreach (Tuple<uint, IBMDSwitcherMediaPlayer> player in players)
                {
                    tested = true;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    MediaPlayerState playerState = stateBefore.MediaPlayers[(int)player.Item1];

                    for (int i = 0; i < 5; i++)
                    {
                        bool playing = i % 2 == 0;
                        playerState.ClipStatus.Playing = playing;

                        helper.SendAndWaitForChange(stateBefore, () => { player.Item2.SetPlaying(playing ? 1 : 0); });
                    }
                }
                Assert.True(tested);
            });
        }

        [Fact]
        public void TestClipFrame()
        {
            try
            {
                _pool.StateSettings.TrackMediaClipFrames = true;
                var handler = CommandGenerator.CreateAutoCommandHandler<MediaPlayerClipStatusSetCommand, MediaPlayerClipStatusGetCommand>("ClipFrame");
                AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MediaPlayerClips, helper =>
                {
                    var tested = false;
                    var players = GetMediaPlayers(helper);

                    foreach (Tuple<uint, IBMDSwitcherMediaPlayer> player in players)
                    {
                        tested = true;

                        AtemState stateBefore = helper.Helper.BuildLibState();
                        MediaPlayerState playerState = stateBefore.MediaPlayers[(int) player.Item1];

                        for (int i = 0; i < 5; i++)
                        {
                            uint frame = playerState.ClipStatus.ClipFrame = Randomiser.RangeInt(40);

                            helper.SendAndWaitForChange(stateBefore, () => { player.Item2.SetClipFrame(frame); });
                        }
                    }

                    Assert.True(tested);
                });
            }
            finally
            {
                _pool.StateSettings.TrackMediaClipFrames = false;
            }
        }

        [Fact]
        public void TestAtBeginning()
        {
            try
            {
                var expectedCommand = new MediaPlayerClipStatusSetCommand
                {
                    Mask = MediaPlayerClipStatusSetCommand.MaskFlags.AtBeginning,
                    AtBeginning = true
                };
                var handler = CommandGenerator.MatchCommand(expectedCommand, true);
                AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MediaPlayerClips, helper =>
                {
                    var tested = false;
                    var players = GetMediaPlayers(helper);

                    foreach (Tuple<uint, IBMDSwitcherMediaPlayer> player in players)
                    {
                        tested = true;

                        expectedCommand.Index = (MediaPlayerId) player.Item1;

                        AtemState stateBefore = helper.Helper.BuildLibState();

                        uint timeBefore = helper.Server.CurrentTime;

                        helper.SendAndWaitForChange(stateBefore, () => { player.Item2.SetAtBeginning(); });

                        // It should have sent a response, but we dont expect any comparable data
                        Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                    }

                    Assert.True(tested);
                });
            }
            finally
            {
                _pool.StateSettings.TrackMediaClipFrames = false;
            }
        }

        [Fact]
        public void TestAtBeginning2()
        {
            try
            {
                _pool.StateSettings.TrackMediaClipFrames = true;
                AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MediaPlayerClips, helper =>
                {
                    var tested = false;
                    var players = GetMediaPlayers(helper);

                    ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                    foreach (Tuple<uint, IBMDSwitcherMediaPlayer> player in players)
                    {
                        tested = true;

                        AtemState stateBefore = helper.Helper.BuildLibState();
                        MediaPlayerState playerState = stateBefore.MediaPlayers[(int)player.Item1];

                        var tmpCmd = previousCommands.OfType<MediaPlayerClipStatusGetCommand>().Single(c => c.Index == (MediaPlayerId) player.Item1);

                        for (int i = 0; i < 5; i++)
                        {
                            bool atBeginning = playerState.ClipStatus.AtBeginning = i % 2 != 0;

                            helper.SendAndWaitForChange(stateBefore, () =>
                            {
                                tmpCmd.AtBeginning = atBeginning;
                                helper.Server.SendCommands(tmpCmd);
                            });
                        }
                    }

                    Assert.True(tested);
                });
            }
            finally
            {
                _pool.StateSettings.TrackMediaClipFrames = false;
            }
        }

        [Fact]
        public void TestSource()
        {
            AtemMockServerWrapper.Each(_output, _pool, SourceCommandHandler, DeviceTestCases.MediaPlayer, helper =>
            {
                var tested = false;
                var players = GetMediaPlayers(helper);

                foreach (Tuple<uint, IBMDSwitcherMediaPlayer> player in players)
                {
                    tested = true;

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    MediaPlayerState playerState = stateBefore.MediaPlayers[(int)player.Item1];

                    for (int i = 0; i < 5; i++)
                    {
                        MediaPlayerSource type = playerState.Source.SourceType = stateBefore.MediaPool.Clips.Count > 0
                            ? Randomiser.EnumValue<MediaPlayerSource>()
                            : MediaPlayerSource.Still;
                        uint index = playerState.Source.SourceIndex = Randomiser.RangeInt(20) + 1;

                        helper.SendAndWaitForChange(stateBefore,
                            () => { player.Item2.SetSource(AtemEnumMaps.MediaPlayerSourceMap[type], index); });
                    }
                }
                Assert.True(tested);
            });
        }

        private static IEnumerable<ICommand> SourceCommandHandler(Lazy<ImmutableList<ICommand>> previousCommands, ICommand cmd)
        {
            if (cmd is MediaPlayerSourceSetCommand setCmd)
            {
                bool isClip = setCmd.ClipIndex != 0;
                bool isStill = setCmd.StillIndex != 0;
                Assert.True(isClip | isStill);

                var mask = MediaPlayerSourceSetCommand.MaskFlags.SourceType;
                if (isClip) mask |= MediaPlayerSourceSetCommand.MaskFlags.ClipIndex;
                if (isStill) mask |= MediaPlayerSourceSetCommand.MaskFlags.StillIndex;

                Assert.Equal(mask, setCmd.Mask);

                var previous = previousCommands.Value.OfType<MediaPlayerSourceGetCommand>().Last(a => a.Index == setCmd.Index);
                Assert.NotNull(previous);

                previous.SourceType = setCmd.SourceType;
                if (isClip) previous.SourceIndex = setCmd.ClipIndex;
                if (isStill) previous.SourceIndex = setCmd.StillIndex;

                yield return previous;
            }
        }

    }
}