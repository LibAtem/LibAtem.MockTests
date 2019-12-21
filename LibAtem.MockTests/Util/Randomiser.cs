using System;

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
    }
}