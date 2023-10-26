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
    public class CatchModDirectionalDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>

    {
        public override string Name => "Directional Dash";

        public override string Acronym => "DD";

        public override LocalisableString Description => "Dashing requires two keys and only follows one direction.";

        public override double ScoreMultiplier => 1.0;

        public override ModType Type => ModType.DifficultyIncrease;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModAutopilot), typeof(CatchModRelax) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchRuleset = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchRuleset.CatcherArea.DisableMainDash = true;
            catchRuleset.CatcherArea.IsDirectionalDash = true;
        }
    }
}
