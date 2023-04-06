// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;
using System.Linq;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModNoHyperDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "No Hyperdash";
        public override string Acronym => "NH";
        public override LocalisableString Description => "The catcher can't hyperdash.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModNoDash) };

        [SettingSource("Spacing Difficulty", "The overall difficulty of the spacing between note")]
        public Bindable<float> SpacingDifficulty { get; } = new BindableFloat((float)0.50)
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
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.NoHyperOffsets = true;
            catchProcessor.SpacingDifficulty = SpacingDifficulty.Value;
        }

    }
}
