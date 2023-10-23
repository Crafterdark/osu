// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Framework.Graphics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Catch.Mods.DebugMods.Utility;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModFadeIn : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToScoreProcessor, IUpdatableByPlayfield
    {
        public override LocalisableString Description => @"Fruits appear out of nowhere.";
        public override string Name => "Fade In";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        [SettingSource("Visibility", "The percentage of playfield height that will be visible.", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> InitialVisibility { get; } = new BindableDouble(0.9)
        {
            MinValue = 0.5,
            MaxValue = 0.9,
            Precision = 0.1,
        };

        public BindableNumber<double> CurrentVisibility { get; set; } = new BindableNumber<double>();
        public BindableNumber<double> FinalVisibility { get; set; } = new BindableNumber<double>();

        [SettingSource("Change size based on combo", "Reduces visibility as combo increases.")]
        public BindableBool ComboBasedSize { get; } = new BindableBool(true);

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();

        private const float fade_in_duration_multiplier = 0.24f;

        public const int COMBO_SCALING = 150;
        public override string Acronym => "FI";
        public override ModType Type => ModType.DifficultyIncrease;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModHidden), typeof(CatchModFlashlight) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = true;

            double mapApproachRate = drawableCatchRuleset.Beatmap.Difficulty.ApproachRate;

            //The final value of visibility that we are enforcing to low approach rate maps
            FinalVisibility.Value = GetFinalVisibilityValue(mapApproachRate, GetTargetApproachRate(mapApproachRate), InitialVisibility.Value);

            //Logger.Log("ApproachRate " + drawableCatchRuleset.Beatmap.Difficulty.ApproachRate);
            //Logger.Log("Initial Visibility " + InitialVisibility.Value);
            //Logger.Log("Final Visibility " + FinalVisibility);
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            // Default value of ScoreProcessor's Rank in Hidden Mod should be SS+
            scoreProcessor.Rank.Value = ScoreRank.XH;

            if (!ComboBasedSize.Value)
            {
                CurrentVisibility.Value = InitialVisibility.Value;
                return;
            }

            double comboBasedDiffVisibility = InitialVisibility.Value - FinalVisibility.Value;

            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(combo =>
            {
                if (combo.NewValue <= COMBO_SCALING)
                    CurrentVisibility.Value = InitialVisibility.Value - (comboBasedDiffVisibility * combo.NewValue / COMBO_SCALING);
            }, true);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }

        public void Update(Playfield playfield)
        {
            CatchPlayfield cpf = (CatchPlayfield)playfield;

            foreach (DrawableHitObject hitObject in cpf.AllHitObjects)
            {
                if (!(hitObject is DrawableCatchHitObject))
                    return;

                if (hitObject.NestedHitObjects.Any())
                {
                    foreach (var nestedDrawable in hitObject.NestedHitObjects)
                    {
                        if (nestedDrawable is DrawableCatchHitObject nestedCatchDrawable)
                            fadeInHitObject(nestedCatchDrawable, cpf);
                    }
                }

                else
                    fadeInHitObject((DrawableCatchHitObject)hitObject, cpf);
            }
        }

        private void fadeInHitObject(DrawableCatchHitObject drawable, CatchPlayfield cpf)
        {
            CatchHitObject hitObject = drawable.HitObject;

            double hitTime = hitObject.StartTime;
            double offsetBeforeFading = hitObject.TimePreempt * CurrentVisibility.Value;
            double offsetAfterFullyVisible = hitObject.TimePreempt * fade_in_duration_multiplier * CurrentVisibility.Value;

            //If we are during the fade in and if the hitobject is still not hit
            if (hitTime - offsetBeforeFading <= cpf.Time.Current && hitTime > cpf.Time.Current)
            {
                // Main difference between Fade In Mod and Hidden Mod implementations:
                // The actual visibility state of the hitobject depends on the current fade in offset and duration
                if ((hitTime - offsetBeforeFading) + offsetAfterFullyVisible >= cpf.Time.Current)
                {
                    if (offsetAfterFullyVisible > 0)
                        drawable.FadeTo((float)((cpf.Time.Current - (hitTime - offsetBeforeFading)) / offsetAfterFullyVisible), 0);

                    //Should be impossible, just in case
                    else
                        drawable.FadeTo(1, 0);
                }
                //This is necessary for changes of Fade In offset (It's not just a safety)
                else
                    drawable.FadeTo(1, 0);
            }
            //Only if we are earlier than the fade in: We don't want to see hitobjects
            else if (hitTime - offsetBeforeFading > cpf.Time.Current)
            {
                drawable.FadeTo(0, 0);
            }
        }

        public static double GetFinalVisibilityValue(double mapAr, double targetAr, double initialVisibility)
        {
            double mapApproachRateTime = CatchUtilityForMods.ApproachRateToTime(mapAr);

            double mapApproachRateTimeTarget = CatchUtilityForMods.ApproachRateToTime(targetAr);

            double finalVisibility = mapApproachRateTimeTarget / mapApproachRateTime;

            if (finalVisibility > initialVisibility)
                return initialVisibility;

            //Logger.Log("Final Visibility After Time" + FinalVisibility);

            return finalVisibility;
        }

        public static double GetTargetApproachRate(double mapAr)
        {
            //Usually from Overdose+/Top Diffs and above
            if (mapAr > 9.4)
                return 10.5f;

            //Usually from Rain and above
            else if (mapAr <= 9.4 && mapAr > 8.6)
                return 10.0f;

            //Most Platter belong here
            else if (mapAr <= 8.6 && mapAr > 7)
                return 9.0f;

            //Most Cup/Salad belong here
            else if (mapAr <= 7 && mapAr > 5)
                return 8.5f;

            //This range considers the usage of EZ
            else
                return 8.0f;
        }
    }
}
