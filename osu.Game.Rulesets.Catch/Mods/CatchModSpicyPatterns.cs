// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Framework.Localisation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSpicyPatterns : Mod, IApplicableToBeatmapProcessor
    {
        public override double ScoreMultiplier => 1;

        public override string Name => "Spicy Patterns";

        public override string Acronym => "SP";

        public override IconUsage? Icon => FontAwesome.Solid.FireAlt;

        public override ModType Type => ModType.Conversion;

        public override LocalisableString Description => "Adjust the patterns to be unpredictable and harder than usual.";

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.SpicyPatterns = true;
        }
    }
}
