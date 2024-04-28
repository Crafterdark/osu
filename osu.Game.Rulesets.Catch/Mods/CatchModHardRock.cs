// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHardRock : ModHardRock, IApplicableToBeatmapProcessor, IApplicableToBeatmap
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;

        public bool MirrorFruitsOnGeneration { get; set; } = true;

        private CatchModMirror internalModMirror = new CatchModMirror();

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            difficulty.CircleSize = Math.Min(difficulty.CircleSize * 1.3f, 10.0f); // CS uses a custom 1.3 ratio.
            difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * ADJUST_RATIO, 10.0f);
        }
        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchBeatmapProcessor.HardRockOffsets = true;
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (MirrorFruitsOnGeneration)
                internalModMirror.ApplyToBeatmap(beatmap);
        }
    }
}
