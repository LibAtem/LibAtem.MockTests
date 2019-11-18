using BMDSwitcherAPI;
using LibAtem.State;
using LibAtem.State.Builder;
using System;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MediaPlayerCallback : IBMDSwitcherMediaPlayerCallback
    {
        private readonly MediaPlayerState _state;
        private readonly AtemStateBuilderSettings _updateSettings;
        private readonly IBMDSwitcherMediaPlayer _props;
        private readonly Action<string> _onChange;

        public MediaPlayerCallback(MediaPlayerState state, AtemStateBuilderSettings updateSettings, IBMDSwitcherMediaPlayer props, Action<string> onChange)
        {
            _state = state;
            _updateSettings = updateSettings;
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
            _state.Source.SourceType = AtemEnumMaps.MediaPlayerSourceMap.FindByValue(type);
            _state.Source.SourceIndex = index;
            _onChange("Source");
        }

        public void PlayingChanged()
        {
            _props.GetPlaying(out int playing);
            _state.Status.Playing = playing != 0;
            _onChange("Status");
        }

        public void LoopChanged()
        {
            _props.GetLoop(out int loop);
            _state.Status.Loop = loop != 0;
            _onChange("Status");
        }

        public void AtBeginningChanged()
        {
            if (!_updateSettings.TrackMediaClipFrames)
                return;

            _props.GetAtBeginning(out int atBegining);
            _state.Status.AtBeginning = atBegining != 0;
            _onChange("Status");
        }

        public void ClipFrameChanged()
        {
            if (!_updateSettings.TrackMediaClipFrames)
                return;

            _props.GetClipFrame(out uint clipFrame);
            _state.Status.ClipFrame = clipFrame;
            _onChange("Status");
        }
    }
}