using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.State;
using LibAtem.State.Builder;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    public sealed class AtemComparisonHelper : IDisposable
    {
        public const int CommandWaitTime = 80;

        private readonly AtemClientWrapper _client;
        public AtemClientWrapper Client => _client;
        public AtemStateBuilderSettings StateSettings => _client.StateSettings;

        private readonly List<ICommand> _receivedCommands;

        private AutoResetEvent responseWait;

        public bool TestResult { get; set; } = true;

        public AtemComparisonHelper(AtemClientWrapper client, ITestOutputHelper output)
        {
            _client = client;
            Output = output;
            _receivedCommands = new List<ICommand>();

            _client.Client.OnReceive += OnReceive;

            AssertStatesMatch();
        }

        public void AssertStatesMatch()
        {
            List<string> before = AtemStateComparer.AreEqual(SdkState, LibState);
            if (before.Count != 0 && Output != null)
            {
                Output.WriteLine("state mismatch:");
                before.ForEach(Output.WriteLine);
            }
            Assert.Empty(before);
        }

        private void OnReceive(object sender, IReadOnlyList<ICommand> commands)
        {
            lock (_receivedCommands)
            {
                _receivedCommands.AddRange(commands);
            }
        }

        public ITestOutputHelper Output { get; }

        public AtemState SdkState => _client.SdkState;
        public AtemState LibState => _client.LibState;

        public IBMDSwitcher SdkSwitcher => _client.SdkSwitcher;

        public LibAtem.DeviceProfile.DeviceProfile Profile => _client.Profile;

        public T FindWithMatching<T>(T srcId) where T : ICommand
        {
            return _client.FindWithMatching(srcId);
        }

        public List<T> FindAllOfType<T>() where T : ICommand
        {
            return _client.FindAllOfType<T>();
        }

        public void ClearReceivedCommands()
        {
            lock (_receivedCommands)
                _receivedCommands.Clear();
        }

        public List<T> GetReceivedCommands<T>() where T : ICommand
        {
            lock (_receivedCommands)
                return _receivedCommands.OfType<T>().ToList();
        }

        public T GetSingleReceivedCommands<T>() where T : ICommand
        {
            lock (_receivedCommands)
                return _receivedCommands.OfType<T>().Single();
        }

        public int CountAndClearReceivedCommands<T>() where T : ICommand
        {
            lock (_receivedCommands)
            {
                int count = _receivedCommands.OfType<T>().Count();
                _receivedCommands.Clear();
                return count;
            }
        }

        public void SendCommand(params ICommand[] commands)
        {
            commands.ForEach(c => _client.Client.SendCommand(c));
        }

        public void Dispose()
        {
            _client.Client.OnReceive -= OnReceive;

            Assert.True(TestResult);
        }

        public void Sleep(int sleep = -1)
        {
            Thread.Sleep(sleep == -1 ? CommandWaitTime : sleep);
        }

        public void EnsureVideoMode(VideoMode mode)
        {
            // TODO - dont do if already on this mode, as it clears some data that would be good to keep
            SdkSwitcher.SetVideoMode(AtemEnumMaps.VideoModesMap[mode]);
            Sleep();
        }

        public Dictionary<VideoSource, T> GetSdkInputsOfType<T>() where T : class
        {
            Guid itId = typeof(IBMDSwitcherInputIterator).GUID;
            SdkSwitcher.CreateIterator(ref itId, out var itPtr);
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
        
        // Note: This doesnt quite work properly yet
        public void SendAndWaitForMatching(CommandQueueKey key, ICommand toSend, int timeout = -1)
        {
            if (responseWait != null)
                return;

            responseWait = new AutoResetEvent(false);

            void Handler (object sender, CommandQueueKey queueKey){
                if (queueKey.Equals(key))
                    responseWait.Set();
            }

            _client.OnCommandKey += Handler;

            if (toSend != null)
                SendCommand(toSend);

            // Wait for the expected time. If no response, then go with last data
            responseWait.WaitOne(timeout == -1 ? CommandWaitTime : timeout);

            responseWait = null;
            _client.OnCommandKey -= Handler;
        }

        public void SendAndWaitForMatching(List<CommandQueueKey> expected, ICommand toSend, int timeout = -1)
        {
            if (responseWait != null)
                throw new Exception("a SendAndWaitForMatching is already running");

            if (expected.Count == 0)
            {
                if (toSend != null)
                    SendCommand(toSend);

                Sleep(timeout);
                return;
            }

            var libWait = new ManualResetEvent(false);
            var sdkWait = new ManualResetEvent(false);

            var pendingLib = expected.ToList();
            var pendingSdk = expected.ToList();

            void HandlerLib(object sender, CommandQueueKey queueKey)
            {
                Output.WriteLine("SendAndWaitForMatching: Got Lib change: " + queueKey);

                lock (pendingLib)
                {
                    pendingLib.Remove(queueKey);
                    if (pendingLib.Count == 0)
                        libWait.Set();
                }
            }
            void HandlerSdk(object sender, CommandQueueKey queueKey)
            {
                Output.WriteLine("SendAndWaitForMatching: Got Sdk change: " + queueKey);

                lock (pendingSdk)
                {
                    pendingSdk.Remove(queueKey);
                    if (pendingSdk.Count == 0)
                        sdkWait.Set();
                }
            }

            _client.OnCommandKey += HandlerLib;
            _client.OnSdkStateChange += HandlerSdk;

            if (toSend != null)
                SendCommand(toSend);

            // Wait for the expected time. If no response, then go with last data
            libWait.WaitOne(timeout == -1 ? CommandWaitTime * 3 : timeout);
            // The Sdk doesn't send the same notifies if nothing changed, so once the lib has finished, wait a small time for sdk to finish up
            sdkWait.WaitOne(timeout == -1 ? CommandWaitTime / 2 : timeout);

            _client.OnCommandKey -= HandlerLib;
            _client.OnSdkStateChange -= HandlerSdk;

            if (pendingLib.Count > 0)
                Output.WriteLine("SendAndWaitForMatching: Pending Lib changes: " + string.Join(", ", pendingLib));

            if (pendingSdk.Count > 0)
                Output.WriteLine("SendAndWaitForMatching: Pending Sdk changes: " + string.Join(", ", pendingSdk));

            Output.WriteLine("");
        }
    }

}