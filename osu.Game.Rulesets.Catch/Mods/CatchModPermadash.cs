// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModPermadash : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Permadash";
        public override string Acronym => "PD";
        public override LocalisableString Description => "Can't stop dashing!";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Running;
        public override ModType Type => ModType.Fun;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var catchDrawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            //Prevent dash key to affect gameplay and to get recorded in the replay
            catchPlayfield.CatcherArea.InvalidCatchActionList.Add(CatchAction.Dash);

            //Permanently enable dashing status
            catchPlayfield.Catcher.Dashing = true;
        }
    }
}
