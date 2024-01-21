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
using TagLib.Ape;

namespace osu.Game.Rulesets.Catch.Replays
{
    internal class CatchAutoGenerator : AutoGenerator<CatchReplayFrame>
    {
        public new CatchBeatmap Beatmap => (CatchBeatmap)base.Beatmap;

        public CatchAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        private bool isComboHitObject(PalpableCatchHitObject hitObject) => hitObject is Fruit || hitObject is Droplet && hitObject is not TinyDroplet;

        protected override void GenerateFrames()
        {
            if (Beatmap.HitObjects.Count == 0)
                return;

            float lastPosition = CatchPlayfield.CENTER_X;
            double lastTime = 0;

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
                bool impossibleJump = speedRequired > Catcher.BASE_DASH_SPEED;

                // todo: get correct catcher size, based on difficulty CS.
                const float catcher_width_half = Catcher.BASE_SIZE * 0.3f * 0.5f;

                if (lastPosition - catcher_width_half < h.EffectiveX && lastPosition + catcher_width_half > h.EffectiveX)
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

            List<PalpableCatchHitObject> objList = new List<PalpableCatchHitObject>();

            List<KeyValuePair<double, PalpableCatchHitObject>> aspireObjectList = new List<KeyValuePair<double, PalpableCatchHitObject>>();

            //Detect if the beatmap has two or more notes happening at the same time
            bool isAspire = false;

            foreach (var obj in Beatmap.HitObjects)
            {
                if (obj is PalpableCatchHitObject palpableObject)
                {
                    if (objList.Exists(x => x.StartTime == obj.StartTime))
                    {
                        isAspire = true;
                        aspireObjectList.Add(new KeyValuePair<double, PalpableCatchHitObject>(obj.StartTime, palpableObject));
                    }
                    else
                        objList.Add(palpableObject);
                }

                foreach (var nestedObj in obj.NestedHitObjects.Cast<CatchHitObject>())
                {
                    if (nestedObj is PalpableCatchHitObject palpableNestedObject)
                    {
                        if (objList.Exists(x => x.StartTime == nestedObj.StartTime))
                        {
                            isAspire = true;
                            aspireObjectList.Add(new KeyValuePair<double, PalpableCatchHitObject>(nestedObj.StartTime, palpableNestedObject));
                        }
                        else
                            objList.Add(palpableNestedObject);
                    }
                }
            }

            List<PalpableCatchHitObject> theoreticalBestObjects = new List<PalpableCatchHitObject>();

            if (isAspire)
            {
                Logger.Log("It is Aspire");

                double currKey = -1;
                PalpableCatchHitObject currObj = null!;
                bool hasCombo = false;
                bool hasTiny = false;
                bool hasBanana = false;
                List<PalpableCatchHitObject> aspirePatternObjects = new List<PalpableCatchHitObject>();

                bool priorityByAccuracy = true;

                foreach (var item in aspireObjectList.OrderBy(x => x.Key))
                {
                    if (currObj != null && currKey != item.Key)
                    {
                        //Last execution iteration of aspireObjects Time

                        float halfCatchWidth = Catcher.CalculateCatchWidth(Beatmap.Difficulty) / 2;

                        //There's combo in this pattern -> maximum priority to combo
                        if (hasCombo)
                        {
                            float bestComboPosition = -1;
                            int bestComboValue = -1;
                            float bestTinyPosition = -1;
                            int bestTinyValue = -1;
                            float bestBananaPosition = -1;
                            int bestBananaValue = -1;

                            float theoreticalBestPosition = -1;

                            for (float currPosition = 0; currPosition < CatchPlayfield.WIDTH; currPosition += (float)Catcher.BASE_WALK_SPEED)
                            {
                                float leftSide = currPosition - halfCatchWidth;
                                float rightSide = currPosition + halfCatchWidth;

                                int localComboValue = 0;
                                int localTinyValue = 0;
                                int localBananaValue = 0;

                                foreach (var hitObject in aspirePatternObjects.OrderBy(x => x.EffectiveX))
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
                                    else
                                    {
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
                                            if (bestTinyValue <= localTinyValue)
                                            {
                                                bestTinyValue = localTinyValue;
                                                bestTinyPosition = currPosition;
                                            }

                                            if (bestBananaValue <= localBananaValue)
                                            {
                                                bestBananaValue = localBananaValue;
                                                bestBananaPosition = currPosition;
                                            }
                                        }

                                        break;
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

                            //Virtual Fruit: Only used to place the best position in the Auto generator
                            theoreticalBestObjects.Add(new Fruit()
                            {
                                StartTime = currObj.StartTime,
                                X = theoreticalBestPosition
                            });

                            Logger.Log(theoreticalBestObjects.ToArray().ToString());
                            Logger.Log("Count" + theoreticalBestObjects.ToArray().Count());
                            Logger.Log("StartTime" + currObj.StartTime);
                        }

                        //There's no combo in this pattern -> priority can be given to tiny or banana
                        else
                        {
                            if (hasTiny)
                            {

                            }
                            else if (hasBanana)
                            {

                            }
                        }


                        //New iteration of aspireObjects Time
                        currKey = item.Key;
                        currObj = item.Value;
                        hasCombo = false;
                        hasTiny = false;
                        hasBanana = false;
                        aspirePatternObjects = new List<PalpableCatchHitObject>();
                    }

                    //Updates the status of the aspireObjects pattern
                    if (isComboHitObject(item.Value))
                        hasCombo = true;
                    else if (item.Value is TinyDroplet)
                        hasTiny = true;
                    else if (item.Value is Banana)
                        hasBanana = true;

                    currKey = item.Key;
                    currObj = item.Value;
                    aspirePatternObjects.Add(currObj);
                }

            }
            else
            {
                Logger.Log("It is not Aspire");
            }

            if (theoreticalBestObjects != null)
            {
                foreach (var item in theoreticalBestObjects)
                {
                    var preObj = objList.Find(x => x.StartTime < item.StartTime);

                    if (preObj != null)
                    {
                        int indexToAppend = objList.IndexOf(preObj);
                        objList.Insert(indexToAppend, item);
                    }
                }
            }

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

        private void addFrame(double time, float? position = null, bool dashing = false)
        {
            Frames.Add(new CatchReplayFrame(time, position, dashing, LastFrame));
        }
    }
}
