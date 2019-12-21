using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.MockTests.DeviceMock;
using LibAtem.State;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Util
{
    public sealed class AtemMockServerWrapper : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public AtemMockServer Server { get; }
        public AtemClientWrapper Clients { get; }
        public AtemTestHelper Helper { get; }

        public AtemMockServerWrapper(ITestOutputHelper output, Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> handler, string filename)
        {
            _output = output;

            var commandData = WiresharkParser.BuildCommands(ProtocolVersion.V8_0_1, filename);
            Server = new AtemMockServer(commandData);
            Clients = new AtemClientWrapper("127.0.0.1");
            Helper = new AtemTestHelper(Clients, _output);
            Server.HandleCommand = cmd => handler(Clients.LibAtemReceived, cmd);
            Clients.SdkState.Info.LastTimecode = Clients.LibState.Info.LastTimecode = new Timecode() // TODO - this might be doing nothign..
            {
                Second = Server.CurrentTime % 60,
                Minute = Server.CurrentTime / 60
            };
        }

        public void Dispose()
        {
            Helper.Dispose();
            Clients.Dispose();
            Server.Dispose();
        }

        public static void Each(ITestOutputHelper output, Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> handler, string[] filenames, Action<AtemMockServerWrapper> runner)
        {
            Assert.NotEmpty(filenames);
            filenames.ForEach(filename =>
            {
                using var helper = new AtemMockServerWrapper(output, handler, filename);
                runner(helper);
            });
        }

        public void SendAndWaitForChange(AtemState expected, Action doSend, int timeout = -1)
        {
            Helper.SendAndWaitForChange(doSend, timeout);
            if (expected != null)
            {
                Helper.AssertStateChanged(expected);
            }
        }

        public Dictionary<VideoSource, T> GetSdkInputsOfType<T>() where T : class
        {
            Guid itId = typeof(IBMDSwitcherInputIterator).GUID;
            Helper.SdkSwitcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherInputIterator iterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(itPtr);

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
            Guid itId = typeof(IBMDSwitcherInputIterator).GUID;
            Helper.SdkSwitcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherInputIterator iterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(itPtr);

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