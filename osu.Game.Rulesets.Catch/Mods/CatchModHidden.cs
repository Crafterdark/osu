﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Mods.Debug_Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHidden : ModHidden, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override LocalisableString Description => @"Play with fading fruits.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        [SettingSource("Invisibility", "The percentage of playfield height that will be invisible.", SettingControlType = typeof(MultiplierSettingsSlider))]

        public BindableNumber<double> InitialInvisibility { get; } = new BindableDouble(0.6)
        {
            MinValue = 0.4,
            MaxValue = 0.8,
            Precision = 0.1,
        };

        private const double fade_out_duration_multiplier = 0.16;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModFadeIn), typeof(CatchModPile) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = false;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
            => ApplyNormalVisibilityState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            if (!(hitObject is DrawableCatchHitObject catchDrawable))
                return;

            if (catchDrawable.NestedHitObjects.Any())
            {
                foreach (var nestedDrawable in catchDrawable.NestedHitObjects)
                {
                    if (nestedDrawable is DrawableCatchHitObject nestedCatchDrawable)
                        fadeOutHitObject(nestedCatchDrawable);
                }
            }
            else
                fadeOutHitObject(catchDrawable);
        }

        private void fadeOutHitObject(DrawableCatchHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            double offset = hitObject.TimePreempt * InitialInvisibility.Value;
            double duration = hitObject.TimePreempt * fade_out_duration_multiplier;

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offset))
                drawable.FadeOut(duration);
        }
    }
}
