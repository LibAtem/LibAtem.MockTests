using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Media;
using LibAtem.Common;
using LibAtem.ComparisonTests.MixEffects;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Media
{
    [Collection("Client")]
    public class TestMediaPlayers
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;
        
        public TestMediaPlayers(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }
        
        [Fact]
        public void TestMediaPlayerCount()
        {
            List<Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer>> players = _client.GetMediaPlayers();
            Assert.Equal((uint) players.Count, _client.Profile.MediaPlayers);
        }

        [Fact]
        public void TestSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Tuple<MediaPlayerSource, uint>[] testValues = GetAllPossibleSources().ToArray();
                Tuple<MediaPlayerSource, uint>[] badValues = GetBadSources().ToArray();

                foreach (Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player in _client.GetMediaPlayers())
                {
                    ICommand Setter(Tuple<MediaPlayerSource, uint> v)
                    {
                        MediaPlayerSourceSetCommand.MaskFlags mask = v.Item1 == MediaPlayerSource.Clip ? MediaPlayerSourceSetCommand.MaskFlags.ClipIndex : MediaPlayerSourceSetCommand.MaskFlags.StillIndex;
                        return new MediaPlayerSourceSetCommand
                        {
                            Index = player.Item1,
                            Mask = MediaPlayerSourceSetCommand.MaskFlags.SourceType | mask,
                            SourceType = v.Item1,
                            ClipIndex = v.Item2,
                            StillIndex = v.Item2,
                        };
                    }

                    void UpdateExpectedState(ComparisonState state, Tuple<MediaPlayerSource, uint> v)
                    {
                        state.MediaPlayers[player.Item1].SourceType = v.Item1;
                        state.MediaPlayers[player.Item1].SourceIndex = v.Item2;
                    }
                    void UpdateFailedState(ComparisonState state, Tuple<MediaPlayerSource, uint> v)
                    {
                        state.MediaPlayers[player.Item1].SourceType = v.Item1;
                        state.MediaPlayers[player.Item1].SourceIndex = v.Item1 == MediaPlayerSource.Clip
                            ? _client.Profile.MediaPoolClips - 1
                            : _client.Profile.MediaPoolStills - 1;
                    }

                    ValueTypeComparer<Tuple<MediaPlayerSource, uint>>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<Tuple<MediaPlayerSource, uint>>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        private IEnumerable<Tuple<MediaPlayerSource, uint>> GetAllPossibleSources()
        {
            for (uint i = 0; i < _client.Profile.MediaPoolClips; i++)
                yield return Tuple.Create(MediaPlayerSource.Clip, i);

            for (uint i = 0; i < _client.Profile.MediaPoolStills; i++)
                yield return Tuple.Create(MediaPlayerSource.Still, i);
        }
        private IEnumerable<Tuple<MediaPlayerSource, uint>> GetBadSources()
        {
            yield return Tuple.Create(MediaPlayerSource.Clip, _client.Profile.MediaPoolClips);
            yield return Tuple.Create(MediaPlayerSource.Still, _client.Profile.MediaPoolStills);
        }

        private void EnsureMediaPlayerHasClip()
        {
            // TODO - ensure something loaded

            foreach (Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player in _client.GetMediaPlayers())
            {
                player.Item2.SetSource(_BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeClip, 0);
            }
        }
        
        [Fact]
        public void TestLoop()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                if (_client.Profile.MediaPoolClips == 0)
                    return;
                EnsureMediaPlayerHasClip();
                helper.Sleep();

                foreach (Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player in _client.GetMediaPlayers())
                {
                    ICommand Setter(bool v)
                    {
                        return new MediaPlayerClipStatusSetCommand()
                        {
                            Index = player.Item1,
                            Mask = MediaPlayerClipStatusSetCommand.MaskFlags.Loop,
                            Loop = v,
                        };
                    }

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        state.MediaPlayers[player.Item1].IsLooped = v;
                    }

                    bool[] testValues = {true, false};
                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestClipFrame()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            using (new SettingEnabler(v => ComparisonStateSettings.TrackMediaClipFrames = v))
            {
                if (_client.Profile.MediaPoolClips == 0)
                    return;
                EnsureMediaPlayerHasClip();
                helper.Sleep();

                foreach (Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player in _client.GetMediaPlayers())
                {
                    ICommand Setter(uint v)
                    {
                        return new MediaPlayerClipStatusSetCommand()
                        {
                            Index = player.Item1,
                            Mask = MediaPlayerClipStatusSetCommand.MaskFlags.ClipFrame,
                            ClipFrame = v,
                        };
                    }

                    void UpdateExpectedState(ComparisonState state, uint v)
                    {
                        var props = state.MediaPlayers[player.Item1];
                        props.AtBeginning = v == 0;
                        props.ClipFrame = v;
                    }

                    void UpdateFailedState(ComparisonState state, uint v)
                    {
                        var props = state.MediaPlayers[player.Item1];
                        props.AtBeginning = false;
                        props.ClipFrame = 51; // TODO - dynamic length of clip
                    }

                    uint[] testValues = {0, 1, 5};
                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, 999);
                }
            }
        }

        [Fact]
        public void TestAtBeginning()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            using (new SettingEnabler(v => ComparisonStateSettings.TrackMediaClipFrames = v))
            {
                if (_client.Profile.MediaPoolClips == 0)
                    return;
                EnsureMediaPlayerHasClip();
                helper.Sleep();

                foreach (Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player in _client.GetMediaPlayers())
                {
                    ICommand Setter(bool v)
                    {
                        if (!v)
                        {
                            return new MediaPlayerClipStatusSetCommand()
                            {
                                Index = player.Item1,
                                Mask = MediaPlayerClipStatusSetCommand.MaskFlags.ClipFrame,
                                ClipFrame = 1,
                            };
                        }

                        return new MediaPlayerClipStatusSetCommand()
                        {
                            Index = player.Item1,
                            Mask = MediaPlayerClipStatusSetCommand.MaskFlags.AtBeginning,
                            AtBeginning = true,
                        };
                    }

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        var props = state.MediaPlayers[player.Item1];
                        props.AtBeginning = v;
                        props.ClipFrame = (uint)(v ? 0 : 1);
                    }

                    ICommand FailSetter(bool v) => new MediaPlayerClipStatusSetCommand()
                    {
                        Index = player.Item1,
                        Mask = MediaPlayerClipStatusSetCommand.MaskFlags.AtBeginning,
                        AtBeginning = v,
                    };

                    bool[] testValues = { false, true };
                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<bool>.Fail(helper, FailSetter, false);
                }
            }
        }
    }
}
