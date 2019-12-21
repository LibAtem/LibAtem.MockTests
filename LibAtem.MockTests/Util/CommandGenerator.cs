using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using LibAtem.Commands;
using Xunit;

namespace LibAtem.MockTests.Util
{
    public static class CommandGenerator
    {
        public static Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>> CreateAutoCommandHandler<TSet, TGet>(string name) where TGet : ICommand where TSet : ICommand
        {
            // Calculate what the mask should ssbe
            PropertyInfo maskProp = typeof(TSet).GetProperty("Mask");
            Assert.NotNull(maskProp);
            dynamic expectedMask = Enum.Parse(maskProp.PropertyType, name);
            Assert.NotEqual(0, (int)expectedMask);

            return (previousCommands, cmd) =>
            {
                if (cmd is TSet setCmd)
                {
                    // Ensure the mask is correct
                    dynamic dynCmd = setCmd;
                    Assert.Equal(expectedMask, dynCmd.Mask);

                    // Find the command to base the result on
                    CommandQueueKey targetCommandKey = CommandQueueKey.ForGetter<TGet>(setCmd);
                    TGet previousCmd = previousCommands.OfType<TGet>().Last(c => targetCommandKey.Equals(new CommandQueueKey(c)));
                    Assert.NotNull(previousCmd);

                    // Now copy the value across
                    PropertyInfo previousProp = typeof(TGet).GetProperty(name);
                    Assert.NotNull(previousProp);
                    PropertyInfo setProp = typeof(TSet).GetProperty(name);
                    Assert.NotNull(setProp);
                    previousProp.SetValue(previousCmd, setProp.GetValue(setCmd));

                    return new ICommand[] { previousCmd };
                }

                return new ICommand[0];
            };
        }
    }
}
