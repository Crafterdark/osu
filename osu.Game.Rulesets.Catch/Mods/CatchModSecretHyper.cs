// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSecretHyper : Mod, IApplicableToDrawableHitObject
    {
        public override LocalisableString Description => @"Hyperdash effect on fruits is no longer visible...";
        public override string Name => "Secret Hyper";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1 : 1;
        public override string Acronym => "SH";
        public override ModType Type => ModType.Fun;

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            drawable.HitObjectApplied += (o) =>
            {
                FindDrawableHitObjectToApplyChanges(o);
            };
        }

        public void FindDrawableHitObjectToApplyChanges(DrawableHitObject drawable)
        {
            if (drawable is not DrawableCatchHitObject)
                return;

            var drawableCatchHitObject = (DrawableCatchHitObject)drawable;

            if (drawableCatchHitObject.NestedHitObjects.Any())
            {
                foreach (var nestedDrawable in drawableCatchHitObject.NestedHitObjects)
                {
                    if (nestedDrawable is DrawableCatchHitObject nestedCatchDrawable)
                        HideHyperDashFromDrawableHitObject(nestedCatchDrawable);
                }
            }

            else
                HideHyperDashFromDrawableHitObject(drawableCatchHitObject);
        }

        public void HideHyperDashFromDrawableHitObject(DrawableCatchHitObject drawable)
        {
            if (drawable is not DrawableBanana && drawable is DrawablePalpableCatchHitObject drawablePalpableCatchHitObject)
                drawablePalpableCatchHitObject.HyperDash.Value = false;
        }
    }
}
