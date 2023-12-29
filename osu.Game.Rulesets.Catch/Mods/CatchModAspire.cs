// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAspire : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Aspire";
        public override string Acronym => "AS";
        public override LocalisableString Description => "The beatmap requires unconventional gameplay.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Conversion;

        [SettingSource("Hyperdash permanent Target", "Initialised Hyperdash Target is not replaced if another Hyper is caught.")]
        public Bindable<bool> AspireHyperdashPermanentTarget { get; } = new BindableBool(true); //Stable default as "true"

        [SettingSource("Hyperdash when Hyper and Target fall at the same time", "Hyperdash is not initialised if the Hyper and the Target fall at the same time.")]
        public Bindable<bool> AspireHyperdashHyperAndTargetSameTime { get; } = new BindableBool(true); //Stable default as "true"

        [SettingSource("Hyperdash Target multidirectional", "Hyperdash initialises if the catcher moves far away from the Target.")]
        public Bindable<bool> AspireHyperdashMultidirectional { get; } = new BindableBool(true); //Stable default as "true"

        [SettingSource("Hyperdash overshoot freeze", "Hyperdash overshooting might not consider extra clock time after surpassing the Target.")]
        public Bindable<bool> AspireHyperdashOvershootFreeze { get; } = new BindableBool(true); //Stable default as "true" (?)

        public override string SettingDescription
        {
            get
            {
                string aspireSettingsOne_string = AspireHyperdashPermanentTarget.IsDefault ? string.Empty : string.Empty;
                string aspireSettingsTwo_string = AspireHyperdashHyperAndTargetSameTime.IsDefault ? string.Empty : string.Empty;
                string aspireSettingsThree_string = AspireHyperdashMultidirectional.IsDefault ? string.Empty : string.Empty;
                string aspireSettingsFour_string = AspireHyperdashOvershootFreeze.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    aspireSettingsOne_string,
                    aspireSettingsTwo_string,
                    aspireSettingsThree_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.AspireApplies = true;
            catchPlayfield.CatcherArea.AspireApplies = true;

            catchPlayfield.Catcher.AspireHyperdashPermanentTarget = AspireHyperdashPermanentTarget.Value;
            catchPlayfield.Catcher.AspireHyperdashHyperAndTargetSameTime = AspireHyperdashHyperAndTargetSameTime.Value;
            catchPlayfield.CatcherArea.AspireHyperdashMultidirectional = AspireHyperdashMultidirectional.Value;
            catchPlayfield.CatcherArea.AspireHyperdashOvershootFreeze = AspireHyperdashOvershootFreeze.Value;
        }

    }
}
