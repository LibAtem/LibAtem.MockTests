using System;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Macro;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Macro
{
    [Collection("ServerClientPool")]
    public class TestMacroPool
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestMacroPool(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestDelete()
        {
            var expectedCommand = new MacroActionCommand
            {
                Action = MacroActionCommand.MacroAction.Delete
            };
            var handler = CommandGenerator.MatchCommand(expectedCommand);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                AtemState stateBefore = helper.Helper.BuildLibState();

                for (int i = 0; i < 5; i++)
                {
                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    expectedCommand.Index = index;

                    uint timeBefore = helper.Server.CurrentTime;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        pool.Delete(index);
                    });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                }
            });
        }

        [Fact]
        public void TestIsUsed()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 10; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    MacroPropertiesGetCommand cmd = previousCommands.OfType<MacroPropertiesGetCommand>().Single(c => c.Index == index);
                    cmd.IsUsed = i % 2 == 0;

                    stateBefore.Macros.Pool[(int) index].IsUsed = cmd.IsUsed;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        helper.Server.SendCommands(cmd);
                    });
                }
            });
        }

        [Fact]
        public void TestHasUnsupportedOps()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.All, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                ImmutableList<ICommand> previousCommands = helper.Server.GetParsedDataDump();

                for (int i = 0; i < 10; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    MacroPropertiesGetCommand cmd = previousCommands.OfType<MacroPropertiesGetCommand>().Single(c => c.Index == index);
                    cmd.HasUnsupportedOps = i % 2 == 0;

                    stateBefore.Macros.Pool[(int)index].HasUnsupportedOps = cmd.HasUnsupportedOps;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        helper.Server.SendCommands(cmd);
                    });
                }
            });
        }

        [Fact]
        public void TestName()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MacroPropertiesSetCommand, MacroPropertiesGetCommand>("Name");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    string name = Guid.NewGuid().ToString().Substring(0, 20);

                    stateBefore.Macros.Pool[(int)index].Name = name;

                    helper.SendAndWaitForChange(stateBefore, () => { pool.SetName(index, name); });
                }
            });
        }

        [Fact]
        public void TestDescription()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MacroPropertiesSetCommand, MacroPropertiesGetCommand>("Description");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    string description = Guid.NewGuid().ToString();

                    stateBefore.Macros.Pool[(int)index].Description = description;

                    helper.SendAndWaitForChange(stateBefore, () => { pool.SetDescription(index, description); });
                }
            });
        }
    }
}