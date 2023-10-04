// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAccuracy : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmap
    {
        public override string Name => "Accuracy";

        public override string Acronym => "AY";

        public override LocalisableString Description => "Implementation of Accuracy value to all fruits.";

        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Conversion;

        public double FinalCatcherAccuracy;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            FinalCatcherAccuracy = beatmap.Difficulty.OverallDifficulty;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitAccuracy = true;

            catchPlayfield.Catcher.CatchAccuracy = FinalCatcherAccuracy;

        }
    }
}
