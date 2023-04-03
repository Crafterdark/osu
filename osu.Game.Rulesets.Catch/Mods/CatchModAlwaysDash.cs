// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAlwaysDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "Always Dash";
        public override string Acronym => "AD";
        public override LocalisableString Description => "The catcher won't stop dashing.";
        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Fun;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModNoDash) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {

            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.CatcherArea.AlwaysDash = true;
            var theCatcherOnArea = catchPlayfield.CatcherArea.Catcher;
            theCatcherOnArea.Dashing = true;
            if (catchPlayfield.CatcherArea.TwinCatchersApplies) {
                var theTwinOnArea = catchPlayfield.CatcherArea.Twin;
                theTwinOnArea.Dashing = true;
            }

        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
        }

    }
}
