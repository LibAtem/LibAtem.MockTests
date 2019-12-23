using System;
using System.Collections.Generic;
using System.Linq;
using LibAtem.Commands;
using LibAtem.Net;
using LibAtem.Util;
using PcapngFile;

namespace LibAtem.MockTests.Util
{
    internal static class WiresharkParser
    {
        public static IReadOnlyList<ICommand> ParseToCommands(ProtocolVersion version, IEnumerable<byte[]> payloads)
        {
            var result = new List<ICommand>();
            foreach (byte[] payload in payloads)
            {
                foreach (ParsedCommand rawCmd in ReceivedPacket.ParseCommands(payload))
                {
                    result.AddIfNotNull(CommandParser.Parse(version, rawCmd));
                }
            }
            return result;
        }

        public static List<byte[]> BuildCommands(ProtocolVersion version, string filename, Action<ParsedCommand, CommandBuilder> mutateCommand = null)
        {
            var commands = ParseCommands(version, $"TestFiles/Handshake/{filename}.pcapng");

            return commands.Select(pkt =>
            {
                return pkt.Commands.SelectMany(cmd =>
                {
                    var builder = new CommandBuilder(cmd.Name);
                    builder.AddByte(cmd.Body);

                    mutateCommand?.Invoke(cmd, builder);

                    return builder.ToByteArray();
                }).ToArray();
            }).ToList();
        }

        private static List<ReceivedPacket> ParseCommands(ProtocolVersion version, string filename)
        {
            var res = new List<ReceivedPacket>();

            using (var reader = new Reader(filename))
            {
                foreach (var readBlock in reader.EnhancedPacketBlocks)
                {
                    var pkt = ParseEnhancedBlock(version, readBlock as EnhancedPacketBlock);
                    if (pkt != null)
                    {
                        res.Add(pkt);
                        if (pkt.Commands.Any(cmd => cmd.Name == "InCm"))
                        {
                            // Init complete
                            break;
                        }
                    }
                }
            }

            return res;
        }

        private static ReceivedPacket ParseEnhancedBlock(ProtocolVersion version, EnhancedPacketBlock block)
        {
            byte[] data = block.Data;

            // Perform some basic checks, to ensure data looks like it could be ATEM
            if (data[23] != 17)
                throw new ArgumentOutOfRangeException("Found packet that appears to not be UDP");
            if ((data[36] << 8) + data[37] != 9910 && (data[34] << 8) + data[35] != 9910)
                throw new ArgumentOutOfRangeException("Found packet that has wrong UDP port");

            data = data.Skip(42).ToArray();
            var packet = new ReceivedPacket(data);
            if (!packet.CommandCode.HasFlag(ReceivedPacket.CommandCodeFlags.AckRequest))
                return null;

            return packet;
        }
    }
}
