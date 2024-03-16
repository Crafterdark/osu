﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSpeedrun : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Speedrun";

        public override string Acronym => "SR";

        public override LocalisableString Description => "The catcher is running for the world record!";

        public override double ScoreMultiplier => 1;

        public override IconUsage? Icon => FontAwesome.Solid.ChevronCircleRight;

        public override ModType Type => ModType.Fun;

        [SettingSource("Catcher speed increase", "The actual increase to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableDouble MovementSpeedIncrease { get; } = new BindableDouble(1.5)
        {
            Precision = 0.1,
            MinValue = 1.1,
            MaxValue = 2.0,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var catchDrawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            catchPlayfield.Catcher.CustomSpeedMultiplier.Value *= MovementSpeedIncrease.Value;
        }
    }
}
