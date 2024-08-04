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
    public class CatchModFlow : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Flow";

        public override string Acronym => "FW";

        public override LocalisableString Description => "Change the direction of the catcher without stopping...";

        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(CatchModCinema) };

        public override IconUsage? Icon => null;

        public override ModType Type => ModType.Conversion;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.CatcherArea.ImmediateDirectionChange = true;
        }
    }
}
