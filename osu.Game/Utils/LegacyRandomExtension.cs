// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public class LegacyRandomExtension : LegacyRandom
    {
        public LegacyRandomExtension(int seed) : base(seed)
        {
        }

        /// <summary>
        /// Generates a random float value within the range [0, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        /// <returns>The random value.</returns>
        public override float NextFloat(float upperBound) => (float)(NextDouble() * (upperBound + 1));

        /// <summary>
        /// Generates a random integer value within the range [0, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        /// <returns>The random value.</returns>
        public override int Next(int upperBound) => (int)(NextDouble() * (upperBound + 1));

        /// <summary>
        /// Generates a random integer value within the range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="lowerBound">The lower bound of the range.</param>
        /// <param name="upperBound">The upper bound of the range.</param>
        /// <returns>The random value.</returns>
        public override int Next(int lowerBound, int upperBound) => (int)(lowerBound + NextDouble() * (upperBound + 1 - lowerBound));

        /// <summary>
        /// Generates a random integer value within the range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>].
        /// </summary>
        /// <param name="lowerBound">The lower bound of the range.</param>
        /// <param name="upperBound">The upper bound of the range.</param>
        /// <returns>The random value.</returns>
        public override int Next(double lowerBound, double upperBound) => (int)(lowerBound + NextDouble() * (upperBound + 1 - lowerBound));
    }
}
