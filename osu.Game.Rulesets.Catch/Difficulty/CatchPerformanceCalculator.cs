// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Catch.Mods.DebugMods;
using osu.Game.Rulesets.Catch.Mods.DebugMods.Utility;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchPerformanceCalculator : PerformanceCalculator
    {
        private int fruitsHit;
        private int ticksHit;
        private int tinyTicksHit;
        private int tinyTicksMissed;
        private int misses;

        public CatchPerformanceCalculator()
            : base(new CatchRuleset())
        {
        }

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var catchAttributes = (CatchDifficultyAttributes)attributes;

            fruitsHit = score.Statistics.GetValueOrDefault(HitResult.Great);
            ticksHit = score.Statistics.GetValueOrDefault(HitResult.LargeTickHit);
            tinyTicksHit = score.Statistics.GetValueOrDefault(HitResult.SmallTickHit);
            tinyTicksMissed = score.Statistics.GetValueOrDefault(HitResult.SmallTickMiss);
            misses = score.Statistics.GetValueOrDefault(HitResult.Miss);

            // We are heavily relying on aim in catch the beat
            double value = Math.Pow(5.0 * Math.Max(1.0, catchAttributes.StarRating / 0.0049) - 4.0, 2.0) / 100000.0;

            // Longer maps are worth more. "Longer" means how many hits there are which can contribute to combo
            double lengthBonus = CurrentSystemLengthBonus(totalComboHits());

            value *= lengthBonus;

            value *= Math.Pow(0.97, misses);

            // Combo scaling
            if (catchAttributes.MaxCombo > 0)
                value *= Math.Min(Math.Pow(score.MaxCombo, 0.8) / Math.Pow(catchAttributes.MaxCombo, 0.8), 1.0);

            double approachRate = catchAttributes.ApproachRate;
            double approachRateFactor = 1.0;

            // Fade In must adjust the approach rate
            if (score.Mods.Any(m => m is CatchModFadeIn))
                approachRate = ExperimentalSystemOneFadeInAdjust(catchAttributes, score);

            if (approachRate > 9.0)
                approachRateFactor += 0.1 * (approachRate - 9.0); // 10% for each AR above 9
            if (approachRate > 10.0)
                approachRateFactor += 0.1 * (approachRate - 10.0); // Additional 10% at AR 11, 30% total
            else if (approachRate < 8.0)
                approachRateFactor += 0.025 * (8.0 - approachRate); // 2.5% for each AR below 8

            value *= approachRateFactor;

            if (score.Mods.Any(m => m is ModHidden))
            {
                // Hiddens gives almost nothing on max approach rate, and more the lower it is
                if (approachRate <= 10.0)
                    value *= 1.05 + 0.075 * (10.0 - approachRate); // 7.5% for each AR below 10
                else if (approachRate > 10.0)
                    value *= 1.01 + 0.04 * (11.0 - Math.Min(11.0, approachRate)); // 5% at AR 10, 1% at AR 11
            }

            if (score.Mods.Any(m => m is ModFlashlight))
                value *= ExperimentalSystemOneFlashlightBonus(score, lengthBonus);

            value *= Math.Pow(accuracy(), 5.5);

            if (score.Mods.Any(m => m is ModNoFail))
                value *= 0.90;

            return new CatchPerformanceAttributes
            {
                Total = value
            };
        }

        public double CurrentSystemLengthBonus(int numTotalHits)
        {
            double lengthBonus = 0.95 + 0.3 * Math.Min(1.0, numTotalHits / 2500.0) + (numTotalHits > 2500 ? Math.Log10(numTotalHits / 2500.0) * 0.475 : 0.0);

            return lengthBonus;
        }

        public double CurrentSystemFlashlightBonus(double lengthBonus)
        {
            return 1.35 * lengthBonus;
        }

        public double ExperimentalSystemOneLengthBonus(ScoreInfo score)
        {
            double drainTime = score.BeatmapInfo == null ? 0 : score.BeatmapInfo.Length / 1000;

            // Check if there's any rate adjust mod to for reducing or increasing drainTime
            for (int index = 0; index < score.Mods.Length; index++)
            {
                if (score.Mods[index] is ModRateAdjust modRa)
                {
                    drainTime /= modRa.SpeedChange.Value;
                    break;
                }
            }

            //Most TV size ranked songs are within the 90 seconds range
            const double short_length = 90.0;

            //If it is less than 1, it is short (linear), otherwise it is long (logarithm)
            double lengthFactor = drainTime / short_length;

            const double long_length_bonus = 0.475;

            double lengthBonus =
                0.95 + CalculateComboRatio(score) * (0.05 * Math.Min(1.0, lengthFactor) + (lengthFactor > 1.0 ? Math.Log10(lengthFactor) * long_length_bonus : 0.0));

            //Logger.Log("drainTime: " + drainTime);
            //Logger.Log("lengthFactor: " + lengthFactor);
            //Logger.Log("lengthBonus: " + lengthBonus);
            //Logger.Log("comboRatio: " + comboRatio);
            return lengthBonus;
        }

        public double ExperimentalSystemOneFlashlightBonus(ScoreInfo score, double lengthBonus)
        {
            //float currentFlashSize = GetFlashlightSize(score.MaxCombo);
            double currentFlashSizeScaleFactor;

            int maxComboFromMap = GetMaximumComboFromMap(score);

            //Weighted Minimum Flash Size Scale Factor
            int minCombo = Math.Max(0, Math.Min(99, score.MaxCombo));
            double weightMinScaleFactor = (double)minCombo / maxComboFromMap;

            //Weighted Mid Flash Size Scale Factor
            int midCombo = Math.Max(0, Math.Min(199, score.MaxCombo) - 99);
            double weightMidScaleFactor = (double)midCombo / maxComboFromMap;

            //Weighted Max Flash Size Scale Factor
            int maxCombo = Math.Max(0, score.MaxCombo - 199);
            double weightMaxScaleFactor = (double)maxCombo / maxComboFromMap;

            //Weighted Remaining Flash Size Scale Factor
            int remCombo = totalSuccessfulComboHits() - score.MaxCombo;
            double weightRemScaleFactor = (double)Math.Max(0, remCombo) / maxComboFromMap * GetFlashlightScaleFactor(GetFlashlightSize(99));

            currentFlashSizeScaleFactor = (double)(weightMinScaleFactor * GetFlashlightScaleFactor(GetFlashlightSize(99)) + weightMidScaleFactor * GetFlashlightScaleFactor(GetFlashlightSize(199)) + weightMaxScaleFactor * GetFlashlightScaleFactor(GetFlashlightSize(200)) + weightRemScaleFactor * GetFlashlightScaleFactor(GetFlashlightSize(99)));

            //Logger.Log("scaleMin: " + GetFlashlightScaleFactor(GetFlashlightSize(99)));
            //Logger.Log("scaleMid: " + GetFlashlightScaleFactor(GetFlashlightSize(199)));
            //Logger.Log("scaleMax: " + GetFlashlightScaleFactor(GetFlashlightSize(200)));
            //Logger.Log("weightMin: " + weightMinScaleFactor);
            //Logger.Log("weightMid: " + weightMidScaleFactor);
            //Logger.Log("weightMax: " + weightMaxScaleFactor);
            //Logger.Log("weightRem: " + weightRemScaleFactor);
            //Logger.Log("weightTot: " + currentFlashSizeScaleFactor);
            //Logger.Log("flashBonus: " + (1.00 + 0.35 * currentFlashSizeScaleFactor));
            return (1.00 + 0.35 * currentFlashSizeScaleFactor) * lengthBonus;
        }

        public double ExperimentalSystemOneFadeInAdjust(CatchDifficultyAttributes catchAttributes, ScoreInfo score)
        {
            double mapAr = catchAttributes.ApproachRate;
            double initialVisibility = catchAttributes.InitialVisibility;
            double finalVisibility = CatchModFadeIn.GetFinalVisibilityValue(mapAr, CatchModFadeIn.GetTargetApproachRate(mapAr), catchAttributes.InitialVisibility);

            double minAr = CatchUtilityForMods.TimeToApproachRate(CatchUtilityForMods.ApproachRateToTime(mapAr) * initialVisibility);
            double maxAr = CatchUtilityForMods.TimeToApproachRate(CatchUtilityForMods.ApproachRateToTime(mapAr) * finalVisibility);

            int maxComboFromMap = GetMaximumComboFromMap(score);

            //Weighted Fade In Maximum AR
            double weightMaxAr = (double)Math.Max(0, score.MaxCombo - 150) / maxComboFromMap * maxAr;

            //Weighted Fade In Increasing AR
            double weightIncrAr = (double)Math.Min(150, score.MaxCombo) / maxComboFromMap * ((minAr + maxAr) / 2);

            //Weighted Fade In Minimum AR
            double weightMinAr = (double)Math.Max(0, maxComboFromMap - score.MaxCombo) / maxComboFromMap * minAr;

            double weightTotalAr = weightMaxAr + weightIncrAr + weightMinAr;

            //Logger.Log("Map AR: " + mapAr);
            //Logger.Log("weightMaxAR: " + weightMaxAr);
            //Logger.Log("weightIncrAR: " + weightIncrAr);
            //Logger.Log("weightMinAR: " + weightMinAr);
            //Logger.Log("weightTotalAR: " + weightTotalAr);
            //Logger.Log("Initial Visibility: " + catchAttributes.InitialVisibility);
            //Logger.Log("Final Visibility: " + finalVisibility);
            //Logger.Log("Current Combo: " + score.Combo);
            return weightTotalAr;
        }

        // Calculate the combo ratio (current combo / maximum combo)
        public double CalculateComboRatio(ScoreInfo score)
        {
            int maxCombo = GetMaximumComboFromMap(score);

            //There's no combo yet
            if (maxCombo <= 0)
                return 0;

            return (double)totalSuccessfulComboHits() / maxCombo;
        }

        public int GetMaximumComboFromMap(ScoreInfo score)
        {
            int maxfruitsHit = score.MaximumStatistics.GetValueOrDefault(HitResult.Great);
            int maxticksHit = score.MaximumStatistics.GetValueOrDefault(HitResult.LargeTickHit);

            return maxfruitsHit + maxticksHit;
        }

        public float GetFlashlightSize(int combo)
        {
            if (combo >= 100 && combo < 200)
                return 0.885f;
            else if (combo >= 200)
                return 0.770f;
            else
                return
                    1.0f;
        }

        public double GetFlashlightScaleFactor(float currentFlashSize)
        {
            if (currentFlashSize == 1.0f)
                return (double)1 / 3;
            else if (currentFlashSize == 0.885f)
                return (double)1 / 2;
            else
                return 1;
        }

        private double accuracy() => totalHits() == 0 ? 0 : Math.Clamp((double)totalSuccessfulHits() / totalHits(), 0, 1);
        private int totalHits() => tinyTicksHit + ticksHit + fruitsHit + misses + tinyTicksMissed;
        private int totalSuccessfulHits() => tinyTicksHit + ticksHit + fruitsHit;
        private int totalComboHits() => misses + ticksHit + fruitsHit;
        private int totalSuccessfulComboHits() => ticksHit + fruitsHit;
    }
}
