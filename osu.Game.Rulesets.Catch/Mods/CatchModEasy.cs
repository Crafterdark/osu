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
    public class CatchModEasy : ModEasyWithExtraLives, IApplicableToBeatmapProcessor
    {
        public override LocalisableString Description => @"Larger fruits, more forgiving HP drain, less accuracy required, and three lives!";

        [SettingSource("Spicy Patterns", "Adjust the patterns as if Hard Rock is enabled.")]
        public BindableBool HardRockOffsets { get; } = new BindableBool();

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.HardRockOffsets = HardRockOffsets.Value;
        }
    }
}
