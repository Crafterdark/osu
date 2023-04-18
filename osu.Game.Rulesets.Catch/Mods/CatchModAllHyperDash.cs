// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAllHyperDash : Mod, IApplicableToBeatmapProcessor
    {
        public override string Name => "All Hyper Dash";
        public override string Acronym => "AH";
        public override LocalisableString Description => "Hyper dashes. Everywhere.";
        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModNoDashing), typeof(CatchModNoHyperDashing) };

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.AllHyperDashOffsets = true;
        }

    }
}
