using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2
{
    [Collection("Client")]
    public class TestSuperSourceBoxes
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestSuperSourceBoxes(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        private IBMDSwitcherInputSuperSource GetSuperSource(AtemComparisonHelper helper)
        {
            return helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>().Select(s => s.Value).FirstOrDefault();
        }

        private IEnumerable<Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox>> GetSuperSourceBoxes(AtemComparisonHelper helper)
        {
            IBMDSwitcherInputSuperSource src = GetSuperSource(helper);
            if (src == null)
                yield break;

            Guid itId = typeof(IBMDSwitcherSuperSourceBoxIterator).GUID;
            src.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherSuperSourceBoxIterator iterator = (IBMDSwitcherSuperSourceBoxIterator)Marshal.GetObjectForIUnknown(itPtr);

            SuperSourceBoxId o = 0;
            for (iterator.Next(out IBMDSwitcherSuperSourceBox r); r != null; iterator.Next(out r))
            {
                yield return Tuple.Create(o, r);

                o++;
            }
        }

        private abstract class SuperSourceBoxTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly SuperSourceBoxId _id;
            protected readonly IBMDSwitcherSuperSourceBox _sdk;

            public SuperSourceBoxTestDefinition(AtemComparisonHelper helper, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper)
            {
                _id = box.Item1;
                _sdk = box.Item2;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new SuperSourceBoxGetCommand() { Index = _id });
            }
        }

        private class SuperSourceBoxEnabledTestDefinition : SuperSourceBoxTestDefinition<bool>
        {
            public SuperSourceBoxEnabledTestDefinition(AtemComparisonHelper helper, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetEnabled(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.Enabled,
                    Index = _id,
                    Enabled = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.SuperSource.Boxes[_id].Enabled = v;
            }
        }

        [SkippableFact]
        public void TestBoxEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    new SuperSourceBoxEnabledTestDefinition(helper, box).Run();
                }
            }
        }

        private class SuperSourceBoxInputTestDefinition : SuperSourceBoxTestDefinition<VideoSource>
        {
            public SuperSourceBoxInputTestDefinition(AtemComparisonHelper helper, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetEnabled(0);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceBox)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.Source,
                    Index = _id,
                    Source = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.SuperSource.Boxes[_id].InputSource = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new CommandQueueKey[0];
            }
        }

        [SkippableFact]
        public void TestBoxInputSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                    // We track this another way
                    _BMDSwitcherInputAvailability availabilityMask = 0;
                    box.Item2.GetInputAvailabilityMask(ref availabilityMask);
                    Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceBox, availabilityMask);

                    new SuperSourceBoxInputTestDefinition(helper, box).Run();
                }
            }
        }

        private class SuperSourceBoxPositionXTestDefinition : SuperSourceBoxTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public SuperSourceBoxPositionXTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
                _mode = mode;
            }

            public static IEnumerable<VideoMode> VideoModes()
            {
                yield return VideoMode.P1080i50;
                yield return VideoMode.N720p5994;
                yield return VideoMode.P625i50PAL;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPositionX(10);
            }

            public override double[] GoodValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { 0, 0.87, 48, 47.99, -48, -47.99, 9.65 };
                    case VideoMode.P625i50PAL:
                        return new double[] { 0, 0.87, 12, 11.99, -12, -11.99, 9.65 };
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] BadValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { -48.01, 48.01, 48.1, -48.1, -55, 55 };
                    case VideoMode.P625i50PAL:
                        return new double[] { -12.01, 12.01, 12.1, -12.1, -15, 15 };
                    default:
                        throw new NotSupportedException();
                }
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.PositionX,
                    Index = _id,
                    PositionX = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.SuperSource.Boxes[_id].PositionX = v;
                }
                else
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            state.SuperSource.Boxes[_id].PositionX = v >= 48 ? 48 : -48;
                            break;
                        case VideoMode.P625i50PAL:
                            state.SuperSource.Boxes[_id].PositionX = v >= 12 ? 12 : -12;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        [SkippableFact]
        public void TestBoxPositionX()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBoxPositionXTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    foreach (var box in GetSuperSourceBoxes(helper))
                    {
                        new SuperSourceBoxPositionXTestDefinition(helper, mode, box).Run();
                    }
                }
            }
        }

        private class SuperSourceBoxPositionYTestDefinition : SuperSourceBoxTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public SuperSourceBoxPositionYTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
                _mode = mode;
            }

            public static IEnumerable<VideoMode> VideoModes()
            {
                yield return VideoMode.P1080i50;
                yield return VideoMode.N720p5994;
                yield return VideoMode.P625i50PAL;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPositionY(10);
            }

            public override double[] GoodValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { 0, 0.87, 34, 33.99, -34, -33.99, 9.65 };
                    case VideoMode.P625i50PAL:
                        return new double[] { 0, 0.87, 10, 9.99, -10, -9.99, 6.65 };
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] BadValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { -34.01, 34.01, 34.1, -34.1, -39, 39 };
                    case VideoMode.P625i50PAL:
                        return new double[] { -10.01, 10.01, 10.1, -10.1, -15, 15 };
                    default:
                        throw new NotSupportedException();
                }
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.PositionY,
                    Index = _id,
                    PositionY = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.SuperSource.Boxes[_id].PositionY = v;
                }
                else
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            state.SuperSource.Boxes[_id].PositionY = v >= 34 ? 34 : -34;
                            break;
                        case VideoMode.P625i50PAL:
                            state.SuperSource.Boxes[_id].PositionY = v >= 10 ? 10 : -10;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        [SkippableFact]
        public void TestBoxPositionY()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBoxPositionXTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    foreach (var box in GetSuperSourceBoxes(helper))
                    {
                        new SuperSourceBoxPositionYTestDefinition(helper, mode, box).Run();
                    }
                }
            }
        }

        private class SuperSourceBoxSizeTestDefinition : SuperSourceBoxTestDefinition<double>
        {
            public SuperSourceBoxSizeTestDefinition(AtemComparisonHelper helper, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSize(0.5);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0.07, 0.874, 0.147, 0.999, 1.00 };
            }
            public override double[] BadValues()
            {
                return new double[] { 0, 0.06, 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.Size,
                    Index = _id,
                    Size = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.SuperSource.Boxes[_id].Size = v;
                else
                    state.SuperSource.Boxes[_id].Size = v >= 1 || v < 0 ? 1 : 0.07;
            }
        }

        [SkippableFact]
        public void TestBoxSize()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    new SuperSourceBoxSizeTestDefinition(helper, box).Run();
                }
            }
        }

        private class SuperSourceBoxCroppedTestDefinition : SuperSourceBoxTestDefinition<bool>
        {
            public SuperSourceBoxCroppedTestDefinition(AtemComparisonHelper helper, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetCropped(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.Cropped,
                    Index = _id,
                    Cropped = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.SuperSource.Boxes[_id].Cropped = v;
            }
        }

        [SkippableFact]
        public void TestBoxCropped()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    new SuperSourceBoxCroppedTestDefinition(helper, box).Run();
                }
            }
        }

        private abstract class SuperSourceBoxCropYTestDefinition : SuperSourceBoxTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public SuperSourceBoxCropYTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
                _mode = mode;
            }

            public static IEnumerable<VideoMode> VideoModes()
            {
                yield return VideoMode.P1080i50;
                yield return VideoMode.N720p5994;
                yield return VideoMode.P625i50PAL;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetCropTop(5);
                _sdk.SetCropBottom(5);
                _helper.Sleep();
            }

            protected double ClampValueToRange(bool goodValue, double v)
            {
                if (goodValue)
                    return v;

                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return v >= 18 ? 18 : 0;
                    case VideoMode.P625i50PAL:
                        return v >= 6 ? 6 : 0;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] GoodValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { 0, 0.87, 18, 17.99, 0.01, 9.65 };
                    case VideoMode.P625i50PAL:
                        return new double[] { 0, 0.87, 6, 5.99, 0.01, 3.65 };
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] BadValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { -0.01, 18.01, 18.1, -0.1, -29, 29 };
                    case VideoMode.P625i50PAL:
                        return new double[] { -0.01, 6.01, 6.1, -0.1, -15, 15 };
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private class SuperSourceBoxCropTopTestDefinition : SuperSourceBoxCropYTestDefinition
        {
            public SuperSourceBoxCropTopTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, mode, box)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.CropTop,
                    Index = _id,
                    CropTop = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.SuperSource.Boxes[_id].CropTop = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBoxCropTop()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBoxCropYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    foreach (var box in GetSuperSourceBoxes(helper))
                    {
                        new SuperSourceBoxCropTopTestDefinition(helper, mode, box).Run();
                    }
                }
            }
        }

        private class SuperSourceBoxCropBottomTestDefinition : SuperSourceBoxCropYTestDefinition
        {
            public SuperSourceBoxCropBottomTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, mode, box)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.CropBottom,
                    Index = _id,
                    CropBottom = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.SuperSource.Boxes[_id].CropBottom = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBoxCropBottom()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBoxCropYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    foreach (var box in GetSuperSourceBoxes(helper))
                    {
                        new SuperSourceBoxCropBottomTestDefinition(helper, mode, box).Run();
                    }
                }
            }
        }

        private abstract class SuperSourceBoxCropXTestDefinition : SuperSourceBoxTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public SuperSourceBoxCropXTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, box)
            {
                _mode = mode;
            }

            public static IEnumerable<VideoMode> VideoModes()
            {
                yield return VideoMode.P1080i50;
                yield return VideoMode.N720p5994;
                yield return VideoMode.P625i50PAL;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetCropTop(5);
                _sdk.SetCropBottom(5);
                _helper.Sleep();
            }

            protected double ClampValueToRange(bool goodValue, double v)
            {
                if (goodValue)
                    return v;

                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return v >= 32 ? 32 : 0;
                    case VideoMode.P625i50PAL:
                        return v >= 8 ? 8 : 0;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] GoodValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { 0, 0.87, 32, 31.99, 0.01, 9.65 };
                    case VideoMode.P625i50PAL:
                        return new double[] { 0, 0.87, 8, 7.99, 0.01, 3.65 };
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] BadValues()
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return new double[] { -0.01, 32.01, 32.1, -0.1, -29 };
                    case VideoMode.P625i50PAL:
                        return new double[] { -0.01, 8.01, 8.1, -0.1, -15, 15 };
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private class SuperSourceBoxCropLeftTestDefinition : SuperSourceBoxCropXTestDefinition
        {
            public SuperSourceBoxCropLeftTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, mode, box)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.CropLeft,
                    Index = _id,
                    CropLeft = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.SuperSource.Boxes[_id].CropLeft = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBoxCropLeft()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBoxCropXTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    foreach (var box in GetSuperSourceBoxes(helper))
                    {
                        new SuperSourceBoxCropLeftTestDefinition(helper, mode, box).Run();
                    }
                }
            }
        }

        private class SuperSourceBoxCropRightTestDefinition : SuperSourceBoxCropXTestDefinition
        {
            public SuperSourceBoxCropRightTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox> box) : base(helper, mode, box)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourceBoxSetCommand
                {
                    Mask = SuperSourceBoxSetCommand.MaskFlags.CropRight,
                    Index = _id,
                    CropRight = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.SuperSource.Boxes[_id].CropRight = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBoxCropRight()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBoxCropXTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    foreach (var box in GetSuperSourceBoxes(helper))
                    {
                        new SuperSourceBoxCropRightTestDefinition(helper, mode, box).Run();
                    }
                }
            }
        }
        
    }
}