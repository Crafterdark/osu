// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModDoubleTime : ModDoubleTime, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {

        [SettingSource("Change catcher speed based on rate", "Adjust the catcher movement to fit new speed changes.")]
        public BindableBool AdjustCatcherSpeed { get; } = new BindableBool(true);

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            if (!AdjustCatcherSpeed.Value) catchPlayfield.Catcher.CustomMultipliers[0] = SpeedChange.Value;
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;

            if (!AdjustCatcherSpeed.Value) catchBeatmapProcessor.CustomMultipliers[0] = SpeedChange.Value;
        }

    }
}
