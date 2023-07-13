// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModDodgeFruits : Mod, IApplicableToHealthProcessor, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDrawableHitObject
    {
        public override string Name => "Dodge Fruits";
        public override string Acronym => "DF";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => @"Dodge the beat! Do not catch fruits.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 0.1 : 0.1;

        [SettingSource("Extra Health Reduction", "The health reduction penalty from catching fruits.")]
        public BindableDouble ExtraHealthReduction { get; } = new BindableDouble(1.00d)
        {
            MinValue = 0.00d,
            MaxValue = 1.00d,
            Precision = 0.01d
        };

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.IsReversed = true;
            healthProcessor.ReverseHealthReduction = ExtraHealthReduction.Value;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.Catcher.CatchDodging = true;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            var drawableCatchHitObject = (DrawableCatchHitObject)drawable;
            drawableCatchHitObject.IsCatcherDodging = true;
        }
    }
}
