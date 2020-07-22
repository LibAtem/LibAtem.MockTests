using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using LibAtem.Commands;
using LibAtem.MockTests.SdkState;
using LibAtem.Serialization;
using LibAtem.Util;
using Xunit;

namespace LibAtem.MockTests.Util
{
    public static class CommandGenerator
    {
        public static Func<Lazy<ImmutableList<ICommand>>, ICommand, IEnumerable<ICommand>> CreateAutoCommandHandler<TSet, TGet>(string name, bool disableMask = false) where TGet : ICommand where TSet : ICommand
        {
            return CreateAutoCommandHandler<TSet, TGet>(new[] { name }, disableMask);
        }
        public static Func<Lazy<ImmutableList<ICommand>>, ICommand, IEnumerable<ICommand>> CreateAutoCommandHandler<TSet, TGet>(string[] names, bool disableMask = false) where TGet : ICommand where TSet : ICommand
        {
            object expectedMask = null;
            if (!disableMask)
            {
                // Calculate what the mask should ssbe
                PropertyInfo maskProp = typeof(TSet).GetProperty("Mask");
                Assert.NotNull(maskProp);
                expectedMask = Enum.Parse(maskProp.PropertyType, string.Join(",", names));
                Assert.NotEqual(0, (int) expectedMask);
            }

            return (previousCommands, cmd) =>
            {
                if (cmd is TSet setCmd)
                {
                    // Ensure the mask is correct
                    dynamic dynCmd = setCmd;
                    if (!disableMask)
                    {
                        Assert.Equal(expectedMask, dynCmd.Mask);
                    }

                    // Find the command to base the result on
                    CommandQueueKey targetCommandKey = CommandQueueKey.ForGetter<TGet>(setCmd);
                    TGet previousCmd = previousCommands.Value.OfType<TGet>().LastOrDefault(c => targetCommandKey.Equals(new CommandQueueKey(c)));
                    Assert.NotNull(previousCmd);

                    // Now copy the value across
                    foreach (string name in names)
                    {
                        PropertyInfo previousProp = typeof(TGet).GetProperty(name);
                        Assert.NotNull(previousProp);
                        PropertyInfo setProp = typeof(TSet).GetProperty(name);
                        Assert.NotNull(setProp);
                        previousProp.SetValue(previousCmd, setProp.GetValue(setCmd));
                    }

                    return new ICommand[] { previousCmd };
                }

                return new ICommand[0];
            };
        }

        public static Func<Lazy<ImmutableList<ICommand>>, ICommand, IEnumerable<ICommand>> MatchCommand<T>(T expectedCmd, bool followMask = false, params string[] ignoreProps) where T : AutoSerializeBase
        {
            return (previousCommands, cmd) =>
            {
                if (ValidateCommandMatches(cmd, expectedCmd, followMask, ignoreProps))
                {
                    // Accept it
                    return new ICommand[] { null };
                }

                return new ICommand[0];
            };
        }

        public static Func<Lazy<ImmutableList<ICommand>>, ICommand, IEnumerable<ICommand>> EchoCommand<T>(T expectedCmd, params string[] ignoreProps) where T : AutoSerializeBase
        {
            return (previousCommands, cmd) =>
            {
                if (expectedCmd == null || ValidateCommandMatches(cmd, expectedCmd, false, ignoreProps))
                {
                    // Echo it
                    return new[] {cmd};
                }

                return new ICommand[0];
            };
        }

        public static bool ValidateCommandMatches<T>(ICommand cmd, T expectedCmd, bool followMask, params string[] ignoreProps) where T : AutoSerializeBase
        {
            if (cmd is T cmd2)
            {
                AutoSerializeBase.CommandPropertySpec spec = AutoSerializeBase.GetPropertySpecForType(typeof(T));

                AutoSerializeBase.PropertySpec maskProp = spec.Properties.FirstOrDefault(p => p.PropInfo.Name == "Mask");
                var maskedNames = new HashSet<string> {"Mask"};
                if (maskProp != null)
                {
                    Enum maskVal = maskProp.Getter.DynamicInvoke(cmd) as Enum;
                    IEnumerable<string> components = Enum.GetValues(maskProp.PropInfo.PropertyType).OfType<Enum>()
                        .Where(v => maskVal.HasFlag(v)).Select(c => c.ToString());
                    maskedNames.AddRange(components);
                }

                foreach (var prop in spec.Properties)
                {
                    if (followMask && !maskedNames.Contains(prop.PropInfo.Name))
                        continue;
                    if (ignoreProps.Contains(prop.PropInfo.Name))
                        continue;

                    object expectedValue = prop.Getter.DynamicInvoke(expectedCmd);
                    object actualValue = prop.Getter.DynamicInvoke(cmd2);
                    Assert.Equal(expectedValue, actualValue);
                }

                // Accept it
                return true;
            }

            return false;
        }
    }
}
