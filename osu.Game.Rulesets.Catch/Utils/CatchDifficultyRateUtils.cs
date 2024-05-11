// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Utils
{
    public static partial class CatchDifficultyRateUtils
    {
        public static void RevertApproachRateChangesFromRateAdjust(BeatmapDifficulty difficulty, double speedChange)
        {
            double timePreempt = (float)IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, CatchHitObject.PREEMPT_MAX, CatchHitObject.PREEMPT_MID, CatchHitObject.PREEMPT_MIN);

            timePreempt *= speedChange;

            float approachRate = (float)IBeatmapDifficultyInfo.InverseDifficultyRange(timePreempt, CatchHitObject.PREEMPT_MAX, CatchHitObject.PREEMPT_MID, CatchHitObject.PREEMPT_MIN);

            difficulty.ApproachRate = approachRate;
        }
    }
}
