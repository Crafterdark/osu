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

        [SettingSource("Change 1#", "Hyper dashing won't be removed too early, in the case of a previous non-target note happening before the expected time of the target note.")]
        public Bindable<bool> AspireSettingsOne { get; } = new BindableBool(true); //Stable default as "true" (?)

        [SettingSource("Change 2#", "Hyper dashing will not apply if the target note falls at the exact same time of the hyper note.")]
        public Bindable<bool> AspireSettingsTwo { get; } = new BindableBool(false); //Stable default as "false" (?)

        [SettingSource("Change 3#", "Hyper dashing will not apply if the target note falls near the hyper note & both can be caught without movement.")]
        public Bindable<bool> AspireSettingsThree { get; } = new BindableBool(false); //Stable default as "false" (?)

        public override string SettingDescription
        {
            get
            {
                string aspireSettingsOne_string = AspireSettingsOne.IsDefault ? string.Empty : string.Empty;
                string aspireSettingsTwo_string = AspireSettingsTwo.IsDefault ? string.Empty : string.Empty;
                string aspireSettingsThree_string = AspireSettingsTwo.IsDefault ? string.Empty : string.Empty;

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
            catchPlayfield.Catcher.AspireSettingsTypeOne = AspireSettingsOne.Value;
            catchPlayfield.Catcher.AspireSettingsTypeTwo = AspireSettingsTwo.Value;
            catchPlayfield.Catcher.AspireSettingsTypeThree = AspireSettingsThree.Value;
        }

    }
}
