using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibAtem.Commands;
using LibAtem.Net;
using LibAtem.Util;

namespace LibAtem.MockTests.Util
{
    internal static class DumpParser
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
            var commands = ParseCommands(version, $"TestFiles/Handshake/{filename}.data");

            return commands.Select(pkt =>
            {
                return pkt.SelectMany(cmd =>
                {
                    var builder = new CommandBuilder(cmd.Name);
                    builder.AddByte(cmd.Body);

                    mutateCommand?.Invoke(cmd, builder);

                    return builder.ToByteArray();
                }).ToArray();
            }).ToList();
        }
        
        private static List<List<ParsedCommand>> ParseCommands(ProtocolVersion version, string filename)
        {
            var res = new List<List<ParsedCommand>>();

            using (var reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var commands = ReceivedPacket.ParseCommands(line.HexToByteArray());
                    res.Add(commands.ToList());
                }
            }

            return res;
        }

    }
}
