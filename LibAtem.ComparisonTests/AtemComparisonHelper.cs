using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.Util;

namespace AtemEmulator.ComparisonTests
{
    public sealed class AtemComparisonHelper : IDisposable
    {
        public const int CommandWaitTime = 80;

        private readonly AtemClientWrapper _client;

        private readonly List<ICommand> _receivedCommands;

        private AutoResetEvent responseWait;
        private CommandQueueKey responseTarget;

        public AtemComparisonHelper(AtemClientWrapper client)
        {
            _client = client;
            _receivedCommands = new List<ICommand>();

            _client.Client.OnReceive += OnReceive;
        }

        private void OnReceive(object sender, IReadOnlyList<ICommand> commands)
        {
            lock (_receivedCommands)
            {
                _receivedCommands.AddRange(commands);
            }
        }

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
        }

        public void Sleep(int sleep = -1)
        {
            Thread.Sleep(sleep == -1 ? CommandWaitTime : sleep);
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
            responseTarget = key;

            void Handler (object sender, CommandQueueKey queueKey){
                if (queueKey.Equals(key))
                    responseWait.Set();
            };

            _client.OnCommandKey += Handler;

            SendCommand(toSend);

            // Wait for the expected time. If no response, then go with last data
            responseWait.WaitOne(timeout == -1 ? CommandWaitTime : timeout);

            responseWait = null;
            _client.OnCommandKey -= Handler;
        }
    }

}