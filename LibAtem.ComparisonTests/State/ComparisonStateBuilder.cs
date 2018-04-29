using System;
using System.Collections.Generic;
using LibAtem.Commands;
using LibAtem.Commands.DeviceProfile;
using LibAtem.Common;

namespace LibAtem.ComparisonTests.State
{
    public static class ComparisonStateBuilder
    {
        private static readonly IReadOnlyDictionary<Type, Action<ComparisonState, ICommand>> updaters;

        static ComparisonStateBuilder()
        {
            updaters = new Dictionary<Type, Action<ComparisonState, ICommand>>()
            {
                {typeof(TopologyCommand), UpdateTopology},
                {typeof(AuxSourceGetCommand), UpdateAux},
                {typeof(ColorGeneratorGetCommand), UpdateColor},
            };
        }

        public static void Update(ComparisonState state, IReadOnlyList<ICommand> commands)
        {
            foreach (ICommand cmd in commands)
                Update(state, cmd);
        }

        private static void Update(ComparisonState state, ICommand cmd)
        {
            if (updaters.TryGetValue(cmd.GetType(), out var updater))
                updater(state, cmd);
            else
                Console.WriteLine("ComparisonState: Missing LibAtem handling of " + cmd.GetType().Name);
        }

        private static void UpdateTopology(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TopologyCommand)rawCmd;
            
            state.Auxiliaries = new Dictionary<AuxiliaryId, ComparisonAuxiliaryState>();
            for (int i = 0; i < cmd.Auxiliaries; i++)
                state.Auxiliaries[(AuxiliaryId) i] = new ComparisonAuxiliaryState();

            state.Colors = new Dictionary<ColorGeneratorId, ComparisonColorState>();
            for (int i = 0; i < cmd.ColorGenerators; i++)
                state.Colors[(ColorGeneratorId)i] = new ComparisonColorState();

            // TODO others
        }

        private static void UpdateAux(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (AuxSourceGetCommand)rawCmd;
            state.Auxiliaries[cmd.Id].Source = cmd.Source;
        }

        private static void UpdateColor(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (ColorGeneratorGetCommand)rawCmd;
            var col = state.Colors[cmd.Index];
            col.Hue = cmd.Hue;
            col.Saturation = cmd.Saturation;
            col.Luma = cmd.Luma;
        }
    }
}