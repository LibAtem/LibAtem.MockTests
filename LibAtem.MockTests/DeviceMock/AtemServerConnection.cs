using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LibAtem.Commands;
using LibAtem.Net;
using LibAtem.Util;

namespace LibAtem.MockTests.DeviceMock
{
    public class AtemServerConnection : AtemConnection
    {
        private readonly ProtocolVersion _version;
        private readonly List<ICommand> _commandQueue;

        public override ProtocolVersion? ConnectionVersion => _version;

        public AtemServerConnection(EndPoint endpoint, int sessionId, ProtocolVersion version) : base(endpoint, sessionId)
        {
            _version = version;
            _commandQueue = new List<ICommand>();
        }

        private bool _sentDataDump;

        public bool ReadyForData
        {
            get
            {
                if (_sentDataDump)
                    return false;

                if (!IsOpened)
                    return false;

                return _sentDataDump = true;
            }
        }

        private static OutboundMessage CompileQueuedUpdateMessage(List<ICommand> queuedCommands)
        {
            var builder = new OutboundMessageBuilder();

            int removeCount = 0;
            foreach (ICommand cmd in queuedCommands)
            {
                if (!builder.TryAddCommands(new List<ICommand> {cmd}))
                    break;

                removeCount++;
            }

            if (removeCount == 0)
            {
                throw new Exception("Failed to dequeue command");
            }

            queuedCommands.RemoveRange(0, removeCount);
            return builder.Create();
        }

        protected override OutboundMessage CompileNextMessage()
        {
            lock (_commandQueue)
            {
                if (_commandQueue.Any())
                    return CompileQueuedUpdateMessage(_commandQueue);
            }
            return null;
        }

        public override void QueueCommand(ICommand command)
        {
            lock (_commandQueue)
            {
                _commandQueue.Add(command);
            }
        }

        public void QueueCommands(IReadOnlyList<ICommand> commands)
        {
            lock (_commandQueue)
            {
                _commandQueue.AddRange(commands);
            }
        }
    }
}