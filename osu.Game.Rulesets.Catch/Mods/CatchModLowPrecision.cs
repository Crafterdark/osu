// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModLowPrecision : Mod, IApplicableToBeatmap, IApplicableToDifficulty
    {
        public override string Name => "Low Precision";

        public override string Acronym => "LP";

        public override LocalisableString Description => "Less precision required!";

        public override ModType Type => ModType.DifficultyReduction;

        public override IconUsage? Icon => FontAwesome.Solid.AngleDoubleDown;

        public override double ScoreMultiplier => 0.3;

        //Expected size of fruits. (Note: Maximum skin size is 160x160, but Stable default skin size is 128x128)
        public const int MAX_HORIZONTAL_HITBOX_FRUIT = 128;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            foreach (var hitObject in CatchBeatmap.GetPalpableObjects(beatmap.HitObjects))
                hitObject.CatchableRangeUpdates.Add(new Func<CatchHitObject, float>((h) => (float)CalculateHalfExtendedWidth(h)));
        }

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            BeatmapDifficulty lowestCircleSizeDifficulty = new BeatmapDifficulty
            {
                CircleSize = 0
            };

            difficulty.OverallDifficulty = (float)(difficulty.OverallDifficulty * (1 - (Catcher.CalculateCatchWidth(difficulty) / Catcher.CalculateCatchWidth(lowestCircleSizeDifficulty))));
        }

        public static double CalculateHalfExtendedWidth(CatchHitObject catchHitObject)
        {
            double rescaleFactor = 0;

            switch (catchHitObject)
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

            return catchHitObject.Scale * rescaleFactor * (MAX_HORIZONTAL_HITBOX_FRUIT / 2.0);
        }
    }
}
