using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibAtem.MockTests.SuperSource
{
    public abstract class SuperSourceTestBase
    {
        protected static void EachSuperSource(AtemMockServerWrapper helper, Action<AtemState, SuperSourceState, IBMDSwitcherInputSuperSource, SuperSourceId, int> fcn, int iterations = 5)
        {
            Dictionary<VideoSource, IBMDSwitcherInputSuperSource> ssrcs = helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>();
            foreach (KeyValuePair<VideoSource, IBMDSwitcherInputSuperSource> ssrc in ssrcs)
            {
                AtemState stateBefore = helper.Helper.LibState;
                SuperSourceId id = (SuperSourceId)(ssrc.Key - VideoSource.SuperSource);
                SuperSourceState ssrcBefore = stateBefore.SuperSources[(int)id];
                Assert.NotNull(ssrcBefore);

                for (int i = 0; i < iterations; i++)
                {
                    fcn(stateBefore, ssrcBefore, ssrc.Value, id, i);
                }
            }
        }

        protected static void EachSuperSourceBorder(AtemMockServerWrapper helper, Action<AtemState, SuperSourceState.BorderState, IBMDSwitcherSuperSourceBorder, SuperSourceId, int> fcn, int iterations = 5)
        {
            EachSuperSource(helper, (stateBefore, ssrcState, sdk, ssrcId, i) =>
            {
                fcn(stateBefore, ssrcState.Border, (IBMDSwitcherSuperSourceBorder)sdk, ssrcId, i);
            }, iterations);
        }

        protected static void EachSuperSourceBox(AtemMockServerWrapper helper, Action<AtemState, SuperSourceState.BoxState, IBMDSwitcherSuperSourceBox, SuperSourceId, SuperSourceBoxId, int> fcn, int iterations = 5)
        {
            var allBoxes = new List<Tuple<SuperSourceId, SuperSourceBoxId, IBMDSwitcherSuperSourceBox>>();

            foreach(KeyValuePair<VideoSource, IBMDSwitcherInputSuperSource> ssrc in helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>())
            {
                SuperSourceId id = (SuperSourceId)(ssrc.Key - VideoSource.SuperSource);
                var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSuperSourceBoxIterator>(ssrc.Value.CreateIterator);
                AtemSDKConverter.Iterate<IBMDSwitcherSuperSourceBox>(iterator.Next, (box, i) =>
                {
                    allBoxes.Add(Tuple.Create(id, (SuperSourceBoxId)i, box));
                });
            }

            var boxes = Randomiser.SelectionOfGroup(allBoxes);
            foreach (Tuple<SuperSourceId, SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box in boxes)
            {
                AtemState stateBefore = helper.Helper.LibState;

                SuperSourceState.BoxState boxBefore = stateBefore.SuperSources[(int)box.Item1].Boxes[(int)box.Item2];
                Assert.NotNull(boxBefore);

                for (int i = 0; i < iterations; i++)
                {
                    fcn(stateBefore, boxBefore, box.Item3, box.Item1, box.Item2, i);
                }
            }
        }
    }
}