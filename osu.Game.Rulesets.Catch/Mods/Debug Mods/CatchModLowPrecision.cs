// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModLowPrecision : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDifficulty
    {
        //(1.00 - 0.75 * slider ) score, min(OD, 10 * (1-slider)), slider dependency -> rescale maximum fruit hitbox with the slider value.
        public override string Name => "Low Precision";

        public override string Acronym => "LP";

        public override LocalisableString Description => "Less precision required. Everything becomes easier to catch...";

        public override ModType Type => ModType.DifficultyReduction;

        [SettingSource("Maximum leniency", "The maximum leniency to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> Leniency { get; } = new BindableDouble(1.00)
        {
            MinValue = 0.01,
            MaxValue = 1.00,
            Precision = 0.01,
        };

        public override double ScoreMultiplier => 1.00 - Leniency.Value * 0.50;

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            //OverallDifficulty will go [OD/2 -> OD] based on the leniency.
            difficulty.OverallDifficulty = (float)(difficulty.OverallDifficulty * (1 - Leniency.Value / 2));
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitLeniency = true;

            catchPlayfield.Catcher.CatchLeniencySlider = Leniency.Value;

        }

        public static double CalculateHalfLeniencyDistanceForHitObject(HitObject hitObject, double leniencySliderValue)
        {
            double rescale_factor = 0;

            if (hitObject is Fruit)
            {
                rescale_factor = 1;
            }

            if (hitObject is Droplet)
            {
                rescale_factor = 0.8;
            }

            if (hitObject is Banana)
            {
                rescale_factor = 0.6;
            }

            if (hitObject is TinyDroplet)
            {
                rescale_factor = 0.4;
            }

            double difficultyValue = 10 * (1 - leniencySliderValue);

            //160 is the current maximum size of fruits.
            return (double)Math.Abs(difficultyValue - 10) / 10 * ((CatchHitObject)hitObject).Scale * rescale_factor * (160 / 2);

        }

    }
}
