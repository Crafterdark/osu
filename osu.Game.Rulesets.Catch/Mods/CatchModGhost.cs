// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModGhost : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDrawableHitObject
    {
        public override string Name => "Ghost";
        public override string Acronym => "GT";
        public override IconUsage? Icon => FontAwesome.Solid.Ghost;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => @"Playing as a ... ghost?!";
        public override double ScoreMultiplier => 1;

        [SettingSource("Visibility", "The maximum percentage of visibility for the ghost")]
        public BindableDouble GhostInvisibility { get; } = new BindableDouble(0.25d)
        {
            MinValue = 0.10d,
            MaxValue = 0.40d,
            Precision = 0.01d
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            //Everything ghost related
            catchPlayfield.Catcher.DisableHitLighting();
            catchPlayfield.Catcher.IsGhost = true;
            catchPlayfield.Catcher.Alpha = (float)GhostInvisibility.Value;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            var drawableCatchHitObject = (DrawableCatchHitObject)drawable;

            drawableCatchHitObject.StateOverride = (s) =>
            {
                if (s != ArmedState.Idle)
                    drawableCatchHitObject.NewState = ArmedState.Miss;
            };
        }
    }
}
