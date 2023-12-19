// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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

        public bool RandomModOffsets { get; set; }

        public int RandomModSeed { get; set; }

        public float? RD_OriginalX_Old { get; set; }

        public float? RD_XOffset_Old { get; set; }

        public float? RD_OriginalX_New { get; set; }

        public float? RD_XOffset_New { get; set; }

        public double RD_FlowFactor { get; set; }

        public float RD_HalfCatchWidth { get; set; }

        public float RD_LastExcess { get; set; }

        public double RD_LastStartTime_Old { get; set; }

        public int RD_LastDirection_New { get; set; }

        public int RD_LastDirection_Old { get; set; }

        private Random randomModRng = null!;

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

            if (RandomModOffsets)
            {
                randomModRng = new Random(RandomModSeed);
                RD_HalfCatchWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;
                RD_HalfCatchWidth /= Catcher.ALLOWED_CATCH_RANGE;

                RD_LastExcess = RD_HalfCatchWidth;
            }

            foreach (var obj in beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.XOffset = 0;

                switch (obj)
                {
                    case Fruit fruit:
                        if (HardRockOffsets)
                            applyHardRockOffset(fruit, ref lastPosition, ref lastStartTime, rng);
                        if (RandomModOffsets)
                            ApplyRandomModOffset(fruit);
                        break;

                    case BananaShower bananaShower:
                        foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                        {
                            banana.XOffset = (float)(rng.NextDouble() * CatchPlayfield.WIDTH);
                            if (RandomModOffsets)
                                ApplyRandomModOffset(banana);
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

                            if (RandomModOffsets)
                                ApplyRandomModOffset(catchObject);
                        }

                        break;
                }
            }

            initialiseHyperDash(beatmap);
        }

        public int GetLastDirection(float hitObjectX, float referenceX)
        {
            return (hitObjectX > referenceX) ? 1 : -1;
        }

        public bool ShouldChangeDirection(bool hasChangedNew, bool hasChangedOriginal)
        {
            return (!hasChangedNew && hasChangedOriginal) || (hasChangedNew && !hasChangedOriginal);
        }

        public void ApplyRandomModOffset(CatchHitObject hitObject)
        {
            //Banana (ends code earlier)
            if (hitObject is Banana)
            {
                hitObject.XOffset = (float)(randomModRng.NextDouble() * CatchPlayfield.WIDTH);
                hitObject.OriginalX = 0;
                return;
            }

            else if (RD_OriginalX_Old == null || RD_XOffset_Old == null || RD_OriginalX_New == null || RD_XOffset_New == null)
            {
                RD_OriginalX_Old = hitObject.OriginalX;
                RD_XOffset_Old = hitObject.XOffset;
                RD_LastDirection_Old = GetLastDirection(hitObject.EffectiveX, CatchPlayfield.WIDTH / 2);

                //First hitobject position will always be completely random
                hitObject.XOffset = (float)(randomModRng.NextDouble() * CatchPlayfield.WIDTH);
                hitObject.OriginalX = 0;

                //TinyDroplet conversion
                if (hitObject is TinyDroplet)
                {
                    hitObject.OriginalX = hitObject.XOffset;
                    hitObject.XOffset = 0;
                }

                RD_OriginalX_New = hitObject.OriginalX;
                RD_XOffset_New = hitObject.XOffset;
                RD_LastDirection_New = GetLastDirection(hitObject.EffectiveX, CatchPlayfield.WIDTH / 2);
                RD_LastStartTime_Old = hitObject.StartTime;
            }

            else
            {
                float jumpOriginal = hitObject.EffectiveX - (RD_OriginalX_Old.Value + RD_XOffset_Old.Value);
                float jumpCurrent = Math.Abs(jumpOriginal);

                int curr_RD_LastDirection_Old = GetLastDirection(hitObject.EffectiveX, RD_OriginalX_Old.Value + RD_XOffset_Old.Value);

                float newEffectiveX = RD_OriginalX_New.Value + RD_XOffset_New.Value;

                float leftJump = newEffectiveX - jumpCurrent;
                float rightJump = newEffectiveX + jumpCurrent;

                bool isLeftJumpPossible = (leftJump > CatchPlayfield.WIDTH || leftJump < 0) ? false : true;
                bool isRightJumpPossible = (rightJump > CatchPlayfield.WIDTH || rightJump < 0) ? false : true;

                bool isBothPossible = isLeftJumpPossible && isRightJumpPossible;
                bool atLeastOnePossible = isLeftJumpPossible || isRightJumpPossible;

                int curr_RD_LastDirection_New;


                //Randomly select a jump: Choose between the original map flow or random direction
                float randSelectedJump = (randomModRng.NextDouble() < 0.5) ? leftJump : rightJump;

                curr_RD_LastDirection_New = GetLastDirection(randSelectedJump - newEffectiveX, 0);

                bool mustChangeDirection = false;

                double timeToNextOld = (int)hitObject.StartTime - (int)RD_LastStartTime_Old - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                double distanceToNextOld = Math.Abs(hitObject.EffectiveX - (RD_OriginalX_Old.Value + RD_XOffset_Old.Value)) - (curr_RD_LastDirection_Old == RD_LastDirection_Old ? RD_LastExcess : RD_HalfCatchWidth);
                float distanceToDashOld = (float)(timeToNextOld * Catcher.BASE_WALK_SPEED - distanceToNextOld);

                if (distanceToDashOld < RD_HalfCatchWidth)
                {
                    RD_LastExcess = RD_HalfCatchWidth;
                    mustChangeDirection = true;
                }
                else
                {
                    RD_LastExcess = Math.Clamp(distanceToDashOld, 0, RD_HalfCatchWidth);
                }

                RD_OriginalX_Old = hitObject.OriginalX;
                RD_XOffset_Old = hitObject.XOffset;

                bool hasChangedLastDirectionNew = curr_RD_LastDirection_New != RD_LastDirection_New;

                bool hasChangedLastDirectionOld = curr_RD_LastDirection_Old != RD_LastDirection_Old;

                if (mustChangeDirection && ShouldChangeDirection(hasChangedLastDirectionNew, hasChangedLastDirectionOld))
                {
                    if (hasChangedLastDirectionOld && !hasChangedLastDirectionNew)
                        curr_RD_LastDirection_New = -RD_LastDirection_New;
                    else if (!hasChangedLastDirectionOld && hasChangedLastDirectionNew)
                        curr_RD_LastDirection_New = RD_LastDirection_New;

                    if (curr_RD_LastDirection_New > 0)
                        randSelectedJump = rightJump;
                    else
                        randSelectedJump = leftJump;

                    hitObject.XOffset = Math.Clamp(randSelectedJump, 0, CatchPlayfield.WIDTH);
                }

                //Use the randomly selected jump
                else if (isBothPossible)
                    hitObject.XOffset = randSelectedJump;
                //Use the other possible jump
                else if (atLeastOnePossible)
                    hitObject.XOffset = isLeftJumpPossible ? leftJump : rightJump;
                //Use the randomly selected jump or the other possible jump with clamp
                //(select the one with the best matching distance)
                else
                {
                    float otherJump = (randSelectedJump == leftJump) ? rightJump : leftJump;
                    float otherJumpOOB = otherJump < 0 ? Math.Abs(otherJump) : otherJump - CatchPlayfield.WIDTH;
                    float randSelectedJumpOOB = randSelectedJump < 0 ? Math.Abs(randSelectedJump) : randSelectedJump - CatchPlayfield.WIDTH;
                    hitObject.XOffset = Math.Clamp((otherJumpOOB < randSelectedJumpOOB) ? otherJump : randSelectedJump, 0, CatchPlayfield.WIDTH);
                }

                hitObject.OriginalX = 0;


                //TinyDroplet conversion
                if (hitObject is TinyDroplet)
                {
                    hitObject.OriginalX = hitObject.XOffset;
                    hitObject.XOffset = 0;
                }

                RD_LastDirection_New = curr_RD_LastDirection_New;
                RD_OriginalX_New = hitObject.OriginalX;
                RD_XOffset_New = hitObject.XOffset;

                RD_LastDirection_Old = curr_RD_LastDirection_Old;
                RD_LastStartTime_Old = hitObject.StartTime;
            }
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

        private static void initialiseHyperDash(IBeatmap beatmap)
        {
            var palpableObjects = CatchBeatmap.GetPalpableObjects(beatmap.HitObjects)
                                              .Where(h => h is Fruit || (h is Droplet && h is not TinyDroplet))
                                              .ToArray();

            double halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;

            // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
            // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
            // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
            halfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            for (int i = 0; i < palpableObjects.Length - 1; i++)
            {
                var currentObject = palpableObjects[i];
                var nextObject = palpableObjects[i + 1];

                // Reset variables in-case values have changed (e.g. after applying HR)
                currentObject.HyperDashTarget = null;
                currentObject.DistanceToHyperDash = 0;

                int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;

                // Int truncation added to match osu!stable.
                double timeToNext = (int)nextObject.StartTime - (int)currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                double distanceToNext = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                float distanceToHyper = (float)(timeToNext * Catcher.BASE_DASH_SPEED - distanceToNext);

                if (distanceToHyper < 0)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    lastExcess = Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
