// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;
using System.Linq;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModNoDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "No Dash";
        public override string Acronym => "ND";
        public override LocalisableString Description => "The catcher can't dash or hyperdash.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Conversion;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModAlwaysDash), typeof(CatchModNoHyperDash) };

        [SettingSource("Spacing Difficulty", "The overall difficulty of the spacing between note")]
        public Bindable<float> SpacingDifficulty { get; } = new BindableFloat((float)0.75)
        {
            Precision = (float)0.01,
            MinValue = (float)0.00,
            MaxValue = (float)1.00
        };


        public override string SettingDescription
        {
            get
            {
                string spacingDifficulty_string = SpacingDifficulty.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    spacingDifficulty_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.CatcherArea.NoDashApplies = true;
            var theCatcherOnArea = catchPlayfield.CatcherArea.Catcher;
            theCatcherOnArea.Dashing = false;
            if (catchPlayfield.CatcherArea.TwinCatchersApplies)
            {
                var theTwinOnArea = catchPlayfield.CatcherArea.Twin;
                theTwinOnArea.Dashing = false;
            }

        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.NoAllDashesOffsets = true;
            catchProcessor.SpacingDifficulty = SpacingDifficulty.Value;
        }

    }
}
