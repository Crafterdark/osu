// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapProcessor
    {
        [SettingSource("Asymmetrical hyperdash generation", "Stable generated asymmetrical hyperdashes during beatmap processing.")]
        public Bindable<bool> AsymmetricalHyperDashGeneration { get; } = new BindableBool(true);

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            ((CatchBeatmap)catchBeatmapProcessor.Beatmap).IsProcessingSymmetricalHyperDash = !AsymmetricalHyperDashGeneration.Value;
        }
    }
}
