using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Macro;
using LibAtem.Common;
using LibAtem.ComparisonTests2.MixEffects;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using LibAtem.MacroOperations;
using LibAtem.Net.DataTransfer;
using LibAtem.State;
using LibAtem.Test.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2
{
    [Collection("Client")]
    public class TestMacros : MixEffectsTestBase
    {
        public TestMacros(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private IBMDSwitcherMacroPool GetMacroPool()
        {
            var pool = Client.SdkSwitcher as IBMDSwitcherMacroPool;
            Assert.NotNull(pool);
            return pool;
        }

        private IBMDSwitcherMacroControl GetMacroControl()
        {
            var ctrl = Client.SdkSwitcher as IBMDSwitcherMacroControl;
            Assert.NotNull(ctrl);
            return ctrl;
        }

        private void CreateDummyMacro(uint index)
        {
            var me = GetMixEffect<IBMDSwitcherMixEffectBlock>();
            Assert.NotNull(me);

            IBMDSwitcherMacroControl ctrl = GetMacroControl();
            using (new StopMacroRecord(ctrl)) // Hopefully this will stop recording if it exceptions
            {
                ctrl.Record(index, "dummy", "");
                me.SetProgramInput((long)VideoSource.Input1);
            }
        }

        private sealed class StopMacroRecord : IDisposable
        {
            private readonly IBMDSwitcherMacroControl _ctrl;

            public StopMacroRecord(IBMDSwitcherMacroControl ctrl)
            {
                _ctrl = ctrl;
            }

            public void Dispose()
            {
                _ctrl.StopRecording();
            }
        }

        [Fact]
        public void TestCount()
        {
            GetMacroPool().GetMaxCount(out uint maxCount);

            Assert.Equal(maxCount, Client.Profile.MacroCount);
        }


        private class MacroLoopTestDefinition : TestDefinitionBase<MacroRunStatusSetCommand, bool>
        {
            private readonly IBMDSwitcherMacroControl _sdk;

            public MacroLoopTestDefinition(AtemComparisonHelper helper, IBMDSwitcherMacroControl sdk) : base(helper)
            {
                _sdk = sdk;
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLoop(0);

            public override string PropertyName => "Loop";
            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v) => state.Macros.RunStatus.Loop = v;

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, bool v)
            {
                yield return new CommandQueueKey(new MacroRunStatusGetCommand());
            }
        }

        [Fact]
        public void TestLoop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                new MacroLoopTestDefinition(helper, GetMacroControl()).Run();
            }
        }

        [Fact]
        public void TestDelete()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                // Ensure there are some macros defined
                CreateDummyMacro(1);
                CreateDummyMacro(5);
                CreateDummyMacro(8);
                helper.Sleep();

                // Ensure these exist
                var pool = helper.LibState.Macros.Pool;
                Assert.True(pool[1].IsUsed);
                Assert.True(pool[5].IsUsed);
                Assert.True(pool[8].IsUsed);
                helper.AssertStatesMatch();

                // Delete one
                helper.SendCommand(new MacroActionCommand()
                {
                    Index = 5,
                    Action = MacroActionCommand.MacroAction.Delete
                });
                helper.Sleep();
                pool = helper.LibState.Macros.Pool;
                Assert.True(pool[1].IsUsed);
                Assert.False(pool[5].IsUsed);
                Assert.True(pool[8].IsUsed);
                helper.AssertStatesMatch();
            }
        }

        private sealed class DownloadMacroCallback : IBMDSwitcherMacroPoolCallback
        {
            private readonly AutoResetEvent _evt;

            public DownloadMacroCallback(AutoResetEvent evt)
            {
                _evt = evt;
            }

            public void Notify(_BMDSwitcherMacroPoolEventType eventType, uint index, IBMDSwitcherTransferMacro macroTransfer)
            {
                switch (eventType)
                {
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCompleted:
                        _evt.Set();
                        break;
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCancelled:
                        break;
                    case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferFailed:
                        break;
                }
            }
        }

        private byte[] DownloadMacro(uint index)
        {
            var pool = GetMacroPool();
            var evt = new AutoResetEvent(false);

            var cb = new DownloadMacroCallback(evt);
            pool.AddCallback(cb);

            pool.Download(index, out IBMDSwitcherTransferMacro transfer);
            Assert.True(evt.WaitOne(2000));

            transfer.GetMacro(out IBMDSwitcherMacro macro);
            macro.GetBytes(out IntPtr buffer);
            
            byte[] resBytes = new byte[macro.GetSize()];
            Marshal.Copy(buffer, resBytes, 0, resBytes.Length);
            return resBytes;
        }

        private static string GetHash(byte[] data)
        {
            using (MD5 md5Hash = MD5.Create())
                return BitConverter.ToString(md5Hash.ComputeHash(data));
        }

        [Fact]
        public void TestRecord()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            using (new StopMacroRecord(GetMacroControl()))
            {
                CreateDummyMacro(1);

                var me = GetMixEffect<IBMDSwitcherMixEffectBlock>();
                Assert.NotNull(me);

                helper.Sleep();
                helper.SendCommand(new MacroRecordCommand()
                {
                    Index = 2,
                    Name = "lib-dummy",
                    Description = "d2"
                });
                helper.Sleep();
                Assert.True(helper.LibState.Macros.RecordStatus.IsRecording);
                Assert.Equal((uint)2, helper.LibState.Macros.RecordStatus.RecordIndex);
                helper.AssertStatesMatch();

                me.SetProgramInput((long)VideoSource.Input1);

                helper.SendCommand(new MacroActionCommand()
                {
                    Action = MacroActionCommand.MacroAction.StopRecord
                });
                helper.Sleep();
                Assert.False(helper.LibState.Macros.RecordStatus.IsRecording);
                helper.AssertStatesMatch();

                // Verify macro
                var mac = helper.LibState.Macros.Pool[2];
                Assert.True(mac.IsUsed);
                Assert.Equal("lib-dummy", mac.Name);
                Assert.Equal("d2", mac.Description);

                helper.Sleep();

                // Verify macros 
                var pool = GetMacroPool();
                var hash1 = GetHash(DownloadMacro(1));
                var hash2 = GetHash(DownloadMacro(2));
                Assert.Equal(hash1, hash2);
            }
        }

        [Fact]
        public void TestRecordSleeps()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            using (new StopMacroRecord(GetMacroControl()))
            {
                CreateSleepingMacro(1);

                var me = GetMixEffect<IBMDSwitcherMixEffectBlock>();
                Assert.NotNull(me);

                helper.Sleep();
                helper.SendCommand(new MacroRecordCommand()
                {
                    Index = 2,
                    Name = "lib-dummy-sleep",
                    Description = "d2"
                });
                helper.Sleep();
                Assert.True(helper.LibState.Macros.RecordStatus.IsRecording);
                Assert.Equal((uint)2, helper.LibState.Macros.RecordStatus.RecordIndex);
                helper.AssertStatesMatch();

                helper.SendCommand(new MacroAddTimedPauseCommand()
                {
                    Frames = 50
                });
                helper.SendCommand(new MacroActionCommand()
                {
                    Action = MacroActionCommand.MacroAction.InsertUserWait
                });
                helper.SendCommand(new MacroAddTimedPauseCommand()
                {
                    Frames = 25
                });

                helper.SendCommand(new MacroActionCommand()
                {
                    Action = MacroActionCommand.MacroAction.StopRecord
                });
                helper.Sleep();
                Assert.False(helper.LibState.Macros.RecordStatus.IsRecording);
                helper.AssertStatesMatch();

                // Verify macro
                var mac = helper.LibState.Macros.Pool[2];
                Assert.True(mac.IsUsed);

                // Verify macros 
                var pool = GetMacroPool();
                var hash1 = GetHash(DownloadMacro(1));
                var hash2 = GetHash(DownloadMacro(2));
                Assert.Equal(hash1, hash2);
            }
        }

        [Fact]
        public void TestDownload()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                CreateDummyMacro(1);
                helper.Sleep();

                string sdkHash = GetHash(DownloadMacro(1));
                helper.Sleep();

                string libHash = null;

                var evt = new AutoResetEvent(false);
                Client.Client.DataTransfer.QueueJob(new DownloadMacroBytesJob(1, (res) =>
                {
                    libHash = GetHash(res.SelectMany(b => b).ToArray());
                    evt.Set();
                }, TimeSpan.FromSeconds(2)));

                Assert.True(evt.WaitOne(3000), "Download Failed");

                Assert.Equal(sdkHash, libHash);
            }
        }

        private void CreateSleepingMacro(uint index)
        {
            var me = GetMixEffect<IBMDSwitcherMixEffectBlock>();
            Assert.NotNull(me);

            IBMDSwitcherMacroControl ctrl = GetMacroControl();
            using (new StopMacroRecord(ctrl)) // Hopefully this will stop recording if it exceptions
            {
                ctrl.Record(index, "dummy-sleep", "");
                ctrl.RecordPause(50); // 2s
                ctrl.RecordUserWait();
                ctrl.RecordPause(25); // 1s
            }
        }

        private sealed class StopMacroRun : IDisposable
        {
            private readonly IBMDSwitcherMacroControl _ctrl;

            public StopMacroRun(IBMDSwitcherMacroControl ctrl)
            {
                _ctrl = ctrl;
            }

            public void Dispose()
            {
                _ctrl.StopRunning();
            }
        }

        [Fact]
        public void TestRunIncludingSleeps()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            using (new StopMacroRun(GetMacroControl()))
            {
                CreateSleepingMacro(1);
                helper.Sleep();
                Assert.True(helper.LibState.Macros.Pool[1].IsUsed);
                Assert.Equal(MacroState.MacroRunStatus.Idle, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();

                helper.SendCommand(new MacroActionCommand()
                {
                    Action = MacroActionCommand.MacroAction.Run,
                    Index = 1,
                });
                helper.Sleep();
                Assert.Equal((uint)1, helper.LibState.Macros.RunStatus.RunIndex);
                Assert.Equal(MacroState.MacroRunStatus.Running, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();

                // Sleep until we should be in user wait
                helper.Sleep(2200);
                Assert.Equal((uint)1, helper.LibState.Macros.RunStatus.RunIndex);
                Assert.Equal(MacroState.MacroRunStatus.UserWait, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();

                // Fire the user continue op
                helper.SendCommand(new MacroActionCommand()
                {
                    Action = MacroActionCommand.MacroAction.Continue,
                });
                helper.Sleep();
                Assert.Equal((uint)1, helper.LibState.Macros.RunStatus.RunIndex);
                Assert.Equal(MacroState.MacroRunStatus.Running, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();

                // wait until the end
                helper.Sleep(1200);
                Assert.Equal(MacroState.MacroRunStatus.Idle, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();
            }
        }

        [Fact]
        public void TestStopRunning()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            using (new StopMacroRun(GetMacroControl()))
            {
                CreateSleepingMacro(1);
                helper.Sleep();
                Assert.True(helper.LibState.Macros.Pool[1].IsUsed);
                Assert.Equal(MacroState.MacroRunStatus.Idle, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();

                helper.SendCommand(new MacroActionCommand()
                {
                    Action = MacroActionCommand.MacroAction.Run,
                    Index = 1,
                });
                helper.Sleep();
                Assert.Equal((uint)1, helper.LibState.Macros.RunStatus.RunIndex);
                Assert.Equal(MacroState.MacroRunStatus.Running, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();

                helper.SendCommand(new MacroActionCommand()
                {
                    Action = MacroActionCommand.MacroAction.Stop
                });
                helper.Sleep();
                Assert.Equal(MacroState.MacroRunStatus.Idle, helper.LibState.Macros.RunStatus.RunStatus);
                helper.AssertStatesMatch();
            }
        }

        [Fact]
        public void AutoTestMacroOps()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                IBMDSwitcherMacroControl ctrl = GetMacroControl();

                var failures = new List<string>();

                Assembly assembly = typeof(ICommand).GetTypeInfo().Assembly;
                IEnumerable<Type> types = assembly.GetTypes().Where(t => typeof(SerializableCommandBase).GetTypeInfo().IsAssignableFrom(t));
                foreach (Type type in types)
                {
                    if (type == typeof(SerializableCommandBase))
                        continue;
/*
                    if (type != typeof(AuxSourceSetCommand))
                        continue;*/

                    try
                    {
                        Output.WriteLine("Testing: {0}", type.Name);
                        for (int i = 0; i < 10; i++)
                        {
                            SerializableCommandBase raw = (SerializableCommandBase)RandomPropertyGenerator.Create(type, (o) => AvailabilityChecker.IsAvailable(helper.Profile, o)); // TODO - wants to be ICommand
                            IEnumerable<MacroOpBase> expectedOps = raw.ToMacroOps(ProtocolVersion.Latest);
                            if (expectedOps == null)
                            {
                                Output.WriteLine("Skipping");
                                break;
                            } 

                            using (new StopMacroRecord(ctrl)) // Hopefully this will stop recording if it exceptions
                            {
                                ctrl.Record(0, string.Format("record-{0}-{1}", type.Name, i), "");
                                helper.SendCommand(raw);
                                helper.Sleep(20);
                            }

                            helper.Sleep(40);
                            byte[] r = DownloadMacro(0);
                            if (r.Length == 0) throw new Exception("Macro has no operations");

                            MacroOpBase decoded = MacroOpManager.CreateFromData(r, false); // This is assuming that there is a single macro op
                            RandomPropertyGenerator.AssertAreTheSame(expectedOps.Single(), decoded);

                        }
                    }
                    catch (Exception e)
                    {
                        var msg = string.Format("{0}: {1}", type.Name, e.Message);
                        Output.WriteLine(msg);
                        failures.Add(msg);
                    }
                }

                Assert.Empty(failures);
            }
        }
    }
}