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

        [SettingSource("No hyperdash on tiny droplets", "Tiny droplets will not initiate a hyperdash.")]
        public Bindable<bool> NoHyperDashTinyDroplet { get; } = new BindableBool(true);

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmap = (CatchBeatmap)beatmapProcessor.Beatmap;
            catchBeatmap.HyperDashTinyDroplet = !NoHyperDashTinyDroplet.Value;
        }
    }
}
