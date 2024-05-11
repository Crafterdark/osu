// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModDodge : Mod, IApplicableToHealthProcessor, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDrawableHitObject, IApplicableToDifficulty
    {
        public override double ScoreMultiplier => 0.05;

        public override string Name => "Dodge";

        public override string Acronym => "DE";

        public override IconUsage? Icon => FontAwesome.Solid.ExclamationTriangle;

        public override ModType Type => ModType.Fun;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(CatchModAutoplay)).Append(typeof(CatchModBlinds)).ToArray();

        public override LocalisableString Description => @"Dodge fruits to the beat!";


        private readonly BindableNumber<double> mutedVolume = new BindableDouble(0);


        [SettingSource("Limited catcher position", "The edges of the playfield cannot be reached")]
        public BindableBool UnreachableEdges { get; } = new BindableBool();


        [SettingSource("Show catcher combo counter", "The catcher combo counter is visible")]
        public BindableBool ShowCatcherComboCounter { get; } = new BindableBool();

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.IsDodge = true;
            catchPlayfield.Catcher.CanHyperDash = false;
            catchPlayfield.Catcher.CatchFruitOnPlate = false;
            catchPlayfield.CatcherArea.HideComboDisplay.Value = !ShowCatcherComboCounter.Value;

            if (UnreachableEdges.Value)
            {
                catchPlayfield.Catcher.MinOffsetX += 16;
                catchPlayfield.Catcher.MaxOffsetX += -16;
            }

            drawableRuleset.Audio.AddAdjustment(AdjustableProperty.Volume, mutedVolume);
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.DrainRate = 0;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            var catchHealthProcessor = ((CatchHealthProcessor)healthProcessor);

            catchHealthProcessor.InvertHealth.Value = true;
            catchHealthProcessor.DisableDrainRate.Value = true;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            var drawableCatchHitObject = (DrawableCatchHitObject)drawable;

            Func<ArmedState, ArmedState> dodgeOverride = (s) =>
            {
                if (s == ArmedState.Hit || drawableCatchHitObject.HitObject is not PalpableCatchHitObject)
                    return ArmedState.ForceMiss;
                else if (s == ArmedState.Miss)
                    return ArmedState.Hit;

                return s;
            };

            drawableCatchHitObject.ArmedStateOverrides.Add(dodgeOverride);
        }
    }
}

