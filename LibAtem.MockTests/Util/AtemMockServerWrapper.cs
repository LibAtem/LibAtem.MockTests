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
        public AtemSdkClientWrapper Clients { get; }
        public AtemTestHelper Helper { get; }

        public AtemMockServerWrapper(ITestOutputHelper output, AtemServerClientPool pool, Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> handler, TestCaseId caseId)
        {
            _output = output;
            _pool = pool;

            Server = _pool.Server;
            Server.CurrentCase = caseId.Item2;
            Server.CurrentVersion = caseId.Item1; // TODO - we need to get the server to send this as a command, to make libatem happy (does that break sdk?)
            Clients = new AtemSdkClientWrapper("127.0.0.1", _pool.StateSettings);

            var profile = _pool.GetDeviceProfile(caseId.Item2);
            var startupState = _pool.GetDefaultState(caseId.Item2);
            Helper = new AtemTestHelper(Clients, _output, _pool.LibAtemClient, profile, startupState, _pool.StateSettings);
            Server.HandleCommand = cmd => handler(Server.GetParsedDataDump(), cmd);
        }

        public void Dispose()
        {
            Helper.Dispose();
            Clients.Dispose();
            Server.CurrentCase = null;
            Server.CurrentVersion = ProtocolVersion.Minimum;
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
                    // TODO - this doesnt throw, it marks as failed, so this is a very broken exception catcher...
                    Helper.CheckStateChanges(expected);
                }
                catch (Exception)
                {
                    // Try and sleep in case of a minor timing glitch
                    Thread.Sleep(200);
                    Helper.CheckStateChanges(expected);
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