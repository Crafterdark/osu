// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModWrench : ModWrench, IApplicableToBeatmapProcessor, IApplicableToBeatmapConverter
    {
        [SettingSource("Symmetric hyperdash generation", "Prevents the generation of impossible or unexpected hyperdash patterns.")]
        public BindableBool BeatmapHyperDashGenerationSymmetric { get; } = new BindableBool(true);

        [SettingSource("Enhanced tiny droplet generation", "Prevents the legacy last tick from changing the tiny droplets at the end path of a juicestream.")]
        public BindableBool BeatmapEnhancedTinyDropletGeneration { get; } = new BindableBool(true);

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            ((CatchBeatmap)catchBeatmapProcessor.Beatmap).IsHyperDashGenerationSymmetric = BeatmapHyperDashGenerationSymmetric.Value;
        }

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var catchBeatmapConverter = (CatchBeatmapConverter)beatmapConverter;

            catchBeatmapConverter.IsBeatLengthLimited = !BeatmapTimingPointBeatLengthUnbounded.Value;
            catchBeatmapConverter.IsTinyDropletGenerationEnhanced = BeatmapEnhancedTinyDropletGeneration.Value;
        }
    }
}
