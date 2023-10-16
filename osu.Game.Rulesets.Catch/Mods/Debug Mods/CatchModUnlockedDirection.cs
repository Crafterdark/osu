// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.Debug_Mods
{
    public class CatchModUnlockedDirection : Mod, IApplicableToDrawableRuleset<CatchHitObject>

    {
        public override string Name => "Unlocked Direction";

        public override string Acronym => "UD";

        public override LocalisableString Description => "Pressing left & right directional keys will not stop the catcher, but move it towards the last pressed key.";

        public override double ScoreMultiplier => 1.0;

        public override ModType Type => ModType.Conversion;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchRuleset = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchRuleset.CatcherArea.UnlockedDirection = true;

        }
    }
}
