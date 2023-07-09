// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAutodash : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield
    {
        public override string Name => "Autodash";
        public override string Acronym => "AD";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Automation;
        public override LocalisableString Description => "Every form of dashing will be handled automatically.";
        public override double ScoreMultiplier => 0.1;

        public CatchHitObject? TrackedHitObject;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.IsAutoDashing = true;
        }

        public void CatcherDashingUpdate(Catcher catcher, CatchHitObject incomingCatchObject, double exactTime)
        {
            double halfCatcherWidth = catcher.CatchWidth / 2;
            int directionToNext = (catcher.X - incomingCatchObject.EffectiveX < 0) ? 0 : 1; //0 is left, 1 is right
            double correctCatcherX = (directionToNext > 0) ? catcher.X + halfCatcherWidth : catcher.X - halfCatcherWidth;
            double totalDistanceToReachObject = Math.Abs(correctCatcherX - incomingCatchObject.EffectiveX);
            double totalTimeLeft = incomingCatchObject.GetEndTime() - exactTime;
            if (totalTimeLeft * Catcher.BASE_WALK_SPEED < totalDistanceToReachObject)
            {
                catcher.Dashing = true;
            }
            else
            {
                catcher.Dashing = false;
            }
        }

        public void Update(Playfield playfield)
        {
            var catchPlayfield = (CatchPlayfield)playfield;

            double currentTime = catchPlayfield.Time.Current;

            double currentElapsed = catchPlayfield.Time.Elapsed;

            double exactTime = currentTime - currentElapsed;

            foreach (DrawableCatchHitObject mainObject in catchPlayfield.AllHitObjects)
            {
                if (mainObject.HitObject.GetEndTime() > exactTime)
                {
                    bool found = false;
                    foreach (DrawableCatchHitObject nestedObject in mainObject.NestedHitObjects)
                    {
                        if (nestedObject.HitObject.GetEndTime() > exactTime && !(nestedObject.HitObject is Banana))
                        {
                            //DEBUG: catchPlayfield.Catcher.X = nestedObject.HitObject.EffectiveX;
                            CatcherDashingUpdate(catchPlayfield.Catcher, nestedObject.HitObject, exactTime);
                            found = true;
                            break;
                        }
                    }
                    if (!found && (mainObject.HitObject is PalpableCatchHitObject) && !(mainObject.HitObject is Banana))
                    {
                        //DEBUG: catchPlayfield.Catcher.X = mainObject.HitObject.EffectiveX;
                        CatcherDashingUpdate(catchPlayfield.Catcher, mainObject.HitObject, exactTime);
                    }
                    break;
                }

                else
                {
                    //Nothing happens, so we stop dashing
                    catchPlayfield.Catcher.Dashing = false;
                }

            }

        }

    }
}
