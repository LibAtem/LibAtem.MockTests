using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Util;

namespace AtemEmulator.ComparisonTests
{
    public sealed class AtemComparisonHelper : IDisposable
    {
        public const int CommandWaitTime = 50;

        private readonly AtemClientWrapper _client;

        private readonly List<ICommand> _receivedCommands;

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
    }

}