// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModLowPrecision : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDifficulty
    {
        public override string Name => "Low Precision";

        public override string Acronym => "LP";

        public override LocalisableString Description => "Fruits can be caught in their entirety!";

        public override ModType Type => ModType.DifficultyReduction;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModClassic) };

        public override IconUsage? Icon => FontAwesome.Solid.AngleDoubleDown;

        public override double ScoreMultiplier => 0.5;

        //Expected size of fruits. (Note: Maximum skin size is 160x160, but Stable default skin size is 128x128)
        public const int MAX_HITBOX_FRUIT = 128;

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            BeatmapDifficulty lowestCircleSizeDifficulty = new BeatmapDifficulty
            {
                CircleSize = 0
            };

            difficulty.OverallDifficulty = (float)(difficulty.OverallDifficulty * (1 - (Catcher.CalculateCatchWidth(difficulty) / Catcher.CalculateCatchWidth(lowestCircleSizeDifficulty))));
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitsHavingVariableWidth = true;
            catchPlayfield.Catcher.CatchFruitsWithinPlateEdges = true;
        }

        public static double CalculateVariableWidth(HitObject hitObject)
        {
            double rescaleFactor = 0;

            switch (hitObject)
            {
                case Fruit:
                    rescaleFactor = 1;
                    break;
                case TinyDroplet:
                    rescaleFactor = 0.4;
                    break;
                case Droplet:
                    rescaleFactor = 0.8;
                    break;
                case Banana:
                    rescaleFactor = 0.6;
                    break;
            }

            return ((CatchHitObject)hitObject).Scale * rescaleFactor * (MAX_HITBOX_FRUIT / 2.0);
        }
    }
}
