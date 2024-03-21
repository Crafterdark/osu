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
    public class CatchModNightcore : ModNightcore<CatchHitObject>, IApplicableToBeatmapProcessor, IApplicableToDrawableRuleset<CatchHitObject>
    {
        [SettingSource("Do not change catcher speed", "The catcher is not affected by speed increase.")]
        public BindableBool IndependentCatcherSpeed { get; } = new BindableBool();

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            if (IndependentCatcherSpeed.Value)
                ((CatchBeatmap)beatmapProcessor.Beatmap).CatcherCustomRate.Value = SpeedChange.Value;
        }

        public new void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            base.ApplyToDrawableRuleset(drawableRuleset);
            var catchDrawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)catchDrawableRuleset.Playfield;

            if (IndependentCatcherSpeed.Value)
                catchPlayfield.Catcher.CustomRate.Value = SpeedChange.Value;
        }
    }
}
