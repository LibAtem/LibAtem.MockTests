using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Common;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
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
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            Client.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

            iterator.Next(out IBMDSwitcherMixEffectBlock meBlock);
            return meBlock as T;
        }

        protected List<Tuple<MixEffectBlockId, T>> GetMixEffects<T>() where T : class
        {
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            Client.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

            var result = new List<Tuple<MixEffectBlockId, T>>();
            int index = 0;
            for (iterator.Next(out IBMDSwitcherMixEffectBlock r); r != null; iterator.Next(out r))
            {
                if (r is T rt)
                    result.Add(Tuple.Create((MixEffectBlockId) index, rt));
                index++;
            }

            return result;
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

        protected bool WriteAndFail(string s)
        {
            Output.WriteLine(s);
            return true;
        }
    }
}