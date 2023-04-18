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
        public bool NoHyperDashingOffsets { get; set; }
        public bool AllHyperDashOffsets { get; set; }

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
            AH, //6
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
                 NoHyperDashingOffsets, //5
                 AllHyperDashOffsets, //6
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
            else if (NoHyperDashingOffsets) return 1; //Cannot use hypers
            return 0; //No difference
        }

        private static void initialiseHyperdashOnPalpableCatchHitObjects(List<double> halfCatcherWidthList, List<bool> modBools, List<double> modValues, bool checkOriginal, bool checkModified, bool modifyOriginal, bool modifyModified, List<List<PalpableCatchHitObject>> ListOfObjectsList)
        {
            //Generic variables
            int movementStatus = (int)modValues[(int)ModValues.MovementStatus];
            bool movementStatusChanged = movementStatus == (int)MovementType.NoChange ? false : true;
            double catcherSpeed = movementStatus == (int)MovementType.NoDashing ? Catcher.BASE_WALK_SPEED : Catcher.BASE_DASH_SPEED;
            double distanceToNextModifiedCatcher;
            int lastDirection;

            //Extra mod-dependent variables
            bool edgeReduction = false;
            bool removeExtraHyperCondition = false;
            bool updateHyperWithOriginalCatcher = checkOriginal;
            bool updateHyperWithModifiedCatcher = checkModified;
            bool modifyOffsetsWithOriginalCatcher = modifyOriginal;
            bool modifyOffsetsWithModifiedCatcher = modifyModified;
            float edgeReductionValue = 0;
            double halfCatcherWidthOriginalCatcher;
            double lastExcessOriginalCatcher;
            double distanceToNextWithOriginalCatcher;

            //Always initialized
            double halfCatcherWidthModifiedCatcher;
            double lastExcessModifiedCatcher;

            //Stop everything if the list is null
            if (ListOfObjectsList == null) return;

            for (int index = 0; index < ListOfObjectsList.Count; index++)
            {

                //Modify offsets for original Catcher
                if (modifyOffsetsWithOriginalCatcher)
                {
                    //Always re-initialized
                    halfCatcherWidthOriginalCatcher = halfCatcherWidthList[1];
                    lastExcessOriginalCatcher = halfCatcherWidthList[1];
                    //Last direction is always in common
                    lastDirection = 0;

                    //Apply edge reduction to the new modified map if the mod ND or NH were used
                    if (movementStatusChanged) edgeReductionValue = (float)(halfCatcherWidthOriginalCatcher * (float)modValues[(int)ModValues.EdgeReduction]);

                    //Here we'll actually modify offsets of the patterns when required.
                    for (int i = 0; i < ListOfObjectsList[index].Count - 1; i++)
                    {
                        if (movementStatusChanged) edgeReduction = false;

                        var currentObject = ListOfObjectsList[index][i];
                        var nextObject = ListOfObjectsList[index][i + 1];

                        int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                        double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable

                        distanceToNextWithOriginalCatcher = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcessOriginalCatcher : halfCatcherWidthOriginalCatcher);

                        float distanceToHyperOriginalCatcher = (float)(timeToNext * catcherSpeed - distanceToNextWithOriginalCatcher);

                        if (movementStatusChanged && distanceToHyperOriginalCatcher < edgeReductionValue) edgeReduction = true; //ND or NH edge reduction condition

                        //Offset when Hyper pattern
                        if (distanceToHyperOriginalCatcher < 0)
                        {
                            if (movementStatusChanged)
                            {
                                if (edgeReduction)
                                {
                                    nextObject.XOffset += (thisDirection == 1) ? distanceToHyperOriginalCatcher : -distanceToHyperOriginalCatcher;
                                    nextObject.XOffset += (thisDirection == 1) ? -edgeReductionValue : edgeReductionValue;

                                    currentObject.DistanceToHyperDash = edgeReductionValue;
                                    lastExcessOriginalCatcher = Math.Clamp(edgeReductionValue, 0, halfCatcherWidthOriginalCatcher);
                                }

                                else
                                {
                                    nextObject.XOffset += (thisDirection == 1) ? distanceToHyperOriginalCatcher : -distanceToHyperOriginalCatcher;

                                    lastExcessOriginalCatcher = 0;
                                }
                            }

                        }

                        //Offset when non-Hyper pattern
                        else
                        {
                            if (movementStatusChanged)
                            {

                                if (edgeReduction)
                                {
                                    nextObject.XOffset += (thisDirection == 1) ? -(float)(edgeReductionValue - distanceToHyperOriginalCatcher) : (float)(edgeReductionValue - distanceToHyperOriginalCatcher);

                                    currentObject.DistanceToHyperDash = edgeReductionValue;
                                    lastExcessOriginalCatcher = Math.Clamp(edgeReductionValue, 0, halfCatcherWidthOriginalCatcher);
                                }

                                else
                                {
                                    currentObject.DistanceToHyperDash = distanceToHyperOriginalCatcher;
                                    lastExcessOriginalCatcher = Math.Clamp(distanceToHyperOriginalCatcher, 0, halfCatcherWidthOriginalCatcher);
                                }
                            }
                        }

                        lastDirection = thisDirection;
                    }
                }

                //Update hyper dashes for original Catcher
                if (updateHyperWithOriginalCatcher)
                {

                    //Always re-initialized
                    halfCatcherWidthOriginalCatcher = halfCatcherWidthList[1];
                    lastExcessOriginalCatcher = halfCatcherWidthList[1];
                    //Last direction is always in common
                    lastDirection = 0;

                    for (int i = 0; i < ListOfObjectsList[index].Count - 1; i++)
                    {

                        var currentObject = ListOfObjectsList[index][i];
                        var nextObject = ListOfObjectsList[index][i + 1];

                        // Reset variables in-case values have changed (e.g. after applying HR)
                        currentObject.HyperDashTarget = null;
                        currentObject.DistanceToHyperDash = 0;

                        int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                        double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable

                        distanceToNextWithOriginalCatcher = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcessOriginalCatcher : halfCatcherWidthOriginalCatcher);

                        float distanceToHyperOriginalCatcher = (float)(timeToNext * catcherSpeed - distanceToNextWithOriginalCatcher);

                        //Hyper placement
                        if (distanceToHyperOriginalCatcher < 0 || modBools[(int)ModBools.AH])
                        {
                            currentObject.HyperDashTarget = nextObject;
                            lastExcessOriginalCatcher = halfCatcherWidthOriginalCatcher;
                        }

                        //Non-Hyper placement
                        else
                        {
                            currentObject.DistanceToHyperDash = distanceToHyperOriginalCatcher;
                            lastExcessOriginalCatcher = Math.Clamp(distanceToHyperOriginalCatcher, 0, halfCatcherWidthOriginalCatcher);
                        }

                        lastDirection = thisDirection;
                    }

                }

                //Modify offsets for new MODDED Catcher
                if (modifyOffsetsWithModifiedCatcher)
                {
                    //Always re-initialized
                    halfCatcherWidthModifiedCatcher = halfCatcherWidthList[0];
                    lastExcessModifiedCatcher = halfCatcherWidthList[0];
                    //Last direction is always in common
                    lastDirection = 0;

                    //Apply edge reduction to the new modified map if the mod ND or NH were used
                    if (movementStatusChanged) edgeReductionValue = (float)(halfCatcherWidthModifiedCatcher * (float)modValues[(int)ModValues.EdgeReduction]);

                    //Here we'll actually modify offsets of the patterns when required.
                    for (int i = 0; i < ListOfObjectsList[index].Count - 1; i++)
                    {
                        if (movementStatusChanged) edgeReduction = false;

                        var currentObject = ListOfObjectsList[index][i];
                        var nextObject = ListOfObjectsList[index][i + 1];

                        int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                        double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable

                        distanceToNextModifiedCatcher = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcessModifiedCatcher : halfCatcherWidthModifiedCatcher);

                        float distanceToHyperModifiedCatcher = (float)(timeToNext * catcherSpeed - distanceToNextModifiedCatcher);

                        if (movementStatusChanged && distanceToHyperModifiedCatcher < edgeReductionValue) edgeReduction = true; //ND or NH edge reduction condition

                        //Offset when Hyper pattern
                        if (distanceToHyperModifiedCatcher < 0)
                        {
                            if (movementStatusChanged)
                            {
                                if (edgeReduction)
                                {
                                    nextObject.XOffset += (thisDirection == 1) ? distanceToHyperModifiedCatcher : -distanceToHyperModifiedCatcher;
                                    nextObject.XOffset += (thisDirection == 1) ? -edgeReductionValue : edgeReductionValue;

                                    currentObject.DistanceToHyperDash = edgeReductionValue;
                                    lastExcessModifiedCatcher = Math.Clamp(edgeReductionValue, 0, halfCatcherWidthModifiedCatcher);
                                }

                                else
                                {
                                    nextObject.XOffset += (thisDirection == 1) ? distanceToHyperModifiedCatcher : -distanceToHyperModifiedCatcher;

                                    lastExcessModifiedCatcher = 0;
                                }
                            }

                        }

                        //Offset when non-Hyper pattern
                        else
                        {
                            if (movementStatusChanged)
                            {

                                if (edgeReduction)
                                {
                                    nextObject.XOffset += (thisDirection == 1) ? -(float)(edgeReductionValue - distanceToHyperModifiedCatcher) : (float)(edgeReductionValue - distanceToHyperModifiedCatcher);

                                    currentObject.DistanceToHyperDash = edgeReductionValue;
                                    lastExcessModifiedCatcher = Math.Clamp(edgeReductionValue, 0, halfCatcherWidthModifiedCatcher);
                                }

                                else
                                {
                                    currentObject.DistanceToHyperDash = distanceToHyperModifiedCatcher;
                                    lastExcessModifiedCatcher = Math.Clamp(distanceToHyperModifiedCatcher, 0, halfCatcherWidthModifiedCatcher);
                                }
                            }
                        }

                        lastDirection = thisDirection;
                    }
                }

                //Update hyper dashes for new MODDED Catcher
                if (updateHyperWithModifiedCatcher)
                {
                    //Always re-initialized
                    halfCatcherWidthModifiedCatcher = halfCatcherWidthList[0];
                    lastExcessModifiedCatcher = halfCatcherWidthList[0];
                    //Last direction is always in common
                    lastDirection = 0;

                    //Here we'll actually modify offsets of the patterns when required.
                    for (int i = 0; i < ListOfObjectsList[index].Count - 1; i++)
                    {
                        var currentObject = ListOfObjectsList[index][i];
                        var nextObject = ListOfObjectsList[index][i + 1];

                        //Do not repeat this step if we already did it with the original catcher
                        if (!updateHyperWithOriginalCatcher)
                        {
                            // Reset variables in-case values have changed (e.g. after applying HR)
                            currentObject.HyperDashTarget = null;
                            currentObject.DistanceToHyperDash = 0;
                        }

                        int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                        double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable

                        distanceToNextModifiedCatcher = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcessModifiedCatcher : halfCatcherWidthModifiedCatcher);

                        float distanceToHyperModifiedCatcher = (float)(timeToNext * catcherSpeed - distanceToNextModifiedCatcher);

                        if (modBools[(int)ModBools.AE]) removeExtraHyperCondition = Math.Abs(distanceToNextModifiedCatcher) <= 2 * halfCatcherWidthModifiedCatcher; //AE Generic Hyper Dashes removal condition

                        //Hyper placement
                        if (distanceToHyperModifiedCatcher < 0 || modBools[(int)ModBools.AH])
                        {
                            currentObject.HyperDashTarget = nextObject;
                            lastExcessModifiedCatcher = halfCatcherWidthModifiedCatcher;
                        }

                        //Non-Hyper placement
                        else
                        {

                            if (modBools[(int)ModBools.AE] && !removeExtraHyperCondition && currentObject.HyperDashTarget != null)
                            {
                                lastExcessModifiedCatcher = halfCatcherWidthModifiedCatcher;
                            }

                            else
                            {
                                currentObject.DistanceToHyperDash = distanceToHyperModifiedCatcher;
                                lastExcessModifiedCatcher = Math.Clamp(distanceToHyperModifiedCatcher, 0, halfCatcherWidthModifiedCatcher);
                            }

                        }

                        lastDirection = thisDirection;
                    }
                }
            }
        }
        private static void initialiseHyperDash(IBeatmap beatmap, List<bool> modBools, List<double> modValues)
        {
            List<PalpableCatchHitObject> palpableObjects = new List<PalpableCatchHitObject>();

            List<PalpableCatchHitObject> palpableObjectsLeftSide = new List<PalpableCatchHitObject>();

            List<PalpableCatchHitObject> palpableObjectsRightSide = new List<PalpableCatchHitObject>();

            List<List<PalpableCatchHitObject>> ListOfObjectsList = new List<List<PalpableCatchHitObject>>();

            List<double> halfCatcherWidthList = new List<double>();

            double halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;

            double halfCatcherWidthOriginalBeatmap = 0;
            double halfCatcherWidthModifiedBeatmap = 0;

            bool updateHyperWithOriginalCatcher = false;

            bool updateHyperWithModifiedCatcher = true;

            bool modifyOffsetsWithOriginalCatcher = false;

            bool modifyOffsetsWithModifiedCatcher = true;

            int movementStatus = (int)modValues[(int)ModValues.MovementStatus];


            if (modBools[(int)ModBools.TC])
            {
                ListOfObjectsList.Add(palpableObjectsLeftSide); //0
                ListOfObjectsList.Add(palpableObjectsRightSide); //1
            }

            else
            {
                ListOfObjectsList.Add(palpableObjects); //0
            }

            if (modBools[(int)ModBools.AE])
            {

                BeatmapDifficulty difficultyCustom = new BeatmapDifficulty();
                beatmap.Difficulty.CopyTo(difficultyCustom);
                difficultyCustom.CircleSize *= 2;

                halfCatcherWidthModifiedBeatmap = halfCatcherWidth;
                halfCatcherWidthModifiedBeatmap /= Catcher.ALLOWED_CATCH_RANGE; //Modified is ready

                halfCatcherWidthOriginalBeatmap = Catcher.CalculateCatchWidth(difficultyCustom) / 2;
                halfCatcherWidthOriginalBeatmap /= Catcher.ALLOWED_CATCH_RANGE; //Original is ready

                halfCatcherWidthList.Add(halfCatcherWidthModifiedBeatmap); //0
                halfCatcherWidthList.Add(halfCatcherWidthOriginalBeatmap); //1

                updateHyperWithOriginalCatcher = (movementStatus == (int)MovementType.NoChange) ? true : false; //Only use the original hyper dashes if it is a valid movement type
                updateHyperWithModifiedCatcher = (movementStatus == (int)MovementType.NoChange) ? modBools[(int)ModBools.AE_NewHyperDashes] : false; //Only use the new CS hyper dashes if it is a valid movement type and the option is selected
                modifyOffsetsWithOriginalCatcher = true;  //The map should be modified only with the original CS
                modifyOffsetsWithModifiedCatcher = false; //The map shouldn't change with the new CS
            }

            else
            {

                // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
                // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
                // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
                halfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

                halfCatcherWidthList.Add(halfCatcherWidth); //0
            }

            foreach (var currentObject in beatmap.HitObjects)
            {
                AddHitObjectToCorrectList((CatchHitObject)currentObject, modBools, modValues, Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2, ListOfObjectsList);
            }

            if (modBools[(int)ModBools.TC])
            {
                ListOfObjectsList[0].Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
                ListOfObjectsList[1].Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
            }
            else
            {
                ListOfObjectsList[0].Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
            }

            initialiseHyperdashOnPalpableCatchHitObjects(halfCatcherWidthList, modBools, modValues, updateHyperWithOriginalCatcher, updateHyperWithModifiedCatcher, modifyOffsetsWithOriginalCatcher, modifyOffsetsWithModifiedCatcher, ListOfObjectsList);

        }

        public static void AddHitObjectToCorrectList(CatchHitObject currentObject, List<bool> modBools, List<double> modValues, double halfCatcherWidth, List<List<PalpableCatchHitObject>> ListOfObjectsList)
        {
            double halfPlayfield = CatchPlayfield.WIDTH / 2;
            double leftSidePlayfield = halfPlayfield - halfCatcherWidth;
            double rightSidePlayfield = halfPlayfield + halfCatcherWidth;

            if (ListOfObjectsList == null) return;

            if (currentObject is Fruit fruitObject)
            {

                if (modBools[(int)ModBools.TC])
                {
                    if (currentObject.EffectiveX <= leftSidePlayfield) ListOfObjectsList[0].Add(fruitObject);
                    if (currentObject.EffectiveX >= rightSidePlayfield) ListOfObjectsList[1].Add(fruitObject);
                }
                else ListOfObjectsList[0].Add(fruitObject);
            }

            if (currentObject is JuiceStream)
            {
                foreach (var juice in currentObject.NestedHitObjects)
                {
                    if (modValues[(int)ModValues.MovementStatus] != (int)MovementType.NoChange)
                    {
                        if (juice is PalpableCatchHitObject palpableObject)
                            if (modBools[(int)ModBools.TC])
                            {
                                if (palpableObject.EffectiveX <= leftSidePlayfield) ListOfObjectsList[0].Add(palpableObject);
                                if (palpableObject.EffectiveX >= rightSidePlayfield) ListOfObjectsList[1].Add(palpableObject);
                            }
                            else ListOfObjectsList[0].Add(palpableObject);
                    }
                    else
                    {
                        if (juice is PalpableCatchHitObject palpableObject && !(juice is TinyDroplet))
                            if (modBools[(int)ModBools.TC])
                            {
                                if (palpableObject.EffectiveX <= leftSidePlayfield) ListOfObjectsList[0].Add(palpableObject);
                                if (palpableObject.EffectiveX >= rightSidePlayfield) ListOfObjectsList[1].Add(palpableObject);
                            }
                            else ListOfObjectsList[0].Add(palpableObject);
                    }
                }
            }
        }
    }
}
