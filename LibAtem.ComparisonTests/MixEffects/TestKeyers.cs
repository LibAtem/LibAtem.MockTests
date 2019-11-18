using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestKeyers : MixEffectsTestBase
    {
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
        public void TestKeyerCount()
        {
            int keyers = GetKeyers<IBMDSwitcherKey>().Select(k => k.Item3).Count();
            Assert.Equal((int) (Client.Profile.UpstreamKeys * Client.Profile.MixEffectBlocks), keyers);
        }

        [Fact]
        public void TestKeyerCanBeDVE()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
                {
                    var me = GetMixEffect<IBMDSwitcherTransitionParameters>();
                    me.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    helper.Sleep();

                    key.Item3.CanBeDVEKey(out int canBeDve);
                    Assert.Equal(1, canBeDve);
                    Assert.Equal((uint)1, helper.Profile.DVE);

                    me.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDVE);
                    helper.Sleep();

                    key.Item3.CanBeDVEKey(out canBeDve);
                    Assert.Equal(0, canBeDve);

                    me.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    helper.Sleep();
                }
            }
        }

        private class KeyTypeTestDefinition : TestDefinitionBase<MixEffectKeyTypeSetCommand, MixEffectKeyType>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKey _sdk;

            public KeyTypeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override void Prepare()
            {
            }

            public override void SetupCommand(MixEffectKeyTypeSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public override string PropertyName => "KeyType";

            public override void UpdateExpectedState(AtemState state, bool goodValue, MixEffectKeyType v)
            {
                MixEffectState.KeyerPropertiesState obj = state.MixEffects[(int)_meId].Keyers[(int)_keyId].Properties;
                if (goodValue) SetCommandProperty(obj, "Mode", v);
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, MixEffectKeyType v)
            {
                yield return $"MixEffects.{_meId:D}.Keyers.{_keyId:D}.Properties";
            }

            public override MixEffectKeyType[] GoodValues => Enum.GetValues(typeof(MixEffectKeyType)).OfType<MixEffectKeyType>().Where(o => o.IsAvailable(_helper.Profile)).ToArray();
            public override MixEffectKeyType[] BadValues => Enum.GetValues(typeof(MixEffectKeyType)).OfType<MixEffectKeyType>().Except(GoodValues).ToArray();
        }
        [Fact]
        public void TestKeyerType()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                try
                {
                    ClearKeyerType(helper);
                    foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
                    {
                        new KeyTypeTestDefinition(helper, key).Run().RunSingle(MixEffectKeyType.Luma);
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

        [SkippableFact]
        public void TestKeyerTypeDVE()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Skip.If(helper.Profile.DVE == 0, "Model does not support DVE key");

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
            Assert.NotEqual((uint)0, limit);

            void SetType(Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> k)
            {
                helper.SendCommand(new MixEffectKeyTypeSetCommand
                {
                    MixEffectIndex = k.Item1,
                    KeyerIndex = k.Item2,
                    Mask = MixEffectKeyTypeSetCommand.MaskFlags.KeyType,
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
            _BMDSwitcherKeyType bmdType = AtemEnumMaps.MixEffectKeyTypeMap[type];
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

        private class KeyCutSourceTestDefinition : TestDefinitionBase<MixEffectKeyCutSourceSetCommand, VideoSource>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKey _sdk;

            public KeyCutSourceTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;

                key.Item3.GetCutInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                Assert.Equal((_BMDSwitcherInputAvailability)(key.Item1 + 1) | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut, availability);
            }

            public override void Prepare() => _sdk.SetInputCut((long)VideoSource.ColorBars);

            public override void SetupCommand(MixEffectKeyCutSourceSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public override string PropertyName => "CutSource";

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                MixEffectState.KeyerPropertiesState obj = state.MixEffects[(int)_meId].Keyers[(int)_keyId].Properties;
                if (goodValue) SetCommandProperty(obj, PropertyName, v);
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return $"MixEffects.{_meId:D}.Keyers.{_keyId:D}.Properties";
            }

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(_meId) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();
        }
        [Fact]
        public void TestKeyerInputCut()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyCutSourceTestDefinition(helper, k).Run());
        }

        private class KeyFillSourceTestDefinition : TestDefinitionBase<MixEffectKeyFillSourceSetCommand, VideoSource>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKey _sdk;

            public KeyFillSourceTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;

                key.Item3.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                Assert.Equal((_BMDSwitcherInputAvailability)(key.Item1 + 1), availability);
            }

            public override void Prepare() => _sdk.SetInputFill((long)VideoSource.ColorBars);

            public override void SetupCommand(MixEffectKeyFillSourceSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public override string PropertyName => "FillSource";

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                MixEffectState.KeyerPropertiesState obj = state.MixEffects[(int)_meId].Keyers[(int)_keyId].Properties;
                if (goodValue)
                {
                    SetCommandProperty(obj, PropertyName, v);
                    if (VideoSourceLists.MediaPlayers.Contains(v))
                        SetCommandProperty(obj, "CutSource", v + 1);
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return $"MixEffects.{_meId:D}.Keyers.{_keyId:D}.Properties";
            }

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(_meId)).ToArray();
        }
        [Fact]
        public void TestKeyerInputFill()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyFillSourceTestDefinition(helper, k).Run());
        }

        private class KeyOnAirTestDefinition : TestDefinitionBase<MixEffectKeyOnAirSetCommand, bool>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKey _sdk;

            public KeyOnAirTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;

                key.Item3.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                Assert.Equal((_BMDSwitcherInputAvailability)(key.Item1 + 1), availability);
            }

            public override void Prepare()
            {
                // Find inputs to use as sources, to ensure tally
                var validInputs = _helper.LibState.Settings.Inputs.Where(i => i.Key.IsAvailable(_meId) && i.Key.IsAvailable(SourceAvailability.KeySource) && !i.Value.Tally.ProgramTally && !i.Value.Tally.PreviewTally).Take(1).ToList();
                Assert.Single(validInputs);
                _sdk.SetInputCut((long)validInputs[0].Key);
                _sdk.SetInputFill((long)validInputs[0].Key);

                _sdk.SetOnAir(0);
            }

            public override void SetupCommand(MixEffectKeyOnAirSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public override string PropertyName => "OnAir";

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                MixEffectState.KeyerState obj = state.MixEffects[(int)_meId].Keyers[(int)_keyId];
                SetCommandProperty(obj, PropertyName, v);

                state.Settings.Inputs[obj.Properties.FillSource].Tally.ProgramTally = v;
                state.Settings.Inputs[obj.Properties.FillSource].Tally.PreviewTally = v;
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, bool v)
            {
                yield return $"MixEffects.{_meId:D}.Keyers.{_keyId:D}.OnAir";
            }
        }
        [Fact]
        public void TestKeyerOnAir()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ClearKeyerType(helper);

                GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyOnAirTestDefinition(helper, k).Run());
            }
        }

        [Fact]
        public void TestKeyerTransitionSelectionMask()
        {
            foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
            {
                key.Item3.GetTransitionSelectionMask(out _BMDSwitcherTransitionSelection mask);
                var expected = key.Item2.ToTransitionLayerKey();
                Assert.Equal((_BMDSwitcherTransitionSelection)expected, mask);
            }
        }

        #region Mask

        private abstract class KeyMaskTestDefinition<T> : TestDefinitionBase<MixEffectKeyMaskSetCommand, T>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKey _sdk;

            public KeyMaskTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override void Prepare()
            {
                _sdk.SetMasked(0);
                _sdk.SetMaskBottom(1);
                _sdk.SetMaskLeft(1);
                _sdk.SetMaskRight(1);
                _sdk.SetMaskTop(1);
                _helper.Sleep();
            }

            public static IEnumerable<VideoMode> VideoModes()
            {
                yield return VideoMode.P1080i50;
                yield return VideoMode.N720p5994;
                yield return VideoMode.P625i50PAL;
            }

            public override void SetupCommand(MixEffectKeyMaskSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.KeyerPropertiesState obj = state.MixEffects[(int)_meId].Keyers[(int)_keyId].Properties;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"MixEffects.{_meId:D}.Keyers.{_keyId:D}.Properties";
            }
        }
        private class KeyMaskEnabledTestDefinition : KeyMaskTestDefinition<bool>
        {
            public KeyMaskEnabledTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key) : base(helper, key)
            {
            }

            public override string PropertyName => "MaskEnabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestKeyerMaskEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ClearKeyerType(helper);

                GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyMaskEnabledTestDefinition(helper, k).Run());
            }
        }

        private class KeyMaskYTestDefinition : KeyMaskTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public KeyMaskYTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key, string propName) : base(helper, key)
            {
                _mode = mode;
                PropertyName = propName;
            }

            public override double MangleBadValue(double v)
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return 38;
                    case VideoMode.P625i50PAL:
                        return 11;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] GoodValues
            {
                get
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            return new double[] { 1, 0, 5, -5, -9, 9, 4.78 };
                        case VideoMode.P625i50PAL:
                            return new double[] { 1, 0, 2.5, -2.5, -3, 3, 1.78 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            // Note: these values are not enforced
            //public override double[] BadValues
            //{
            //    get
            //    {
            //        switch (_mode)
            //        {
            //            case VideoMode.P1080i50:
            //            case VideoMode.N720p5994:
            //                return new double[] { -9.01, 9.01, 9.1, -9.1 };
            //            case VideoMode.P625i50PAL:
            //                return new double[] { -3.01, 3.01, 3.1, -3.1 };
            //        default:
            //                throw new NotSupportedException();
            //        }
            //    }
            //}
        }
        [Fact]
        public void TestKeyerMaskTop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ClearKeyerType(helper);

                foreach (var mode in KeyMaskTestDefinition<double>.VideoModes())
                    GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyMaskYTestDefinition(helper, mode, k, "MaskTop").Run());
            }
        }
        [Fact]
        public void TestKeyerMaskBottom()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ClearKeyerType(helper);

                foreach (var mode in KeyMaskTestDefinition<double>.VideoModes())
                    GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyMaskYTestDefinition(helper, mode, k, "MaskBottom").Run());
            }
        }
        
        private class KeyMaskXTestDefinition : KeyMaskTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public KeyMaskXTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key, string propName) : base(helper, key)
            {
                _mode = mode;
                PropertyName = propName;
            }

            public override double MangleBadValue(double v)
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return 52;
                    case VideoMode.P625i50PAL:
                        return 13;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] GoodValues
            {
                get
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            return new double[] { 1, 0, 8, -8, -16, 16, 6.78 };
                        case VideoMode.P625i50PAL:
                            return new double[] { 1, 0, 2.5, -2.5, -4, 4, 1.78 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            // Note: these values are not enforced
            //public override double[] BadValues
            //{
            //    get
            //    {
            //        switch (_mode)
            //        {
            //            case VideoMode.P1080i50:
            //            case VideoMode.N720p5994:
            //                return new double[] { -16.01, 16.01, 16.1, -16.1 };
            //            case VideoMode.P625i50PAL:
            //                return new double[] { -4.01, 4.01, 4.1, -4.1 };
            //            default:
            //                throw new NotSupportedException();
            //        }
            //    }
            //}
        }
        [Fact]
        public void TestKeyerMaskLeft()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ClearKeyerType(helper);

                foreach(var mode in KeyMaskTestDefinition<double>.VideoModes())
                    GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyMaskXTestDefinition(helper, mode, k, "MaskLeft").Run());
            }
        }
        [Fact]
        public void TestKeyerMaskRight()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                ClearKeyerType(helper);

                foreach (var mode in KeyMaskTestDefinition<double>.VideoModes())
                    GetKeyers<IBMDSwitcherKey>().ForEach(k => new KeyMaskXTestDefinition(helper, mode, k, "MaskRight").Run());
            }
        }

        [Fact]
        public void TestKeyerResetMask()
        {
            // This uses a client side set
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                var modes = new List<Tuple<_BMDSwitcherVideoMode, double, double>>()
                {
                    Tuple.Create(_BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i50, 16.0, 9.0),
                    Tuple.Create(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50, 16.0, 9.0),
                    Tuple.Create(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL, 4.0, 3.0)
                };

                foreach (var mode in modes)
                {
                    Client.SdkSwitcher.SetVideoMode(mode.Item1);
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

                        MixEffectKeyPropertiesGetCommand cmd = helper.FindWithMatching(new MixEffectKeyPropertiesGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 });
                        Assert.NotNull(cmd);

                        Assert.True(Math.Abs(-mode.Item2 - cmd.MaskLeft) < 0.001);
                        Assert.True(Math.Abs(mode.Item2 - cmd.MaskRight) < 0.001);
                        Assert.True(Math.Abs(mode.Item3 - cmd.MaskTop) < 0.001);
                        Assert.True(Math.Abs(-mode.Item3 - cmd.MaskBottom) < 0.001);
                    }
                }
            }
        }

        #endregion Mask

    }
}