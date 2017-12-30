using System;
using System.Collections.Generic;
using System.Linq;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.XmlState;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestKeyers : ComparisonTestBase
    {
        private static readonly IReadOnlyDictionary<MixEffectKeyType, _BMDSwitcherKeyType> TypeMap;

        static TestKeyers()
        {
            TypeMap = new Dictionary<MixEffectKeyType, _BMDSwitcherKeyType>
            {
                {MixEffectKeyType.Luma, _BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma},
                {MixEffectKeyType.Chroma, _BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma},
                {MixEffectKeyType.Pattern, _BMDSwitcherKeyType.bmdSwitcherKeyTypePattern},
                {MixEffectKeyType.DVE, _BMDSwitcherKeyType.bmdSwitcherKeyTypeDVE},
            };
        }

        public TestKeyers(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private void ClearKeyerType(AtemComparisonHelper helper)
        {
            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
            {
                key.Item3.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma);
                key.Item3.SetOnAir(0);
            }

            foreach (var me in GetMixEffects<IBMDSwitcherTransitionParameters>())
                me.Item2.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);

            helper.Sleep();
        }

        [Fact]
        public void EnsureTypeMapIsComplete()
        {
            EnumMap.EnsureIsComplete(TypeMap);
        }

        [Fact]
        public void TestKeyerCount()
        {
            int keyers = GetKeyers<IBMDSwitcherKey>().Select(k => k.Item3).Count();
            Assert.Equal((int) Client.Profile.UpstreamKeys, keyers);
        }

        [Fact]
        public void TestKeyerCanBeDVE()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
                {
                    var me = GetMixEffect<IBMDSwitcherTransitionParameters>();
                    me.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    helper.Sleep();

                    key.Item3.CanBeDVEKey(out int canBeDve);
                    Assert.Equal(1, canBeDve);

                    me.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDVE);
                    helper.Sleep();

                    key.Item3.CanBeDVEKey(out canBeDve);
                    Assert.Equal(0, canBeDve);

                    me.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    helper.Sleep();
                }
            }
        }

        [Fact]
        public void TestKeyerType()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                try
                {
                    ClearKeyerType(helper);
                    foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
                    {
                        List<MixEffectKeyType> options = Enum.GetValues(typeof(MixEffectKeyType)).OfType<MixEffectKeyType>().Where(o => o.IsAvailable(Client.Profile)).ToList();
                        List<MixEffectKeyType> badOptions = Enum.GetValues(typeof(MixEffectKeyType)).OfType<MixEffectKeyType>().Where(o => !o.IsAvailable(Client.Profile)).ToList();

                        ICommand Setter(MixEffectKeyType type)
                        {
                            return new MixEffectKeyTypeSetCommand
                            {
                                MixEffectIndex = key.Item1,
                                KeyerIndex = key.Item2,
                                Mask = MixEffectKeyTypeSetCommand.MaskFlags.Type,
                                KeyType = type,
                            };
                        }

                        MixEffectKeyType? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand {MixEffectIndex = key.Item1, KeyerIndex = key.Item2})?.Mode;

                        EnumValueComparer<MixEffectKeyType, _BMDSwitcherKeyType>.Run(helper, TypeMap, Setter, key.Item3.GetType, Getter, options.ToArray());
                        EnumValueComparer<MixEffectKeyType, _BMDSwitcherKeyType>.Fail(helper, TypeMap, Setter, key.Item3.GetType, Getter, badOptions.ToArray());
                        // Ensure any special ones are clear for the next keyer setup
                        EnumValueComparer<MixEffectKeyType, _BMDSwitcherKeyType>.Run(helper, TypeMap, Setter, key.Item3.GetType, Getter, MixEffectKeyType.Luma);
                    }

                    // then something...??
                }
                finally
                {
                    ClearKeyerType(helper);
                }
            }
        }

        #region Type DVE

        [Fact]
        public void TestKeyerTypeDVE()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                if (Client.Profile.DVE == 0)
                {
                    Output.WriteLine("Skipping DVE keyer enabler props, as device does not support it");
                    return;
                }

                ClearKeyerType(helper);
                List<Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey>> keyers = GetKeyers<IBMDSwitcherKey>();
                if (Client.Profile.DVE > keyers.Count)
                    Output.WriteLine("Warning: More DVE Keyers than Keyers. Sounds like the profile is incorrect");

                try
                {
                    SetKeyerTypeWithLimit(helper, keyers, MixEffectKeyType.DVE, Client.Profile.DVE);
                }
                finally
                {
                    ClearKeyerType(helper);
                }
            }
        }

        private void SetKeyerTypeWithLimit(AtemComparisonHelper helper, List<Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey>> keyers, MixEffectKeyType type, uint limit)
        {
            Assert.NotEqual((uint) 0, limit);

            void SetType(Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k)
            {
                helper.SendCommand(new MixEffectKeyTypeSetCommand
                {
                    MixEffectIndex = k.Item1,
                    KeyerIndex = k.Item2,
                    Mask = MixEffectKeyTypeSetCommand.MaskFlags.Type,
                    KeyType = type,
                });
            }
            void SetOnAir(Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k)
            {
                helper.SendCommand(new MixEffectKeyOnAirSetCommand()
                {
                    MixEffectIndex = k.Item1,
                    KeyerIndex = k.Item2,
                    OnAir = true,
                });
            }

            // first we set them all to the type
            _BMDSwitcherKeyType bmdType = TypeMap[type];
            keyers.ForEach(SetType);

            helper.Sleep();
            // ensure they were all set
            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k in keyers)
            {
                MixEffectKeyType? val = helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand { MixEffectIndex = k.Item1, KeyerIndex = k.Item2 })?.Mode;
                Assert.True(val.HasValue);
                Assert.Equal(val.Value, type);
            }

            // with the keyers turned on, try changing them to the type
            // This limit is currently enforced client side, so we cannot test for this
            //            ClearKeyerType(helper);
            //            keyers.ForEach(k => k.Item3.SetOnAir(1));
            //            helper.Sleep();
            //
            //            uint active = 0;
            //            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k in keyers)
            //            {
            //                SetType(k);
            //
            //                helper.Sleep();
            //                k.Item3.GetType(out _BMDSwitcherKeyType res);
            //
            //                if (active < limit)
            //                {
            //                    Assert.Equal(res, bmdType);
            //                    active++;
            //                }
            //                else
            //                {
            //                    Assert.NotEqual(res, bmdType);
            //                }
            //            }
            
            // try turning them on one by one until the limit
            ClearKeyerType(helper);
            keyers.ForEach(k => k.Item3.SetOnAir(0));
            keyers.ForEach(SetType);
            helper.Sleep();
            
            uint active = 0;
            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k in keyers)
            {
                SetOnAir(k);
                helper.Sleep();

                k.Item3.GetOnAir(out int onAir);
            
                if (active < limit)
                {
                    Assert.False(onAir == 0);
                    active++;
                }
                else
                {
                    Assert.True(onAir == 0);
                }
            }

            // Note: this suffers the same issue as above. It is possible to set a second keyer to dve, even if the dve is already on air
//            // now reset them all
//            ClearKeyerType(helper);
//
//            // now set as many are allowed to type and on air, then try setting more to type
//            for (uint i = 0; i < limit; i++)
//            {
//                Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k = keyers[(int) i];
//                SetOnAir(k);
//                SetType(k);
//                helper.Sleep();
//            }
//
//            for (uint i = limit; i < keyers.Count; i++)
//            {
//                Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k = keyers[(int)i];
//                SetType(k);
//                helper.Sleep();
//
//                k.Item3.GetType(out _BMDSwitcherKeyType newType);
//                Assert.NotEqual(bmdType, newType);
//            }
        }

        #endregion Type DVE
        
        [Fact]
        public void TestKeyerInputCut()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    key.Item3.GetCutInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                    Assert.Equal((_BMDSwitcherInputAvailability) (key.Item1 + 1) | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut, availability);

                    long[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(Client.Profile) && s.IsAvailable(key.Item1) && s.IsAvailable(SourceAvailability.KeySource)).Select(s => (long)s).ToArray();
                    long[] badValues = VideoSourceLists.All.Select(s => (long)s).Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(long v) => new MixEffectKeyCutSourceSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        CutSource = (VideoSource)v,
                    };

                    long? Getter() => (long?) helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand {MixEffectIndex = key.Item1, KeyerIndex = key.Item2})?.CutSource;

                    ValueTypeComparer<long>.Run(helper, Setter, key.Item3.GetInputCut, Getter, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, key.Item3.GetInputCut, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerInputFill()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    key.Item3.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                    Assert.Equal((_BMDSwitcherInputAvailability) (key.Item1 + 1), availability);

                    long[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(Client.Profile) && s.IsAvailable(key.Item1)).Select(s => (long)s).ToArray();
                    long[] badValues = VideoSourceLists.All.Select(s => (long)s).Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(long v) => new MixEffectKeyFillSourceSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        FillSource = (VideoSource)v,
                    };

                    long? Getter() => (long?)helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.FillSource;

                    ValueTypeComparer<long>.Run(helper, Setter, key.Item3.GetInputFill, Getter, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, key.Item3.GetInputFill, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerOnAir()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                ClearKeyerType(helper);

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new MixEffectKeyOnAirSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        OnAir = v
                    };

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyOnAirGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.OnAir;
                    
                    BoolValueComparer.Run(helper, Setter, key.Item3.GetOnAir, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestKeyerTransitionSelectionMask()
        {
            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
            {
                key.Item3.GetTransitionSelectionMask(out _BMDSwitcherTransitionSelection mask);
                var expected = key.Item2.ToTransitionLayerKey();
                Assert.Equal((_BMDSwitcherTransitionSelection) expected, mask);
            }
        }

        #region Mask

        [Fact]
        public void TestKeyerMaskEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                ClearKeyerType(helper);

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.Enabled,
                        MaskEnabled = v
                    };

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskEnabled;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetMasked, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskTop()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                ClearKeyerType(helper);

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 5, -5, -9, 9, 4.78 };
                    //double[] badValues = { -9.01, 9.01, 9.1, -9.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskTop,
                        MaskTop = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskTop;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskTop, Getter, testValues);
                    // Note: there is no enforcement on thes values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskTop, Getter, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep();

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -3, 3, 1.78 };
                    //double[] badValues = { -3.01, 3.01, 3.1, -3.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskTop,
                        MaskTop = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskTop;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskTop, Getter, testValues);
                    // Note: there is no enforcement on thes values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskTop, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskBottom()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                ClearKeyerType(helper);

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 5, -5, -9, 9, 4.78 };
                    //double[] badValues = { -9.01, 9.01, 9.1, -9.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskBottom,
                        MaskBottom = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskBottom;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskBottom, Getter, testValues);
                    // Note: there is no enforcement on these values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskBottom, Getter, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep();

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -3, 3, 1.78 };
                    //double[] badValues = { -3.01, 3.01, 3.1, -3.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskBottom,
                        MaskBottom = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskBottom;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskBottom, Getter, testValues);
                    // Note: there is no enforcement on thes values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskBottom, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskLeft()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                ClearKeyerType(helper);

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 8, -8, -16, 16, 6.78 };
                    //double[] badValues = { -16.01, 16.01, 16.1, -16.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskLeft,
                        MaskLeft = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskLeft;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskLeft, Getter, testValues);
                    // Note: there is no enforcement on these values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskLeft, Getter, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep();

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -4, 4, 1.78 };
                    //double[] badValues = { -4.01, 4.01, 4.1, -4.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskLeft,
                        MaskLeft = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskLeft;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskLeft, Getter, testValues);
                    // Note: there is no enforcement on thes values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskLeft, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskRight()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                ClearKeyerType(helper);

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 8, -8, -16, 16, 6.78 };
                    //double[] badValues = { -16.01, 16.01, 16.1, -16.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskRight,
                        MaskRight = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskRight;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskRight, Getter, testValues);
                    // Note: there is no enforcement on these values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskRight, Getter, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep();

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -4, 4, 1.78 };
                    //double[] badValues = { -4.01, 4.01, 4.1, -4.1 };

                    ICommand Setter(double v) => new MixEffectKeyMaskSetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyMaskSetCommand.MaskFlags.MaskRight,
                        MaskRight = v
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskRight;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskRight, Getter, testValues);
                    // Note: there is no enforcement on thes values
                    //DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskRight, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerResetMask()
        {
            // This uses a client side set
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                ClearKeyerType(helper);

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    key.Item3.SetMaskBottom(1);
                    key.Item3.SetMaskTop(1);
                    key.Item3.SetMaskLeft(1);
                    key.Item3.SetMaskRight(1);
                    helper.Sleep();
                    key.Item3.ResetMask();
                    helper.Sleep();

                    MixEffectKeyPropertiesGetCommand cmd = helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand {MixEffectIndex = key.Item1, KeyerIndex = key.Item2});
                    Assert.NotNull(cmd);

                    Assert.True(Math.Abs(-16 - cmd.MaskLeft) < 0.001);
                    Assert.True(Math.Abs(16 - cmd.MaskRight) < 0.001);
                    Assert.True(Math.Abs(9 - cmd.MaskTop) < 0.001);
                    Assert.True(Math.Abs(-9 - cmd.MaskBottom) < 0.001);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep();

                foreach (var key in GetKeyers<IBMDSwitcherKey>())
                {
                    key.Item3.SetMaskBottom(1);
                    key.Item3.SetMaskTop(1);
                    key.Item3.SetMaskLeft(1);
                    key.Item3.SetMaskRight(1);
                    helper.Sleep();
                    key.Item3.ResetMask();
                    helper.Sleep();

                    MixEffectKeyPropertiesGetCommand cmd = helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 });
                    Assert.NotNull(cmd);

                    Assert.True(Math.Abs(-4 - cmd.MaskLeft) < 0.001);
                    Assert.True(Math.Abs(4 - cmd.MaskRight) < 0.001);
                    Assert.True(Math.Abs(3 - cmd.MaskTop) < 0.001);
                    Assert.True(Math.Abs(-3 - cmd.MaskBottom) < 0.001);
                }
            }
        }

        #endregion Mask
    }
}