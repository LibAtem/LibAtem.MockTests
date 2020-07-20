﻿using BMDSwitcherAPI;
using LibAtem.State;
using LibAtem.State.Builder;
using System.Collections.Generic;
using System.Linq;

namespace LibAtem.MockTests.SdkState
{
    public static class MediaPoolStateBuilder
    {
        public static void Build(MediaPoolState state, IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMediaPool;

            // General
            // TODO

            // Stills
            pool.GetStills(out IBMDSwitcherStills stills);
            stills.GetCount(out uint stillCount);
            state.Stills = Enumerable.Range(0, (int)stillCount).Select(i => BuildStill(stills, (uint)i)).ToList();


            // Clips
            pool.GetFrameTotalForClips(out uint unassignedFrames);
            pool.GetClipCount(out uint clipCount);
            state.Clips = Enumerable.Range(0, (int)clipCount).Select(i =>
            {
                pool.GetClip((uint)i, out IBMDSwitcherClip clip);
                clip.GetMaxFrameCount(out uint maxFrameCount);
                unassignedFrames -= maxFrameCount;
                clip.IsValid(out int valid);
                clip.GetName(out string name);
                return new MediaPoolState.ClipState
                {
                    IsUsed = valid != 0,
                    Name = name,
                    MaxFrames = maxFrameCount,
                };
            }).ToList();

            state.UnassignedFrames = unassignedFrames;
        }

        private static MediaPoolState.StillState BuildStill(IBMDSwitcherStills props, uint index)
        {
            var state = new MediaPoolState.StillState();

            props.IsValid(index, out int valid);
            state.IsUsed = valid != 0;
            props.GetName(index, out string name);
            state.Filename = name;
            props.GetHash(index, out BMDSwitcherHash hash);
            state.Hash = hash.data;

            return state;
        }
    }
    public static class MediaPlayerStateBuilder
    {
        public static IReadOnlyList<MediaPlayerState> Build(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings, bool hasClips)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMediaPlayerIterator>(switcher.CreateIterator);
            return AtemSDKConverter.IterateList<IBMDSwitcherMediaPlayer, MediaPlayerState>(iterator.Next,
                (media, id) => BuildOne(media, updateSettings, hasClips));
        }

        private static MediaPlayerState BuildOne(IBMDSwitcherMediaPlayer props, AtemStateBuilderSettings updateSettings, bool hasClips)
        {
            var state = new MediaPlayerState();
            if (hasClips)
                state.ClipStatus = new MediaPlayerState.ClipStatusState();

            props.GetSource(out _BMDSwitcherMediaPlayerSourceType type, out uint index);
            state.Source.SourceType = AtemEnumMaps.MediaPlayerSourceMap.FindByValue(type);
            state.Source.SourceIndex = index;

            if (state.ClipStatus != null)
            {
                props.GetPlaying(out int playing);
                state.ClipStatus.Playing = playing != 0;
                props.GetLoop(out int loop);
                state.ClipStatus.Loop = loop != 0;

                if (updateSettings.TrackMediaClipFrames)
                {
                    props.GetAtBeginning(out int atBegining);
                    state.ClipStatus.AtBeginning = atBegining != 0;
                    props.GetClipFrame(out uint clipFrame);
                    state.ClipStatus.ClipFrame = clipFrame;
                }
            }

            return state;
        }

    }
}