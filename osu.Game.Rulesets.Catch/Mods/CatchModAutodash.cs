// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
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
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(CatchModRelax)).ToArray();

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.IsAutoDashing = true;
        }

        public void CatcherDashingUpdate(Catcher catcher, CatchHitObject incomingCatchObject, double exactTime)
        {

            //TODO 2: Consider edge cases in the dashing update
            // (early dash, edges, pixels, ecc)

            double halfCatcherWidth = catcher.CatchWidth / 2;
            int directionToNext = (catcher.X - incomingCatchObject.EffectiveX < 0) ? 0 : 1; //0 is left, 1 is right
            bool isObjectInPlate = CheckIfObjectInPlate(catcher.X, halfCatcherWidth, incomingCatchObject.EffectiveX);
            double correctCatcherX = (directionToNext > 0) ? catcher.X + halfCatcherWidth : catcher.X - halfCatcherWidth;
            double totalDistanceToReachObject = isObjectInPlate ? 0 : Math.Abs(correctCatcherX - incomingCatchObject.EffectiveX);
            double totalTimeLeft = incomingCatchObject.GetEndTime() - exactTime;

            catcher.Dashing = (totalTimeLeft * Catcher.BASE_WALK_SPEED < totalDistanceToReachObject) ? true : false;

        }

        public bool CheckIfObjectInPlate(double catcherX, double halfCatcherX, double catchObjectX)
        {
            return catchObjectX <= catcherX + halfCatcherX && catchObjectX >= catcherX - halfCatcherX;
        }

        public void Update(Playfield playfield)
        {
            var catchPlayfield = (CatchPlayfield)playfield;
            double currentTime = catchPlayfield.Time.Current;
            double currentElapsed = catchPlayfield.Time.Elapsed;
            double exactTime = currentTime - currentElapsed;

            //The following code will search for the first incoming hitobject that the catcher should catch

            //TODO 1: Ignore uncatchable hitobjects (this can only happen when the catcher misses fruits = recover mechanics)

            foreach (DrawableCatchHitObject mainObject in catchPlayfield.AllHitObjects)
            {
                if (mainObject.HitObject.GetEndTime() > exactTime)
                {
                    bool found = false;
                    foreach (DrawableCatchHitObject nestedObject in mainObject.NestedHitObjects)
                    {
                        if (nestedObject.HitObject.GetEndTime() > exactTime && !(nestedObject.HitObject is Banana))
                        {
                            //DEBUG CODE: catchPlayfield.Catcher.X = nestedObject.HitObject.EffectiveX;
                            CatcherDashingUpdate(catchPlayfield.Catcher, nestedObject.HitObject, exactTime);
                            found = true;
                            break;
                        }
                    }
                    if (!found && (mainObject.HitObject is PalpableCatchHitObject) && !(mainObject.HitObject is Banana))
                    {
                        //DEBUG CODE: catchPlayfield.Catcher.X = mainObject.HitObject.EffectiveX;
                        CatcherDashingUpdate(catchPlayfield.Catcher, mainObject.HitObject, exactTime);
                    }
                    break;
                }

                else
                {
                    //No hitobject is incoming -> Do nothing
                    catchPlayfield.Catcher.Dashing = false;
                }

            }

        }

    }
}
