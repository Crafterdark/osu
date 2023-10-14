// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using System;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Mods.Debug_Mods
{
    public class CatchModFadeIn : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToScoreProcessor
    {
        public override LocalisableString Description => @"Play with fading fruits.";
        public override string Name => "Fade In";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        private const double fade_in_offset_multiplier = 0.76;
        private const double fade_in_duration_multiplier = 0.60;
        public override string Acronym => "FI";
        public override ModType Type => ModType.DifficultyIncrease;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModHidden), typeof(CatchModFlashlight) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = true;

        }

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
                        fadeInHitObject(nestedCatchDrawable);
                }
            }
            else
                fadeInHitObject(catchDrawable);
        }

        private void fadeInHitObject(DrawableCatchHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            bool isLowApproachRate = hitObject.TimePreempt >= 1200 ? true : false;

            double lanecover_multiplier = 1.0;
            if (isLowApproachRate) lanecover_multiplier = 4.0 / 7.0;

            double offset = hitObject.TimePreempt * (fade_in_offset_multiplier * lanecover_multiplier);
            double duration = offset - hitObject.TimePreempt * (fade_in_duration_multiplier * lanecover_multiplier);

            //Instant fade out
            drawable.FadeOut(0);

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offset))
                drawable.FadeIn(duration);

        }


        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            // Default value of ScoreProcessor's Rank in Fade In Mod should be SS+
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
