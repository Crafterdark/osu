// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHardRock : ModHardRock, IApplicableToBeatmapProcessor
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;

        [SettingSource("Remove Spicy Patterns", "Removes random offsets applied by the mod.")]
        public Bindable<bool> RemoveHardRockOffsets { get; } = new BindableBool(false);

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.HardRockOffsets = !RemoveHardRockOffsets.Value;
        }

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            difficulty.CircleSize = Math.Min(difficulty.CircleSize * 1.3f, 10.0f); // CS uses a custom 1.3 ratio.
            difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * ADJUST_RATIO, 10.0f);
        }
    }
}
