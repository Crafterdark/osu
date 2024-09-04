// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHoldDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => @"Hold Dash";
        public override string Acronym => @"HO";
        public override LocalisableString Description => @"Dashing will be automatically held.";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(CatchModCinema) };
        public override ModType Type => ModType.Automation;

        [SettingSource("Reverse dashing", "Don't hold dash when the dashing key is pressed.")]
        public BindableBool ReverseDashing { get; } = new BindableBool();

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.CatcherArea.IsHoldDashing = true;
            catchPlayfield.CatcherArea.IsReverseDashing = ReverseDashing.Value;
            catchPlayfield.Catcher.Dashing = true;
        }
    }
}
