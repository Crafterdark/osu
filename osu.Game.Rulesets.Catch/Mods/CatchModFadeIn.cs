// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
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

        [SettingSource("Fade In Distance", "The distance to apply the fade in")]
        public BindableFloat SizeMultiplier { get; } = new BindableFloat(0.76f)
        {
            MinValue = 0.50f,
            MaxValue = 0.76f,
            Precision = 0.01f
        };

        private const double fade_in_duration_multiplier = 0.16; //Maybe make this value lower?

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

            double offset = hitObject.TimePreempt * SizeMultiplier.Value;
            double duration = offset - hitObject.TimePreempt * fade_in_duration_multiplier;
            double initialFadeOutTime = -1 * offset; //Seems necessary for low AR

            using (drawable.BeginAbsoluteSequence(initialFadeOutTime))
                drawable.FadeOut(0);
            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offset))
                drawable.FadeIn(duration);
        }
    }
}
