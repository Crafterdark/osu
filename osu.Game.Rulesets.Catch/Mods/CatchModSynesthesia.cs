// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSynesthesia : ModSynesthesia, IApplicableToBeatmap, IApplicableToDrawableHitObject
    {
        public override LocalisableString Description => "Colours fruits based on the rhythm.";

        private readonly OsuColour colours = new OsuColour();

        private IBeatmap? currentBeatmap { get; set; }

        [SettingSource("Replace default color")]
        public BindableBool ReplaceDefaultColor { get; } = new BindableBool();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            //Store a reference to the current beatmap to look up the beat divisor when notes are drawn
            if (currentBeatmap != beatmap)
                currentBeatmap = beatmap;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject d)
        {
            if (currentBeatmap == null) return;

            if (d is DrawableBanana || d is not DrawablePalpableCatchHitObject)
                return;

            Color4? timingBasedColour = null;

            d.HitObjectApplied += _ =>
            {
                double snapTime = d.HitObject.StartTime;
                timingBasedColour = BindableBeatDivisor.GetColourFor(currentBeatmap.ControlPointInfo.GetClosestBeatDivisor(snapTime), colours);
            };

            // Need to set this every update to ensure it doesn't get overwritten by DrawableHitObject.OnApply() -> UpdateComboColour().
            d.OnUpdate += _ =>
            {
                if (timingBasedColour != null)
                {
                    d.AccentColour.Value = timingBasedColour.Value;

                    if (ReplaceDefaultColor.Value)
                    {
                        d.Colour = timingBasedColour.Value;
                    }
                }
            };
        }
    }
}
