// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using System;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Catch.Mods.Debug_Mods
{
    public class CatchModFadeIn : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override LocalisableString Description => @"Play with fading fruits.";

        public override string Name => "Fade In";

        public override string Acronym => "FI";
        public override ModType Type => ModType.DifficultyIncrease;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModHidden), typeof(CatchModFlashlight) };

        [SettingSource("Fading duration", "Speed of the fading effect", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> FadeInDurationMultiplier { get; } = new BindableDouble(0.50)
        {
            MinValue = 0.30,
            MaxValue = 0.50,
            Precision = 0.01,
        };

        [SettingSource("Fading height", "Height where fading fruits start to be visible", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> FadeInHeightOffset { get; } = new BindableDouble(0.86)
        {
            MinValue = 0.60,
            MaxValue = 0.86,
            Precision = 0.01,
        };

        public override double ScoreMultiplier => 1.09 - 0.07 * ((FadeInHeightOffset.Value - 0.60) / 0.26) - 0.01 * (Math.Abs(FadeInDurationMultiplier.Value - 0.50) / 0.20);


        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = true;
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
                        fadeInHitObject(nestedCatchDrawable);
                }
            }
            else
                fadeInHitObject(catchDrawable);
        }

        private void fadeInHitObject(DrawableCatchHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            double offset = hitObject.TimePreempt * FadeInHeightOffset.Value;
            double duration = hitObject.TimePreempt * FadeInDurationMultiplier.Value;

            drawable.FadeOut(0);

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offset))
                drawable.FadeIn(duration);

        }
    }
}
