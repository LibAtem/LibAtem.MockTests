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
        private readonly List<byte[]> _commandQueue;

        public override ProtocolVersion? ConnectionVersion => _version;

        public AtemServerConnection(EndPoint endpoint, int sessionId, ProtocolVersion version) : base(endpoint, sessionId)
        {
            _version = version;
            _commandQueue = new List<byte[]>();
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

        private static OutboundMessage CompileQueuedUpdateMessage(List<byte[]> queuedCommands)
        {
            var builder = new OutboundMessageBuilder();

            int removeCount = 0;
            foreach (byte[] cmd in queuedCommands)
            {
                if (!builder.TryAddData(new List<byte[]> {cmd}))
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
            byte[] bytes = command.ToByteArray();
            lock (_commandQueue)
            {
                _commandQueue.Add(bytes);
            }
        }

        public void QueueCommands(IReadOnlyList<byte[]> bytes)
        {
            lock (_commandQueue)
            {
                _commandQueue.AddRange(bytes);
            }
        }
    }
}