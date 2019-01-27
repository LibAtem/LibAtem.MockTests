using BMDSwitcherAPI;
using LibAtem.Common;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibAtem.ComparisonTests2.Util
{
    public static class ComparisonTestUtil
    {
        public static List<Tuple<MixEffectBlockId, T>> GetMixEffects<T>(this AtemClientWrapper client) where T : class
        {
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            client.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            var iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

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

            Guid itId = typeof(IBMDSwitcherAudioInputIterator).GUID;
            mixer.CreateIterator(ref itId, out IntPtr itPtr);
            var iterator = (IBMDSwitcherAudioInputIterator)Marshal.GetObjectForIUnknown(itPtr);

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
            Guid itId = typeof(IBMDSwitcherMediaPlayerIterator).GUID;
            client.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            var iterator = (IBMDSwitcherMediaPlayerIterator)Marshal.GetObjectForIUnknown(itPtr);

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
