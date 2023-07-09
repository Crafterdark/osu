// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Mods.SharedMods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHidden : CatchSharedModVisibility, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmap
    {
        public override string Name => "Hidden";
        public override string Acronym => "HD";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => @"Play with fading fruits.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(CatchModGhostMode)).ToArray();

        [SettingSource("Hidden Distance", "The distance to apply Hidden")]
        public BindableDouble HiddenDistance { get; } = new BindableDouble(0.60d)
        {
            MinValue = 0.60d,
            MaxValue = 0.76d,
            Precision = 0.01d
        };

        [SettingSource("Hidden Duration", "The duration to apply Hidden")]
        public BindableDouble HiddenDuration { get; } = new BindableDouble(0.16d)
        {
            MinValue = 0.08d,
            MaxValue = 0.16d,
            Precision = 0.01d
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = false;
        }

        public new void ApplyToBeatmap(IBeatmap beatmap)
        {
            CatchSharedModVariables.SharedHiddenDistance = HiddenDistance.Value;
            CatchSharedModVariables.SharedHiddenDuration = HiddenDuration.Value;
            var catchBeatmap = (CatchBeatmap)beatmap;
            catchBeatmap.CatchModHiddenApplied = true;
            CatchSharedModVariables.UpdateSharedMods(catchBeatmap);
        }

    }
}
