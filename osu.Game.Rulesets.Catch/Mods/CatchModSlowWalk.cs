// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSlowWalk : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "Slow Walk";

        public override string Acronym => "SW";

        public override LocalisableString Description => "The catcher is a bit tired...";

        public override double ScoreMultiplier
        {
            get
            {
                // Round to the nearest multiple of 0.1.
                double value = (int)(MovementSpeedDecrease.Value * 10) / 10.0;

                // Offset back to 0.
                value -= 1;

                if (MovementSpeedDecrease.Value >= 1)
                    return 1 + value / 5;
                else
                    return 0.6 + value;
            }
        }

        public override IconUsage? Icon => FontAwesome.Solid.Walking;

        public override ModType Type => ModType.Fun;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModSpeedRun) };

        [SettingSource("Catcher speed decrease", "The actual decrease to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableDouble MovementSpeedDecrease { get; } = new BindableDouble(0.75)
        {
            Precision = 0.01,
            MinValue = 0.50,
            MaxValue = 0.99,
        };

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmap = (CatchBeatmap)beatmapProcessor.Beatmap;

            catchBeatmap.CatcherAdjustedDashSpeed.Value *= MovementSpeedDecrease.Value;
            catchBeatmap.UsesLimitedCatchPlayfield.Value = true;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var catchDrawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            catchPlayfield.Catcher.CustomSpeedMultiplier.Value *= MovementSpeedDecrease.Value;
            catchPlayfield.Catcher.MinX = ((CatchBeatmap)catchDrawableRuleset.Beatmap).LimitedCatchPlayfieldContainer.MinWidth;
            catchPlayfield.Catcher.MaxX = ((CatchBeatmap)catchDrawableRuleset.Beatmap).LimitedCatchPlayfieldContainer.MaxWidth;
        }
    }
}
