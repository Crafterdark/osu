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
    public class CatchModNoDashing : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "No Dashing";
        public override string Acronym => "ND";
        public override LocalisableString Description => "The catcher can't dash.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Conversion;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModAlwaysDash), typeof(CatchModNoHyperDash) };

        [SettingSource("Special Hyper 'Walk' Dashes", "The hyper 'walk' dashes will generate.")]
        public Bindable<bool> SpecialHyperWalkDashes { get; } = new BindableBool(false);

        [SettingSource("Edge Reduction", "Maximum distance from the center of the plate to the next note.")]
        public Bindable<float> EdgeReduction { get; } = new BindableFloat((float)0.50)
        {
            Precision = (float)0.01,
            MinValue = (float)0.00,
            MaxValue = (float)1.00
        };


        public override string SettingDescription
        {
            get
            {
                string edgeReduction_string = EdgeReduction.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    edgeReduction_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.CatcherArea.NoDashingApplies = true;
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
            catchProcessor.NoDashingOffsets = true;
            catchProcessor.EdgeReduction = EdgeReduction.Value;
        }

    }
}
