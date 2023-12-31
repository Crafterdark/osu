// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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

        public const float HR_CS_RATIO = 1.3f;

        public const float HR_ADJUST_RATIO = 1.4f;

        [SettingSource("Reversed Hard Rock", "Changes beatmap parameters with the adjust ratio of Hard Rock.")]
        public Bindable<bool> EasyReversedHardRock { get; } = new BindableBool(false);

        [SettingSource("Adjust Dull Patterns", "Removes uncomfortable patterns from the beatmap.")]
        public Bindable<bool> DullPatterns { get; } = new BindableBool(false);

        public override string SettingDescription
        {
            get
            {
                string DullPatterns_string = DullPatterns.IsDefault ? string.Empty : $"{"Dull Patterns:" + DullPatterns.Value}";
                string EasyReverseHardRock_string = EasyReversedHardRock.IsDefault ? string.Empty : $"{"Reversed Hard Rock:" + EasyReversedHardRock.Value}";

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    DullPatterns_string,
                    EasyReverseHardRock_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            if (EasyReversedHardRock.Value)
            {
                difficulty.CircleSize = Math.Max(difficulty.CircleSize / HR_CS_RATIO, 0.0f); // CS uses a custom 1.3 ratio.
                difficulty.ApproachRate = Math.Max(difficulty.ApproachRate / HR_ADJUST_RATIO, 0.0f);
                difficulty.DrainRate = Math.Max(difficulty.ApproachRate / HR_ADJUST_RATIO, 0.0f);
                difficulty.OverallDifficulty = Math.Max(difficulty.ApproachRate / HR_ADJUST_RATIO, 0.0f);
            }
            else
                base.ApplyToDifficulty(difficulty);
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmap = (CatchBeatmap)beatmapProcessor.Beatmap;
            catchBeatmap.DullPatterns = DullPatterns.Value;
            catchBeatmap.EasyReversedHardRock = EasyReversedHardRock.Value;
        }
    }
}
