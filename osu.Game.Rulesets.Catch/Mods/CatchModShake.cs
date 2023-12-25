// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModShake : Mod, IApplicableToBeatmapProcessor
    {
        //Note: Name of this mod is temporary and it is created to test potential changes to catch randomization.
        public override string Name => "Shake";

        public override string Acronym => "SK";

        public override LocalisableString Description => "Created temporarily to test randomization changes.";

        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1 : 1;

        public override ModType Type => ModType.Conversion;

        //This setting will replace any other mod using the Spicy Patterns option.
        [SettingSource("Global Spicy Patterns", "Global adjust of the spicy patterns.")]
        public BindableBool ShakeOffsets { get; } = new BindableBool(true);

        //This setting will replace the droplet random effect of the beatmap.
        [SettingSource("Global Droplet Random", "Global adjust of the droplet random power.")]
        public BindableNumber<float> DropletRandomPower { get; } = new BindableFloat(1f)
        {
            MinValue = 0f,
            MaxValue = 1f,
            Precision = 0.1f,
        };

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.ShakeApplied = true;
            catchProcessor.ShakeOffsets = ShakeOffsets.Value;
            catchProcessor.DropletRandomPower = DropletRandomPower.Value;
        }
    }
}
