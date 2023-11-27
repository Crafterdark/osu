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

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModLowPrecision : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDifficulty
    {
        public override string Name => "Low Precision";

        public override string Acronym => "LP";

        public override LocalisableString Description => "Less precision required. Everything becomes easier to catch...";

        public override ModType Type => ModType.DifficultyReduction;

        //Leniency slider dependency -> rescale the current fruit hitbox with the leniency slider value.

        [SettingSource("Maximum Leniency", "The catch leniency to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> Leniency { get; } = new BindableDouble(1.0)
        {
            MinValue = 0.1,
            MaxValue = 1.0,
            Precision = 0.1,
        };

        public override double ScoreMultiplier => 1.00 - (Leniency.Value * 0.50);

        //Current maximum allowed size of fruits.

        public const int MAX_HITBOX_FRUIT = 160;

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            //OverallDifficulty will go [OD/4 -> OD] based on the leniency.
            difficulty.OverallDifficulty = (float)(difficulty.OverallDifficulty * (1 - Leniency.Value * 0.50));
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
            double rescaleFactor = 0;

            if (hitObject is Fruit)
            {
                rescaleFactor = 1;
            }

            if (hitObject is Droplet)
            {
                rescaleFactor = 0.8;
            }

            if (hitObject is Banana)
            {
                rescaleFactor = 0.6;
            }

            if (hitObject is TinyDroplet)
            {
                rescaleFactor = 0.4;
            }

            double difficultyValue = 10 * (1 - leniencySliderValue);

            return Math.Abs(difficultyValue - 10) / 10 * ((CatchHitObject)hitObject).Scale * rescaleFactor * (MAX_HITBOX_FRUIT / 2.0);
        }
    }
}
