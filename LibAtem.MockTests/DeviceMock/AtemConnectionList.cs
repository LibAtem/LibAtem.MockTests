using System.Collections.Generic;
using System.Linq;
using System.Net;
using LibAtem.Commands;
using LibAtem.Net;
using LibAtem.Util;
using log4net;

namespace LibAtem.MockTests.DeviceMock
{
    public class AtemConnectionList
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AtemConnectionList));

        private readonly Dictionary<EndPoint, AtemServerConnection> connections;

        public List<AtemServerConnection> OrderedConnections { get; }
        
        public AtemConnectionList()
        {
            connections = new Dictionary<EndPoint, AtemServerConnection>();
            OrderedConnections = new List<AtemServerConnection>();
        }

        public void SendDataDumps(IEnumerable<OutboundMessage> messages)
        {
            List<OutboundMessage> messages2 = messages.ToList();
            lock (connections)
            {
                // minor optimisation by skipping the libatem client
                connections.Skip(1).ForEach(conn => { messages2.ForEach(conn.Value.QueueMessage); });
            }
        }

        public AtemServerConnection FindOrCreateConnection(EndPoint ep, ProtocolVersion version, out bool isNew)
        {
            lock (connections)
            {
                AtemServerConnection val;
                if (connections.TryGetValue(ep, out val))
                {
                    isNew = false;
                    return val;
                }
                
                val = new AtemServerConnection(ep, 0x8008, version, OrderedConnections.Count);
                connections[ep] = val;
                OrderedConnections.Add(val);
                val.OnDisconnect += RemoveTimedOut;

                Log.InfoFormat("New connection from {0}", ep);

                isNew = true;
                return val;
            }
        }

        private void RemoveTimedOut(object sender)
        {
            var conn = sender as AtemServerConnection;
            if (conn == null)
                return;

            Log.InfoFormat("Lost connection to {0}", conn.Endpoint);

            lock (connections)
            {
                connections.Remove(conn.Endpoint);
            }
        }

        internal void QueuePings()
        {
            lock (connections)
            {
                var toRemove = new List<EndPoint>();
                foreach (KeyValuePair<EndPoint, AtemServerConnection> conn in connections)
                {
                    if (conn.Value.HasTimedOut)
                    {
                        toRemove.Add(conn.Key);
                        continue;
                    }

                    conn.Value.QueuePing();
                }

                foreach (var ep in toRemove)
                {
                    Log.InfoFormat("Lost connection to {0}", ep);
                    connections.Remove(ep);
                }
            }
        }

        public void SendCommands(IReadOnlyList<byte[]> bytes)
        {
            lock (connections)
            {
                foreach (KeyValuePair<EndPoint, AtemServerConnection> conn in connections)
                {
                    if (!conn.Value.HasTimedOut)
                    {
                        conn.Value.QueueCommands(bytes);
                    }
                }
            }
        }
    }
}