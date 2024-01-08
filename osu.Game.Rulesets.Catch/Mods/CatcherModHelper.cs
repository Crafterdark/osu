// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public interface IApplicableToCatcher
    {
        public void ChangeCatcherSpeed(Catcher catcher, double walk = Catcher.BASE_WALK_SPEED, double dash = Catcher.BASE_DASH_SPEED)
        {
            catcher.VARIABLE_WALK_SPEED = walk / Catcher.BASE_WALK_SPEED;
            catcher.VARIABLE_DASH_SPEED = dash / Catcher.BASE_DASH_SPEED;
        }
    }
}
