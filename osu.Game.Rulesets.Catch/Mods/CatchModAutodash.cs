// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public partial class CatchModAutodash : Mod, IApplicableMod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield
    {
        public override string Name => "Autodash";
        public override string Acronym => "AD";
        public override LocalisableString Description => "Automatic catcher dashing.";
        public override double ScoreMultiplier => 0.5;
        public override IconUsage? Icon => FontAwesome.Solid.HandsHelping;
        public override ModType Type => ModType.Automation;

        private IFrameStableClock gameplayClock = null!;

        private Catcher catcher = null!;

        private Playfield playfield = null!;

        private PalpableCatchHitObject? incomingHitObject;

        private double halfCatchWidth;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var catchDrawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            catchPlayfield.CatcherArea.InvalidCatchActionList.Add(CatchAction.Dash);

            gameplayClock = drawableRuleset.FrameStableClock;
            catcher = catchPlayfield.Catcher;
            playfield = catchDrawableRuleset.Playfield;

            halfCatchWidth = Catcher.CalculateCatchWidth(drawableRuleset.Beatmap.Difficulty) / 2;
        }

        public void Update(Playfield playfield)
        {
            incomingHitObject = GetIncomingObject(gameplayClock.CurrentTime);
            AdjustDashing(gameplayClock.CurrentTime);
        }

        public void AdjustDashing(double currentTime)
        {
            //THERE'S AN INCOMING COMBO OBJECT
            if (incomingHitObject != null)
            {
                int directionCenter = catcher.X > incomingHitObject.EffectiveX ? -1 : (catcher.X == incomingHitObject.EffectiveX ? 0 : 1);
                float totalDistanceFromCenters = (float)((incomingHitObject.EffectiveX - catcher.X) * directionCenter);
                float totalDistanceFromWalking = (float)((incomingHitObject.StartTime - currentTime) * Catcher.BASE_WALK_SPEED);
                if (totalDistanceFromWalking < 0)
                    totalDistanceFromWalking = 0;
                float lastExcess = (float)Math.Clamp(totalDistanceFromCenters - totalDistanceFromWalking, 0, halfCatchWidth);
                float lastExcessUncapped = (float)Math.Clamp(totalDistanceFromCenters - totalDistanceFromWalking, 0, float.PositiveInfinity);

                //INCOMING OBJECT IS HYPERDASH
                if (incomingHitObject.HyperDash)
                {
                    Logger.Log("object H");
                    if (totalDistanceFromWalking > 0)
                    {
                        if (lastExcessUncapped <= halfCatchWidth && LookAheadCanCatch(incomingHitObject.EffectiveX - directionCenter * lastExcess, incomingHitObject.StartTime))
                            catcher.Dashing = false;
                        else
                            catcher.Dashing = true;
                    }
                    else
                        catcher.Dashing = true;
                }
                //INCOMING OBJECT IS NOT HYPERDASH
                else
                {
                    Logger.Log("object NH");
                    if (totalDistanceFromWalking > 0)
                    {
                        //float newCatcherCenter = incomingHitObject.EffectiveX - directionCenter * (lastExcess + (float)halfCatchWidth);
                        if (lastExcessUncapped <= halfCatchWidth && LookAheadCanCatch(incomingHitObject.EffectiveX - directionCenter * lastExcess, incomingHitObject.StartTime))
                            catcher.Dashing = false;
                        else
                            catcher.Dashing = true;
                    }
                    else
                        catcher.Dashing = true;

                }
            }
            //OTHERWISE
            else
                catcher.Dashing = true;
        }

        public bool IsInsideCatcherPlateRange(float objectX) => catcher.X - halfCatchWidth < objectX && objectX < catcher.X + halfCatchWidth;

        public bool LookAheadCanCatch(float startPosition, double startTime)
        {
            if (playfield.AllHitObjects != null && playfield.AllHitObjects.Count() > 0)
            {
                var firstHyperDashOrLastObject = GetIncomingObjectHyperOrLast(startTime);
                var drawableHitObjectList = playfield.AllHitObjects.Where(x => x.HitObject.StartTime > startTime && (firstHyperDashOrLastObject != null ? x.HitObject.StartTime < firstHyperDashOrLastObject.StartTime : true) && x.HitObject is not BananaShower).Reverse();

                var firstObject = firstHyperDashOrLastObject;

                if (firstObject == null)
                    return false;

                float currentLeftCenter = (float)(firstObject.EffectiveX - halfCatchWidth);
                float currentCenter = firstObject.EffectiveX;
                float currentRightCenter = (float)(firstObject.EffectiveX + halfCatchWidth);
                double currentTime = firstObject.StartTime;

                CenterTracker mainCenterTrack = new CenterTracker(currentCenter);
                mainCenterTrack.SetSideCenters(currentLeftCenter, currentRightCenter);
                mainCenterTrack.SetTime(currentTime);

                foreach (var drawableHitObject in drawableHitObjectList)
                {
                    var hitObject = drawableHitObject.HitObject;
                    bool isContainer = hitObject is not PalpableCatchHitObject;

                    if (isContainer)
                    {
                        foreach (var nestedHitObject in hitObject.NestedHitObjects)
                        {
                            if (!UpdateLookAheadCenter(mainCenterTrack, (PalpableCatchHitObject)nestedHitObject))
                                return false;
                        }
                    }

                    else
                    {
                        if (!UpdateLookAheadCenter(mainCenterTrack, (PalpableCatchHitObject)hitObject))
                            return false;
                    }
                }

                float thresholdDistance = (float)((mainCenterTrack.GetTime() - startTime) * Catcher.BASE_WALK_SPEED);
                float finalDistanceAtCenter = Math.Abs(mainCenterTrack.GetAverageCenter() - startPosition);

                if (thresholdDistance >= finalDistanceAtCenter)
                    return true;
                else
                    return false;
            }

            return false;
        }

        public bool UpdateLookAheadCenter(CenterTracker centerTracker, PalpableCatchHitObject palpableNestedHitObject)
        {
            float currentLeftCenter = centerTracker.GetCenter(TypeCenter.Left);
            float currentRightCenter = centerTracker.GetCenter(TypeCenter.Right);
            double currentTime = centerTracker.GetTime();

            double oldTime = palpableNestedHitObject.StartTime;

            float totalDistanceFromTime = (float)((currentTime - oldTime) * Catcher.BASE_WALK_SPEED);

            float oldLeftCenter = (float)(palpableNestedHitObject.EffectiveX - halfCatchWidth);
            float oldCenter = palpableNestedHitObject.EffectiveX;
            float oldRightCenter = (float)(palpableNestedHitObject.EffectiveX + halfCatchWidth);

            float leftDistance = Math.Abs(currentLeftCenter - oldLeftCenter);
            float rightDistance = Math.Abs(currentRightCenter - oldRightCenter);

            float leftOffset = 0;
            float rightOffset = 0;

            if (totalDistanceFromTime > leftDistance)
            {
                leftOffset = totalDistanceFromTime - leftDistance;
            }

            if (totalDistanceFromTime > rightDistance)
            {
                rightOffset = totalDistanceFromTime - rightDistance;
            }

            currentLeftCenter = oldLeftCenter + leftOffset;
            currentRightCenter = oldRightCenter - rightOffset;

            if (currentLeftCenter > currentRightCenter)
                return false;

            centerTracker.SetSideCenters(currentLeftCenter, currentRightCenter);
            centerTracker.SetCenter(oldCenter);
            centerTracker.SetTime(oldTime);

            return true;
        }

        public PalpableCatchHitObject? GetIncomingObject(double currentTime)
        {
            if (playfield.AllHitObjects != null && playfield.AllHitObjects.Count() > 0)
            {
                foreach (var drawableHitObject in playfield.AllHitObjects)
                {
                    var hitObject = drawableHitObject.HitObject;
                    bool isContainer = hitObject is not PalpableCatchHitObject;

                    if (hitObject is not BananaShower)
                    {
                        if (isContainer)
                        {
                            foreach (var nestedHitObject in hitObject.NestedHitObjects)
                            {
                                if (nestedHitObject.StartTime >= currentTime)
                                    return (PalpableCatchHitObject?)nestedHitObject;
                            }
                        }
                        else if (hitObject.StartTime >= currentTime)
                        {
                            return (PalpableCatchHitObject?)hitObject;
                        }
                    }
                }
            }

            return null;
        }

        public PalpableCatchHitObject? GetIncomingObjectHyperOrLast(double currentTime)
        {
            PalpableCatchHitObject currObj = null!;

            if (playfield.AllHitObjects != null && playfield.AllHitObjects.Count() > 0)
            {
                foreach (var drawableHitObject in playfield.AllHitObjects)
                {
                    var hitObject = drawableHitObject.HitObject;
                    bool isContainer = hitObject is not PalpableCatchHitObject;

                    if (hitObject is not BananaShower)
                    {
                        if (isContainer)
                        {
                            foreach (var nestedHitObject in hitObject.NestedHitObjects)
                            {
                                if (nestedHitObject.StartTime >= currentTime && ((PalpableCatchHitObject)nestedHitObject).HyperDash)
                                    return (PalpableCatchHitObject?)nestedHitObject;
                                currObj = (PalpableCatchHitObject)nestedHitObject;
                            }
                        }
                        else if (hitObject.StartTime >= currentTime && ((PalpableCatchHitObject)hitObject).HyperDash)
                        {
                            return (PalpableCatchHitObject?)hitObject;
                        }
                        else
                            currObj = (PalpableCatchHitObject)hitObject;
                    }
                }
            }

            return currObj;
        }

        public enum TypeCenter
        {
            Left,
            Real,
            Right
        }

        public class CenterTracker
        {
            private double time;
            private float realCenter;
            private float leftCenter;
            private float rightCenter;
            public CenterTracker(float realCenter)
            {
                this.realCenter = realCenter;
            }
            public void SetCenter(float center)
            {
                realCenter = center;
            }

            public void SetSideCenters(float left, float right)
            {
                leftCenter = left;
                rightCenter = right;
            }
            public void SetTime(double time)
            {
                this.time = time;
            }

            public float GetCenter(TypeCenter typeCenter)
            {
                switch (typeCenter)
                {
                    case TypeCenter.Real:
                        return realCenter;
                    case TypeCenter.Left:
                        return leftCenter;
                    case TypeCenter.Right:
                        return rightCenter;
                }

                return -1;
            }

            public float GetAverageCenter()
            {
                return (leftCenter + rightCenter) / 2;
            }

            public double GetTime()
            {
                return time;
            }
        }

    }

}
