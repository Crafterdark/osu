// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSuddenDeath : ModSuddenDeath
    {
        //Internal Immediate Fail disabled by default
        public bool ImmediateFailOnCondition { get; set; }

        public override bool GlobalPerformFail() => ImmediateFailOnCondition;
    }
}
