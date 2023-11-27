// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModDropletStabilizer : Mod, IApplicableToBeatmapProcessor

    {
        public override string Name => "Droplet Stabilizer";

        public override string Acronym => "DS";

        public override LocalisableString Description => "Droplets are more stable and linear...";

        public override double ScoreMultiplier => 0.90;

        public override ModType Type => ModType.Conversion;

        [SettingSource("Stabilizer Power", "The actual stability to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> StabilizerPower { get; } = new BindableDouble(1.0)
        {
            MinValue = 0.1,
            MaxValue = 1.0,
            Precision = 0.1,
        };

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchBeatmapProcessor.IsDropletStabilized = true;
            catchBeatmapProcessor.StabilizerPower = StabilizerPower.Value;
        }
    }
}
