// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool HasAspirePatterns { get; set; }

        //Default Autoplay priority: Maximum accuracy on the beatmap.
        public PriorityType AutoplayPriorityType { get; set; } = PriorityType.Accuracy;

        public ObjectTracker ComboTracker = new ObjectTracker(0, 0, 0);

        public ObjectTracker TinyDropletTracker = new ObjectTracker(0, 0, 0);

        public ObjectTracker BananaTracker = new ObjectTracker(0, 0, 0);

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

                float halfCatcherWidth = Catcher.CalculateCatchWidth(Beatmap.Difficulty) * 0.5f;

                if (HasAspirePatterns)
                {
                    addFrame(h.StartTime, h.EffectiveX);
                    lastTime = h.StartTime;
                    lastPosition = h.EffectiveX;
                    return;
                }

                if (lastPosition - halfCatcherWidth < h.EffectiveX && lastPosition + halfCatcherWidth > h.EffectiveX)
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
                double currKey = -1;

                PalpableCatchHitObject currObj = null!;

                List<PalpableCatchHitObject> aspireTemporaryPatternObjects = new List<PalpableCatchHitObject>();

                //Placeholder value used to allow the last aspire object list to process
                KeyValuePairAspireObjectList.Add(new KeyValuePair<double, PalpableCatchHitObject>(double.MaxValue, null!));

                foreach (var item in KeyValuePairAspireObjectList.OrderBy(x => x.Key))
                {
                    if (currObj != null && currKey != item.Key)
                    {
                        //Last execution iteration of aspireObjects Time

                        float halfCatchWidth = Catcher.CalculateCatchWidth(Beatmap.Difficulty) * 0.5f;

                        float leftSide;
                        float rightSide;

                        ComboTracker.GlobalBestValue = 0;
                        TinyDropletTracker.GlobalBestValue = 0;
                        BananaTracker.GlobalBestValue = 0;

                        for (float currPosition = 0; currPosition <= CatchPlayfield.WIDTH; currPosition = (float)(currPosition + Catcher.BASE_WALK_SPEED))
                        {
                            leftSide = currPosition - halfCatchWidth;
                            rightSide = currPosition + halfCatchWidth;

                            ComboTracker.LocalBestValue = 0;
                            TinyDropletTracker.LocalBestValue = 0;
                            BananaTracker.LocalBestValue = 0;

                            foreach (var hitObject in aspireTemporaryPatternObjects.OrderBy(x => x.EffectiveX).ToList())
                            {
                                if (leftSide <= hitObject.EffectiveX && hitObject.EffectiveX <= rightSide)
                                {
                                    if (isComboHitObject(hitObject))
                                        ComboTracker.LocalBestValue++;
                                    else if (hitObject is TinyDroplet)
                                        TinyDropletTracker.LocalBestValue++;
                                    else if (hitObject is Banana)
                                        BananaTracker.LocalBestValue++;
                                }
                            }

                            UpdateTrackersWithPriority(AutoplayPriorityType, currPosition);
                        }

                        float theoreticalBestPosition = GetBestPositionWithPriority(AutoplayPriorityType);

                        //Virtual Fruit: Only used to place the best position in the Auto generator
                        finalAspireList.Add(new Fruit()
                        {
                            StartTime = currObj.StartTime,
                            OriginalX = theoreticalBestPosition,
                            XOffset = 0
                        });

                        //New iteration of aspireObjects Time
                        currKey = item.Key;
                        currObj = item.Value;
                        aspireTemporaryPatternObjects = new List<PalpableCatchHitObject>();
                    }

                    currKey = item.Key;
                    currObj = item.Value;
                    if (currObj != null)
                        aspireTemporaryPatternObjects.Add(currObj);
                }

            }

            //Aspire List, generation of Auto frames after concatenating and reordering the new Aspire positions
            if (finalAspireList.Count > 0)
            {
                HasAspirePatterns = true;

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
                HasAspirePatterns = false;

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

        public void UpdateTrackersWithPriority(PriorityType priorityType, float currPosition)
        {
            //Combo always has maximum priority regardless of PriorityType
            if (ComboTracker.LocalBestValue > 0 || ComboTracker.GlobalBestValue > 0)
            {
                if (ComboTracker.GlobalBestValue < ComboTracker.LocalBestValue)
                {
                    ComboTracker.GlobalBestValue = ComboTracker.LocalBestValue;
                    ComboTracker.GlobalBestPosition = currPosition;

                    TinyDropletTracker.GlobalBestValue = TinyDropletTracker.LocalBestValue;
                    TinyDropletTracker.GlobalBestPosition = currPosition;

                    BananaTracker.GlobalBestValue = BananaTracker.LocalBestValue;
                    BananaTracker.GlobalBestPosition = currPosition;
                }
                else if (ComboTracker.GlobalBestValue == ComboTracker.LocalBestValue)
                {
                    if (TinyDropletTracker.GlobalBestValue < TinyDropletTracker.LocalBestValue)
                    {
                        TinyDropletTracker.GlobalBestValue = TinyDropletTracker.LocalBestValue;
                        TinyDropletTracker.GlobalBestPosition = currPosition;
                    }

                    if (BananaTracker.GlobalBestValue < BananaTracker.LocalBestValue)
                    {
                        BananaTracker.GlobalBestValue = BananaTracker.LocalBestValue;
                        BananaTracker.GlobalBestPosition = currPosition;
                    }

                }
            }
            else if (priorityType == PriorityType.Accuracy)
            {
                if (TinyDropletTracker.GlobalBestValue < TinyDropletTracker.LocalBestValue)
                {
                    TinyDropletTracker.GlobalBestValue = TinyDropletTracker.LocalBestValue;
                    TinyDropletTracker.GlobalBestPosition = currPosition;

                    BananaTracker.GlobalBestValue = BananaTracker.LocalBestValue;
                    BananaTracker.GlobalBestPosition = currPosition;
                }
                else if (TinyDropletTracker.GlobalBestValue == TinyDropletTracker.LocalBestValue)
                {
                    if (BananaTracker.GlobalBestValue < BananaTracker.LocalBestValue)
                    {
                        BananaTracker.GlobalBestValue = BananaTracker.LocalBestValue;
                        BananaTracker.GlobalBestPosition = currPosition;
                    }
                }
            }
            else if (priorityType == PriorityType.Score)
            {
                if (BananaTracker.GlobalBestValue < BananaTracker.LocalBestValue)
                {
                    TinyDropletTracker.GlobalBestValue = TinyDropletTracker.LocalBestValue;
                    TinyDropletTracker.GlobalBestPosition = currPosition;

                    BananaTracker.GlobalBestValue = BananaTracker.LocalBestValue;
                    BananaTracker.GlobalBestPosition = currPosition;
                }
                else if (BananaTracker.GlobalBestValue == BananaTracker.LocalBestValue)
                {
                    if (TinyDropletTracker.GlobalBestValue < TinyDropletTracker.LocalBestValue)
                    {
                        TinyDropletTracker.GlobalBestValue = TinyDropletTracker.LocalBestValue;
                        TinyDropletTracker.GlobalBestPosition = currPosition;
                    }
                }
            }
        }

        public float GetBestPositionWithPriority(PriorityType priorityType)
        {
            if (ComboTracker.LocalBestValue > 0 || ComboTracker.GlobalBestValue > 0)
            {
                if (priorityType == PriorityType.Accuracy)
                {
                    if (TinyDropletTracker.GlobalBestValue > 0 && ComboTracker.GlobalBestPosition != TinyDropletTracker.GlobalBestPosition)
                        return TinyDropletTracker.GlobalBestPosition;
                    else
                        return ComboTracker.GlobalBestPosition;
                }
                else if (priorityType == PriorityType.Score)
                {
                    if (BananaTracker.GlobalBestValue > 0 && ComboTracker.GlobalBestPosition != BananaTracker.GlobalBestPosition)
                        return BananaTracker.GlobalBestPosition;
                    else
                        return ComboTracker.GlobalBestPosition;
                }
            }
            else
            {
                if (priorityType == PriorityType.Accuracy)
                {
                    if (BananaTracker.GlobalBestValue > 0 && TinyDropletTracker.GlobalBestPosition != BananaTracker.GlobalBestPosition)
                        return BananaTracker.GlobalBestPosition;
                    else
                        return TinyDropletTracker.GlobalBestPosition;
                }
                else if (priorityType == PriorityType.Score)
                {
                    if (TinyDropletTracker.GlobalBestValue > 0 && BananaTracker.GlobalBestPosition != TinyDropletTracker.GlobalBestPosition)
                        return TinyDropletTracker.GlobalBestPosition;
                    else
                        return BananaTracker.GlobalBestPosition;
                }
            }

            //This value means something went wrong
            return -1337;
        }

        public enum PriorityType
        {
            Accuracy,
            Score
        }

        public class ObjectTracker
        {
            public int LocalBestValue;
            public int GlobalBestValue;

            public float GlobalBestPosition;

            public ObjectTracker(int localBestValue, int globalBestValue, float globalBestPosition)
            {
                LocalBestValue = localBestValue;
                GlobalBestValue = globalBestValue;
                GlobalBestPosition = globalBestPosition;
            }
        }
    }
}
