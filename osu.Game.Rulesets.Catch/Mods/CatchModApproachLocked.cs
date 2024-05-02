// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModApproachLocked : ModApproachLocked
    {
        public override void RestoreApproachRate(IBeatmap beatmap, BeatmapDifficulty difficulty, IReadOnlyList<Mod> mods)
        {
            double timePreempt = (float)IBeatmapDifficultyInfo.DifficultyRange(beatmap.BeatmapInfo.Difficulty.Clone().ApproachRate, CatchHitObject.PREEMPT_MAX, CatchHitObject.PREEMPT_MID, CatchHitObject.PREEMPT_MIN);

            var rateAdjustMods = mods.OfType<ModRateAdjust>();

            foreach (ModRateAdjust mod in rateAdjustMods)
                timePreempt *= mod.SpeedChange.Value;

            float approachRate = (float)IBeatmapDifficultyInfo.InverseDifficultyRange(timePreempt, CatchHitObject.PREEMPT_MAX, CatchHitObject.PREEMPT_MID, CatchHitObject.PREEMPT_MIN);

            difficulty.ApproachRate = approachRate;
        }
    }
}
