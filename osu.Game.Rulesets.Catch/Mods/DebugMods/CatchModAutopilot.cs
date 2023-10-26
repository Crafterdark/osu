// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModAutopilot : Mod, IApplicableToDrawableRuleset<CatchHitObject>

    {
        public override string Name => "Autopilot";

        public override string Acronym => "AP";

        public override LocalisableString Description => @"Automatic catcher movement - directional dashing with two keys.";

        public override double ScoreMultiplier => 1.0;

        public override ModType Type => ModType.Automation;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModDirectionalDash), typeof(CatchModRelax), typeof(CatchModAutoplay) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchRuleset = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchRuleset.CatcherArea.DisableMainDash = true;
        }
    }
}
