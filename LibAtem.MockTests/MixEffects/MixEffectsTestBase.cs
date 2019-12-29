using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
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

        protected static void SelectionOfKeyers<T>(AtemMockServerWrapper helper, Action<AtemState, MixEffectState.KeyerState, T, MixEffectBlockId, UpstreamKeyId, int> fcn, int iterations = 5) where T : class
        {
            var keyers = GetKeyers<T>(helper);
            var useKeyers = Randomiser.SelectionOfGroup(keyers);

            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, T> keyer in useKeyers)
            {
                AtemState stateBefore = helper.Helper.LibState;
                MixEffectState.KeyerState keyerBefore = stateBefore.MixEffects[(int)keyer.Item1].Keyers[(int)keyer.Item2];

                for (int i = 0; i < iterations; i++)
                {
                    fcn(stateBefore, keyerBefore, keyer.Item3, keyer.Item1, keyer.Item2, i);
                }
            }
        }

        protected static void EachMixEffect<T>(AtemMockServerWrapper helper, Action<AtemState, MixEffectState, T, MixEffectBlockId, int> fcn, int iterations = 5) where T : class
        {
            List<Tuple<MixEffectBlockId, T>> mixEffects = GetMixEffects<T>(helper);
            foreach (Tuple<MixEffectBlockId, T> me in mixEffects)
            {
                AtemState stateBefore = helper.Helper.LibState;
                MixEffectState meBefore = stateBefore.MixEffects[(int)me.Item1];

                for (int i = 0; i < iterations; i++)
                {
                    fcn(stateBefore, meBefore, me.Item2, me.Item1, i);
                }
            }
        }

        protected static VideoSource[] GetTransitionSourcesSelection(AtemMockServerWrapper helper)
        {
            List<VideoSource> deviceSources = helper.Helper.LibState.Settings.Inputs.Keys.ToList();
            VideoSource[] validSources = deviceSources.Where(s =>
                s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask) &&
                s.IsAvailable(SourceAvailability.KeySource)).ToArray();
            return VideoSourceUtil.TakeSelection(validSources);
        }
    }
}