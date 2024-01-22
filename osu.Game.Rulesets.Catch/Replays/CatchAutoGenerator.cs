// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    internal class CatchAutoGenerator : AutoGenerator<CatchReplayFrame>
    {
        public new CatchBeatmap Beatmap => (CatchBeatmap)base.Beatmap;

        public CatchAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        private bool isComboHitObject(PalpableCatchHitObject hitObject) => hitObject is Fruit || (hitObject is Droplet && hitObject is not TinyDroplet);

        protected override void GenerateFrames()
        {
            if (Beatmap.HitObjects.Count == 0)
                return;

            float lastPosition = CatchPlayfield.CENTER_X;
            double lastTime = 0;
            bool isAspire = false;

            void moveToNext(PalpableCatchHitObject h)
            {
                float positionChange = Math.Abs(lastPosition - h.EffectiveX);
                double timeAvailable = h.StartTime - lastTime;

                if (timeAvailable < 0)
                {
                    return;
                }

                // So we can either make it there without a dash or not.
                // If positionChange is 0, we don't need to move, so speedRequired should also be 0 (could be NaN if timeAvailable is 0 too)
                // The case where positionChange > 0 and timeAvailable == 0 results in PositiveInfinity which provides expected beheaviour.
                double speedRequired = positionChange == 0 ? 0 : positionChange / timeAvailable;

                bool dashRequired = speedRequired > Catcher.BASE_WALK_SPEED;
                bool impossibleJump = speedRequired > Catcher.BASE_DASH_SPEED || isAspire;

                // todo: get correct catcher size, based on difficulty CS.
                //const float catcher_width_half = Catcher.BASE_SIZE * 0.3f * 0.5f;

                float halfCatcherWidth = Catcher.CalculateCatchWidth(Beatmap.Difficulty) * 0.5f;

                if (!isAspire && lastPosition - halfCatcherWidth < h.EffectiveX && lastPosition + halfCatcherWidth > h.EffectiveX)
                {
                    // we are already in the correct range.
                    lastTime = h.StartTime;
                    addFrame(h.StartTime, lastPosition);
                    return;
                }

                if (impossibleJump)
                {
                    addFrame(h.StartTime, h.EffectiveX);
                }
                else if (h.HyperDash)
                {
                    addFrame(h.StartTime - timeAvailable, lastPosition);
                    addFrame(h.StartTime, h.EffectiveX);
                }
                else if (dashRequired)
                {
                    // we do a movement in two parts - the dash part then the normal part...
                    double timeAtNormalSpeed = positionChange / Catcher.BASE_WALK_SPEED;
                    double timeWeNeedToSave = timeAtNormalSpeed - timeAvailable;
                    double timeAtDashSpeed = timeWeNeedToSave / 2;

                    float midPosition = (float)Interpolation.Lerp(lastPosition, h.EffectiveX, (float)timeAtDashSpeed / timeAvailable);

                    // dash movement
                    addFrame(h.StartTime - timeAvailable + 1, lastPosition, true);
                    addFrame(h.StartTime - timeAvailable + timeAtDashSpeed, midPosition);
                    addFrame(h.StartTime, h.EffectiveX);
                }
                else
                {
                    double timeBefore = positionChange / Catcher.BASE_WALK_SPEED;

                    addFrame(h.StartTime - timeBefore, lastPosition);
                    addFrame(h.StartTime, h.EffectiveX);
                }

                lastTime = h.StartTime;
                lastPosition = h.EffectiveX;
            }

            List<PalpableCatchHitObject> toFilterFinalObjectList = new List<PalpableCatchHitObject>();
            List<PalpableCatchHitObject> finalObjectList = new List<PalpableCatchHitObject>();
            List<PalpableCatchHitObject> finalAspireList = new List<PalpableCatchHitObject>();

            List<KeyValuePair<double, PalpableCatchHitObject>> KeyValuePairAspireObjectList = new List<KeyValuePair<double, PalpableCatchHitObject>>();

            //Add objects to toFilterFinalObjectList and KeyValuePairAspireObjectList
            foreach (var obj in Beatmap.HitObjects)
            {
                if (obj is PalpableCatchHitObject palpableObject)
                {
                    if (toFilterFinalObjectList.Exists(x => x.StartTime == obj.StartTime))
                    {
                        KeyValuePairAspireObjectList.Add(new KeyValuePair<double, PalpableCatchHitObject>(obj.StartTime, palpableObject));
                    }
                    else
                        toFilterFinalObjectList.Add(palpableObject);
                }

                foreach (var nestedObj in obj.NestedHitObjects.Cast<CatchHitObject>())
                {
                    if (nestedObj is PalpableCatchHitObject palpableNestedObject)
                    {
                        if (toFilterFinalObjectList.Exists(x => x.StartTime == nestedObj.StartTime))
                        {
                            KeyValuePairAspireObjectList.Add(new KeyValuePair<double, PalpableCatchHitObject>(nestedObj.StartTime, palpableNestedObject));
                        }
                        else
                            toFilterFinalObjectList.Add(palpableNestedObject);
                    }
                }
            }

            //filter the toFilterFinalObjectList into finalObjectList and add remaining objects to KeyValuePairAspireObjectList
            foreach (var obj in toFilterFinalObjectList)
            {
                if (obj is PalpableCatchHitObject palpableObject)
                {
                    if (!KeyValuePairAspireObjectList.Exists(x => x.Key == obj.StartTime))
                    {
                        finalObjectList.Add(palpableObject);
                    }
                    else
                        KeyValuePairAspireObjectList.Add(new KeyValuePair<double, PalpableCatchHitObject>(obj.StartTime, palpableObject));

                }
            }

            if (KeyValuePairAspireObjectList.Count > 0)
            {
                Logger.Log("It is Aspire");

                double currKey = -1;
                PalpableCatchHitObject currObj = null!;
                bool hasCombo = false;
                List<PalpableCatchHitObject> aspireTemporaryPatternObjects = new List<PalpableCatchHitObject>();

                bool priorityByAccuracy = true;

                //Placeholder value used to allow the last aspire object list to process
                KeyValuePairAspireObjectList.Add(new KeyValuePair<double, PalpableCatchHitObject>(double.MaxValue, null!));

                foreach (var item in KeyValuePairAspireObjectList.OrderBy(x => x.Key))
                {
                    if (currObj != null && currKey != item.Key)
                    {
                        //Last execution iteration of aspireObjects Time

                        float halfCatchWidth = Catcher.CalculateCatchWidth(Beatmap.Difficulty) * 0.5f;

                        //There's combo in this pattern -> maximum priority to combo
                        if (hasCombo)
                        {
                            float bestComboPosition = 0;
                            int bestComboValue = 0;
                            float bestTinyPosition = 0;
                            int bestTinyValue = 0;
                            float bestBananaPosition = 0;
                            int bestBananaValue = 0;

                            float theoreticalBestPosition = 0;

                            int localComboValue = 0;
                            int localTinyValue = 0;
                            int localBananaValue = 0;

                            float leftSide;
                            float rightSide;

                            for (float currPosition = CatchPlayfield.WIDTH; currPosition >= 0; currPosition = (float)(currPosition - Catcher.BASE_WALK_SPEED))
                            {
                                leftSide = currPosition - halfCatchWidth;
                                rightSide = currPosition + halfCatchWidth;

                                localComboValue = 0;
                                localTinyValue = 0;
                                localBananaValue = 0;

                                foreach (var hitObject in aspireTemporaryPatternObjects.OrderBy(x => x.EffectiveX).ToList())
                                {
                                    if (leftSide <= hitObject.EffectiveX && hitObject.EffectiveX <= rightSide)
                                    {
                                        if (isComboHitObject(hitObject))
                                            localComboValue++;
                                        else if (hitObject is TinyDroplet)
                                            localTinyValue++;
                                        else if (hitObject is Banana)
                                            localBananaValue++;
                                    }
                                }

                                //Combo has the highest priority! Older best tiny or banana values are scrapped
                                if (bestComboValue < localComboValue)
                                {
                                    bestComboValue = localComboValue;
                                    bestComboPosition = currPosition;

                                    bestTinyValue = localTinyValue;
                                    bestTinyPosition = currPosition;

                                    bestBananaValue = localBananaValue;
                                    bestBananaPosition = currPosition;
                                }
                                //If the best combo didn't decrease, find new best tiny or banana
                                else if (bestComboValue == localComboValue)
                                {
                                    if (bestTinyValue < localTinyValue)
                                    {
                                        bestTinyValue = localTinyValue;
                                        bestTinyPosition = currPosition;
                                    }

                                    if (bestBananaValue < localBananaValue)
                                    {
                                        bestBananaValue = localBananaValue;
                                        bestBananaPosition = currPosition;
                                    }
                                }
                            }

                            if (priorityByAccuracy)
                            {
                                if (bestComboPosition != bestTinyPosition)
                                    theoreticalBestPosition = bestTinyPosition;
                                else
                                    theoreticalBestPosition = bestComboPosition;
                            }
                            else
                            {
                                if (bestComboPosition != bestBananaPosition)
                                    theoreticalBestPosition = bestBananaPosition;
                                else
                                    theoreticalBestPosition = bestComboPosition;
                            }

                            //Virtual Fruit: Only used to place the best position in the Auto generator
                            finalAspireList.Add(new Fruit()
                            {
                                StartTime = currObj.StartTime,
                                OriginalX = theoreticalBestPosition,
                                XOffset = 0
                            });

                            //Logger.Log(theoreticalBestObjects.ToArray().ToString());
                            //Logger.Log("Count" + theoreticalBestObjects.ToArray().Count());
                            //Logger.Log("StartTime" + currObj.StartTime);
                        }

                        //There's no combo in this pattern -> priority can be given to tiny or banana
                        else
                        {
                            float bestTinyPosition = 0;
                            int bestTinyValue = 0;
                            float bestBananaPosition = 0;
                            int bestBananaValue = 0;

                            float theoreticalBestPosition = 0;

                            int localTinyValue = 0;
                            int localBananaValue = 0;

                            float leftSide;
                            float rightSide;

                            for (float currPosition = CatchPlayfield.WIDTH; currPosition >= 0; currPosition = (float)(currPosition - Catcher.BASE_WALK_SPEED))
                            {
                                leftSide = currPosition - halfCatchWidth;
                                rightSide = currPosition + halfCatchWidth;

                                localTinyValue = 0;
                                localBananaValue = 0;

                                foreach (var hitObject in aspireTemporaryPatternObjects.OrderBy(x => x.EffectiveX).ToList())
                                {
                                    if (leftSide <= hitObject.EffectiveX && hitObject.EffectiveX <= rightSide)
                                    {
                                        if (hitObject is TinyDroplet)
                                            localTinyValue++;
                                        else if (hitObject is Banana)
                                            localBananaValue++;
                                    }
                                }

                                //Tiny droplet has the highest priority! Older best banana values are scrapped

                                if (priorityByAccuracy && bestTinyValue < localTinyValue)
                                {
                                    bestTinyValue = localTinyValue;
                                    bestTinyPosition = currPosition;

                                    bestBananaValue = localBananaValue;
                                    bestBananaPosition = currPosition;
                                }
                                //Banana has the highest priority! Older best tiny values are scrapped
                                else if (!priorityByAccuracy && bestBananaValue < localBananaValue)
                                {
                                    bestTinyValue = localTinyValue;
                                    bestTinyPosition = currPosition;

                                    bestBananaValue = localBananaValue;
                                    bestBananaPosition = currPosition;
                                }

                                //If the best tiny didn't decrease, find new best banana
                                if (priorityByAccuracy && bestTinyValue == localTinyValue)
                                {
                                    if (bestTinyValue < localTinyValue)
                                    {
                                        bestTinyValue = localTinyValue;
                                        bestTinyPosition = currPosition;
                                    }

                                    if (bestBananaValue < localBananaValue)
                                    {
                                        bestBananaValue = localBananaValue;
                                        bestBananaPosition = currPosition;
                                    }
                                }
                                //If the best banana didn't decrease, find new best tiny
                                else if (!priorityByAccuracy && bestBananaValue == localBananaValue)
                                {
                                    if (bestTinyValue < localTinyValue)
                                    {
                                        bestTinyValue = localTinyValue;
                                        bestTinyPosition = currPosition;
                                    }

                                    if (bestBananaValue < localBananaValue)
                                    {
                                        bestBananaValue = localBananaValue;
                                        bestBananaPosition = currPosition;
                                    }
                                }
                            }

                            if (priorityByAccuracy)
                            {
                                if (bestTinyPosition != bestBananaPosition)
                                    theoreticalBestPosition = bestBananaPosition;
                                else
                                    theoreticalBestPosition = bestTinyPosition;
                            }
                            else
                            {
                                if (bestBananaPosition != bestTinyPosition)
                                    theoreticalBestPosition = bestTinyPosition;
                                else
                                    theoreticalBestPosition = bestBananaPosition;
                            }


                            //Virtual Fruit: Only used to place the best position in the Auto generator
                            finalAspireList.Add(new Fruit()
                            {
                                StartTime = currObj.StartTime,
                                OriginalX = theoreticalBestPosition,
                                XOffset = 0
                            });

                            //Logger.Log(theoreticalBestObjects.ToArray().ToString());
                            //Logger.Log("Count" + theoreticalBestObjects.ToArray().Count());
                            //Logger.Log("StartTime" + currObj.StartTime);
                        }


                        //New iteration of aspireObjects Time
                        currKey = item.Key;
                        currObj = item.Value;
                        hasCombo = false;
                        aspireTemporaryPatternObjects = new List<PalpableCatchHitObject>();
                    }

                    //Updates the status of the aspireObjects pattern
                    if (isComboHitObject(item.Value))
                        hasCombo = true;

                    currKey = item.Key;
                    currObj = item.Value;
                    if (currObj != null)
                        aspireTemporaryPatternObjects.Add(currObj);
                }

            }
            else
            {
                Logger.Log("It is not Aspire");
            }

            //Aspire List, generation of Auto frames after concatenating and reordering the new Aspire positions
            if (finalAspireList.Count > 0)
            {
                isAspire = true;

                finalObjectList = finalObjectList.Concat(finalAspireList).OrderBy(x => x.StartTime).ToList();

                foreach (var obj in finalObjectList)
                {
                    if (obj is PalpableCatchHitObject palpableObject)
                    {
                        moveToNext(palpableObject);
                    }
                }
            }
            //No Aspire List, regular generation of Auto frames
            else
            {
                foreach (var obj in Beatmap.HitObjects)
                {
                    if (obj is PalpableCatchHitObject palpableObject)
                    {
                        moveToNext(palpableObject);
                    }

                    foreach (var nestedObj in obj.NestedHitObjects.Cast<CatchHitObject>())
                    {
                        if (nestedObj is PalpableCatchHitObject palpableNestedObject)
                        {
                            moveToNext(palpableNestedObject);
                        }
                    }
                }
            }

        }

        private void addFrame(double time, float? position = null, bool dashing = false)
        {
            Frames.Add(new CatchReplayFrame(time, position, dashing, LastFrame));
        }
    }
}
