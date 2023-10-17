// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods.Debug_Mods
{
    public class CatchModSpeedRun : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor

    {
        public override string Name => "Speed Run";

        public override string Acronym => "SR";

        public override LocalisableString Description => "The catcher is about to break the world record...";

        public override double ScoreMultiplier => 1.0;

        public override ModType Type => ModType.Conversion;

        [SettingSource("Catcher speed", "The actual speed to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> CatcherSpeed { get; } = new BindableDouble(1.00)
        {
            MinValue = 1.00,
            MaxValue = 2,
            Precision = 0.01,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CustomMultipliers[0] = CatcherSpeed.Value; //Walk Speed
            catchPlayfield.Catcher.CustomMultipliers[1] = CatcherSpeed.Value; //Dash Speed

        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {

        }
    }
}
