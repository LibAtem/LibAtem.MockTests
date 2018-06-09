using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MediaPlayerCallback : IBMDSwitcherMediaPlayerCallback
    {
        private readonly ComparisonMediaPlayerState _state;
        private readonly IBMDSwitcherMediaPlayer _props;

        public MediaPlayerCallback(ComparisonMediaPlayerState state, IBMDSwitcherMediaPlayer props)
        {
            _state = state;
            _props = props;
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
        }

        public void PlayingChanged()
        {
            _props.GetPlaying(out int playing);
            _state.IsPlaying = playing != 0;
        }

        public void LoopChanged()
        {
            _props.GetLoop(out int loop);
            _state.IsLooped = loop != 0;
        }

        public void AtBeginningChanged()
        {
            _props.GetAtBeginning(out int atBegining);
            _state.AtBeginning = atBegining != 0;
        }

        public void ClipFrameChanged()
        {
            _props.GetClipFrame(out uint clipFrame);
            _state.ClipFrame = clipFrame;
        }
    }
}