using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Media;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.Media
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

        private class MediaPlayerSourceTestDefinition : TestDefinitionBase<MediaPlayerSourceSetCommand, Tuple<MediaPlayerSource, uint>>
        {
            protected readonly MediaPlayerId _id;
            protected readonly IBMDSwitcherMediaPlayer _sdk;

            public MediaPlayerSourceTestDefinition(AtemComparisonHelper helper, Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> pl) : base(helper)
            {
                _id = pl.Item1;
                _sdk = pl.Item2;
            }

            public override void Prepare()
            {
                _sdk.SetSource(_BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill, 5);
            }

            public override string PropertyName => throw new NotImplementedException();
            public override void SetupCommand(MediaPlayerSourceSetCommand cmd) => throw new NotImplementedException();
            public override ICommand GenerateCommand(Tuple<MediaPlayerSource, uint> v)
            {
                MediaPlayerSourceSetCommand.MaskFlags mask = v.Item1 == MediaPlayerSource.Clip ? MediaPlayerSourceSetCommand.MaskFlags.ClipIndex : MediaPlayerSourceSetCommand.MaskFlags.StillIndex;
                return new MediaPlayerSourceSetCommand
                {
                    Index = _id,
                    Mask = MediaPlayerSourceSetCommand.MaskFlags.SourceType | mask,
                    SourceType = v.Item1,
                    ClipIndex = v.Item2,
                    StillIndex = v.Item2,
                };
            }

            public override void UpdateExpectedState(AtemState state, bool goodValue, Tuple<MediaPlayerSource, uint> v)
            {
                MediaPlayerState obj = state.MediaPlayers[(int)_id];
                if (goodValue)
                {
                    obj.Source.SourceType = v.Item1;
                    obj.Source.SourceIndex = v.Item2;
                }
                else
                {
                    obj.Source.SourceType = v.Item1;
                    obj.Source.SourceIndex = v.Item1 == MediaPlayerSource.Clip
                        ? _helper.Profile.MediaPoolClips - 1
                        : _helper.Profile.MediaPoolStills - 1;
                }
            }

            public override Tuple<MediaPlayerSource, uint>[] GoodValues => GetAllPossibleSources().ToArray();
            public override Tuple<MediaPlayerSource, uint>[] BadValues => GetBadSources().ToArray();

            private IEnumerable<Tuple<MediaPlayerSource, uint>> GetAllPossibleSources()
            {
                for (uint i = 0; i < _helper.Profile.MediaPoolClips; i++)
                    yield return Tuple.Create(MediaPlayerSource.Clip, i);

                for (uint i = 0; i < _helper.Profile.MediaPoolStills; i++)
                    yield return Tuple.Create(MediaPlayerSource.Still, i);
            }
            private IEnumerable<Tuple<MediaPlayerSource, uint>> GetBadSources()
            {
                yield return Tuple.Create(MediaPlayerSource.Clip, _helper.Profile.MediaPoolClips);
                yield return Tuple.Create(MediaPlayerSource.Still, _helper.Profile.MediaPoolStills);
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, Tuple<MediaPlayerSource, uint> v)
            {
                yield return new CommandQueueKey(new MediaPlayerSourceGetCommand() { Index = _id });
            }
        }

        [Fact]
        public void TestSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player in _client.GetMediaPlayers())
                {
                    new MediaPlayerSourceTestDefinition(helper, player).Run();

                }
            }
        }

        private void EnsureMediaPlayerHasClip()
        {
            // TODO - ensure something loaded

            foreach (Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player in _client.GetMediaPlayers())
                player.Item2.SetSource(_BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeClip, 0);
        }

        private abstract class MediaPlayerClipStatusTestDefinition<T> : TestDefinitionBase<MediaPlayerClipStatusSetCommand, T>
        {
            protected readonly MediaPlayerId _id;
            protected readonly IBMDSwitcherMediaPlayer _sdk;

            public MediaPlayerClipStatusTestDefinition(AtemComparisonHelper helper, Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player) : base(helper)
            {
                _id = player.Item1;
                _sdk = player.Item2;
            }

            public override void SetupCommand(MediaPlayerClipStatusSetCommand cmd)
            {
                cmd.Index = _id;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MediaPlayerState obj = state.MediaPlayers[(int)_id];
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MediaPlayerClipStatusGetCommand() { Index = _id });
            }
        }

        private class MediaPlayerClipStatusLoopTestDefinition : MediaPlayerClipStatusTestDefinition<bool>
        {
            public MediaPlayerClipStatusLoopTestDefinition(AtemComparisonHelper helper, Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player) : base(helper, player)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLoop(0);

            public override string PropertyName => "Loop";
            public override bool MangleBadValue(bool v) => v;
        }
        [SkippableFact]
        public void TestLoop()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            using (new MediaPoolUtil.SolidClipUploadHelper(helper, 0, "black", 5, 0, 0, 0, 0))
            {
                EnsureMediaPlayerHasClip();
                helper.Sleep();

                _client.GetMediaPlayers().ForEach(mp => new MediaPlayerClipStatusLoopTestDefinition(helper, mp).Run());
            }
        }

        private class MediaPlayerClipStatusClipFrameTestDefinition : MediaPlayerClipStatusTestDefinition<uint>
        {
            public MediaPlayerClipStatusClipFrameTestDefinition(AtemComparisonHelper helper, Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer> player) : base(helper, player)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetClipFrame(3);

            public override string PropertyName => "ClipFrame";
            public override uint MangleBadValue(uint v) => 7;
            public override void UpdateExpectedState(AtemState state, bool goodValue, uint v)
            {
                base.UpdateExpectedState(state, goodValue, v);

                MediaPlayerState obj = state.MediaPlayers[(int)_id];
                obj.Status.AtBeginning = (v == 0);
            }

            public override uint[] GoodValues => new uint[] { 0, 1, 5, 7 };
            public override uint[] BadValues => new uint[] {8, 10, 250, 999 };
        }
        [SkippableFact]
        public void TestClipFrame() // Also covers AtBeginning
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            using (new SettingEnabler(v => AtemStateSettings.TrackMediaClipFrames = v))
            using (new MediaPoolUtil.SolidClipUploadHelper(helper, 0, "black", 8, 0, 0, 0, 0))
            {
                EnsureMediaPlayerHasClip();
                helper.Sleep();

                _client.GetMediaPlayers().ForEach(mp => new MediaPlayerClipStatusClipFrameTestDefinition(helper, mp).Run());
            }
        }
    }
}
