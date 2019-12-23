using System;
using System.Collections.Generic;

namespace LibAtem.MockTests.Util
{
    public static class Randomiser
    {
        private static Random _random;

        static Randomiser()
        {
            _random = new Random();
        }
        
        public static double Range(double min = -100, double max = 6, double rounding = 100)
        {
            double scale = max - min;
            return RoundTo(_random.NextDouble() * scale + min, rounding);
        }

        public static double RoundTo(double value, double rounding)
        {
            return Math.Round(value * rounding) / rounding;
        }

        public static IEnumerable<T> SelectionOfGroup<T>(List<T> options, int randomCount = 3)
        {
            var rand = new Random();

            for (int i = 0; i < randomCount && options.Count > 0; i++)
            {
                int ind = rand.Next(0, options.Count);
                yield return options[ind];
                options.RemoveAt(ind);
            }
        }
    }

}