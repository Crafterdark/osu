// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDaycore : ModDaycore
    {
        [SettingSource("Affects approach rate")]
        public BindableBool AffectsApproach { get; } = new BindableBool(true);

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            if (!AffectsApproach.Value)
                OsuDifficultyRateUtils.RevertApproachRateChangesFromRateAdjust(difficulty, SpeedChange.Value);
        }
    }
}
