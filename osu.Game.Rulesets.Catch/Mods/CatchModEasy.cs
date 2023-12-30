// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModEasy : ModEasyWithExtraLives
    {
        public override LocalisableString Description => @"Larger fruits, more forgiving HP drain, less accuracy required, and three lives!";

        protected const float HR_ADJUST_RATIO = 1.4f;

        [SettingSource("Reverse Hard Rock", "Reverse parameters with the adjust ratio of Hard Rock")]
        public Bindable<bool> ReverseHardRock { get; } = new BindableBool(false);

        public override string SettingDescription
        {
            get
            {
                string ReverseHardRock_string = ReverseHardRock.IsDefault ? string.Empty : $"{"Reverse Hard Rock:" + ReverseHardRock.Value}"; ;

                return string.Join(", ", new[]
{
                    base.SettingDescription,
                    ReverseHardRock_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            if (ReverseHardRock.Value)
            {
                difficulty.CircleSize = Math.Max(difficulty.CircleSize / 1.3f, 0.0f); // CS uses a custom 1.3 ratio.
                difficulty.ApproachRate = Math.Max(difficulty.ApproachRate / HR_ADJUST_RATIO, 0.0f);
                difficulty.DrainRate = Math.Max(difficulty.ApproachRate / HR_ADJUST_RATIO, 0.0f);
                difficulty.OverallDifficulty = Math.Max(difficulty.ApproachRate / HR_ADJUST_RATIO, 0.0f);
            }
            else
                base.ApplyToDifficulty(difficulty);
        }
    }
}
