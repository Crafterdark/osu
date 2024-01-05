// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModExtend : Mod, IApplicableToBeatmap
    {
        public override string Name => "Extend";
        public override string Acronym => "EX";
        public override LocalisableString Description => "The catcher plate is extended...";
        public override double ScoreMultiplier => 0.75;
        public override ModType Type => ModType.DifficultyReduction;

        public CatchModExtend()
        {
            CatcherExtra.INSTANCE.IsApplied = false;
            CatcherExtra.INSTANCE.CatchExtend = false;
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            CatcherExtra.INSTANCE.IsApplied = true;
            CatcherExtra.INSTANCE.CatchExtend = true;
        }
    }
}
