// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps.HyperDashGeneration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModCenter : Mod, IApplicableToBeatmapProcessor
    {
        public override string Name => @"Center";
        public override string Acronym => @"CR";
        public override LocalisableString Description => @"Remove edge dashes. Everything can be caught on the center.";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax) };
        public override ModType Type => ModType.Conversion;

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;

            catchBeatmapProcessor.HyperDashGenerator.Options.Add(HyperDashGeneratorOptions.Center);
        }
    }
}
