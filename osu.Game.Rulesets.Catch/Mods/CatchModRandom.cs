// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModRandom : ModRandom, IApplicableToBeatmapProcessor
    {
        public override LocalisableString Description => "It never gets boring!";

        [SettingSource("Flow factor", "How close the map will be to the original flow", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> FlowFactor { get; } = new BindableDouble(0.00)
        {
            MinValue = 0.00,
            MaxValue = 0.75,
            Precision = 0.01,
        };

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            CatchBeatmapProcessor catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchBeatmapProcessor.RandomModOffsets = true;
            Seed.Value ??= RNG.Next();
            catchBeatmapProcessor.RandomModSeed = (int)Seed.Value;
        }
    }
}
