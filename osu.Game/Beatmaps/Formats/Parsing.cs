// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;

namespace osu.Game.Beatmaps.Formats
{
    /// <summary>
    /// Helper methods to parse from string to number and perform very basic validation.
    /// </summary>
    public static class Parsing
    {
        public const int MAX_COORDINATE_VALUE = 131072;

        public const double MAX_PARSE_VALUE = int.MaxValue;

        //Note: Must be doublechecked
        private const bool are_bounds_allowed = true;

        public static float ParseFloat(string input, float parseLimit = (float)MAX_PARSE_VALUE, bool allowNaN = false)
        {
            float output = float.Parse(input, CultureInfo.InvariantCulture);

            if (!are_bounds_allowed && output < -parseLimit) throw new OverflowException("Value is too low");
            if (!are_bounds_allowed && output > parseLimit) throw new OverflowException("Value is too high");

            if (are_bounds_allowed)
                output = Math.Clamp(output, -parseLimit, parseLimit);

            if (!allowNaN && float.IsNaN(output)) throw new FormatException("Not a number");

            return output;
        }

        public static double ParseDouble(string input, double parseLimit = MAX_PARSE_VALUE, bool allowNaN = false)
        {
            double output = double.Parse(input, CultureInfo.InvariantCulture);

            if (!are_bounds_allowed && output < -parseLimit) throw new OverflowException("Value is too low");
            if (!are_bounds_allowed && output > parseLimit) throw new OverflowException("Value is too high");

            if (are_bounds_allowed)
                output = Math.Clamp(output, -parseLimit, parseLimit);

            if (!allowNaN && double.IsNaN(output)) throw new FormatException("Not a number");

            return output;
        }

        public static int ParseInt(string input, int parseLimit = (int)MAX_PARSE_VALUE)
        {
            int output = int.Parse(input, CultureInfo.InvariantCulture);

            if (!are_bounds_allowed && output < -parseLimit) throw new OverflowException("Value is too low");
            if (!are_bounds_allowed && output > parseLimit) throw new OverflowException("Value is too high");

            if (are_bounds_allowed)
                output = Math.Clamp(output, -parseLimit, parseLimit);

            return output;
        }
    }
}
