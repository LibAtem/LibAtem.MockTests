using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AtemSDKComparisonMonitor : IDisposable
    {
        public AtemState State { get; }

        private readonly List<Action> _cleanupCallbacks = new List<Action>();

        public AtemSDKComparisonMonitor(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            State = new AtemState();

            SetupInputs(switcher);
            SetupMixEffects(switcher);
            SetupSerialPorts(switcher);
            SetupMultiViews(switcher);
            SetupDownstreamKeyers(switcher);
            SetupMediaPool(switcher);
            SetupMediaPlayers(switcher, updateSettings);
            SetupMacros(switcher);
            SetupAudio(switcher);
            SetupHyperdecks(switcher);

            //switcher.AllowStreamingToResume();

            SetupDisposable(new SwitcherPropertiesCallback(State, switcher, GetFireCommandKey(null)));
        }

        public void Dispose()
        {
            _cleanupCallbacks.ForEach(cb => cb());
        }

        private Action<string> GetFireCommandKey(string root) => SdkCallbackUtil.AppendChange(p => OnStateChange?.Invoke(this, p), root);

        public delegate void StateChangeHandler(object sender, string path);
        public event StateChangeHandler OnStateChange;
        
        private void SetupDisposable(IDisposable obj) => _cleanupCallbacks.Add(obj.Dispose);

        private void SetupAudio(IBMDSwitcher switcher)
        {
            if (switcher is IBMDSwitcherAudioMixer mixer)
            {
                State.Audio = new AudioState();
                SetupDisposable(new AudioMixerCallback(State.Audio, mixer, GetFireCommandKey("Audio")));

                var talkback = switcher as IBMDSwitcherTalkback;
                if (talkback != null)
                {
                    var cbt = new TalkbackCallback(State.Audio.Talkback, talkback, GetFireCommandKey("Audio.Talkback"));
                    SetupDisposable(cbt);
                    cbt.NotifyAll(State.Audio.Inputs.Keys);
                }

                // TODO others
            } else if (switcher is IBMDSwitcherFairlightAudioMixer fairlightMixer)
            {
                State.Fairlight = new FairlightAudioState();
                SetupDisposable(new FairlightAudioMixerCallback(State.Fairlight, fairlightMixer, GetFireCommandKey("Fairlight")));
            }
        }

        private void SetupSerialPorts(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSerialPortIterator>(switcher.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherSerialPort>(iterator.Next, (port, id) =>
            {
                Assert.Equal((uint) 0, id);

                SetupDisposable(new SerialPortPropertiesCallback(State.Settings, port, GetFireCommandKey("Settings.SerialPort")));
            });
        }

        private void SetupHyperdecks(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherHyperDeckIterator>(switcher.CreateIterator);
            State.Settings.Hyperdecks = AtemSDKConverter.IterateList<IBMDSwitcherHyperDeck, SettingsState.HyperdeckState>(iterator.Next, (deck, id) =>
            {
                var deckState = new SettingsState.HyperdeckState();
                SetupDisposable(new HyperDeckPropertiesCallback(deckState, deck, GetFireCommandKey($"Settings.Hyperdecks.{id:D}")));
                return deckState;
            });
        }

        private void SetupMultiViews(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(switcher.CreateIterator);
            State.Settings.MultiViewers = AtemSDKConverter.IterateList<IBMDSwitcherMultiView, MultiViewerState>(iterator.Next,
                (mv, id) =>
                {
                    var mvState = new MultiViewerState();
                    SetupDisposable(new MultiViewPropertiesCallback(mvState, mv, GetFireCommandKey($"Settings.MultiViewers.{id:D}")));
                    return mvState;
                });
        }

        private void SetupMediaPlayers(IBMDSwitcher switcher, AtemStateBuilderSettings updateSettings)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMediaPlayerIterator>(switcher.CreateIterator);
            State.MediaPlayers = AtemSDKConverter.IterateList<IBMDSwitcherMediaPlayer, MediaPlayerState>(iterator.Next,
                (media, id) =>
                {
                    var player = new MediaPlayerState();
                    if (State.MediaPool.Clips.Count > 0)
                        player.ClipStatus = new MediaPlayerState.ClipStatusState();

                    SetupDisposable(new MediaPlayerCallback(player, updateSettings, media, GetFireCommandKey($"MediaPlayers.{id:D}")));
                    return player;
                });

        }
        
        private void SetupMediaPool(IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMediaPool;

            // General
            // TODO

            // Stills
            pool.GetStills(out IBMDSwitcherStills stills);
            SetupDisposable(new MediaPoolStillsCallback(State.MediaPool, stills, GetFireCommandKey("MediaPool.Stills")));

            // Clips
            pool.GetClipCount(out uint clipCount);
            State.MediaPool.Clips = Enumerable.Repeat(0, (int)clipCount).Select(i => new MediaPoolState.ClipState()).ToList();
            for (uint i = 0; i < clipCount; i++)
            {
                pool.GetClip(i, out IBMDSwitcherClip clip);
                SetupDisposable(new MediaPoolClipCallback(State.MediaPool.Clips[(int)i], clip, GetFireCommandKey($"MediaPool.Clips.{i:D}")));
            }
        }

        private void SetupMacros(IBMDSwitcher switcher)
        {
            var pool = switcher as IBMDSwitcherMacroPool;
            SetupDisposable(new MacroPoolCallback(State.Macros, pool, GetFireCommandKey("Macros.Pool")));

            var ctrl = switcher as IBMDSwitcherMacroControl;
            SetupDisposable(new MacroControlCallback(State.Macros, ctrl, GetFireCommandKey("Macros")));
        }

        private void SetupDownstreamKeyers(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherDownstreamKeyIterator>(switcher.CreateIterator);
            State.DownstreamKeyers = AtemSDKConverter.IterateList<IBMDSwitcherDownstreamKey, DownstreamKeyerState>(
                iterator.Next,
                (key, id) =>
                {
                    var dsk = new DownstreamKeyerState();
                    SetupDisposable(new DownstreamKeyerPropertiesCallback(dsk, key, GetFireCommandKey($"DownstreamKeyers.{id:D}")));
                    return dsk;
                });
        }

        private void SetupMixEffects(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(switcher.CreateIterator);
            State.MixEffects = AtemSDKConverter.IterateList<IBMDSwitcherMixEffectBlock, MixEffectState>(iterator.Next, (me, id) =>
            {
                var meState = new MixEffectState();
                SetupDisposable(new MixEffectPropertiesCallback(meState, me, GetFireCommandKey($"MixEffects.{id:D}")));
                return meState;
            });
        }

        private void SetupInputs(IBMDSwitcher switcher)
        {
            var auxes = new List<AuxState>();
            var cols = new List<ColorState>();
            var ssrcs = new List<SuperSourceState>();

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(switcher.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherInput>(iterator.Next, (input, i) =>
            {
                input.GetInputId(out long id);
                var src = (VideoSource)id;

                var st = State.Settings.Inputs[src] = new InputState();
                SetupDisposable(new InputCallback(st, input, GetFireCommandKey($"Settings.Inputs.{id:D}")));

                if (input is IBMDSwitcherInputAux aux)
                {
                    AuxiliaryId id2 = AtemEnumMaps.GetAuxId(src);
                    var st2 = new AuxState();
                    SetupDisposable(new AuxiliaryCallback(st2, aux, GetFireCommandKey($"Auxiliaries.{id2:D}")));
                    auxes.Add(st2);
                }

                if (input is IBMDSwitcherInputColor col)
                {
                    ColorGeneratorId id2 = AtemEnumMaps.GetSourceIdForGen(src);
                    var st2 = new ColorState();
                    SetupDisposable(new ColorCallback(st2, col, GetFireCommandKey($"ColorGenerators.{id2:D}")));
                    cols.Add(st2);
                }

                if (input is IBMDSwitcherInputSuperSource ssrc)
                {
                    // TODO - properly
                    SuperSourceId ssrcId = SuperSourceId.One;
                    var st2 = new SuperSourceState();
                    SetupDisposable(new SuperSourceCallback(st2, ssrc, GetFireCommandKey($"SuperSources.{ssrcId:D}")));
                    ssrcs.Add(st2);
                }
            });

            State.Auxiliaries = auxes;
            State.ColorGenerators = cols;
            State.SuperSources = ssrcs;
        }

    }
}