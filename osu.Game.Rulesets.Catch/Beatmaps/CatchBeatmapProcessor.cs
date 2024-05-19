// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Catch.Utils;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmapProcessor : BeatmapProcessor
    {
        public const int RNG_SEED = 1337;

        public bool SpicyPatterns { get; set; }

        public bool HardRockOffsets { get; set; }

        public bool NewTinyGeneration { get; set; } = true;

        public bool IsDropletStabilized { get; set; }

        public int StabilizedOffset { get; set; }

        public bool ClassicSpicyPatterns { get; set; }

        public bool ClassicLegacyRandom { get; set; }


        private CatchBeatmap catchBeatmap = null!;

        public CatchBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
            catchBeatmap = (CatchBeatmap)beatmap;
        }

        public override void PreProcess()
        {
            IHasComboInformation? lastObj = null;

            // For sanity, ensures that both the first hitobject and the first hitobject after a banana shower start a new combo.
            // This is normally enforced by the legacy decoder, but is not enforced by the editor.
            foreach (var obj in Beatmap.HitObjects.OfType<IHasComboInformation>())
            {
                if (obj is not BananaShower && (lastObj == null || lastObj is BananaShower))
                    obj.NewCombo = true;
                lastObj = obj;
            }

            base.PreProcess();
        }

        public override void PostProcess()
        {
            base.PostProcess();

            ApplyPositionOffsets(Beatmap);

            int index = 0;

            foreach (var obj in Beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.IndexInBeatmap = index;
                foreach (var nested in obj.NestedHitObjects.OfType<CatchHitObject>())
                    nested.IndexInBeatmap = index;

                if (obj.LastInCombo && obj.NestedHitObjects.LastOrDefault() is IHasComboInformation lastNested)
                    lastNested.LastInCombo = true;

                index++;
            }
        }

        public void ApplyPositionOffsets(IBeatmap beatmap)
        {
            var rng = !ClassicLegacyRandom ? new LegacyRandomExtension(RNG_SEED) : new LegacyRandom(RNG_SEED);

            //Independent random for missing tiny droplets in Stable
            var rngNew = NewTinyGeneration ? new Random(RNG_SEED) : null;

            float? lastPosition = null;
            double lastStartTime = 0;

            if (catchBeatmap.UsesLimitedCatchPlayfield.Value)
            {
                double newCatchPlayfieldWidth = CatchPlayfield.WIDTH * catchBeatmap.CatcherAdjustedDashSpeed.Value;
                catchBeatmap.LimitedCatchPlayfieldContainer = new LimitedCatchPlayfieldContainer(newCatchPlayfieldWidth);
            }

            CatchHitObject prevObj = null!;

            foreach (var obj in beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.XOffset = 0;

                switch (obj)
                {
                    case Fruit fruit:
                        if (SpicyPatterns || (HardRockOffsets && ClassicSpicyPatterns))
                            applySpicyPatternsOffset(fruit, ref lastPosition, ref lastStartTime, rng);
                        catchBeatmap.LimitedCatchPlayfieldContainer?.Convert(fruit);

                        if (catchBeatmap.HitObjectWithNextTarget.Value)
                        {
                            if (prevObj != null)
                                prevObj.NextTarget = fruit;
                            prevObj = fruit;
                        }

                        break;

                    case BananaShower bananaShower:
                        foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                        {
                            banana.XOffset = rng.NextFloat(CatchPlayfield.WIDTH);
                            rng.Next(); // osu!stable retrieved a random banana type
                            rng.Next(); // osu!stable retrieved a random banana rotation
                            rng.Next(); // osu!stable retrieved a random banana colour
                            catchBeatmap.LimitedCatchPlayfieldContainer?.Convert(banana);

                            if (catchBeatmap.HitObjectWithNextTarget.Value)
                            {
                                if (prevObj != null)
                                    prevObj.NextTarget = banana;
                                prevObj = banana;
                            }
                        }

                        break;

                    case JuiceStream juiceStream:
                        // Todo: BUG!! Stable used the last control point as the final position of the path, but it should use the computed path instead.
                        lastPosition = juiceStream.OriginalX + juiceStream.Path.ControlPoints[^1].Position.X;

                        // Todo: BUG!! Stable attempted to use the end time of the stream, but referenced it too early in execution and used the start time instead.
                        lastStartTime = juiceStream.StartTime;

                        if (catchBeatmap.HitObjectWithNextTarget.Value)
                        {
                            if (prevObj != null)
                                prevObj.NextTarget = juiceStream;
                            prevObj = juiceStream;
                        }

                        foreach (var nested in juiceStream.NestedHitObjects)
                        {
                            var catchObject = (CatchHitObject)nested;
                            catchObject.XOffset = 0;

                            int rngOffset = IsDropletStabilized ? StabilizedOffset : 20;

                            if (catchObject is TinyDroplet || (catchObject is Droplet droplet && droplet.HasRandomOffset))
                                catchObject.XOffset = Math.Clamp(catchObject.IsUsingOldRandom ? rng.Next(-rngOffset, rngOffset) : (rngNew != null ? rngNew.Next(-rngOffset, rngOffset) : 0), -catchObject.OriginalX, CatchPlayfield.WIDTH - catchObject.OriginalX);
                            else if (catchObject is Droplet)
                                rng.Next(); // osu!stable retrieved a random droplet rotation

                            catchBeatmap.LimitedCatchPlayfieldContainer?.Convert(catchObject);

                            if (catchBeatmap.HitObjectWithNextTarget.Value)
                            {
                                if (prevObj != null)
                                    prevObj.NextTarget = catchObject;
                                prevObj = catchObject;
                            }
                        }

                        break;
                }
            }

            initialiseHyperDash(beatmap);
        }

        private static void applySpicyPatternsOffset(CatchHitObject hitObject, ref float? lastPosition, ref double lastStartTime, LegacyRandom rng)
        {
            float offsetPosition = hitObject.OriginalX;
            double startTime = hitObject.StartTime;

            if (lastPosition == null ||
                // some objects can get assigned position zero, making stable incorrectly go inside this if branch on the next object. to maintain behaviour and compatibility, do the same here.
                // reference: https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/GameplayElements/HitObjects/Fruits/HitFactoryFruits.cs#L45-L50
                // todo: should be revisited and corrected later probably.
                lastPosition == 0)
            {
                lastPosition = offsetPosition;
                lastStartTime = startTime;

                return;
            }

            float positionDiff = offsetPosition - lastPosition.Value;

            // Todo: BUG!! Stable calculated time deltas as ints, which affects randomisation. This should be changed to a double.
            int timeDiff = (int)(startTime - lastStartTime);

            if (timeDiff > 1000)
            {
                lastPosition = offsetPosition;
                lastStartTime = startTime;
                return;
            }

            if (positionDiff == 0)
            {
                applyRandomOffset(ref offsetPosition, timeDiff / 4d, rng);
                hitObject.XOffset = offsetPosition - hitObject.OriginalX;
                return;
            }

            // ReSharper disable once PossibleLossOfFraction
            if (Math.Abs(positionDiff) < timeDiff / 3)
                applyOffset(ref offsetPosition, positionDiff);

            hitObject.XOffset = offsetPosition - hitObject.OriginalX;

            lastPosition = offsetPosition;
            lastStartTime = startTime;
        }

        /// <summary>
        /// Applies a random offset in a random direction to a position, ensuring that the final position remains within the boundary of the playfield.
        /// </summary>
        /// <param name="position">The position which the offset should be applied to.</param>
        /// <param name="maxOffset">The maximum offset, cannot exceed 20px.</param>
        /// <param name="rng">The random number generator.</param>
        private static void applyRandomOffset(ref float position, double maxOffset, LegacyRandom rng)
        {
            bool right = rng.NextBool();
            float rand = Math.Min(20, (float)rng.Next(0, Math.Max(0, maxOffset)));

            if (right)
            {
                // Clamp to the right bound
                if (position + rand <= CatchPlayfield.WIDTH)
                    position += rand;
                else
                    position -= rand;
            }
            else
            {
                // Clamp to the left bound
                if (position - rand >= 0)
                    position -= rand;
                else
                    position += rand;
            }
        }

        /// <summary>
        /// Applies an offset to a position, ensuring that the final position remains within the boundary of the playfield.
        /// </summary>
        /// <param name="position">The position which the offset should be applied to.</param>
        /// <param name="amount">The amount to offset by.</param>
        private static void applyOffset(ref float position, float amount)
        {
            if (amount > 0)
            {
                // Clamp to the right bound
                if (position + amount < CatchPlayfield.WIDTH)
                    position += amount;
            }
            else
            {
                // Clamp to the left bound
                if (position + amount > 0)
                    position += amount;
            }
        }

        private static int calculateDirection(PalpableCatchHitObject current, PalpableCatchHitObject next, int lastDirection, bool isSymmetrical) => isSymmetrical && (next.EffectiveX == current.EffectiveX) ? lastDirection : (next.EffectiveX > current.EffectiveX ? 1 : -1);

        private static void initialiseHyperDash(IBeatmap beatmap)
        {
            var palpableObjects = CatchBeatmap.GetPalpableObjects(beatmap.HitObjects)
                                              .Where(h => h is Fruit || (h is Droplet droplet && !droplet.HasRandomOffset && h is not TinyDroplet))
                                              .ToArray();

            double originalHalfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.BeatmapInfo.Difficulty.Clone()) / 2;
            double modifiedHalfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;

            var catchBeatmap = (CatchBeatmap)beatmap;

            // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
            // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
            // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
            originalHalfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;
            modifiedHalfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

            int originalLastDirection = 0;
            double originalLastExcess = originalHalfCatcherWidth;

            int modifiedLastDirection = 0;
            double modifiedLastExcess = modifiedHalfCatcherWidth;

            for (int i = 0; i < palpableObjects.Length - 1; i++)
            {
                var currentObject = palpableObjects[i];
                var nextObject = palpableObjects[i + 1];

                // Reset variables in-case values have changed (e.g. after applying HR)
                currentObject.HyperDashTarget = null;
                currentObject.DistanceToHyperDash = 0;

                catchBeatmap.LimitedCatchPlayfieldContainer?.UnconvertPair(currentObject, nextObject);

                int thisOriginalDirection = calculateDirection(currentObject, nextObject, originalLastDirection, catchBeatmap.IsProcessingSymmetricalHyperDash.Value);

                if (catchBeatmap.OriginalHyperDashGeneration.Value)
                    originalLastExcess = processHyperDash(catchBeatmap, currentObject, nextObject, originalHalfCatcherWidth, originalLastExcess, originalLastDirection, thisOriginalDirection, true);

                catchBeatmap.LimitedCatchPlayfieldContainer?.ConvertPair(currentObject, nextObject);

                int thisModifiedDirection = calculateDirection(currentObject, nextObject, modifiedLastDirection, catchBeatmap.IsProcessingSymmetricalHyperDash.Value);

                if (catchBeatmap.ModifiedHyperDashGeneration.Value)
                    modifiedLastExcess = processHyperDash(catchBeatmap, currentObject, nextObject, modifiedHalfCatcherWidth, modifiedLastExcess, modifiedLastDirection, thisModifiedDirection, false);

                originalLastDirection = thisOriginalDirection;
                modifiedLastDirection = thisModifiedDirection;
            }
        }

        private static double processHyperDash(CatchBeatmap catchBeatmap, PalpableCatchHitObject currentObject, PalpableCatchHitObject nextObject, double halfCatcherWidth, double lastExcess, int lastDirection, int thisDirection, bool regularHyperDashRemoval)
        {
            // Int truncation added to match osu!stable.
            double timeToNext = (int)nextObject.StartTime - (int)currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
            double distanceToNext = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
            float distanceToHyper = (float)(timeToNext * (regularHyperDashRemoval ? Catcher.BASE_DASH_SPEED : catchBeatmap.CatcherAdjustedDashSpeed.Value) - distanceToNext);

            if (distanceToHyper < 0)
            {
                //HyperDash object
                currentObject.HyperDashTarget = nextObject;
                currentObject.DistanceToHyperDash = 0;
                return halfCatcherWidth;
            }
            else if (regularHyperDashRemoval)
            {
                //No HyperDash object and uses this catcherMaxSpeed for distance.
                currentObject.HyperDashTarget = null;
                currentObject.DistanceToHyperDash = distanceToHyper;
                return Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
            }
            else
            {
                if (currentObject.HyperDashTarget != null)
                {
                    //HyperDash object already exists
                    return halfCatcherWidth;
                }
                else
                {
                    //No HyperDash object and uses this catcherMaxSpeed for distance.
                    currentObject.HyperDashTarget = null;
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    return Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
                }
            }
        }
    }
}
