using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.MockTests.DeviceMock;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Util
{
    using TestCaseId = Tuple<ProtocolVersion, string>;

    public sealed class AtemMockServerWrapper : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;
        private readonly Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> _handler;
        private readonly AtemMockServerPoolItem _case;

        public AtemMockServer Server => _case.Server;
        public AtemSdkClientWrapper SdkClient { get; }
        public AtemTestHelper Helper { get; }

        public AtemMockServerWrapper(ITestOutputHelper output, AtemServerClientPool pool, Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> handler, TestCaseId caseId)
        {
            _output = output;
            _pool = pool;
            _handler = handler;

            _case = _pool.GetCase(caseId.Item2);
            SdkClient = _case.SelectSdkClient();

            var resetEvent = new ManualResetEvent(false);
            void TmpHandler(object o) => resetEvent.Set();
            SdkClient.OnSdkStateChange += TmpHandler;
            Server.ResetClient(SdkClient.Id);
            resetEvent.WaitOne(2000); // TODO - monitor result
            SdkClient.OnSdkStateChange -= TmpHandler;

            Helper = new AtemTestHelper(SdkClient, _output, _case.LibAtemClient, _case.DeviceProfile, _pool.StateSettings);
        }

        public void Dispose()
        {
            Helper.Dispose();
            _case.ResetSdkClient(SdkClient);
            lock (Server.PendingPackets)
                Assert.Empty(Server.PendingPackets);
        }

        public static void Each(ITestOutputHelper output, AtemServerClientPool pool, Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> handler, TestCaseId[] cases, Action<AtemMockServerWrapper> runner)
        {
            cases = cases.Where(c => !string.IsNullOrEmpty(c.Item2)).ToArray();
            Assert.NotEmpty(cases);
            cases.ForEach(caseId =>
            {
                using var helper = new AtemMockServerWrapper(output, pool, handler, caseId);
                runner(helper);
            });
        }

        public void SendAndWaitForChange(AtemState expected, Action doSend, int timeout = -1, Action<AtemState, AtemState> mutateStates = null)
        {
            SendAndWaitForChangeInner(doSend);
            if (expected != null)
            {
                Helper.CheckStateChanges(expected, mutateStates);
                /*
                try
                {
                    // TODO - this doesnt throw, it marks as failed, so this is a very broken exception catcher...
                    Helper.CheckStateChanges(expected);
                }
                catch (Exception)
                {
                    // Try and sleep in case of a minor timing glitch
                    Thread.Sleep(200);
                    Helper.CheckStateChanges(expected);
                }
                */
            }
        }

        public const int CommandWaitTime = 80;
        private void SendAndWaitForChangeInner(Action doSend)
        {
            var libWait = new ManualResetEvent(false);
            var sdkWait = new ManualResetEvent(false);

            void HandlerLib(object sender, string queueKey)
            {
                // Helper.Output.WriteLine("SendAndWaitForMatching: Got Lib change: " + queueKey);
                if (queueKey == "Info.LastTimecode")
                    libWait.Set();
            }
            void HandlerSdk(object sender)
            {
                // Helper.Output.WriteLine("SendAndWaitForMatching: Got Sdk change");
                sdkWait.Set();
            }

            Helper.OnLibAtemStateChange += HandlerLib;
            Helper.SdkClient.OnSdkStateChange += HandlerSdk;

            lock (Server.PendingPackets)
            {
                if (Server.PendingPackets.Count > 0)
                    Helper.Output.WriteLine("SendAndWaitForMatching: Server had pending packets");
                Server.HasPendingPackets.Reset();

                doSend();
            }

            if (_handler != null)
            {
                Assert.True(Server.HasPendingPackets.WaitOne(1000));
                // if (!ok) Helper.Output.WriteLine("SendAndWaitForMatching: Server did not receive packet");

                lock (Server.PendingPackets)
                {
                    try
                    {
                        foreach (var pkt in Server.PendingPackets)
                        {
                            ImmutableList<ICommand> rawCommands = Server.GetParsedDataDump();
                            foreach (ParsedCommand rawCmd in pkt.Commands)
                            {
                                ICommand cmd = CommandParser.Parse(Server.CurrentVersion, rawCmd);
                                if (cmd == null)
                                    throw new Exception($"Unknown command \"{rawCmd.Name}\" in server");

                                List<ICommand> response = _handler(rawCommands, cmd).ToList();
                                if (response.Count == 0)
                                    throw new Exception($"Unhandled command \"{cmd.GetType().Name}\" in server");

                                Server.SendCommands(ListExtensions.WhereNotNull(response).ToArray());
                            }
                        }
                    }
                    finally
                    {
                        Server.PendingPackets.Clear();
                    }
                }
            }

            // Wait for the expected time. If no response, then go with last data
            bool libTimedOut = libWait.WaitOne(1000);
            // The Sdk doesn't send the same notifies if nothing changed, so once the lib has finished, wait a small time for sdk to finish up
            bool sdkTimedOut = sdkWait.WaitOne(500);

            Helper.OnLibAtemStateChange -= HandlerLib;
            Helper.SdkClient.OnSdkStateChange -= HandlerSdk;

            if (!libTimedOut)
                Helper.Output.WriteLine("SendAndWaitForMatching: Missed Lib change");
            if (!sdkTimedOut)
                Helper.Output.WriteLine("SendAndWaitForMatching: Missed Sdk change");

            Helper.Output.WriteLine("");
        }

        public Dictionary<VideoSource, T> GetSdkInputsOfType<T>() where T : class
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(Helper.SdkSwitcher.CreateIterator);

            Dictionary<VideoSource, T> inputs = new Dictionary<VideoSource, T>();
            for (iterator.Next(out IBMDSwitcherInput input); input != null; iterator.Next(out input))
            {
                var colGen = input as T;
                if (colGen == null)
                    continue;

                input.GetInputId(out long id);
                inputs[(VideoSource)id] = colGen;
            }

            return inputs;
        }

        public IBMDSwitcherInput GetSdkInput(long targetId)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(Helper.SdkSwitcher.CreateIterator);

            for (iterator.Next(out IBMDSwitcherInput input); input != null; iterator.Next(out input))
            {
                input.GetInputId(out long id);
                if (targetId == id)
                    return input;
            }

            return null;
        }

    }
}