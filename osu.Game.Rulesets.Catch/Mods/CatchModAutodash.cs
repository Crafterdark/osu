// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAutodash : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield
    {
        public override string Name => "Autodash";
        public override string Acronym => "AD";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Automation;
        public override LocalisableString Description => "Dashing will be handled automatically.";
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModRelax), typeof(CatchModAutopilot) };

        public CatchHitObject? FirstIncomingCatchableObject, SecondIncomingMustDashObject;

        public List<CatchHitObject>? ArrayOfCatchableObjectsAfterFirstWithNoDashing;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.IsAutoDashing = true;
        }

        public bool CatcherDashingUpdate(Catcher catcher, CatchHitObject incomingCatchObject, double exactTime)
        {
            return true;
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

            FindIncomingCatchHitObjects(catchPlayfield, catchPlayfield.Catcher, exactTime);

        }


        public void FindIncomingCatchHitObjects(CatchPlayfield catchPlayfield, Catcher catcher, double exactTime)
        {
        }

        //This phase tries to find the first incoming object that is possible to catch (if it does exist)
        public bool PhaseOneDashingUpdate(Catcher catcher, CatchHitObject incomingCatchObject, double exactTime)
        {
            return true;
        }

    }
}
