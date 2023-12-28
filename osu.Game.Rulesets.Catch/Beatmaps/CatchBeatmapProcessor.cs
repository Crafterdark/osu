// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmapProcessor : BeatmapProcessor
    {
        public const int RNG_SEED = 1337;

        public bool HardRockOffsets { get; set; }

        public CatchBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
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
            var rng = new LegacyRandom(RNG_SEED);

            float? lastPosition = null;
            double lastStartTime = 0;

            foreach (var obj in beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.XOffset = 0;

                switch (obj)
                {
                    case Fruit fruit:
                        if (HardRockOffsets)
                            applyHardRockOffset(fruit, ref lastPosition, ref lastStartTime, rng);
                        break;

                    case BananaShower bananaShower:
                        foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                        {
                            banana.XOffset = (float)(rng.NextDouble() * CatchPlayfield.WIDTH);
                            rng.Next(); // osu!stable retrieved a random banana type
                            rng.Next(); // osu!stable retrieved a random banana rotation
                            rng.Next(); // osu!stable retrieved a random banana colour
                        }

                        break;

                    case JuiceStream juiceStream:
                        // Todo: BUG!! Stable used the last control point as the final position of the path, but it should use the computed path instead.
                        lastPosition = juiceStream.OriginalX + juiceStream.Path.ControlPoints[^1].Position.X;

                        // Todo: BUG!! Stable attempted to use the end time of the stream, but referenced it too early in execution and used the start time instead.
                        lastStartTime = juiceStream.StartTime;

                        foreach (var nested in juiceStream.NestedHitObjects)
                        {
                            var catchObject = (CatchHitObject)nested;
                            catchObject.XOffset = 0;

                            if (catchObject is TinyDroplet)
                                catchObject.XOffset = Math.Clamp(rng.Next(-20, 20), -catchObject.OriginalX, CatchPlayfield.WIDTH - catchObject.OriginalX);
                            else if (catchObject is Droplet)
                                rng.Next(); // osu!stable retrieved a random droplet rotation
                        }

                        break;
                }
            }

            initialiseHyperDash(beatmap);
        }

        private static void applyHardRockOffset(CatchHitObject hitObject, ref float? lastPosition, ref double lastStartTime, LegacyRandom rng)
        {
            float offsetPosition = hitObject.OriginalX;
            double startTime = hitObject.StartTime;

            if (lastPosition == null)
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

        private static bool isTinyDropletPathStart(PalpableCatchHitObject hitObject1, PalpableCatchHitObject hitObject2) => hitObject1 is not TinyDroplet && hitObject2 is TinyDroplet;

        private static bool isTinyDropletPathMid(PalpableCatchHitObject hitObject1, PalpableCatchHitObject hitObject2) => hitObject1 is TinyDroplet && hitObject2 is TinyDroplet;

        private static bool isTinyDropletPathEnd(PalpableCatchHitObject hitObject1, PalpableCatchHitObject hitObject2) => hitObject1 is TinyDroplet && hitObject2 is not TinyDroplet;

        private static bool isTinyDropletPath(PalpableCatchHitObject hitObject1, PalpableCatchHitObject hitObject2) => hitObject1 is TinyDroplet || hitObject2 is TinyDroplet;

        //private static bool isInHyperdashPath(PalpableCatchHitObject hitObjectX, PalpableCatchHitObject hitObject1, PalpableCatchHitObject hitObject2, double halfCatchWidth) =>
        //    (hitObject1.EffectiveX > hitObject2.EffectiveX) ?
        //    hitObject1.EffectiveX + halfCatchWidth >= hitObjectX.EffectiveX || hitObject2.EffectiveX - halfCatchWidth <= hitObjectX.EffectiveX :
        //    hitObject1.EffectiveX - halfCatchWidth <= hitObjectX.EffectiveX || hitObject2.EffectiveX + halfCatchWidth >= hitObjectX.EffectiveX;

        private static bool isInHyperdashPath(PalpableCatchHitObject hitObjectX, PalpableCatchHitObject hitObject1, PalpableCatchHitObject hitObject2, double halfCatchWidth) =>
            (hitObject1.EffectiveX > hitObject2.EffectiveX) ?
            hitObject2.EffectiveX <= hitObjectX.EffectiveX :
            hitObject2.EffectiveX >= hitObjectX.EffectiveX;


        private static void resetHyperdashStatus(PalpableCatchHitObject hitObject)
        {
            // Reset variables in-case values have changed (e.g. after applying HR)
            hitObject.HyperDashTarget = null;
            hitObject.DistanceToHyperDash = 0;
        }

        private static void initialiseHyperDash(IBeatmap beatmap)
        {
            var palpableObjects = CatchBeatmap.GetPalpableObjects(beatmap.HitObjects)
                                              .Where(h => h is Fruit || (h is Droplet && (h is not TinyDroplet || ((CatchBeatmap)beatmap).HyperDashTinyDroplet)))
                                              .ToArray();

            double halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;

            // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
            // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
            // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
            halfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            int thisDirection = 0;
            int thisDirectionTiny = 0;
            double localLastExcess;
            double localLastExcessTiny;

            int lastDirectionTiny = 0;
            double lastExcessTiny = halfCatcherWidth;

            double catcherMaximumSpeed;
            double catcherHyperDashSpeed = Catcher.BASE_DASH_SPEED;

            PalpableCatchHitObject? prevFruitObject = null;
            PalpableCatchHitObject? nextFruitObject = null;

            for (int i = 0; i < palpableObjects.Length - 1; i++)
            {
                var currentObject = palpableObjects[i];
                var nextObject = palpableObjects[i + 1];

                if (isTinyDropletPathStart(currentObject, nextObject))
                {
                    resetHyperdashStatus(currentObject);

                    for (int j = i + 2; j < palpableObjects.Length - 1; j++)
                    {
                        PalpableCatchHitObject newObject = palpableObjects[j];
                        if (newObject is not TinyDroplet)
                        {
                            resetHyperdashStatus(newObject);

                            // Store the fruit that ends the path
                            nextFruitObject = newObject;
                            break;
                        }
                    }

                    if (nextFruitObject == null)
                        continue;

                    // Store the fruit that starts the path
                    prevFruitObject = currentObject;

                    // Initialise start fruit path to end fruit path hyperdash
                    thisDirection = nextFruitObject.EffectiveX > prevFruitObject.EffectiveX ? 1 : -1;
                    localLastExcess = lastDirection == thisDirection ? lastExcess : halfCatcherWidth;
                    double oldLastExcess = lastExcess;
                    catcherMaximumSpeed = Catcher.BASE_DASH_SPEED;

                    lastExcess = initialiseHyperDashGetLastExcess(prevFruitObject, nextFruitObject, localLastExcess, halfCatcherWidth, catcherMaximumSpeed, true, true);
                    lastDirection = thisDirection;

                    // Calculate eventual hyperdash speed
                    if (prevFruitObject.HyperDashTarget != null)
                    {
                        double timeDifference = prevFruitObject.HyperDashTarget.StartTime - prevFruitObject.StartTime;
                        double positionDifference = prevFruitObject.HyperDashTarget.EffectiveX - prevFruitObject.EffectiveX;
                        double velocity = positionDifference / Math.Max(1.0, timeDifference - 1000.0 / 60.0);

                        catcherHyperDashSpeed = Math.Abs(velocity) / Catcher.BASE_DASH_SPEED;

                        if (catcherHyperDashSpeed < 1)
                            catcherHyperDashSpeed = Catcher.BASE_DASH_SPEED;
                    }

                    // Initialise fruit to tiny (don't change target for now) [use old localLastExcess]
                    thisDirectionTiny = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;

                    localLastExcessTiny = thisDirection == thisDirectionTiny ? oldLastExcess : halfCatcherWidth;

                    if (prevFruitObject != null && nextFruitObject != null && prevFruitObject.HyperDashTarget != null && isInHyperdashPath(nextObject, prevFruitObject, nextFruitObject, halfCatcherWidth))
                    {
                        catcherMaximumSpeed = catcherHyperDashSpeed;
                    }

                    //TO DO: DOUBLE DISTANCE CHECK!
                    else
                    {
                        if (prevFruitObject != null && nextFruitObject != null && prevFruitObject.HyperDashTarget != null && !isInHyperdashPath(currentObject, prevFruitObject, nextFruitObject, halfCatcherWidth))
                            Logger.Log("Why");
                        catcherMaximumSpeed = Catcher.BASE_DASH_SPEED;
                    }
                    lastExcessTiny = initialiseHyperDashGetLastExcess(currentObject, nextObject, localLastExcessTiny, halfCatcherWidth, catcherMaximumSpeed, false, false);
                    lastDirectionTiny = thisDirectionTiny;
                }

                else if (prevFruitObject != null && isTinyDropletPathEnd(currentObject, nextObject))
                {
                    resetHyperdashStatus(currentObject);

                    // Initialise tiny to fruit hyperdash
                    thisDirectionTiny = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                    localLastExcessTiny = lastDirectionTiny == thisDirectionTiny ? lastExcessTiny : halfCatcherWidth;

                    if (prevFruitObject != null && nextFruitObject != null && prevFruitObject.HyperDashTarget != null && isInHyperdashPath(currentObject, prevFruitObject, nextFruitObject, halfCatcherWidth))
                    {
                        catcherMaximumSpeed = catcherHyperDashSpeed;
                    }

                    //TO DO: DOUBLE DISTANCE CHECK!
                    else
                    {
                        if (prevFruitObject != null && nextFruitObject != null && prevFruitObject.HyperDashTarget != null && !isInHyperdashPath(currentObject, prevFruitObject, nextFruitObject, halfCatcherWidth))
                            Logger.Log("Why");
                        catcherMaximumSpeed = Catcher.BASE_DASH_SPEED;
                    }
                    lastExcessTiny = initialiseHyperDashGetLastExcess(currentObject, nextObject, localLastExcessTiny, halfCatcherWidth, catcherMaximumSpeed, false, true);

                    lastDirectionTiny = thisDirectionTiny;

                    prevFruitObject = null;
                    nextFruitObject = null;
                }

                else if (isTinyDropletPathMid(currentObject, nextObject))
                {
                    resetHyperdashStatus(currentObject);

                    // Initialise tiny to tiny hyperdash
                    thisDirectionTiny = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                    localLastExcessTiny = lastDirectionTiny == thisDirectionTiny ? lastExcessTiny : halfCatcherWidth;

                    if (prevFruitObject != null && nextFruitObject != null && prevFruitObject.HyperDashTarget != null && isInHyperdashPath(currentObject, prevFruitObject, nextFruitObject, halfCatcherWidth))
                    {
                        catcherMaximumSpeed = catcherHyperDashSpeed;
                    }

                    //TO DO: DOUBLE DISTANCE CHECK!
                    else
                    {
                        if (prevFruitObject != null && nextFruitObject != null && prevFruitObject.HyperDashTarget != null && !isInHyperdashPath(currentObject, prevFruitObject, nextFruitObject, halfCatcherWidth))
                            Logger.Log("Why");
                        catcherMaximumSpeed = Catcher.BASE_DASH_SPEED;
                    }
                    lastExcessTiny = initialiseHyperDashGetLastExcess(currentObject, nextObject, localLastExcessTiny, halfCatcherWidth, catcherMaximumSpeed, false, true);

                    lastDirectionTiny = thisDirectionTiny;
                }

                else if (!isTinyDropletPath(currentObject, nextObject))
                {
                    // Safety for aspire maps
                    prevFruitObject = null;
                    nextFruitObject = null;

                    resetHyperdashStatus(currentObject);

                    // Initialise fruit to fruit hyperdash (always necessary)
                    thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                    localLastExcess = lastDirection == thisDirection ? lastExcess : halfCatcherWidth;
                    catcherMaximumSpeed = Catcher.BASE_DASH_SPEED;

                    lastExcess = initialiseHyperDashGetLastExcess(currentObject, nextObject, localLastExcess, halfCatcherWidth, catcherMaximumSpeed, true, true);

                    lastDirection = thisDirection;
                }
            }
        }

        private static double initialiseHyperDashGetLastExcess(PalpableCatchHitObject currentObject, PalpableCatchHitObject nextObject, double localLastExcess, double halfCatcherWidth, double catcherMaximumSpeed, bool isGrace, bool changeTarget)
        {
            // Int truncation added to match osu!stable.

            float graceTime = isGrace ? 1000f / 60f / 4 : 0f;

            double timeToNext = (int)nextObject.StartTime - (int)currentObject.StartTime - graceTime; // 1/4th of a frame of grace time, taken from osu-stable
            double distanceToNext = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - localLastExcess;
            float distanceToHyper = (float)(timeToNext * catcherMaximumSpeed - distanceToNext);

            if (distanceToHyper < 0)
            {
                if (currentObject is TinyDroplet)
                {
                    Logger.Log("Curr and Next: " + currentObject.EffectiveX + " " + nextObject.EffectiveX);
                    Logger.Log("Time to Next: " + timeToNext);
                    Logger.Log("Distance to Next: " + distanceToNext);
                    Logger.Log("IsGrace: " + isGrace);
                    Logger.Log("Speed: " + catcherMaximumSpeed);
                    Logger.Log("Distance: " + distanceToHyper);
                }
                if (changeTarget)
                    currentObject.HyperDashTarget = nextObject;
                return halfCatcherWidth;
            }
            else
            {
                if (changeTarget)
                {
                    currentObject.HyperDashTarget = null;
                    currentObject.DistanceToHyperDash = distanceToHyper;
                }
                return Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
            }

        }
    }
}
