// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        //Mod Bools
        public bool HardRockOffsets { get; set; }
        public bool AnotherEasyOffsets { get; set; }
        public bool AnotherEasyNewHyperDashes { get; set; }
        public bool TwinCatchersOffsets { get; set; }
        public bool NoDashingOffsets { get; set; }
        public bool NoHyperOffsets { get; set; }
        public bool AnotherEasyEdgeReduction { get; set; }

        //Mod Values
        public float EdgeReduction { get; set; }

        public enum ModBools
        {
            HR, //0
            AE, //1
            AE_NewHyperDashes, //2
            TC, //3
            ND, //4
            NH, //5
            AE_EdgeReduction //6
        };

        public enum ModValues
        {
            MovementStatus, //0
            EdgeReduction //1
        };

        public enum MovementType
        {
            NoChange, //0
            NoHyperDashing, //1
            NoDashing, //2
        };


        //Utility variables

        //Used to generate a symmetrical pattern when objects fall in the middle of the Playfield
        public bool TC_InvertGen_Util;

        public CatchBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
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

            float? previousPosition = null;
            double previousStartTime = 0;


            List<bool> modBools = new List<bool>
            {
                 HardRockOffsets, //0
                 AnotherEasyOffsets, //1
                 AnotherEasyNewHyperDashes, //2
                 TwinCatchersOffsets, //3
                 NoDashingOffsets, //4
                 NoHyperOffsets, //5
                 AnotherEasyEdgeReduction //6
            };

            List<double> modValues = new List<double>
            {
                 GetMovementStatus(), //0 (Shared between ND, NH and NM)
                 EdgeReduction, //1 (Shared between ND and NH)
            };

            foreach (var obj in beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.XOffset = 0;

                switch (obj)
                {
                    case Fruit fruit:
                        if (modBools[(int)ModBools.HR])
                            applyHardRockOffset(fruit, ref previousPosition, ref previousStartTime, rng);
                        if (modBools[(int)ModBools.TC])
                            applyTwinCatchersOffset(fruit, beatmap, TC_InvertGen_Util);
                        break;

                    case BananaShower bananaShower:
                        foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                        {
                            banana.XOffset = (float)(rng.NextDouble() * CatchPlayfield.WIDTH);
                            if (modBools[(int)ModBools.TC])
                                applyTwinCatchersOffset(banana, beatmap, TC_InvertGen_Util);

                            rng.Next(); // osu!stable retrieved a random banana type
                            rng.Next(); // osu!stable retrieved a random banana rotation
                            rng.Next(); // osu!stable retrieved a random banana colour
                        }

                        break;

                    case JuiceStream juiceStream:
                        // Todo: BUG!! Stable used the last control point as the final position of the path, but it should use the computed path instead.
                        previousPosition = juiceStream.OriginalX + juiceStream.Path.ControlPoints[^1].Position.X;

                        // Todo: BUG!! Stable attempted to use the end time of the stream, but referenced it too early in execution and used the start time instead.
                        previousStartTime = juiceStream.StartTime;

                        foreach (var nested in juiceStream.NestedHitObjects)
                        {
                            var catchObject = (CatchHitObject)nested;
                            catchObject.XOffset = 0;

                            if (catchObject is TinyDroplet)
                                catchObject.XOffset = Math.Clamp(rng.Next(-20, 20), -catchObject.OriginalX, CatchPlayfield.WIDTH - catchObject.OriginalX);
                            else if (catchObject is Droplet)
                                rng.Next(); // osu!stable retrieved a random droplet rotation

                            if (modBools[(int)ModBools.TC])
                                applyTwinCatchersOffset(catchObject, beatmap, TC_InvertGen_Util);
                        }
                        break;
                }
            }

            initialiseHyperDash(beatmap, modBools, modValues);

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


        private static void applyTwinCatchersOffset(CatchHitObject hitObject, IBeatmap beatmap, bool invertGen)

        {

            //Taken from Hyperdash calculations
            float initialOffset = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;

            float rightSideFromMiddle = (CatchPlayfield.WIDTH / 2) + initialOffset;

            float leftSideFromMiddle = (CatchPlayfield.WIDTH / 2) - initialOffset;

            float minMapSide = 0;

            float maxMapSide = CatchPlayfield.WIDTH;

            if (hitObject is Banana)
            {
                if (hitObject.XOffset < rightSideFromMiddle && hitObject.XOffset > leftSideFromMiddle)
                {

                    if (hitObject.XOffset < CatchPlayfield.WIDTH / 2) hitObject.XOffset = Math.Clamp(hitObject.XOffset, minMapSide, leftSideFromMiddle);

                    else if (hitObject.XOffset == CatchPlayfield.WIDTH / 2)
                    {

                        invertGen = !invertGen; //Invert

                        if (invertGen)
                        {
                            hitObject.XOffset = Math.Clamp(hitObject.XOffset, minMapSide, leftSideFromMiddle);
                        }
                        else
                        {
                            hitObject.XOffset = Math.Clamp(hitObject.XOffset, rightSideFromMiddle, maxMapSide);
                        }
                    }

                    else hitObject.XOffset = Math.Clamp(hitObject.XOffset, rightSideFromMiddle, maxMapSide);
                }

                return;

            }
            if (hitObject is TinyDroplet)
            {
                initialOffset += hitObject.XOffset;
            }

            float currentObjectComparedX = hitObject.OriginalX;

            float currentObjectComparisonX = CatchPlayfield.WIDTH / 2;

            if (currentObjectComparedX < rightSideFromMiddle && currentObjectComparedX > leftSideFromMiddle)
            {

                if (currentObjectComparedX > currentObjectComparisonX) hitObject.XOffset = Math.Clamp(currentObjectComparedX + initialOffset, rightSideFromMiddle, maxMapSide) - currentObjectComparedX;

                else if (currentObjectComparedX == currentObjectComparisonX)
                {
                    invertGen = !invertGen; //Invert
                    if (invertGen)
                    {
                        hitObject.XOffset = Math.Clamp(currentObjectComparedX + initialOffset, rightSideFromMiddle, maxMapSide) - currentObjectComparedX;
                    }
                    else
                    {
                        hitObject.XOffset = Math.Clamp(currentObjectComparedX - initialOffset, minMapSide, leftSideFromMiddle) - currentObjectComparedX;
                    }

                }

                else hitObject.XOffset = Math.Clamp(currentObjectComparedX - initialOffset, minMapSide, leftSideFromMiddle) - currentObjectComparedX;
            }
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

        public int GetMovementStatus()
        {
            if (NoDashingOffsets) return 2; //Cannot use regular dash
            else if (NoHyperOffsets) return 1; //Cannot use hypers
            return 0; //No difference
        }

        private static void elaborateHyperdashPalpableCatchHitObject(List<double> halfCatcherWidthList, List<PalpableCatchHitObject> hitObjectsList, List<bool> modBools, List<double> modValues)
        {
            double catcherSpeed;
            bool edgeReduction = false;
            bool placeHyperCondition = false;

            bool checkOriginalBeatmap = false;

            int movementStatus = (int)modValues[(int)ModValues.MovementStatus];

            bool movementStatusChanged = movementStatus == (int)MovementType.NoChange ? false : true;

            if (movementStatus == (int)MovementType.NoDashing) catcherSpeed = Catcher.BASE_WALK_SPEED;
            else catcherSpeed = Catcher.BASE_DASH_SPEED;

            double halfCatcherWidthModifiedBeatmap = halfCatcherWidthList[0];
            int lastDirection = 0;
            double lastExcessModifiedBeatmap = halfCatcherWidthList[0];
            double distanceToNextModifiedBeatmap;

            double halfCatcherWidthOriginalBeatmap = -1;
            double lastExcessOriginalBeatmap = -1;
            double distanceToNextOriginalBeatmap;

            if (halfCatcherWidthList.Count > 1)
            {
                halfCatcherWidthOriginalBeatmap = halfCatcherWidthList[1];
                lastExcessOriginalBeatmap = halfCatcherWidthList[1];
                checkOriginalBeatmap = true;
            }

            float edgeReductionValue = 0;

            if (movementStatusChanged) edgeReductionValue = (float)(halfCatcherWidthModifiedBeatmap * (float)modValues[(int)ModValues.EdgeReduction]);

            for (int i = 0; i < hitObjectsList.Count - 1; i++)
            {
                if (movementStatusChanged) edgeReduction = false;

                var currentObject = hitObjectsList[i];
                var nextObject = hitObjectsList[i + 1];

                // This is not nomod
                // Reset variables in-case values have changed (e.g. after applying HR)
                currentObject.HyperDashTarget = null;
                currentObject.DistanceToHyperDash = 0;

                int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable

                distanceToNextModifiedBeatmap = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcessModifiedBeatmap : halfCatcherWidthModifiedBeatmap);
                distanceToNextOriginalBeatmap = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcessOriginalBeatmap : halfCatcherWidthOriginalBeatmap);

                float distanceToHyperModifiedBeatmap = (float)(timeToNext * catcherSpeed - distanceToNextModifiedBeatmap);
                float distanceToHyperOriginalBeatmap = -1;
                if (checkOriginalBeatmap) distanceToHyperOriginalBeatmap = (float)(timeToNext * catcherSpeed - distanceToNextOriginalBeatmap);

                if (!movementStatusChanged && modBools[(int)ModBools.AE_EdgeReduction]) placeHyperCondition = Math.Abs(distanceToNextModifiedBeatmap) > 2 * halfCatcherWidthModifiedBeatmap; //another easy edge reduction

                if (movementStatusChanged && distanceToHyperModifiedBeatmap < edgeReductionValue) edgeReduction = true; //no dashing or no hyper dash

                if (distanceToHyperModifiedBeatmap < 0)
                {
                    if (movementStatusChanged)
                    {
                        if (edgeReduction)
                        {
                            if (thisDirection == 1)
                            {
                                nextObject.XOffset += distanceToHyperModifiedBeatmap;
                                nextObject.XOffset -= edgeReductionValue;
                            }
                            else if (thisDirection == -1)
                            {
                                nextObject.XOffset -= distanceToHyperModifiedBeatmap;
                                nextObject.XOffset += edgeReductionValue;
                            }

                            currentObject.DistanceToHyperDash = edgeReductionValue;
                            lastExcessModifiedBeatmap = Math.Clamp(edgeReductionValue, 0, halfCatcherWidthModifiedBeatmap);

                        }

                        else
                        {
                            if (thisDirection == 1)
                            {
                                nextObject.XOffset += distanceToHyperModifiedBeatmap;
                            }
                            else if (thisDirection == -1)
                            {
                                nextObject.XOffset -= distanceToHyperModifiedBeatmap;
                            }

                            lastExcessModifiedBeatmap = 0; //Should be correct (?)
                        }

                    }

                    else //nomod
                    {
                        currentObject.HyperDashTarget = nextObject;
                        lastExcessModifiedBeatmap = halfCatcherWidthModifiedBeatmap;
                    }

                }

                if (checkOriginalBeatmap && distanceToHyperOriginalBeatmap < 0)
                {
                    if (!movementStatusChanged)
                    {
                        currentObject.HyperDashTarget = nextObject;
                        lastExcessOriginalBeatmap = halfCatcherWidthOriginalBeatmap;
                    }
                }

                else if (distanceToHyperModifiedBeatmap >= 0 || checkOriginalBeatmap && distanceToHyperOriginalBeatmap >= 0)
                {

                    if (movementStatusChanged)
                    {

                        if (edgeReduction)
                        {
                            if (thisDirection == 1)
                            {
                                nextObject.XOffset -= (float)(edgeReductionValue - distanceToHyperModifiedBeatmap);
                            }
                            else if (thisDirection == -1)
                            {
                                nextObject.XOffset += (float)(edgeReductionValue - distanceToHyperModifiedBeatmap);
                            }
                            currentObject.DistanceToHyperDash = edgeReductionValue;
                            lastExcessModifiedBeatmap = Math.Clamp(edgeReductionValue, 0, halfCatcherWidthModifiedBeatmap);
                        }
                        else
                        {
                            currentObject.DistanceToHyperDash = distanceToHyperModifiedBeatmap;
                            lastExcessModifiedBeatmap = Math.Clamp(distanceToHyperModifiedBeatmap, 0, halfCatcherWidthModifiedBeatmap);
                        }
                    }

                    else //nomod
                    {
                        currentObject.DistanceToHyperDash = distanceToHyperModifiedBeatmap;
                        lastExcessModifiedBeatmap = Math.Clamp(distanceToHyperModifiedBeatmap, 0, halfCatcherWidthModifiedBeatmap);
                    }


                    if (checkOriginalBeatmap && modBools[(int)ModBools.AE_EdgeReduction] && placeHyperCondition)
                    {
                        currentObject.HyperDashTarget = nextObject;
                        currentObject.DistanceToHyperDash = 0;
                        lastExcessOriginalBeatmap = halfCatcherWidthOriginalBeatmap;
                    }

                }

                lastDirection = thisDirection;
            }

        }

        private static void elaborateNewHyperdashFromExistingPalpableCatchHitObject(double halfCatcherWidth_input, int lastDirection_input, List<PalpableCatchHitObject> listHitObjects, bool anotherEasyEdgeReduction)
        {
            double catcherSpeed;

            catcherSpeed = Catcher.BASE_DASH_SPEED;

            double halfCatcherWidth = halfCatcherWidth_input;
            int lastDirection = lastDirection_input;
            double lastExcess = halfCatcherWidth_input;
            double distanceToNext;
            bool placeHyperCondition = false;
            bool isAnotherEasyEdgeReduction = anotherEasyEdgeReduction;

            for (int i = 0; i < listHitObjects.Count - 1; i++)
            {

                var currentObject = listHitObjects[i];
                var nextObject = listHitObjects[i + 1];

                int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable

                distanceToNext = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth); //nomod

                float distanceToHyper = (float)(timeToNext * catcherSpeed - distanceToNext); //nomod

                if (isAnotherEasyEdgeReduction) placeHyperCondition = Math.Abs(distanceToNext) > 2 * halfCatcherWidth; //another easy edge reduction

                if (distanceToHyper < 0)
                {
                    currentObject.HyperDashTarget = nextObject;
                    currentObject.DistanceToHyperDash = 0;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    if (isAnotherEasyEdgeReduction && placeHyperCondition)
                    {
                        currentObject.HyperDashTarget = nextObject;
                        currentObject.DistanceToHyperDash = 0;
                        lastExcess = halfCatcherWidth;
                    }
                    else
                    {
                        if (currentObject.HyperDashTarget == null)
                        {
                            currentObject.DistanceToHyperDash = distanceToHyper;
                            lastExcess = Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
                        }
                        else
                        {
                            currentObject.DistanceToHyperDash = 0;
                            lastExcess = halfCatcherWidth;
                        }
                    }
                }

                lastDirection = thisDirection; //nomod

            }

        }

        private static void initialiseHyperDash(IBeatmap beatmap, List<bool> modBools, List<double> modValues)
        {
            List<PalpableCatchHitObject> palpableObjects = new List<PalpableCatchHitObject>();

            List<PalpableCatchHitObject> palpableObjectsLeft = new List<PalpableCatchHitObject>();

            List<PalpableCatchHitObject> palpableObjectsRight = new List<PalpableCatchHitObject>();

            List<double> halfCatcherWidthList = new List<double>();

            double halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;
            double halfCatcherWidthOriginalBeatmap = 0;
            double halfCatcherWidthModifiedBeatmap = 0;

            int movementStatus = (int)modValues[(int)ModValues.MovementStatus];

            if (modBools[(int)ModBools.AE])
            {
                BeatmapDifficulty difficultyCustom = new BeatmapDifficulty();
                beatmap.Difficulty.CopyTo(difficultyCustom);
                difficultyCustom.CircleSize *= 2;

                halfCatcherWidthModifiedBeatmap = halfCatcherWidth;
                halfCatcherWidthModifiedBeatmap /= Catcher.ALLOWED_CATCH_RANGE; //Modified is ready

                halfCatcherWidthOriginalBeatmap = Catcher.CalculateCatchWidth(difficultyCustom) / 2;
                halfCatcherWidthOriginalBeatmap /= Catcher.ALLOWED_CATCH_RANGE; //Original is ready

                halfCatcherWidthList.Add(halfCatcherWidthModifiedBeatmap);
                halfCatcherWidthList.Add(halfCatcherWidthOriginalBeatmap);
            }

            else
            {
                // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
                // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
                // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
                halfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

                halfCatcherWidthList.Add(halfCatcherWidth);
            }

            double halfPlayfield = CatchPlayfield.WIDTH / 2;

            double leftSidePlayfield = halfPlayfield - halfCatcherWidth;
            double rightSidePlayfield = halfPlayfield + halfCatcherWidth;

            foreach (var currentObject in beatmap.HitObjects)
            {
                if (currentObject is Fruit fruitObject)
                {
                    if (modBools[(int)ModBools.TC])
                    {
                        if (((PalpableCatchHitObject)currentObject).EffectiveX <= leftSidePlayfield) palpableObjectsLeft.Add(fruitObject);
                        else palpableObjectsRight.Add(fruitObject);
                    }
                    else palpableObjects.Add(fruitObject); //nomod
                }

                if (currentObject is JuiceStream)
                {
                    foreach (var juice in currentObject.NestedHitObjects)
                    {
                        if (movementStatus != (int)MovementType.NoChange)
                        {
                            if (juice is PalpableCatchHitObject palpableObject)
                                if (modBools[(int)ModBools.TC])
                                {
                                    if (palpableObject.EffectiveX <= leftSidePlayfield) palpableObjectsLeft.Add(palpableObject);
                                    else palpableObjectsRight.Add(palpableObject);
                                }
                                else palpableObjects.Add(palpableObject); //nomod
                        }
                        else
                        {
                            if (juice is PalpableCatchHitObject palpableObject && !(juice is TinyDroplet))
                                if (modBools[(int)ModBools.TC])
                                {
                                    if (palpableObject.EffectiveX <= leftSidePlayfield) palpableObjectsLeft.Add(palpableObject);
                                    else palpableObjectsRight.Add(palpableObject);
                                }
                                else palpableObjects.Add(palpableObject); //nomod
                        }
                    }
                }
            }

            if (modBools[(int)ModBools.TC])
            {
                palpableObjectsLeft.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
                palpableObjectsRight.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
            }
            else palpableObjects.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime)); //nomod

            if (modBools[(int)ModBools.TC])
            {

                //if (modBools[(int)ModBools.AE])
                //{
                //elaborateHyperdashPalpableCatchHitObject(halfCatcherWidthList, lastDirectionList, palpableObjectsLeft, modBools, modValues); //Used to elaborate hyperdash from NM spacing (!) [Left]
                //if (modBools[(int)ModBools.AE_NewHyperDashes] && movementStatus == (int)MovementType.NoChange) elaborateNewHyperdashFromExistingPalpableCatchHitObject(halfCatcherWidthModifiedBeatmap, 0, palpableObjectsLeft, isAnotherEasyEdgeReduction); //Only used to add hyperdash from new CS spacing OR calculate new SR with new CS (!) [Left]

                //elaborateHyperdashPalpableCatchHitObject(halfCatcherWidthList, lastDirectionList, palpableObjectsRight, modBools, modValues); //Used to elaborate hyperdash from NM spacing (!) [Right]
                //if (modBools[(int)ModBools.AE_NewHyperDashes] && movementStatus == (int)MovementType.NoChange) elaborateNewHyperdashFromExistingPalpableCatchHitObject(halfCatcherWidthModifiedBeatmap, 0, palpableObjectsRight, isAnotherEasyEdgeReduction); //Only used to add hyperdash from new CS spacing OR calculate new SR with new CS  (!) [Right]
                //}

                //else
                //{
                elaborateHyperdashPalpableCatchHitObject(halfCatcherWidthList, palpableObjectsLeft, modBools, modValues); //[Left]
                elaborateHyperdashPalpableCatchHitObject(halfCatcherWidthList, palpableObjectsRight, modBools, modValues); //[Right]
                //}
            }
            else
            {
                //if (modBools[(int)ModBools.AE])
                //{
                elaborateHyperdashPalpableCatchHitObject(halfCatcherWidthList, palpableObjects, modBools, modValues); // [Standard]
                //if (modBools[(int)ModBools.AE_NewHyperDashes] && movementStatus == (int)MovementType.NoChange) elaborateNewHyperdashFromExistingPalpableCatchHitObject(halfCatcherWidthModifiedBeatmap, 0, palpableObjects, isAnotherEasyEdgeReduction); //Only used to add hyperdash from new CS spacing OR calculate new SR with new CS  (!)
                //}

                //else elaborateHyperdashPalpableCatchHitObject(halfCatcherWidthList, lastDirectionList, palpableObjects, modBools, modValues); //This is the main hyperdash gen + other mods
            }
        }
    }
}
