﻿using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.ComparisonTests2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    public abstract class MixEffectsTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly AtemClientWrapper Client;

        internal protected MixEffectsTestBase(ITestOutputHelper output, AtemClientWrapper client)
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
                IBMDSwitcherKeyIterator iterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(itPtr);

                int o = 0;
                for (iterator.Next(out IBMDSwitcherKey r); r != null; iterator.Next(out r))
                {
                    if (r is T rt)
                        result.Add(Tuple.Create(me.Item1, (UpstreamKeyId)o, rt));
                    o++;
                }
            }

            return result;
        }
    }
}
