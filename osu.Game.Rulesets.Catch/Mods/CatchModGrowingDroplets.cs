// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModGrowingDroplets : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Growing Droplets";
        public override string Acronym => "GD";
        public override LocalisableString Description => "Pouring water on the soil to let tiny droplets grow...";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Seedling;
        public override ModType Type => ModType.Conversion;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var catchBeatmapConverter = (CatchBeatmapConverter)beatmapConverter;

            catchBeatmapConverter.OnlyLargeDroplets = true;
        }
    }
}
