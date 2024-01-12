// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapConverter
    {
        [SettingSource("Old tiny droplet generation", "Old beatmaps did not generate tiny droplets at the end of sliders on certain conditions.")]
        public Bindable<bool> OldTinyDropletGeneration { get; } = new BindableBool(true);

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var catchBeatmapConverter = (CatchBeatmapConverter)beatmapConverter;
            catchBeatmapConverter.OldTinyTickGeneration = OldTinyDropletGeneration.Value;
        }
    }
}
