// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.Debug_Mods
{
    public class CatchModAutoDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Autodash";

        public override string Acronym => "AD";

        public override LocalisableString Description => "Dashing will be handled automatically.";

        public override double ScoreMultiplier => 0.5;

        public override ModType Type => ModType.Automation;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {

        }
    }
}
