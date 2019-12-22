using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    public abstract class MixEffectsTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly AtemServerClientPool Pool;

        protected internal MixEffectsTestBase(ITestOutputHelper output, AtemServerClientPool pool)
        {
            Output = output;
            Pool = pool;
        }

        protected static T GetMixEffect<T>(AtemMockServerWrapper helper) where T : class
        {
            return GetMixEffects<T>(helper).Select(m => m.Item2).First();
        }

        protected static List<Tuple<MixEffectBlockId, T>> GetMixEffects<T>(AtemMockServerWrapper helper) where T : class
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMixEffectBlockIterator>(helper.Clients.SdkSwitcher.CreateIterator);

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

        protected static List<Tuple<MixEffectBlockId, UpstreamKeyId, T>> GetKeyers<T>(AtemMockServerWrapper helper) where T : class
        {
            var result = new List<Tuple<MixEffectBlockId, UpstreamKeyId, T>>();

            List<Tuple<MixEffectBlockId, IBMDSwitcherMixEffectBlock>> mes = GetMixEffects<IBMDSwitcherMixEffectBlock>(helper);
            foreach (var me in mes)
            {
                var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherKeyIterator>(me.Item2.CreateIterator);

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

        protected static IEnumerable<T> SelectionOfGroup<T>(List<T> options, int randomCount = 3)
        {
            var rand = new Random();

            for (int i = 0; i < randomCount && options.Count > 0; i++)
            {
                int ind = rand.Next(0, options.Count);
                yield return options[ind];
                options.RemoveAt(ind);
            }
        }
    }
}