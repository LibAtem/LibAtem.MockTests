using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Common;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
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

    public abstract class ComparisonTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly AtemClientWrapper Client;

        protected ComparisonTestBase(ITestOutputHelper output, AtemClientWrapper client)
        {
            Client = client;
            Output = output;
        }

        protected T GetMixEffect<T>() where T : class
        {
            return GetMixEffects<T>().Select(m => m.Item2).First();
        }
        
        protected List<Tuple<MixEffectBlockId, T>> GetMixEffects<T>() where T : class
        {
            return Client.GetMixEffects<T>();
        }

        protected List<Tuple<MixEffectBlockId, UpstreamKeyId, T>> GetKeyers<T>() where T : class
        {
            var result = new List<Tuple<MixEffectBlockId, UpstreamKeyId, T>>();

            List<Tuple<MixEffectBlockId, IBMDSwitcherMixEffectBlock>> mes = GetMixEffects<IBMDSwitcherMixEffectBlock>();
            foreach (var me in mes)
            {
                Guid itId = typeof(IBMDSwitcherKeyIterator).GUID;
                me.Item2.CreateIterator(ref itId, out IntPtr itPtr);
                IBMDSwitcherKeyIterator iterator = (IBMDSwitcherKeyIterator) Marshal.GetObjectForIUnknown(itPtr);

                int o = 0;
                for (iterator.Next(out IBMDSwitcherKey r); r != null; iterator.Next(out r))
                {
                    if (r is T rt)
                        result.Add(Tuple.Create(me.Item1, (UpstreamKeyId) o, rt));
                    o++;
                }
            }

            return result;
        }
    }
}