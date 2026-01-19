using System;
using System.Collections.Generic;

namespace BikeWars.Utilities
{
    public static class RandomUtil
    {
        public static int NextInt(int min, int max)
        {
            return Random.Shared.Next(min, max);
        }

        public static double NextDouble()
        {
            return Random.Shared.NextDouble();
        }
    }
}
