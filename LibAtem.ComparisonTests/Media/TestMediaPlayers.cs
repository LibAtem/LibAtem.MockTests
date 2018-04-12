using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Media;
using LibAtem.Common;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.Media
{
    [Collection("Client")]
    public class TestMediaPlayers
    {
        private static readonly IReadOnlyDictionary<MediaPlayerSource, _BMDSwitcherMediaPlayerSourceType> SourceMap;

        static TestMediaPlayers()
        {
            SourceMap = new Dictionary<MediaPlayerSource, _BMDSwitcherMediaPlayerSourceType>
            {
                {MediaPlayerSource.Clip, _BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeClip},
                {MediaPlayerSource.Still, _BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill},
            };
        }

        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestMediaPlayers(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        // TODO GetPlaying, GetAtBeginning, GetClipFrame, GetLoop

        [Fact]
        public void EnsureSourceMapIsComplete()
        {
            EnumMap.EnsureIsComplete(SourceMap);
        }

        private List<IBMDSwitcherMediaPlayer> GetMediaPlayers()
        {
            Guid itId = typeof(IBMDSwitcherMediaPlayerIterator).GUID;
            _client.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherMediaPlayerIterator iterator = (IBMDSwitcherMediaPlayerIterator)Marshal.GetObjectForIUnknown(itPtr);

            List<IBMDSwitcherMediaPlayer> result = new List<IBMDSwitcherMediaPlayer>();
            for (iterator.Next(out IBMDSwitcherMediaPlayer r); r != null; iterator.Next(out r))
                result.Add(r);

            return result;
        }

        private IBMDSwitcherMediaPlayer GetPlayer()
        {
            return GetMediaPlayers().First();
        }

        [Fact]
        public void TestMediaPlayerCount()
        {
            List<IBMDSwitcherMediaPlayer> players = GetMediaPlayers();
            Assert.Equal((uint) players.Count, _client.Profile.MediaPlayers);
        }

        #region Source

        [Fact]
        public void TestMediaPlayerSource()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                List<IBMDSwitcherMediaPlayer> players = GetMediaPlayers();

                foreach (Tuple<MediaPlayerSource, uint> src in GetAllPossibleSources())
                {
                    for (int i = 0; i < players.Count; i++)
                        SetAndCheckSource(helper, players[i], (MediaPlayerId) i, ClassValueComparer<Tuple<MediaPlayerSource, uint>>.Run, src);
                }

                foreach (Tuple<MediaPlayerSource, uint> src in GetBadSources())
                {
                    for (int i = 0; i < players.Count; i++)
                        SetAndCheckSource(helper, players[i], (MediaPlayerId)i, ClassValueComparer<Tuple<MediaPlayerSource, uint>>.Fail, src);
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

        private delegate void SourceRun(AtemComparisonHelper helper, Func<Tuple<MediaPlayerSource, uint>, ICommand> setter, Func<Tuple<MediaPlayerSource, uint>> getter, Func<Tuple<MediaPlayerSource, uint>> libget, Tuple<MediaPlayerSource, uint> newVal = null);

        private void SetAndCheckSource(AtemComparisonHelper helper, IBMDSwitcherMediaPlayer sdkProps, MediaPlayerId id, SourceRun run, Tuple<MediaPlayerSource, uint> newVal)
        {
            ICommand Setter(Tuple<MediaPlayerSource, uint> v)
            {
                MediaPlayerSourceSetCommand.MaskFlags mask = v.Item1 == MediaPlayerSource.Clip ? MediaPlayerSourceSetCommand.MaskFlags.ClipIndex : MediaPlayerSourceSetCommand.MaskFlags.StillIndex;
                return new MediaPlayerSourceSetCommand
                {
                    Index = id,
                    Mask = MediaPlayerSourceSetCommand.MaskFlags.SourceType | mask,
                    SourceType = v.Item1,
                    ClipIndex = v.Item2,
                    StillIndex = v.Item2,
                };
            }

            Tuple<MediaPlayerSource, uint> SdkGetter()
            {
                sdkProps.GetSource(out _BMDSwitcherMediaPlayerSourceType type, out uint index);
                MediaPlayerSource src = SourceMap.First(v => v.Value == type).Key;
                return Tuple.Create(src, index);
            }

            Tuple<MediaPlayerSource, uint> Getter()
            {
                var res = helper.FindWithMatching(new MediaPlayerSourceGetCommand {Index = id});
                if (res == null)
                    return null;
                return Tuple.Create(res.SourceType, res.SourceIndex);
            }

            run(helper, Setter, SdkGetter, Getter, newVal);
        }

        #endregion Source
        

    }
}
