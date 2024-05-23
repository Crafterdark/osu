// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Represents a mod which can override (and block) a fail.
    /// </summary>
    public interface IApplicableFailOverride : IApplicableMod
    {
        /// <summary>
        /// Whether we should allow failing at the current point in time.
        /// </summary>
        /// <returns>Whether the fail should be allowed to proceed. Return false to block.</returns>
        bool LocalPerformFail();

        /// <summary>
        /// Whether we should allow failing at the current point in time. Ignores every other condition.
        /// </summary>
        /// <returns>Whether the fail should be allowed to proceed. Cannot be blocked by other mods. Return false to block.</returns>
        bool GlobalPerformFail();

        /// <summary>
        /// Whether we want to restart on fail. Only used if one <see cref="GlobalPerformFail"/> returns true or all <see cref="LocalPerformFail"/> return true.
        /// </summary>
        bool RestartOnFail { get; }
    }
}
