// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHardRock : ModHardRock, IApplicableToBeatmapProcessor
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(CatchModSpicyPatterns) }).ToArray();
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1.06;

        private CatchModSpicyPatterns internalCatchModSpicyPatterns = new CatchModSpicyPatterns();

        [SettingSource("Spicy patterns", "Adjust the patterns to be slightly more unpredictable.")]
        public BindableBool SpicyPatterns { get; } = new BindableBool(true);

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            if (SpicyPatterns.Value)
                internalCatchModSpicyPatterns.ApplyToBeatmapProcessor(beatmapProcessor);
        }

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            difficulty.CircleSize = Math.Min(difficulty.CircleSize * 1.3f, 10.0f); // CS uses a custom 1.3 ratio.
            difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * ADJUST_RATIO, 10.0f);
        }
    }
}
