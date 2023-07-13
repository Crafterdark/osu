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
    public class CatchModGhostMode : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDrawableHitObject
    {
        public override string Name => "Ghost Mode";
        public override string Acronym => "GM";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => @"Playing as a ... Ghost?! Spooky!";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1 : 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(CatchModHidden)).ToArray();

        [SettingSource("Ghost Invisibility", "The maximum percentage of visibility for the ghost")]
        public BindableDouble GhostInvisibility { get; } = new BindableDouble(0.50d)
        {
            MinValue = 0.10d,
            MaxValue = 0.60d,
            Precision = 0.01d
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            //Just in case, the mod looks better with this
            catchPlayfield.Catcher.CatchFruitOnPlate = true;

            //Everything ghost related
            catchPlayfield.Catcher.CatchLighting = false;
            catchPlayfield.Catcher.IsGhost = true;
            catchPlayfield.Catcher.Alpha = (float)GhostInvisibility.Value;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            var drawableCatchHitObject = (DrawableCatchHitObject)drawable;
            drawableCatchHitObject.IsCatcherGhost = true;
        }
    }
}
