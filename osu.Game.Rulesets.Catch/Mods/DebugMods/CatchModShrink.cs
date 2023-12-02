// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.DebugMods
{
    public class CatchModShrink : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "Shrink";

        public override string Acronym => "SK";

        public override LocalisableString Description => @"Fruits are shrinked towards the center of the playfield...";

        public override double ScoreMultiplier => UsesDefaultConfiguration ? 0.5 : 0.1;

        public override ModType Type => ModType.DifficultyReduction;

        public override IconUsage? Icon => null;

        [SettingSource("Shrink Factor", "The compression that should be applied to the playfield.", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> ShrinkFactor { get; } = new BindableDouble(0.3f)
        {
            MinValue = 0.1f,
            MaxValue = 0.6f,
            Precision = 0.1f,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            catchPlayfield.CatcherArea.IsShrinked = true;
            catchPlayfield.CatcherArea.ShrinkFactor = 1f - (float)ShrinkFactor.Value;
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            CatchBeatmapProcessor catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchBeatmapProcessor.PlayfieldShrinkFactor = 1f - (float)ShrinkFactor.Value;
        }
    }
}
