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
//using osu.Framework.Logging;

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

        public float CurrentVisibility { get; set; }
        public float FinalVisibility { get; set; }

        [SettingSource("Change size based on combo", "Reduces visibility as combo increases. (Up to Approach Rate 9)")]
        public BindableBool ComboBasedSize { get; } = new BindableBool(true);

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();

        private const float fade_in_duration_multiplier = 0.24f;
        private float target_approach_rate = 9.0f;
        private int combo_scaling = 150;
        public override string Acronym => "FI";
        public override ModType Type => ModType.DifficultyIncrease;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModHidden), typeof(CatchModFlashlight) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = true;

            float mapApproachRate = drawableCatchRuleset.Beatmap.Difficulty.ApproachRate;

            //Usually from Overdose+/Top Diffs and above
            if (mapApproachRate > 9.4)
                target_approach_rate = 10.5f;

            //Usually from Rain and above
            else if (mapApproachRate <= 9.4 && mapApproachRate > 8.6)
                target_approach_rate = 10.0f;

            //Most Platter belong here
            else if (mapApproachRate <= 8.6 && mapApproachRate > 7)
                target_approach_rate = 9.0f;

            //Most Cup/Salad belong here
            else if (mapApproachRate <= 7 && mapApproachRate > 5)
                target_approach_rate = 8.5f;

            //This range considers the usage of EZ
            else
                target_approach_rate = 8.0f;

            float mapApproachRateTime = (float)CatchUsefulForMods.ApproachRateToTime(mapApproachRate);

            float mapApproachRateTimeTarget = (float)CatchUsefulForMods.ApproachRateToTime(target_approach_rate);


            //The final value of visibility that we are enforcing to low approach rate maps
            FinalVisibility = mapApproachRateTimeTarget / mapApproachRateTime;

            //Logger.Log("Final Visibility After Time" + FinalVisibility);

            if (FinalVisibility > InitialVisibility.Value)
                FinalVisibility = (float)InitialVisibility.Value;

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
                CurrentVisibility = (float)InitialVisibility.Value;
                return;
            }

            float ComboBasedDiffVisibility = (float)(InitialVisibility.Value - FinalVisibility);

            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(combo =>
            {
                if (combo.NewValue <= combo_scaling)
                    CurrentVisibility = (float)InitialVisibility.Value - (ComboBasedDiffVisibility * combo.NewValue / combo_scaling);
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

                if (!(hitObject is DrawableCatchHitObject catchDrawable))
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
            double offset_before_fading = hitObject.TimePreempt * CurrentVisibility;
            double offset_after_fully_visible = hitObject.TimePreempt * fade_in_duration_multiplier * CurrentVisibility;

            //If we are during the fade in and if the hitobject is still not hit
            if (hitTime - offset_before_fading <= cpf.Time.Current && hitTime > cpf.Time.Current)
            {
                // Main difference between Fade In Mod and Hidden Mod implementations:
                // The actual visibility state of the hitobject depends on the current fade in offset and duration
                if ((hitTime - offset_before_fading) + offset_after_fully_visible >= cpf.Time.Current)
                {
                    if (offset_after_fully_visible > 0)
                        drawable.FadeTo((float)((cpf.Time.Current - (hitTime - offset_before_fading)) / offset_after_fully_visible), 0);

                    //Should be impossible, just in case
                    else
                        drawable.FadeTo(1, 0);
                }
                //This is necessary for changes of Fade In offset (It's not just a safety)
                else
                    drawable.FadeTo(1, 0);
            }
            //Only if we are earlier than the fade in: We don't want to see hitobjects
            else if (hitTime - offset_before_fading > cpf.Time.Current)
            {
                drawable.FadeTo(0, 0);
            }

        }

    }
}
