// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;

namespace osu.Game.Rulesets.Catch.Mods.SharedMods
{
    public class CatchSharedModVariables
    {

        public static bool[] VisibilityArray = new bool[2];
        public static double SharedFadeInDistance { get; set; }
        public static double SharedFadeInDuration { get; set; }
        public static double SharedHiddenDistance { get; set; }
        public static double SharedHiddenDuration { get; set; }

        public static void UpdateSharedMods(IBeatmap beatmap)
        {
            var catchBeatmap = (CatchBeatmap)beatmap;
            VisibilityArray[(int)EnumMods.FadeIn] = catchBeatmap.CatchModFadeInApplied;
            VisibilityArray[(int)EnumMods.Hidden] = catchBeatmap.CatchModHiddenApplied;
        }


        public enum EnumMods
        {
            Hidden,
            FadeIn
        }


    }
}
