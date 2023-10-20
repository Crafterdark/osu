// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModAutoDashing : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield

    {
        public override string Name => "Auto Dashing";

        public override string Acronym => "AD";

        public override LocalisableString Description => "Dashing will be handled automatically.";

        public override double ScoreMultiplier => 0.5;

        public override ModType Type => ModType.Automation;

        public CatchHitObject? IncomingCatchableObject;

        public List<CatchHitObject>? ArrayOfCatchableObjects;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchRuleset = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchRuleset.CatcherArea.DisabledDashing = true;
        }

        public bool CatcherDashingUpdate(Catcher catcher, CatchHitObject incomingCatchObject, double exactTime)
        {
            return true;
        }

        public bool CheckIfObjectInPlate(CatchHitObject hitObject, Catcher catcher)
        {
            double halfCatchWidth = catcher.CatchWidth / 2;

            double accuracyDistance = 0;

            if (catcher.CatchFruitLeniency)
            {
                double rescaleFactor = 0;

                if (hitObject is Fruit)
                {
                    rescaleFactor = 1;
                }

                if (hitObject is Droplet)
                {
                    rescaleFactor = 0.8;
                }

                if (hitObject is Banana)
                {
                    rescaleFactor = 0.6;
                }

                if (hitObject is TinyDroplet)
                {
                    rescaleFactor = 0.4;
                }

                accuracyDistance = Math.Abs(catcher.CatchLeniencySlider - 10) / 10 * (double)hitObject.Scale * rescaleFactor * (CatchModLowPrecision.MAX_HITBOX_FRUIT / 2.0);
            }

            return hitObject.EffectiveX <= catcher.X + (halfCatchWidth + accuracyDistance) && hitObject.EffectiveX >= catcher.X - (halfCatchWidth + accuracyDistance);
        }

        public void Update(Playfield playfield)
        {
            var catchPlayfield = (CatchPlayfield)playfield;
            double currentTime = catchPlayfield.Time.Current;
            double currentElapsed = catchPlayfield.Time.Elapsed;
            double exactTime = currentTime - currentElapsed;

            FindIncomingCatchHitObjects(catchPlayfield, catchPlayfield.Catcher, exactTime);
        }

        public void FindIncomingCatchHitObjects(CatchPlayfield catchPlayfield, Catcher catcher, double exactTime)
        {
            double countBananaShower = 0;
            double countJuiceStream = 0;

            foreach (DrawableHitObject hitObject in catchPlayfield.AllHitObjects)
            {
                if ((DrawableCatchHitObject)hitObject is DrawableBananaShower) countBananaShower++;
                if ((DrawableCatchHitObject)hitObject is DrawableJuiceStream) countJuiceStream++;
            }

            Logger.Log("Current count of bananashower: " + countBananaShower);
            Logger.Log("Current count of juicestream: " + countJuiceStream);
        }

        //This phase tries to find the first incoming object that is possible to catch (if it does exist)
        public bool PhaseOneDashingUpdate(Catcher catcher, CatchHitObject incomingCatchObject, double exactTime)
        {
            return true;
        }
    }
}
