using BMDSwitcherAPI;
using LibAtem.Common;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibAtem.ComparisonTests.State.SDK;

namespace LibAtem.ComparisonTests.Util
{
    public static class ComparisonTestUtil
    {
        public static List<Tuple<MixEffectBlockId, T>> GetMixEffects<T>(this AtemClientWrapper client) where T : class
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(client.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<MixEffectBlockId, T>>();
            int index = 0;
            for (iterator.Next(out IBMDSwitcherMixEffectBlock r); r != null; iterator.Next(out r))
            {
                if (r is T rt)
                    result.Add(Tuple.Create((MixEffectBlockId)index, rt));
                index++;
            }

            return result;
        }

        /*
        
        public static List<Tuple<AudioSource, IBMDSwitcherAudioInput>> GetAudioInputs(this AtemClientWrapper client)
        {
            var mixer = (IBMDSwitcherAudioMixer)client.SdkSwitcher;

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioInputIterator>(client.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<AudioSource, IBMDSwitcherAudioInput>>();
            for (iterator.Next(out IBMDSwitcherAudioInput r); r != null; iterator.Next(out r))
            {
                r.GetAudioInputId(out long id);
                result.Add(Tuple.Create((AudioSource)id, r));
            }

            return result;
        }
        */
        public static List<Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer>> GetMediaPlayers(this AtemClientWrapper client)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMediaPlayerIterator>(client.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<MediaPlayerId, IBMDSwitcherMediaPlayer>>();
            int index = 0;
            for (iterator.Next(out IBMDSwitcherMediaPlayer r); r != null; iterator.Next(out r))
            {
                result.Add(Tuple.Create((MediaPlayerId)index, r));
                index++;
            }

            return result;
        }
    }
}
