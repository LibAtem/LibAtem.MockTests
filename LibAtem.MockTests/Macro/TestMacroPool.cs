using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Commands.Macro;
using LibAtem.Common;
using LibAtem.MacroOperations;
using LibAtem.MacroOperations.MixEffects;
using LibAtem.MockTests.Media;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Macro
{
    [Collection("ServerClientPool")]
    public class TestMacroTransfer
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestMacroTransfer(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }


        class TransferCallback : IBMDSwitcherMacroPoolCallback
        {
            public ManualResetEvent Wait { get; }
            public _BMDSwitcherMacroPoolEventType Result { get; private set; }

            public TransferCallback()
            {
                Wait = new ManualResetEvent(false);
            }
            
            public void Notify(_BMDSwitcherMacroPoolEventType eventType, uint index, IBMDSwitcherTransferMacro macroTransfer)
            {
                switch(eventType)
                {
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeValidChanged:
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeHasUnsupportedOpsChanged:
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeNameChanged:
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeDescriptionChanged:
                        break;
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCompleted:
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCancelled:
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferFailed:
                        Result = eventType;
                        Wait.Set();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        }

        [Fact]
        public void TestUpload()
        {
            UploadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MacroTransfer, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    string name = Guid.NewGuid().ToString();

                    byte[] op = new CutTransitionMacroOp {Index = MixEffectBlockId.One}.ToByteArray();
                    byte[] fakeOp = {0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
                    byte[] combined = op.Concat(fakeOp).ToArray();
                    worker = new UploadJobWorker((uint) combined.Length, _output, 0xffff, index,
                        DataTransferUploadRequestCommand.TransferMode.Write2 |
                        DataTransferUploadRequestCommand.TransferMode.Clear2);

                    // var cb = new TestMediaPoolStills.LockCallback();
                    // helper.SendAndWaitForChange(stateBefore, () => { .Lock(cb); });
                    // Assert.True(cb.Wait.WaitOne(2000));

                    pool.CreateMacro((uint) combined.Length, out IBMDSwitcherMacro macro);
                    macro.GetBytes(out IntPtr buffer);
                    Marshal.Copy(combined, 0, buffer, combined.Length);

                    var uploadCb = new TransferCallback();
                    pool.AddCallback(uploadCb);
                    pool.Upload(index, name, "not now", macro, out IBMDSwitcherTransferMacro transfer);

                    MacroState.ItemState macroState = stateBefore.Macros.Pool[(int) index];
                    macroState.Name = name;
                    macroState.Description = "not now";
                    macroState.HasUnsupportedOps = true;
                    macroState.IsUsed = true;

                    helper.HandleUntil(uploadCb.Wait, 1000);
                    Assert.True(uploadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCompleted,
                        uploadCb.Result);
                    Assert.Equal(BitConverter.ToString(combined), BitConverter.ToString(worker.Buffer.ToArray()));

                    helper.Helper.CheckStateChanges(stateBefore);
                }
            });
        }

        [Fact]
        public void TestAbortUpload()
        {
            AbortedUploadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MacroTransfer, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                for (int i = 0; i < 3; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);

                    byte[] op = new CutTransitionMacroOp { Index = MixEffectBlockId.One }.ToByteArray();
                    byte[] fakeOp = { 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    byte[] combined = op.Concat(fakeOp).ToArray();
                    worker = new AbortedUploadJobWorker(_output);

                    pool.CreateMacro((uint)combined.Length, out IBMDSwitcherMacro macro);
                    macro.GetBytes(out IntPtr buffer);
                    Marshal.Copy(combined, 0, buffer, combined.Length);

                    var uploadCb = new TransferCallback();
                    pool.AddCallback(uploadCb);
                    pool.Upload(index, "some thing", "not now", macro, out IBMDSwitcherTransferMacro transfer);

                    // Short bit of work before the abort
                    helper.HandleUntil(uploadCb.Wait, 1000);
                    transfer.Cancel();
                    
                    helper.HandleUntil(uploadCb.Wait, 1000);
                    Assert.True(uploadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCancelled,
                        uploadCb.Result);

                    helper.Helper.CheckStateChanges(stateBefore);
                }
            });
        }

        [Fact]
        public void TestDownload()
        {
            DownloadJobWorker worker = null;
            AtemMockServerWrapper.Each(_output, _pool, (a, b) => worker?.HandleCommand(a, b), DeviceTestCases.MacroTransfer, helper =>
            {
                var pool = helper.SdkClient.SdkSwitcher as IBMDSwitcherMacroPool;
                Assert.NotNull(pool);

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();

                    uint index = Randomiser.RangeInt((uint)stateBefore.Macros.Pool.Count);
                    string name = Guid.NewGuid().ToString();

                    byte[] op = new CutTransitionMacroOp { Index = MixEffectBlockId.One }.ToByteArray();
                    byte[] fakeOp = { 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    byte[] combined = op.Concat(fakeOp).ToArray();
                    worker = new DownloadJobWorker(_output, null, 0xffff, index, combined);

                    // var cb = new TestMediaPoolStills.LockCallback();
                    // helper.SendAndWaitForChange(stateBefore, () => { .Lock(cb); });
                    // Assert.True(cb.Wait.WaitOne(2000));

                    var downloadCb = new TransferCallback();
                    pool.AddCallback(downloadCb);
                    pool.Download(index, out IBMDSwitcherTransferMacro transfer);
                    
                    helper.HandleUntil(downloadCb.Wait, 1000);
                    Assert.True(downloadCb.Wait.WaitOne(500));
                    Assert.Equal(_BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCompleted,
                        downloadCb.Result);

                    transfer.GetMacro(out IBMDSwitcherMacro macro);
                    macro.GetBytes(out IntPtr buffer);
                    byte[] bytes = new byte[macro.GetSize()];
                    Marshal.Copy(buffer, bytes, 0, bytes.Length);
                    Assert.Equal(BitConverter.ToString(combined), BitConverter.ToString(bytes));

                    helper.Helper.CheckStateChanges(stateBefore);
                }
            });
        }
    }

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
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MacroTransfer, helper =>
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
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MacroTransfer, helper =>
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
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.MacroTransfer, helper =>
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
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MacroTransfer, helper =>
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
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.MacroTransfer, helper =>
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