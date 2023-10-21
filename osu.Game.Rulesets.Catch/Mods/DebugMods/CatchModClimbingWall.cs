// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModClimbingWall : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Climbing Wall";
        public override string Acronym => "CW";
        public override LocalisableString Description => "The catcher is ... a wall climber?";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Building;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            //WIP
        }
    }
}
