// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Mods.SharedMods
{
    public abstract class CatchSharedModVisibility : ModWithVisibilityAdjustment, IApplicableToScoreProcessor
    {

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
                        applyVisibilityModToHitObject(nestedCatchDrawable);
                }
            }
            else
                applyVisibilityModToHitObject(catchDrawable);
        }

        private static void applyVisibilityModToHitObject(DrawableCatchHitObject drawable)
        {

            var hitObject = drawable.HitObject;

            if (CatchSharedModVariables.VisibilityArray[(int)CatchSharedModVariables.EnumMods.FadeIn])
            {

                double offsetFI = hitObject.TimePreempt * CatchSharedModVariables.SharedFadeInDistance;
                double durationFI = hitObject.TimePreempt * CatchSharedModVariables.SharedFadeInDuration;

                drawable.FadeOut(0);

                using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offsetFI))
                    drawable.FadeIn(durationFI);

            }


            if (CatchSharedModVariables.VisibilityArray[(int)CatchSharedModVariables.EnumMods.Hidden])
            {
                double offsetHD = hitObject.TimePreempt * CatchSharedModVariables.SharedHiddenDistance;
                double durationHD = hitObject.TimePreempt * CatchSharedModVariables.SharedHiddenDuration;

                using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offsetHD))
                    drawable.FadeOut(durationHD);
            }
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            // Default value of ScoreProcessor's Rank in Hidden Mod should be SS+
            scoreProcessor.Rank.Value = ScoreRank.XH;
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
    }
}
