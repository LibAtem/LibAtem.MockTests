using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.DeviceMock;
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

        public AtemMockServer Server { get; }
        public AtemClientWrapper Clients { get; }
        public AtemTestHelper Helper { get; }

        public AtemMockServerWrapper(ITestOutputHelper output, AtemServerClientPool pool, Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> handler, TestCaseId caseId)
        {
            _output = output;
            _pool = pool;

            Server = _pool.Server;
            Server.CurrentCase = caseId.Item2;
            Clients = _pool.GetOrCreateClients(caseId.Item2);


            //Clients = new AtemClientWrapper("127.0.0.1");
            Helper = new AtemTestHelper(Clients, _output);
            Server.HandleCommand = cmd => handler(Clients.LibAtemReceived, cmd);
            /*
            Clients.SdkState.Info.LastTimecode = Clients.LibState.Info.LastTimecode = new Timecode() // TODO - this might be doing nothign..
            {
                Second = Server.CurrentTime % 60,
                Minute = Server.CurrentTime / 60
            };*/
        }

        public void Dispose()
        {
            Helper.Dispose();
            //Clients.Dispose();
            //Server.Dispose();
            Server.CurrentCase = null;
        }

        public static void Each(ITestOutputHelper output, AtemServerClientPool pool, Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> handler, TestCaseId[] cases, Action<AtemMockServerWrapper> runner)
        {
            Assert.NotEmpty(cases);
            cases.ForEach(caseId =>
            {
                using var helper = new AtemMockServerWrapper(output, pool, handler, caseId);
                runner(helper);
            });
        }

        public void SendAndWaitForChange(AtemState expected, Action doSend, int timeout = -1)
        {
            Helper.SendAndWaitForChange(doSend, timeout);
            if (expected != null)
            {
                try
                {
                    Helper.AssertStateChanged(expected);
                }
                catch (Exception)
                {
                    // Try and sleep in case of a minor timing glitch
                    Thread.Sleep(200);
                    Helper.AssertStateChanged(expected);
                }
            }
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