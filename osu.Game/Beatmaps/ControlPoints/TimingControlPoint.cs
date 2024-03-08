// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint, IEquatable<TimingControlPoint>
    {
        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public readonly Bindable<TimeSignature> TimeSignatureBindable = new Bindable<TimeSignature>(TimeSignature.SimpleQuadruple);

        /// <summary>
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public readonly BindableBool OmitFirstBarLineBindable = new BindableBool();

        /// <summary>
        /// Default length of a beat in milliseconds. Used whenever there is no beatmap or track playing.
        /// </summary>
        private const double default_beat_length = 60000.0 / 60.0;

        public bool IsNegativeBPM() => BeatLength < 0;

        public override Color4 GetRepresentingColour(OsuColour colours) => colours.Orange1;

        public static readonly TimingControlPoint DEFAULT = new TimingControlPoint
        {
            BeatLengthBindable =
            {
                Value = default_beat_length,
                Disabled = true
            },
            OmitFirstBarLineBindable = { Disabled = true },
            TimeSignatureBindable = { Disabled = true }
        };

        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public TimeSignature TimeSignature
        {
            get => TimeSignatureBindable.Value;
            set => TimeSignatureBindable.Value = value;
        }

        /// <summary>
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public bool OmitFirstBarLine
        {
            get => OmitFirstBarLineBindable.Value;
            set => OmitFirstBarLineBindable.Value = value;
        }

        public const double DEFAULT_BEAT_LENGTH = 1000;

        public const double MIN_BEAT_LENGTH_CAP = 6;

        public const double MAX_BEAT_LENGTH_CAP = 60000;

        public const double MIN_BEAT_LENGTH_UNCAP = double.Epsilon;

        public const double MAX_BEAT_LENGTH_UNCAP = double.MaxValue;

        public const double VIEW_MIN_BPM_INT_CAP = 1;

        public const double VIEW_MAX_BPM_INT_CAP = 999999;

        /// <summary>
        /// The beat length at this control point.
        /// </summary>
        public readonly BindableDouble BeatLengthBindable = new BindableDouble(DEFAULT_BEAT_LENGTH)
        {
            MinValue = double.MinValue,
            MaxValue = double.MaxValue
        };

        /// <summary>
        /// The beat length at this control point.
        /// </summary>
        public double BeatLength
        {
            get => Math.Abs(BeatLengthBindable.Value);
            set => BeatLengthBindable.Value = value;
        }

        /// <summary>
        /// The beat length at this control point with condition.
        /// </summary>
        public double BeatLengthWithCondition(bool isLimited)
        {
            return Math.Clamp(BeatLength, isLimited ? MIN_BEAT_LENGTH_CAP : MIN_BEAT_LENGTH_UNCAP, isLimited ? MAX_BEAT_LENGTH_CAP : MAX_BEAT_LENGTH_UNCAP);
        }

        /// <summary>
        /// The BPM at this control point.
        /// </summary>
        public double BPM => 60000 / Math.Clamp(BeatLength, 60000 / double.MaxValue, double.MaxValue);

        // Timing points are never redundant as they can change the time signature.
        public override bool IsRedundant(ControlPoint? existing) => false;

        public override void CopyFrom(ControlPoint other)
        {
            TimeSignature = ((TimingControlPoint)other).TimeSignature;
            OmitFirstBarLine = ((TimingControlPoint)other).OmitFirstBarLine;
            BeatLength = ((TimingControlPoint)other).BeatLength;

            base.CopyFrom(other);
        }

        public override bool Equals(ControlPoint? other)
            => other is TimingControlPoint otherTimingControlPoint
               && Equals(otherTimingControlPoint);

        public bool Equals(TimingControlPoint? other)
            => base.Equals(other)
               && TimeSignature.Equals(other.TimeSignature)
               && OmitFirstBarLine == other.OmitFirstBarLine
               && BeatLength.Equals(other.BeatLength);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), TimeSignature, BeatLength, OmitFirstBarLine);
    }
}
