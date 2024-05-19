// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Utils
{
    public class LegacyRandomExtension : LegacyRandom
    {
        private const double int_to_real = 1.0 / (int.MaxValue + 1.0);

        private Random random;

        public LegacyRandomExtension(int seed) : base(seed)
        {
            random = new Random(seed);
        }

        /// <summary>
        /// Generates a random double value within the range [0, 1].
        /// </summary>
        /// <returns>The random value.</returns>
        public override double NextDouble()
        {
            int nextInt = Next();

            //Allow to eventually reach the upper bound
            if (nextInt == int.MaxValue - 1)
            {
                double nextIntAsDouble = nextInt;

                //Randomly increase this integer by 0, 1, 2 (If 2, then the upper bound is reached)
                nextIntAsDouble += random.Next(3);
                return int_to_real * nextIntAsDouble;
            }

            return int_to_real * nextInt;
        }

        /// <summary>
        /// Generates a random float value within the range [0, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        /// <returns>The random value.</returns>
        public override float NextFloat(float upperBound) => (float)(NextDouble() * upperBound);

        /// <summary>
        /// Generates a random integer value within the range [0, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        /// <returns>The random value.</returns>
        public override int Next(int upperBound) => (int)(base.NextDouble() * (upperBound + 1));

        /// <summary>
        /// Generates a random integer value within the range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="lowerBound">The lower bound of the range.</param>
        /// <param name="upperBound">The upper bound of the range.</param>
        /// <returns>The random value.</returns>
        public override int Next(int lowerBound, int upperBound) => (int)((lowerBound - 1) + base.NextDouble() * (upperBound - lowerBound + 2));

        /// <summary>
        /// Generates a random integer value within the range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="lowerBound">The lower bound of the range.</param>
        /// <param name="upperBound">The upper bound of the range.</param>
        /// <returns>The random value.</returns>
        public override int Next(double lowerBound, double upperBound) => (int)((lowerBound - 1) + base.NextDouble() * (upperBound - lowerBound + 2));
    }
}
