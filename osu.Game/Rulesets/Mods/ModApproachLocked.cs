// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    public class ModApproachLocked : Mod, IApplicableMod
    {
        public override string Name => "Approach Locked";

        public override string Acronym => "AL";

        public override LocalisableString Description => "The original approach rate never changes...";

        public override double ScoreMultiplier => 1;

        public override IconUsage? Icon => FontAwesome.Solid.Eye;

        public override ModType Type => ModType.Fun;

        public virtual void RestoreApproachRate(IBeatmap beatmap, BeatmapDifficulty difficulty, IReadOnlyList<Mod> mods)
        {
        }
    }
}
