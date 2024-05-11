// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Utils
{
    public static partial class OsuDifficultyRateUtils
    {
        public static void RevertApproachRateChangesFromRateAdjust(BeatmapDifficulty difficulty, double speedChange)
        {
            double timePreempt = (float)IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, OsuHitObject.PREEMPT_MAX, OsuHitObject.PREEMPT_MID, OsuHitObject.PREEMPT_MIN);

            timePreempt *= speedChange;

            float approachRate = (float)IBeatmapDifficultyInfo.InverseDifficultyRange(timePreempt, OsuHitObject.PREEMPT_MAX, OsuHitObject.PREEMPT_MID, OsuHitObject.PREEMPT_MIN);

            difficulty.ApproachRate = approachRate;
        }
    }
}
