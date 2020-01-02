using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LibAtem.Commands;
using LibAtem.MockTests.Util;
using LibAtem.Net;
using LibAtem.Util;
using log4net;
using ListExtensions = LibAtem.Util.ListExtensions;

namespace LibAtem.MockTests.DeviceMock
{
    public class AtemMockServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AtemMockServer));

        private readonly AtemConnectionList _connections;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<byte[]>> _handshakeStates;

        private readonly AutoResetEvent _receiveRunning;
        private bool _isDisposing = false;

        private Socket _socket;

        // TODO - remove this list, and replace with something more sensible...
        private readonly List<Timer> timers = new List<Timer>();

        public uint CurrentTime { get; private set; } = 100;

        public string CurrentCase { get; set; }
        public ProtocolVersion CurrentVersion { get; set; } = ProtocolVersion.Minimum;

        public List<ReceivedPacket> PendingPackets { get; } = new List<ReceivedPacket>();
        public AutoResetEvent HasPendingPackets{ get; } = new AutoResetEvent(false);

        public AtemMockServer(IReadOnlyDictionary<string, IReadOnlyList<byte[]>> handshakeStates)
        {
            _handshakeStates = handshakeStates;
            _connections = new AtemConnectionList();

            _receiveRunning = new AutoResetEvent(false);
            StartReceive();
            StartPingTimer();
        }

        public AtemMockServer(IReadOnlyList<byte[]> state) : this(
            new Dictionary<string, IReadOnlyList<byte[]>>(new[] {KeyValuePair.Create("default", state)}))
        {
            CurrentCase = "default";
        }

        public void SendCommands(params ICommand[] cmds)
        {
            var allCommands = cmds.ToList();
            allCommands.Add(CreateTimeCommand());
            _connections.SendCommands(allCommands);
        }

        public void Dispose()
        {
            // _isDisposing = true;
            // _client.Dispose();
            // TODO - reenable once LibAtem allows disconnection
            // Assert.True(_disposeEvent.WaitOne(TimeSpan.FromSeconds(1)), "LibAtem: Cleanup timed out");

            // Thread.Sleep(1000);

            _isDisposing = true;
            timers.ForEach(t => t.Dispose());
            _socket.Dispose();

            // Wait for the receive thread to stop
            _receiveRunning.WaitOne(5000);
        }

        private void StartPingTimer()
        {
            timers.Add(new Timer(o =>
            {
                _connections.QueuePings();
            }, null, 0, AtemConstants.PingInterval));
        }

        private static Socket CreateSocket()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, 9910);
            serverSocket.Bind(ipEndPoint);

            return serverSocket;
        }

        private TimeCodeCommand CreateTimeCommand()
        {
            uint time = CurrentTime++;

            var cmd = new TimeCodeCommand();
            cmd.Second += time % 60;
            cmd.Minute = time / 60;
            return cmd;
        }

        private void StartReceive()
        {
            _socket = CreateSocket();

            var thread = new Thread(async () =>
            {
                while (!_isDisposing)
                {
                    try
                    {
                        //Start receiving data
                        ArraySegment<byte> buff = new ArraySegment<byte>(new byte[2500]);
                        var end = new IPEndPoint(IPAddress.Any, 0);
                        SocketReceiveFromResult v = await _socket.ReceiveFromAsync(buff, SocketFlags.None, end);

                        AtemServerConnection conn = _connections.FindOrCreateConnection(v.RemoteEndPoint, CurrentVersion, out _);
                        if (conn == null)
                            continue;

                        byte[] buffer = buff.Array;
                        var packet = new ReceivedPacket(buffer);

                        if (packet.CommandCode.HasFlag(ReceivedPacket.CommandCodeFlags.Handshake))
                        {
                            conn.ResetConnStatsInfo();
                            // send handshake back
                            byte[] test =
                            {
                                buffer[0], buffer[1], // flags + length
                                buffer[2], buffer[3], // session id
                                0x00, 0x00, // acked pkt id
                                0x00, 0x00, // retransmit request
                                buffer[8], buffer[9], // unknown2
                                0x00, 0x00, // server pkt id
                                0x02, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00
                            };

                            var sendThread = new Thread(o =>
                            {
                                while (!conn.HasTimedOut)
                                {
                                    conn.TrySendQueued(_socket);
                                    Task.Delay(3).Wait();
                                }
                            });
                            sendThread.Start();

                            await _socket.SendToAsync(new ArraySegment<byte>(test, 0, 20), SocketFlags.None,
                                v.RemoteEndPoint);

                            continue;
                        }

                        if (!conn.IsOpened)
                        {
                            conn.OnReceivePacket += (sender, pkt) =>
                            {
                                lock (PendingPackets)
                                {
                                    // Queue the packets for parsing and processing in the main thread
                                    PendingPackets.Add(pkt);
                                    HasPendingPackets.Set();
                                }
                            };
                            var recvThread = new Thread(o =>
                            {
                                while (!conn.HasTimedOut || conn.HasCommandsToProcess)
                                {
                                    // We dont want this, but we dont want this to build up and be a memory leak
                                    conn.GetNextCommands();

                                    //conn.HandleInner(_state, connection, cmds);
                                }
                            });
                            recvThread.Start();
                        }

                        conn.Receive(_socket, packet);

                        if (conn.ReadyForData)
                            QueueDataDumps(conn);
                    }
                    catch (SocketException)
                    {
                        // Reinit the socket as it is now unavailable
                        //_socket = CreateSocket();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Normal part of shutdown
                        break;
                    }
                }

                // Notify finished
                _receiveRunning.Set();
            });
            thread.Start();
        }

        public void ResendDataDumps()
        {
            _connections.SendDataDumps(BuildDataDumps());
            CurrentTime = 99;
            SendCommands(); // Send a time
        }

        public ImmutableList<ICommand> GetParsedDataDump()
        {
            return WiresharkParser.ParseToCommands(CurrentVersion, _handshakeStates[CurrentCase]).ToImmutableList();
        }

        private IEnumerable<OutboundMessage> BuildDataDumps()
        {
            foreach (byte[] cmd in _handshakeStates[CurrentCase])
                yield return new OutboundMessage(OutboundMessage.OutboundMessageType.Command, cmd);
        }

        private void QueueDataDumps(AtemConnection conn)
        {
            BuildDataDumps().ForEach(conn.QueueMessage);
            Log.InfoFormat("Sent all data to {0}", conn.Endpoint);
        }
    }
}