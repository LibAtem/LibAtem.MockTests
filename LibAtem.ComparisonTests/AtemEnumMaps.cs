using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;

namespace LibAtem.ComparisonTests
{
    public static class AtemEnumMaps
    {
        public static readonly IReadOnlyDictionary<BorderBevel, _BMDSwitcherBorderBevelOption> BorderBevelMap;

        static AtemEnumMaps()
        {
            BorderBevelMap = new Dictionary<BorderBevel, _BMDSwitcherBorderBevelOption>()
            {
                {BorderBevel.None, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionNone},
                {BorderBevel.InOut, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut},
                {BorderBevel.In, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionIn},
                {BorderBevel.Out, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionOut},
            };
        }

        public static Tk FindByValue<Tk, Tv>(this IReadOnlyDictionary<Tk, Tv> dict, Tv value)
        {
            return dict.First(v => Equals(v.Value, value)).Key;
        }

        public static AuxiliaryId GetAuxId(VideoSource id)
        {
            if (id >= VideoSource.Auxilary1 && id <= VideoSource.Auxilary6)
                return (AuxiliaryId)(id - VideoSource.Auxilary1);

            throw new Exception("Not an Aux");
        }

        public static ColorGeneratorId GetSourceIdForGen(VideoSource id)
        {
            switch (id)
            {
                case VideoSource.Color1:
                    return ColorGeneratorId.One;
                case VideoSource.Color2:
                    return ColorGeneratorId.Two;
                default:
                    throw new Exception("Not a ColorGen");
            }
        }
    }
}
