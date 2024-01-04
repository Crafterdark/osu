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
using osu.Framework.Graphics;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModTwinCatchers : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "Twin Catchers";
        public override string Acronym => "TC";
        public override LocalisableString Description => "Play with a new catcher hologram created from Yuzu's magic ring...";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.Conversion;

        //The edge of the Left field, near the middle of the screen
        public float LeftFieldEdge;

        //The edge of the Right field, near the middle of the screen
        public float RightFieldEdge;

        public List<float> CatcherNewRanges = new List<float>();

        public List<float> TwinNewRanges = new List<float>();

        [SettingSource("Unified playfield", "The catchers can freely move in the entire playfield.")]
        public BindableBool UnifiedPlayfield { get; } = new BindableBool(false);

        [SettingSource("Original map generation", "Patterns will stay the same as the original beatmap.")]
        public BindableBool TwinOriginalMapGeneration { get; } = new BindableBool(false);

        [SettingSource("Swap catcher field", "The catchers will exchange their field.")]
        public BindableBool SwapCatcherField { get; } = new BindableBool(false);

        [SettingSource("Twin autoplay", "The twin will play automatically.")]
        public BindableBool TwinAutoplay { get; } = new BindableBool(false);

        public override string SettingDescription
        {
            get
            {
                string twinCatchersPatterns = TwinOriginalMapGeneration.IsDefault ? string.Empty : string.Empty;
                string twinCatcherInvert = SwapCatcherField.IsDefault ? string.Empty : string.Empty;
                string twinCatcherAutoplay = TwinAutoplay.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    twinCatchersPatterns,
                    twinCatcherInvert,
                    twinCatcherAutoplay,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        private CatcherBundle twinBundle = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;
            var catcher = catchPlayfield.CatcherArea.Catcher;
            var catchBeatmap = (CatchBeatmap)drawableCatchRuleset.Beatmap;

            catchPlayfield.CatcherArea.IsPlayfieldUnique = UnifiedPlayfield.Value;

            twinBundle = new CatcherBundle(catchBeatmap.Difficulty);

            var twin = twinBundle.Catcher;

            twinBundle.Catcher.X = CatchPlayfield.CENTER_X;
            twinBundle.Catcher.CatchFruitOnPlate = catcher.CatchFruitOnPlate;
            twinBundle.Catcher.HitLighting = catcher.HitLighting;
            twinBundle.Catcher.IsTwin = true;

            catchPlayfield.CatcherArea.Add(twinBundle.CatcherTrailDisplay);
            catchPlayfield.CatcherArea.Add(twinBundle.ComboDisplay);
            catchPlayfield.CatcherArea.Add(twinBundle.DroppedObjectContainer);

            catchPlayfield.CatcherArea.Add(twin);

            catchPlayfield.CatcherBundleList.Add(twinBundle);

            //TEST
            twin.FadeTo(0.66f);

            float halfCatcherWidth = catchPlayfield.Catcher.CatchWidth / 2;

            //The edge of the Catcher field, near the the middle of the screen
            LeftFieldEdge = (CatchPlayfield.WIDTH / 2) - halfCatcherWidth;

            //The edge of the Twin catcher field, near the the middle of the screen
            RightFieldEdge = (CatchPlayfield.WIDTH / 2) + halfCatcherWidth;

            twin.X = SwapCatcherField.Value ? LeftFieldEdge / 2 : CatchPlayfield.WIDTH - ((CatchPlayfield.WIDTH - RightFieldEdge) / 2);
            twin.VisualDirection = SwapCatcherField.Value ? UI.Direction.Right : UI.Direction.Left;
            TwinNewRanges.Add(SwapCatcherField.Value ? 0 : RightFieldEdge);
            TwinNewRanges.Add(SwapCatcherField.Value ? LeftFieldEdge : CatchPlayfield.WIDTH);

            catcher.X = SwapCatcherField.Value ? CatchPlayfield.WIDTH - ((CatchPlayfield.WIDTH - RightFieldEdge) / 2) : LeftFieldEdge / 2;
            catcher.VisualDirection = SwapCatcherField.Value ? UI.Direction.Left : UI.Direction.Right;
            CatcherNewRanges.Add(SwapCatcherField.Value ? RightFieldEdge : 0);
            CatcherNewRanges.Add(SwapCatcherField.Value ? CatchPlayfield.WIDTH : LeftFieldEdge);

            catchPlayfield.CatcherArea.TwinNewRanges = TwinNewRanges;
            catchPlayfield.CatcherArea.CatcherNewRanges = CatcherNewRanges;
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            //catchProcessor.TwinCatchersOffsets = TwinCatchersOffsets.Value;
        }
    }
}
