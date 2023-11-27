﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods.Utility
{
    public class CatchUtilityForMods
    {
        public static double ApproachRateToTime(double ar) => IBeatmapDifficultyInfo.DifficultyRange(ar, 1800, 1200, 450);

        public static double TimeToApproachRate(double time)
        {
            //Less than AR5
            if (time > 1200) return 15.0d - time / 120;

            //Higher than AR5
            if (time < 1200) return 13.0d - time / 150;

            //AR 5
            return 5.0d;
        }

        public static float GetPlayfieldWidth(float playfieldCompressionFactor)
        {
            return CatchPlayfield.WIDTH * playfieldCompressionFactor;
        }

        public static float GetMinPlayfieldWidth(float playfieldCompressionFactor)
        {
            return (CatchPlayfield.WIDTH / 2) - (CatchPlayfield.WIDTH * playfieldCompressionFactor / 2);
        }

        public static float GetMaxPlayfieldWidth(float playfieldCompressionFactor)
        {
            return (CatchPlayfield.WIDTH / 2) + (CatchPlayfield.WIDTH * playfieldCompressionFactor / 2);
        }
    }
}
