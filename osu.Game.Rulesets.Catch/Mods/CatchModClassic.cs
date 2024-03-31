// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapProcessor
    {
        public override Type[] IncompatibleMods => new[] {
            typeof(CatchModSpicyPatterns),
            typeof(CatchModLowPrecision),
        };

        [SettingSource("Hard Rock spicy patterns", "Include spicy patterns when using Hard Rock.")]
        public BindableBool ClassicSpicyPatterns { get; } = new BindableBool(true);

        [SettingSource("Remove regular hyperdashes", "Removes the original hyperdashes and keeps the ones from the modified beatmap.")]
        public BindableBool RemoveRegularHyperDashes { get; } = new BindableBool(true);

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            var catchBeatmap = (CatchBeatmap)beatmapProcessor.Beatmap;

            catchBeatmap.RegularHyperDashGeneration.Value = !RemoveRegularHyperDashes.Value;
            catchBeatmapProcessor.ClassicSpicyPatterns = ClassicSpicyPatterns.Value;
        }
    }
}
