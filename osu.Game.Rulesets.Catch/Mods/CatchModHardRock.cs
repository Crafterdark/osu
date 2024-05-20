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
    public class CatchModHardRock : ModHardRock, IApplicableToBeatmapProcessor, IApplicableToBeatmap
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;

        //Internal Mirror enabled by default
        public bool MirrorFruitsOnGeneration { get; set; } = true;

        //Internal Spicy patterns disabled by default
        public bool SpicyPatternsOnGeneration { get; set; }

        private CatchModMirror internalModMirror = new CatchModMirror();

        private CatchModSpicyPatterns internalModSpicyPatterns = new CatchModSpicyPatterns();

        [SettingSource("Affects approach rate")]
        public BindableBool AffectsApproach { get; } = new BindableBool(true);

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            if (AffectsApproach.Value)
                difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * ADJUST_RATIO, 10.0f);
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchBeatmapProcessor.HardRockOffsets = true;

            if (SpicyPatternsOnGeneration)
                internalModSpicyPatterns.ApplyToBeatmapProcessor(beatmapProcessor);
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (MirrorFruitsOnGeneration)
                internalModMirror.ApplyToBeatmap(beatmap);
        }
    }
}
