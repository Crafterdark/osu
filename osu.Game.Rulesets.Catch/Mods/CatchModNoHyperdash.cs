// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModNoHyperdash : Mod, IApplicableToBeatmapProcessor
    {
        public override string Name => @"No Hyperdash";
        public override string Acronym => @"NH";
        public override LocalisableString Description => @"Convert the beatmap to the days where hyperdashes didn't exist.";
        public override double ScoreMultiplier => 1.0;
        public override ModType Type => ModType.Conversion;

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.ModOffsets.Add(ModOffsetType.NoHyperdash);
        }
    }
}
