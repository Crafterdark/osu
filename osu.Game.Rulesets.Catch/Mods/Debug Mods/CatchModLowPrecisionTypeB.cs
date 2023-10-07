// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModLowPrecisionTypeB : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmap, IApplicableToDifficulty
    {
        //Type B: 0.25x score, OD/4, no OD dependency -> always apply maximum fruit hitbox.
        public override string Name => "Low Precision (Type B)";

        public override string Acronym => "LPB";

        public override LocalisableString Description => "Less precision required. Everything becomes easier to catch...";

        public override double ScoreMultiplier => 0.25;

        public override ModType Type => ModType.DifficultyReduction;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModLowPrecisionTypeA) };


        public double FinalCatcherAccuracy;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            //Debug: Placeholder to keep Type A code at the same time
            FinalCatcherAccuracy = 0.0d;
        }

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 4.0f;
            difficulty.OverallDifficulty /= ratio;
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
