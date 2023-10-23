// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Logging;
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
        private double comboRatio;

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

            // Calculate the combo ratio (current combo / maximum combo)
            comboRatio = CalculateComboRatio(score);

            // We are heavily relying on aim in catch the beat
            double value = Math.Pow(5.0 * Math.Max(1.0, catchAttributes.StarRating / 0.0049) - 4.0, 2.0) / 100000.0;

            // Longer maps are worth more. "Longer" means how many hits there are which can contribute to combo
            //int numTotalHits = totalComboHits();

            double lengthBonus = NewReworkLengthBonus(score);
            //double lengthBonus = OldReworkLengthBonus(numTotalHits);

            value *= lengthBonus;

            value *= Math.Pow(0.97, misses);

            // Combo scaling
            if (catchAttributes.MaxCombo > 0)
                value *= Math.Min(Math.Pow(score.MaxCombo, 0.8) / Math.Pow(catchAttributes.MaxCombo, 0.8), 1.0);

            double approachRate = catchAttributes.ApproachRate;
            double approachRateFactor = 1.0;

            // Fade In must adjust the approach rate
            if (score.Mods.Any(m => m is CatchModFadeIn))
            {
                double mapAR = catchAttributes.ApproachRate;
                double initialVisibility = catchAttributes.InitialVisibility;
                double finalVisibility = CatchModFadeIn.GetFinalVisibilityValue(mapAR, CatchModFadeIn.GetTargetApproachRate(mapAR), catchAttributes.InitialVisibility);

                double minAR = CatchUtilityForMods.TimeToApproachRate(CatchUtilityForMods.ApproachRateToTime(mapAR) * initialVisibility);
                double maxAR = CatchUtilityForMods.TimeToApproachRate(CatchUtilityForMods.ApproachRateToTime(mapAR) * finalVisibility);

                int maxComboFromMap = GetMaximumComboFromMap(score);

                //Weighted Fade In Maximum AR
                double weightMaxAR = (double)Math.Max(0, score.MaxCombo - 150) / maxComboFromMap * maxAR;

                //Weighted Fade In Increasing AR
                double weightIncrAR = (double)Math.Min(150, score.MaxCombo) / maxComboFromMap * ((minAR + maxAR) / 2);

                //Weighted Fade In Minimum AR
                double weightMinAR = (double)Math.Max(0, maxComboFromMap - score.MaxCombo) / maxComboFromMap * minAR;

                double weightTotalAR = weightMaxAR + weightIncrAR + weightMinAR;

                Logger.Log("Map AR: " + mapAR);

                Logger.Log("weightMaxAR: " + weightMaxAR);
                Logger.Log("weightIncrAR: " + weightIncrAR);
                Logger.Log("weightMinAR: " + weightMinAR);

                Logger.Log("weightTotalAR: " + weightTotalAR);

                approachRate = weightTotalAR;

                //Logger.Log("Initial Visibility: " + catchAttributes.InitialVisibility);
                //Logger.Log("Final Visibility: " + finalVisibility);
                //Logger.Log("Current Combo: " + score.Combo);
                //Logger.Log("Current Visibility: " + currentVisibility);
            }

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
                value *= 1.35 * lengthBonus;

            value *= Math.Pow(accuracy(), 5.5);

            if (score.Mods.Any(m => m is ModNoFail))
                value *= 0.90;

            return new CatchPerformanceAttributes
            {
                Total = value
            };
        }

        public double OldReworkLengthBonus(int numTotalHits)
        {
            double lengthBonus = 0.95 + 0.3 * Math.Min(1.0, numTotalHits / 2500.0) + (numTotalHits > 2500 ? Math.Log10(numTotalHits / 2500.0) * 0.475 : 0.0);

            return lengthBonus;
        }

        public double NewReworkLengthBonus(ScoreInfo score)
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
                0.95 + comboRatio * (0.05 * Math.Min(1.0, lengthFactor) + (lengthFactor > 1.0 ? Math.Log10(lengthFactor) * long_length_bonus : 0.0));

            //Logger.Log("drainTime: " + drainTime);
            //Logger.Log("lengthFactor: " + lengthFactor);
            //Logger.Log("lengthBonus: " + lengthBonus);
            //Logger.Log("comboRatio: " + comboRatio);

            return lengthBonus;
        }

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

        private double accuracy() => totalHits() == 0 ? 0 : Math.Clamp((double)totalSuccessfulHits() / totalHits(), 0, 1);
        private int totalHits() => tinyTicksHit + ticksHit + fruitsHit + misses + tinyTicksMissed;
        private int totalSuccessfulHits() => tinyTicksHit + ticksHit + fruitsHit;
        private int totalComboHits() => misses + ticksHit + fruitsHit;
        private int totalSuccessfulComboHits() => ticksHit + fruitsHit;
    }
}
