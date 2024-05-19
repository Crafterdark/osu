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
    public class CatchModDropletStabilizer : Mod, IApplicableToBeatmapProcessor
    {
        public override string Name => "Droplet Stabilizer";

        public override string Acronym => "DS";

        public override LocalisableString Description => "Droplets are more stable and linear...";

        public override double ScoreMultiplier => 0.90;

        public override ModType Type => ModType.Conversion;

        [SettingSource("Stabilized Offset", "The maximum random offset from origin")]
        public BindableNumber<double> StabilizedOffset { get; } = new BindableDouble(10)
        {
            MinValue = 0,
            MaxValue = 19,
            Precision = 1,
        };

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;

            catchBeatmapProcessor.IsDropletStabilized = true;
            catchBeatmapProcessor.StabilizedOffset = (int)StabilizedOffset.Value;
        }
    }
}
