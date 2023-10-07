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
    public class CatchModLowPrecisionTypeA : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmap, IApplicableToDifficulty
    {
        //Type A: 0.50x score, OD/4, OD dependency -> rescale maximum fruit hitbox with the OD value.
        public override string Name => "Low Precision (Type A)";

        public override string Acronym => "LPA";

        public override LocalisableString Description => "Less precision required. Everything becomes easier to catch...";

        public override double ScoreMultiplier => 0.5;

        public override ModType Type => ModType.DifficultyReduction;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModLowPrecisionTypeB) };


        public double FinalCatcherAccuracy;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            FinalCatcherAccuracy = beatmap.Difficulty.OverallDifficulty;
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
