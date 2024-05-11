// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a TimePreempt.
    /// </summary>
    public interface IHasTimePreempt
    {
        /// <summary>
        /// The time preempt.
        /// </summary>
        /// 
        public double TimePreempt { get; set; }
    }
}
