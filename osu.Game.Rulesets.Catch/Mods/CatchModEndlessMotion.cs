// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModEndlessMotion : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Endless Motion";
        public override string Acronym => "EM";
        public override IconUsage? Icon => FontAwesome.Solid.Infinity;
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override LocalisableString Description => @"The catcher cannot stop moving...";
        public override Type[] IncompatibleMods => new[] { typeof(CatchModRelax) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.CatcherArea.ForceCustomCurrentDirection = true;

            //Start by going right (default)
            catchPlayfield.CatcherArea.CustomDirection.Value = 1;
        }
    }
}
