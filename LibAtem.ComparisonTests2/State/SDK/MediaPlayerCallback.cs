using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Media;
using LibAtem.Common;
using System;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class MediaPlayerCallback : IBMDSwitcherMediaPlayerCallback
    {
        private readonly ComparisonMediaPlayerState _state;
        private readonly MediaPlayerId _id;
        private readonly IBMDSwitcherMediaPlayer _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MediaPlayerCallback(ComparisonMediaPlayerState state, MediaPlayerId id, IBMDSwitcherMediaPlayer props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify()
        {
            SourceChanged();
            PlayingChanged();
            LoopChanged();
            AtBeginningChanged();
            ClipFrameChanged();
        }

        public void SourceChanged()
        {
            _props.GetSource(out _BMDSwitcherMediaPlayerSourceType type, out uint index);
            _state.SourceType = AtemEnumMaps.MediaPlayerSourceMap.FindByValue(type);
            _state.SourceIndex = index;
            _onChange(new CommandQueueKey(new MediaPlayerSourceGetCommand() { Index = _id }));
        }

        public void PlayingChanged()
        {
            _props.GetPlaying(out int playing);
            _state.IsPlaying = playing != 0;
            _onChange(new CommandQueueKey(new MediaPlayerClipStatusGetCommand() { Index = _id }));
        }

        public void LoopChanged()
        {
            _props.GetLoop(out int loop);
            _state.Loop = loop != 0;
            _onChange(new CommandQueueKey(new MediaPlayerClipStatusGetCommand() { Index = _id }));
        }

        public void AtBeginningChanged()
        {
            if (!ComparisonStateSettings.TrackMediaClipFrames)
                return;

            _props.GetAtBeginning(out int atBegining);
            _state.AtBeginning = atBegining != 0;
            _onChange(new CommandQueueKey(new MediaPlayerClipStatusGetCommand() { Index = _id }));
        }

        public void ClipFrameChanged()
        {
            if (!ComparisonStateSettings.TrackMediaClipFrames)
                return;

            _props.GetClipFrame(out uint clipFrame);
            _state.ClipFrame = clipFrame;
            _onChange(new CommandQueueKey(new MediaPlayerClipStatusGetCommand() { Index = _id }));
        }
    }
}