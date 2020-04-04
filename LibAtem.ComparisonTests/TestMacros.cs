using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Macro;
using LibAtem.Common;
using LibAtem.ComparisonTests.MixEffects;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
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

namespace LibAtem.ComparisonTests
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