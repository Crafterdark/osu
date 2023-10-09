// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Catch.Difficulty.Skills;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.153;

        private float halfCatcherWidth;

        private bool lowPrecisionStatus = false;

        private double lowPrecisionValue;

        public override int Version => 20220701;

        public CatchDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new CatchDifficultyAttributes { Mods = mods };

            // this is the same as osu!, so there's potential to share the implementation... maybe
            double preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            CatchDifficultyAttributes attributes = new CatchDifficultyAttributes
            {
                StarRating = Math.Sqrt(skills[0].DifficultyValue()) * star_scaling_factor,
                Mods = mods,
                ApproachRate = preempt > 1200.0 ? -(preempt - 1800.0) / 120.0 : -(preempt - 1200.0) / 150.0 + 5.0,
                MaxCombo = beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>().SelectMany(j => j.NestedHitObjects).Count(h => !(h is TinyDroplet)),
            };

            return attributes;
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            CatchHitObject? lastObject = null;

            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            // In 2B beatmaps, it is possible that a normal Fruit is placed in the middle of a JuiceStream.
            foreach (var hitObject in beatmap.HitObjects
                                             .SelectMany(obj => obj is JuiceStream stream ? stream.NestedHitObjects.AsEnumerable() : new[] { obj })
                                             .Cast<CatchHitObject>()
                                             .OrderBy(x => x.StartTime))
            {
                // We want to only consider fruits that contribute to the combo.
                if (hitObject is BananaShower || hitObject is TinyDroplet)
                    continue;

                if (lastObject != null)
                {

                    double accuracyDistance = 0;

                    if (lowPrecisionStatus)
                    {

                        double rescale_factor = 0;

                        if (hitObject is Fruit)
                        {
                            rescale_factor = 1;
                        }

                        if (hitObject is Droplet)
                        {
                            rescale_factor = 0.8;
                        }

                        if (hitObject is Banana)
                        {
                            rescale_factor = 0.6;
                        }

                        if (hitObject is TinyDroplet)
                        {
                            rescale_factor = 0.4;
                        }

                        //OD must stay in range [0,10] (Temporary)
                        double localCatchAccuracy = Math.Clamp(lowPrecisionValue, 0.0d, 10.0d);

                        //CatchAccuracy is calculated before starting the beatmap. (See CatchModAccuracy.cs)
                        //160 is the current maximum size of fruits.
                        accuracyDistance = (double)Math.Abs(localCatchAccuracy - 10) / 10 * hitObject.Scale * rescale_factor * (160 / 2);

                        accuracyDistance *= 1 - (Math.Max(0, beatmap.Difficulty.CircleSize - 5.5f) * 0.0625f);
                    }

                    objects.Add(new CatchDifficultyHitObject(hitObject, lastObject, clockRate, halfCatcherWidth + (float)accuracyDistance, objects, objects.Count));

                }
                lastObject = hitObject;
            }

            return objects;
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) * 0.5f;

            // For circle sizes above 5.5, reduce the catcher width further to simulate imperfect gameplay.
            halfCatcherWidth *= 1 - (Math.Max(0, beatmap.Difficulty.CircleSize - 5.5f) * 0.0625f);

            for (int index = 0; index < mods.Length; index++)
            {
                if (mods[index] is CatchModLowPrecisionTypeA)
                {
                    lowPrecisionStatus = true;
                    lowPrecisionValue = beatmap.Difficulty.OverallDifficulty;
                    break;
                }
                if (mods[index] is CatchModLowPrecisionTypeB)
                {
                    lowPrecisionStatus = true;
                    lowPrecisionValue = 0.0d;
                    break;
                }
            }

            return new Skill[]
            {
                new Movement(mods, halfCatcherWidth, clockRate),
            };
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new CatchModDoubleTime(),
            new CatchModHalfTime(),
            new CatchModHardRock(),
            new CatchModEasy(),
        };
    }
}
