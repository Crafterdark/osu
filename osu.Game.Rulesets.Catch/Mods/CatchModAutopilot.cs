// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAutopilot : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield
    {
        public override string Name => "Autopilot";
        public override string Acronym => "AP";
        public override IconUsage? Icon => OsuIcon.ModAutopilot;
        public override ModType Type => ModType.Automation;
        public override LocalisableString Description => @"Automatic catcher movement - Change the type of plate when necessary.";
        public override double ScoreMultiplier => 0.1;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModAutodash), typeof(CatchModRelax) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            //Used to remove all walking inputs
            catchPlayfield.Catcher.IsAutopilot = true;
        }

        public void CatcherMovementUpdate(CatcherArea catcherArea, CatchHitObject incomingCatchObject, double exactTime)
        {
        }

        public bool CheckIfObjectInPlate(double catcherX, double halfCatcherX, double catchObjectX)
        {
            return catchObjectX <= catcherX + halfCatcherX && catchObjectX >= catcherX - halfCatcherX;
        }

        public void Update(Playfield playfield)
        {
            var catchPlayfield = (CatchPlayfield)playfield;
            var catcherArea = catchPlayfield.CatcherArea;
            double currentTime = catchPlayfield.Time.Current;
            double currentElapsed = catchPlayfield.Time.Elapsed;
            double exactTime = currentTime - currentElapsed;

            FindIncomingCatchHitObjects(catchPlayfield, catcherArea, exactTime);

        }

        public void FindIncomingCatchHitObjects(CatchPlayfield catchPlayfield, CatcherArea catcherArea, double exactTime)
        {
        }


    }
}
