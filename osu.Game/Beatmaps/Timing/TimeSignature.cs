// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;

namespace osu.Game.Beatmaps.Timing
{
    /// <summary>
    /// Stores the time signature of a track.
    /// For now, the lower numeral can only be 4; support for other denominators can be considered at a later date.
    /// </summary>
    public class TimeSignature : IEquatable<TimeSignature>
    {
        /// <summary>
        /// The numerator of a signature.
        /// </summary>
        public int Numerator { get; }

        /// <summary>
        /// When the numerator of a signature is negative or null.
        /// </summary>
        public bool LegacyNegativeOrNull { get; }

        // TODO: support time signatures with a denominator other than 4
        // this in particular requires a new beatmap format.

        public TimeSignature(int numerator)
        {
            Numerator = Math.Abs(numerator);
            LegacyNegativeOrNull = numerator <= 0;
        }

        public static TimeSignature SimpleTriple { get; } = new TimeSignature(3);
        public static TimeSignature SimpleQuadruple { get; } = new TimeSignature(4);

        public override string ToString() => $"{Numerator}/4";

        public bool Equals(TimeSignature other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Numerator == other.Numerator;
        }

        public override int GetHashCode() => Numerator;
    }
}
