// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModTwinCatchers : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "Twin Catchers";
        public override string Acronym => "TC";
        public override LocalisableString Description => "Two catchers, two fields.";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModWraparound) };

        //The edge of the Catcher field, near the middle of the screen
        public float LeftEdgeFromMiddle;

        //The edge of the Twin catcher field, near the middle of the screen
        public float RightEdgeFromMiddle;

        [SettingSource("Enhanced Generation", "Patterns don't show up near the center of the Playfield.")]
        public BindableBool TwinCatchersOffsets { get; } = new BindableBool(true);

        [SettingSource("Invert Catchers", "The twins exchange their location.")]
        public BindableBool TwinCatchersInvert { get; } = new BindableBool(false);

        [SettingSource("Half Autoplay", "The twin plays automatically.")]
        public BindableBool TwinCatcherAutoplay { get; } = new BindableBool(false);

        public override string SettingDescription
        {
            get
            {
                string twinCatchersPatterns = TwinCatchersOffsets.IsDefault ? string.Empty : string.Empty;
                string twinCatcherInvert = TwinCatchersInvert.IsDefault ? string.Empty : string.Empty;
                string twinCatcherAutoplay = TwinCatcherAutoplay.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    twinCatchersPatterns,
                    twinCatcherInvert,
                    twinCatcherAutoplay,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            var theCatcherOnArea = catchPlayfield.CatcherArea.Catcher;

            catchPlayfield.CatcherArea.Twin = catchPlayfield.Twin; //Set it to be visible ingame

            var theTwinOnArea = catchPlayfield.CatcherArea.Twin;

            catchPlayfield.CatcherArea.TwinCatchersApplies = true; //Apply the mod

            catchPlayfield.CatcherArea.TwinCatchersInvertApplies = TwinCatchersInvert.Value; //Apply the mod config

            float HalfCatcherLength = catchPlayfield.Catcher.CatchWidth / 2;

            //The edge of the Catcher field, near the the middle of the screen
            LeftEdgeFromMiddle = (CatchPlayfield.WIDTH / 2) - (HalfCatcherLength);

            //The edge of the Twin catcher field, near the the middle of the screen
            RightEdgeFromMiddle = (CatchPlayfield.WIDTH / 2) + (HalfCatcherLength);

            if (TwinCatchersInvert.Value)
            {
                theTwinOnArea.X = LeftEdgeFromMiddle / 2;
                theTwinOnArea.VisualDirection = Direction.Right;
                theCatcherOnArea.X = CatchPlayfield.WIDTH - ((CatchPlayfield.WIDTH - RightEdgeFromMiddle) / 2);
                theCatcherOnArea.VisualDirection = Direction.Left;
            }
            else
            {
                theCatcherOnArea.X = LeftEdgeFromMiddle / 2;
                theCatcherOnArea.VisualDirection = Direction.Right;
                theTwinOnArea.X = CatchPlayfield.WIDTH - ((CatchPlayfield.WIDTH - RightEdgeFromMiddle) / 2);
                theTwinOnArea.VisualDirection = Direction.Left;
            }
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.TwinCatchersOffsets = TwinCatchersOffsets.Value;
        }

    }
}
