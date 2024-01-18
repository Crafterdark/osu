// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapConverter, IApplicableToBeatmapProcessor
    {
        [SettingSource("Asymmetrical hyperdash generation", "Stable used asymmetrical hyperdash generation.")]
        public Bindable<bool> IsHyperDashAsymmetrical { get; } = new BindableBool(true);
    
        [SettingSource("Old tiny droplet generation", "Old beatmaps mistimed or prevented tiny droplet generation under particular conditions.")]
        public Bindable<bool> OldTinyGeneration { get; } = new BindableBool(true);

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var catchBeatmapConverter = (CatchBeatmapConverter)beatmapConverter;
            catchBeatmapConverter.GenerationWithLegacyLastTick = OldTinyGeneration.Value;
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchBeatmapProcessor.IsHyperDashSymmetrical = !IsHyperDashAsymmetrical.Value;
            catchBeatmapProcessor.IsOldTinyGeneration = OldTinyGeneration.Value;
        }
    }
}
