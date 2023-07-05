// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Mods.SharedMods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFadeIn : CatchSharedModVisibility, IApplicableToBeatmap
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.DifficultyIncrease;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModFlashlight)).ToArray();

        public override LocalisableString Description => @"Play with fading fruits.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        [SettingSource("Fade In Distance", "The distance to apply the fade in")]
        public BindableDouble FadeInDistance { get; } = new BindableDouble(0.86d)
        {
            MinValue = 0.70d,
            MaxValue = 0.96d,
            Precision = 0.01d
        };

        [SettingSource("Fade In Duration", "The duration to apply the fade in")]
        public BindableDouble FadeInDuration { get; } = new BindableDouble(0.16d)
        {
            MinValue = 0.08d,
            MaxValue = 0.16d,
            Precision = 0.01d
        };

        public new void ApplyToBeatmap(IBeatmap beatmap)
        {
            CatchSharedModVariables.SharedFadeInDistance = FadeInDistance.Value;
            CatchSharedModVariables.SharedFadeInDuration = FadeInDuration.Value;
            var catchBeatmap = (CatchBeatmap)beatmap;
            catchBeatmap.CatchModFadeInApplied = true;
            CatchSharedModVariables.UpdateSharedMods(catchBeatmap);
        }

    }
}
