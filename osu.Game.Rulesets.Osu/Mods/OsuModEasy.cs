// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModEasy : ModEasy
    {
        public override LocalisableString Description => @"Larger circles, more forgiving HP drain and less accuracy required!";

        [SettingSource("Affects approach rate")]
        public BindableBool AffectsApproach { get; } = new BindableBool(true);

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            const float ratio = 0.5f;

            if (AffectsApproach.Value)
                difficulty.ApproachRate *= ratio;
        }
    }
}
