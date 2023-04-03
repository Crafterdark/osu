// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModNoDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "No Dash";
        public override string Acronym => "ND";
        public override LocalisableString Description => "The catcher can't dash or hyperdash.";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Moon; //Placeholder
        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModAlwaysDash) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.CatcherArea.NoDash = true;
            var theCatcherOnArea = catchPlayfield.CatcherArea.Catcher;
            theCatcherOnArea.Dashing = false;
            if (catchPlayfield.CatcherArea.TwinCatchersApplies)
            {
                var theTwinOnArea = catchPlayfield.CatcherArea.Twin;
                theTwinOnArea.Dashing = false;
            }
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.NoDashHyperOffsets = true;
        }

    }
}
