// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSpicyPatterns : Mod, IApplicableToBeatmapProcessor
    {
        public override string Name => "Spicy Patterns";
        public override string Acronym => "SP";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Conversion;
        public override LocalisableString Description => "Adjust the patterns to be slightly more unpredictable.";
        public override Type[] IncompatibleMods => new[] { typeof(CatchModHardRock) };
        public override double ScoreMultiplier => 1;

        [SettingSource("Spicy objects", "The objects that will be adjusted by the mod.")]
        public Bindable<SpicyVariant> SpicyObjects { get; } = new Bindable<SpicyVariant>();

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;

            switch (SpicyObjects.Value)
            {
                case SpicyVariant.Classic:
                    catchProcessor.FruitSpicyPatternsOffsets = true;
                    break;
                case SpicyVariant.Combo:
                    catchProcessor.FruitSpicyPatternsOffsets = true;
                    catchProcessor.JuiceStreamSpicyPatternsOffsets = true;
                    break;
            }
        }

        public enum SpicyVariant
        {
            Classic,
            Combo,
        }
    }
}
