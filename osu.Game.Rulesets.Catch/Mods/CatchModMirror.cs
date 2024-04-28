// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModMirror : ModHorizontalMirror, IApplicableToBeatmap
    {
        public override LocalisableString Description => "Fruits are flipped horizontally.";

        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAltH;

        /// <remarks>
        /// <see cref="IApplicableToBeatmap"/> is used instead of <see cref="IApplicableToHitObject"/>,
        /// as <see cref="CatchBeatmapProcessor"/> applies offsets in <see cref="CatchBeatmapProcessor.PostProcess"/>.
        /// <see cref="IApplicableToBeatmap"/> runs after post-processing, while <see cref="IApplicableToHitObject"/> runs before it.
        /// </remarks>
        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            foreach (var hitObject in beatmap.HitObjects)
                applyToHitObject(hitObject);
        }

        private void applyToHitObject(HitObject hitObject)
        {
            var catchObject = (CatchHitObject)hitObject;

            CatchHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(catchObject);
        }
    }
}
