// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFadeIn : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToScoreProcessor, IUpdatableByPlayfield
    {
        public override LocalisableString Description => @"Fruits appear out of nowhere.";
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {
            typeof(CatchModHidden),
            typeof(CatchModFlashlight),
        }).ToArray();

        public CatchModFadeIn()
        {
            Visibility.BindValueChanged(e =>
            {
                if (e.NewValue < 1 && ComboBasedSize.Value)
                    ComboBasedSize.Value = false;
                else if (e.NewValue == 1 && !ComboBasedSize.Value)
                    ComboBasedSize.Value = true;
            });

            ComboBasedSize.BindValueChanged(e =>
            {
                if (e.NewValue == true && Visibility.Value < 1)
                    Visibility.Value = 1;
                else if (e.NewValue == false && Visibility.Value == 1)
                    Visibility.Value = 0.9;
            });
        }

        [SettingSource("Visibility", "The percentage of playfield height that will be visible.", SettingControlType = typeof(SettingsSlider<double, FadeInVisibilitySlider>))]
        public BindableNumber<double> Visibility { get; } = new BindableDouble(1)
        {
            MinValue = 0.5,
            MaxValue = 1,
            Precision = 0.1,
        };

        [SettingSource("Change size based on combo", "Reduces visibility as combo increases.")]
        public BindableBool ComboBasedSize { get; } = new BindableBool(true);

        private double currentVisibility { get; set; }
        private double finalVisibility { get; set; }

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();

        private const float fade_in_duration_multiplier = 0.24f;

        public const int COMBO_SCALING = 150;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;

            if (ComboBasedSize.Value)
            {
                double mapApproachRate = drawableCatchRuleset.Beatmap.Difficulty.ApproachRate;

                //The final value of visibility that we are enforcing to low approach rate maps.
                finalVisibility = getFinalVisibilityValue(mapApproachRate, getTargetApproachRate(mapApproachRate));
            }
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (!ComboBasedSize.Value)
            {
                currentVisibility = Visibility.Value;
                finalVisibility = Visibility.Value;
                return;
            }

            double comboBasedVisibility = 1d - finalVisibility;

            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(combo =>
            {
               currentVisibility = 1d - comboBasedVisibility * Math.Clamp((double)combo.NewValue / COMBO_SCALING, 0, 1);
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
            double offsetBeforeFading = hitObject.TimePreempt * currentVisibility;
            double offsetAfterFullyVisible = hitObject.TimePreempt * fade_in_duration_multiplier * currentVisibility;
            double timeBeforeFading = hitObject.StartTime - offsetBeforeFading;

            //If we are during the fade in and if the hitobject is still not hit
            if (timeBeforeFading <= cpf.Time.Current && hitTime > cpf.Time.Current)
            {
                // Main difference between Fade In Mod and Hidden Mod implementations:
                // The actual visibility state of the hitobject depends on the current fade in offset and duration
                if (timeBeforeFading + offsetAfterFullyVisible >= cpf.Time.Current)
                {
                    if (offsetAfterFullyVisible > 0)
                        drawable.FadeTo((float)((cpf.Time.Current - timeBeforeFading) / offsetAfterFullyVisible), 0);

                    //Should be impossible, just in case
                    else
                        drawable.FadeTo(1, 0);
                }
                //This is necessary for changes of Fade In offset (It's not just a safety)
                else
                    drawable.FadeTo(1, 0);
            }
            //Only if we are earlier than the fade in: We don't want to see hitobjects
            else if (timeBeforeFading > cpf.Time.Current)
            {
                drawable.FadeTo(0, 0);
            }
        }

        private double getFinalVisibilityValue(double mapAr, double targetAr)
        {
            double mapApproachRateTime = (float)IBeatmapDifficultyInfo.DifficultyRange(mapAr, CatchHitObject.PREEMPT_MAX, CatchHitObject.PREEMPT_MID, CatchHitObject.PREEMPT_MIN);
            double mapApproachRateTimeTarget = (float)IBeatmapDifficultyInfo.DifficultyRange(targetAr, CatchHitObject.PREEMPT_MAX, CatchHitObject.PREEMPT_MID, CatchHitObject.PREEMPT_MIN);

            return mapApproachRateTimeTarget / mapApproachRateTime;
        }

        private double getTargetApproachRate(double mapAr)
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

    public partial class FadeInVisibilitySlider : RoundedSliderBar<double>
    {
        public override LocalisableString TooltipText => Current.Value == 1 ? "variable" : base.TooltipText;
    }
}
