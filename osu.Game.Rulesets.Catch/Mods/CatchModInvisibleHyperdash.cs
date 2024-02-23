// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModInvisibleHyperdash : Mod, IApplicableToDrawableHitObject, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override LocalisableString Description => @"Where's the hyperdash?";
        public override string Name => "Invisible Hyperdash";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "IH";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.QuestionCircle;

        public void ApplyToDrawableHitObject(DrawableHitObject drawable) => drawable.HitObjectApplied += FindDrawableHitObjectToApplyChanges;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var catchDrawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            //Always hide the catcher trail hyperdash
            catchPlayfield.CatcherArea.Catcher.ShowHyperDashTrail = false;
        }

        public void FindDrawableHitObjectToApplyChanges(DrawableHitObject drawable)
        {
            bool isObjectContainer = drawable is not DrawablePalpableCatchHitObject;

            if (isObjectContainer)
                drawable.NestedHitObjects.ForEach(x => ((DrawablePalpableCatchHitObject)x).HyperDash.Value = false);
            else
                ((DrawablePalpableCatchHitObject)drawable).HyperDash.Value = false;
        }
    }
}
