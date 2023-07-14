// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModStroller : Mod, IApplicableToBeatmapProcessor
    {
        public override string Name => "Stroller";
        public override string Acronym => "SR";
        public override LocalisableString Description => @"Catching fruits while being very relaxed...";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModAutodash), typeof(CatchModRelax), typeof(CatchModTeleport) };

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.StrollerOffsets = true;
        }

    }
}
