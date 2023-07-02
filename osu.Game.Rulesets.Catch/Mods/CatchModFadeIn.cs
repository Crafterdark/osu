// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFadeIn : ModHidden
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.DifficultyIncrease;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModHidden)).ToArray();

        public override LocalisableString Description => @"Play with fading fruits.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        private const double fade_in_offset_multiplier = 0.76;
        private const double fade_in_duration_multiplier = 0.6;

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

            double offset = hitObject.TimePreempt * fade_in_offset_multiplier;
            double duration = offset - hitObject.TimePreempt * fade_in_duration_multiplier;
            using (drawable.BeginAbsoluteSequence(0))
                drawable.FadeOut(0);
            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offset))
                drawable.FadeIn(duration);
        }
    }
}
