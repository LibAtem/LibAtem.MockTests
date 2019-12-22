using BMDSwitcherAPI;
using LibAtem.State;
using LibAtem.State.Builder;
using System;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MediaPlayerCallback : SdkCallbackBase<IBMDSwitcherMediaPlayer>, IBMDSwitcherMediaPlayerCallback
    {
        private readonly MediaPlayerState _state;
        private readonly AtemStateBuilderSettings _updateSettings;

        public MediaPlayerCallback(MediaPlayerState state, AtemStateBuilderSettings updateSettings, IBMDSwitcherMediaPlayer props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            _updateSettings = updateSettings;

            Notify();
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
            Props.GetSource(out _BMDSwitcherMediaPlayerSourceType type, out uint index);
            _state.Source.SourceType = AtemEnumMaps.MediaPlayerSourceMap.FindByValue(type);
            _state.Source.SourceIndex = index;
            OnChange("Source");
        }

        public void PlayingChanged()
        {
            if (_state.ClipStatus != null)
            {
                Props.GetPlaying(out int playing);
                _state.ClipStatus.Playing = playing != 0;
                OnChange("Status");
            }
        }

        public void LoopChanged()
        {
            if (_state.ClipStatus != null)
            {
                Props.GetLoop(out int loop);
                _state.ClipStatus.Loop = loop != 0;
                OnChange("Status");
            }
        }

        public void AtBeginningChanged()
        {
            if (_state.ClipStatus != null)
            {
                if (!_updateSettings.TrackMediaClipFrames)
                    return;

                Props.GetAtBeginning(out int atBegining);
                _state.ClipStatus.AtBeginning = atBegining != 0;
                OnChange("Status");
            }
        }

        public void ClipFrameChanged()
        {
            if (_state.ClipStatus != null)
            {
                if (!_updateSettings.TrackMediaClipFrames)
                    return;

                Props.GetClipFrame(out uint clipFrame);
                _state.ClipStatus.ClipFrame = clipFrame;
                OnChange("Status");
            }
        }
    }
}